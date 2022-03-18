using System.Collections.Generic;
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
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _state = VariableHelper.SaferequestInt(Request.Form["state"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);

            using (DynamicRepository db = new DynamicRepository((new logEntities())))
            {
                if (_type > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "LogType={0}", Param = _type });
                }

                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(Ip like {0}) or (Url like {0})", Param = "%" + _keyword + "%" });
                }

                if (_state > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "State={0}", Param = (_state - 1) });
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,CreateTime,{0})=0 ", Param = VariableHelper.SaferequestTime(_time) });
                }
                //查询
                var _list = db.GetPage<WebApiAccessLog>("select * from WebApiAccessLog order by ID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
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
