using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(spMapper2.Startup))]
namespace spMapper2
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
