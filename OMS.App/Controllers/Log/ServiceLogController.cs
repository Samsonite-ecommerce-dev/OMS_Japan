using System;
using System.Collections.Generic;
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
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            int _level = VariableHelper.SaferequestInt(Request.Form["level"]);
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            //服务列表
            List<ServiceModuleInfo> objServiceModuleInfo_List = ModuleService.GetModuleObject();
            using (DynamicRepository db = new DynamicRepository((new logEntities())))
            {
                //搜索条件
                if (_type > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "LogType={0}", Param = _type });
                }

                if (_level > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "LogLevel={0}", Param = _level });
                }

                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "LogMessage like {0}", Param = "%" + _keyword + "%" });
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,CreateTime,{0})=0 ", Param = VariableHelper.SaferequestTime(_time) });
                }
                //查询
                var _list = db.GetPage<ServiceLog>("select ID,LogType,LogLevel,LogMessage,CreateTime from ServiceLog order by ID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
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
