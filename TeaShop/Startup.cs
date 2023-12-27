using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TeaShop.Startup))]
namespace TeaShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }


    }
}
