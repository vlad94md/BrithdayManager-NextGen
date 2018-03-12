using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BirthdayManager.Startup))]
namespace BirthdayManager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
