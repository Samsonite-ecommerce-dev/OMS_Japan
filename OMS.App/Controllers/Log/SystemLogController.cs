using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using OMS.App.Helper;

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
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            //账号列表
            List<UserInfo> objUser_List = new List<UserInfo>();
            using (var db = new ebEntities())
            {
                objUser_List = db.UserInfo.ToList();
            }

            using (DynamicRepository db = new DynamicRepository((new logEntities())))
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(UserIp like {0}) or (LogMessage like {0})", Param = "%" + _keyword + "%" });
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,AddTime,{0})=0 ", Param = VariableHelper.SaferequestTime(_time) });
                }
                //查询
                var _list = db.GetPage<WebAppErrorLog>("select LogID,UserID,UserIP,LogLevel,LogMessage,AddTime from WebAppErrorLog order by LogID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
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
