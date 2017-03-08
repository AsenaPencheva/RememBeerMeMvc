﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using RememBeer.Data.DbContexts;
using RememBeer.Data.DbContexts.Contracts;

namespace RememBeer.MvcClient
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapConfig.RegisterMappings();

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<RememBeerMeDbContext, Data.Migrations.Configuration>());
        }
    }
}
