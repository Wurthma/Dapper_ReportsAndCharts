using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DapperGraphs.Startup))]
namespace DapperGraphs
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
