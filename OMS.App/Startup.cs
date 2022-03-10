using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OMS.App.Startup))]
namespace OMS.App
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}
