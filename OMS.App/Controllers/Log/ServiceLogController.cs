using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class ServiceLogController : BaseController
    {
        //
        // GET: /ServiceLog/

        #region 服务日志
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();
            //服务列表
            ViewData["module_list"] = ModuleService.GetModuleObject();
            //日志等级
            ViewData["loglevel_list"] = LogHelper.LogLevelObject();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            int _level = VariableHelper.SaferequestInt(Request.Form["level"]);
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            //服务列表
            List<ServiceModuleInfo> objServiceModuleInfo_List = ModuleService.GetModuleObject();
            using (var db = new logEntities())
            {
                var _lambda = db.ServiceLog.AsQueryable();

                //搜索条件
                if (_type > 0)
                {
                    _lambda = _lambda.Where(p => p.LogType == _type);
                }

                if (_level > 0)
                {
                    _lambda = _lambda.Where(p => p.LogLevel == _level);
                }

                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.LogMessage.Contains(_keyword));
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    var _datetime = VariableHelper.SaferequestTime(_time);
                    _lambda = _lambda.Where(p => SqlFunctions.DateDiff("day", p.CreateTime, _datetime) == 0);
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.ID, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ID,
                               s1 = (objServiceModuleInfo_List.Where(p => p.ModuleID == dy.LogType).SingleOrDefault() != null) ? objServiceModuleInfo_List.Where(p => p.ModuleID == dy.LogType).SingleOrDefault().ModuleTitle : string.Empty,
                               s2 = LogHelper.GetLogTypeDisplay(dy.LogLevel),
                               s3 = dy.LogMessage,
                               s4 = dy.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
                return _result;
            }
        }
        #endregion
    }
}
