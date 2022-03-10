using System.Web.Mvc;

using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class LoginController : BaseController
    {
        //
        // GET: /Login/

        public ActionResult Index()
        {
            ////是否是移动端
            //if (System.Web.HttpContext.Current.Request.Browser.IsMobileDevice)
            //{
            //    return new RedirectResult("/Mobile/Login/Index");
            //}
            //else
            //{
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            return View();
            //}
        }

        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            string _username = VariableHelper.SaferequestStr(Request.Form["username"]);
            string _password = VariableHelper.SaferequestStr(Request.Form["password"]);
            int _is_remember = VariableHelper.SaferequestInt(Request.Form["isremember"]);
            object[] _O = UserLoginService.UserLogin(_username, _password, (_is_remember == 1));
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
            return Redirect("~/Login/Index");
        }
    }
}
