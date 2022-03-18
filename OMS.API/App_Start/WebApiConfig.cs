using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace OMS.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //自定义权限验证标签
            config.Filters.Add(new AuthorizeFilterAttribute());

            //自定义异常过滤器
            config.Filters.Add(new ApiExceptionFilterAttribute());
        }
    }
}
