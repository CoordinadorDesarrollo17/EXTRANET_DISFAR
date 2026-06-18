using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.IO;
using System.Web.Services.Description;
using Microsoft.Extensions.DependencyInjection;
namespace Capa_Usuario
{ 
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            HostedService h = new HostedService();
            h.StartAsync();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
