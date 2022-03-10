using System;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Mvc;

using OMS.API.Models;
using OMS.API.Utils;

namespace OMS.API
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            NinjectConfig.RegisterNinject(GlobalConfiguration.Configuration);
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            //var _error = Server.GetLastError();
            //错误提示
            ApiResponse apiResult = new ApiResponse()
            {
                Code = (int)ApiResultCode.Fail,
                Message = "The specified API Path is invalid!"
            };
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Response.Charset = "utf-8";
            Response.ContentType = "application/json";
            Response.Write(UtilsHelper.JsonSerialize(apiResult));
            Server.ClearError();
        }
    }
}
