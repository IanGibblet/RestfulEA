using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestfulEA
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

           routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //Catalogue
           // routes.MapRoute
             //   (
              //  name: "SPs",
              //  url: "{controller}",
             //   defaults: new { controller = "SP", action = "ParseIncoming", }
              //  );


            routes.MapRoute
                (
                name: "RestEA",
                url: "{controller}",
                defaults: new { Controller = "RESTEA", action = "ParseURL", }            
                );

            routes.MapRoute
            (
            name: "OSLC_BigPreview",
            url: "{controller}/{hh}/{jj}/BigPreview",
            defaults: new { Controller = "RESTEA", action = "OSLC_BigPreview", }
            );


            routes.MapRoute
            (
            name: "OSLC_SmallPreview",
            url: "{controller}/{hh}/{jj}/SmallPreview",
            defaults: new { Controller = "RESTEA", action = "OSLC_SmallPreview", }
            );




            //Any combination of route, is mapped to the parse action.
            routes.MapRoute(
              "Default",
              "{*url}",
              new { controller = "RESTEA", action = "ParseURL", path = string.Empty }
   );


            //      routes.MapRoute(
            //          name: "Default",
            //          url: "{controller}/{action}/{id}",
            //          defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //        );
        }
    }
}
