using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class SystemLogController : BaseController
    {
        //
        // GET: /SystemLog/

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

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            //账号列表
            List<UserInfo> objUser_List = new List<UserInfo>();
            using (var db = new ebEntities())
            {
                objUser_List = db.UserInfo.ToList();
            }

            using (var db = new logEntities())
            {
                var _lambda = db.WebAppErrorLog.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.UserIP.Contains(_keyword) || p.LogMessage.Contains(_keyword));
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    var _datetime = VariableHelper.SaferequestTime(_time);
                    _lambda = _lambda.Where(p => SqlFunctions.DateDiff("day", p.AddTime, _datetime) == 0);
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.LogID, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.LogID,
                               s1 = (objUser_List.Where(p => p.UserID == dy.UserID).SingleOrDefault() != null) ? objUser_List.Where(p => p.UserID == dy.UserID).SingleOrDefault().RealName : string.Empty,
                               s2 = dy.UserIP,
                               s3 = dy.LogMessage,
                               s4 = dy.AddTime.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
                return _result;
            }
        }
        #endregion
    }
}
