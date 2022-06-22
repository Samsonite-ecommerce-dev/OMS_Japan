using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using OMS.App.Helper;


namespace OMS.App.Controllers
{
    public class ApiLogController : BaseController
    {
        //
        // GET: /ApiLog/

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
            //API用途
            ViewData["type_list"] = APIHelper.APITypeObject();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _state = VariableHelper.SaferequestInt(Request.Form["state"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);

            using (var db = new logEntities())
            {
                var _lambda = db.WebApiAccessLog.AsQueryable();

                if (_type > 0)
                {
                    _lambda = _lambda.Where(p => p.LogType == _type);
                }

                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.Ip.Contains(_keyword) || p.Url.Contains(_keyword));
                }

                if (_state == 1)
                {
                    _lambda = _lambda.Where(p => !p.State);
                }
                else
                {
                    _lambda = _lambda.Where(p => p.State);
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    var _datetime = VariableHelper.SaferequestTime(_time);
                    _lambda = _lambda.Where(p => SqlFunctions.DateDiff("day", p.CreateTime, _datetime) == 0);
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.id, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.id,
                               s1 = dy.Ip,
                               s2 = dy.Url,
                               s3 = dy.RequestID,
                               s4 = (dy.State) ? "<label class=\"color_primary\">成功</label>" : "<label class=\"color_danger\">失败<label>",
                               s5 = dy.Remark,
                               s6 = dy.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
                return _result;
            }
        }
        #endregion
    }
}
