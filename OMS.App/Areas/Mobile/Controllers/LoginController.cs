using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Areas.Mobile.Controllers
{
    public class LoginController : BaseController
    {
        // GET: Mobile/Login
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            return View();
        }

        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            string _username = VariableHelper.SaferequestStr(Request.Form["username"]);
            string _password = VariableHelper.SaferequestStr(Request.Form["password"]);
            object[] _O = UserLoginService.UserLogin(_username, _password, true);
            _result.Data = new
            {
                result = _O[0],
                msg = _O[1]
            };
            return _result;
        }


        public RedirectResult LoginOut()
        {
            //清空信息
            UserLoginService.UserLoginOut();
            return Redirect("~/Mobile/Login/Index");
        }
    }
}