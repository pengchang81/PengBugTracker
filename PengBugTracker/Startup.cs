using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PengBugTracker.Startup))]
namespace PengBugTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
