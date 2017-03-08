﻿using Microsoft.Owin;
using Owin;

using RememBeer.MvcClient.App_Start;

[assembly: OwinStartupAttribute(typeof(RememBeer.MvcClient.Startup))]
namespace RememBeer.MvcClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app, NinjectWebCommon.Kernel);
        }
    }
}
