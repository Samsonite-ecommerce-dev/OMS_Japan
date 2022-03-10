using Microsoft.Owin;
using OMS.API.Interface;
using Owin;
using System.Web.Http;

[assembly: OwinStartupAttribute(typeof(OMS.API.Startup))]
namespace OMS.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
