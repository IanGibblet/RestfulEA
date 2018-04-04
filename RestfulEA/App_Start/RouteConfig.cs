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


            routes.MapRoute(
                "Contact",
                "contact",
                new { Controller = "Contact", action = "ContactForm" }  );


            routes.MapRoute(
                name: "Default",
                url: "SPC", //Show service provider catalog
                defaults: new { Controller = "RESTEA", Action = "ServiceProviderCatalog",  }
                );

            routes.MapRoute(
                 name: "Default2",
                 url: "SPC/{SP}",  //show the service provider
                 defaults: new { Controller = "RESTEA", Action = "ServiceProviderTop", }
                 );


            routes.MapRoute(
                 name: "Default3",
                 url: "SPC/{SP}/{EA_Content}",
                 defaults: new { Controller = "RESTEA", Action = "ServiceProviderContent", }
                 );


            routes.MapRoute(
                 name: "Default4",
                 url: "SPC/{SP}/{EA_Content}/BigPreview",
                 defaults: new { Controller = "RESTEA", Action = "OSLC_BigPreview", }
                 );

            routes.MapRoute(
                 name: "Default5",
                 url: "SPC/{SP}/{EA_Content}/SmallPreview",
                 defaults: new { Controller = "RESTEA", Action = "OSLC_SmallPreview", }
                 );






            //     routes.MapRoute(
            //         name: "Default2",
            //        url: "Boom/{dog}/{cat}",
            //        defaults: new { Controller = "RESTEA", Action = "second",  }
            //        );




            //  routes.MapRoute("MyRoute", "{controller}", new { Action = "ParseURL" });



            /*

            //Catalogue
            routes.MapRoute
                (
                name: "Welcome",
                url: "{controller}",
                defaults: new { controller = "RESTEA", action = "ParseURL", }
                );

            routes.MapRoute
                (
                name: "Inside_EAP",
                url: "{controller}",
                defaults: new { controller = "RESTEA", action = "ParseURL", }
                );

*/


            /* 
                      routes.MapRoute
                          (
                          name: "RestEA",
                          url: "{controller}",
                          defaults: new { Controller = "RESTEA", action = "ParseURL", }            
                          );


                      routes.MapRoute
                          (
                          name: "RestEA_SP",
                          url: "{controller}/{SP}",
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
          */

            //        routes.MapRoute(
            //           name: "Default",
            //          url: "{controller}/{action}/{id}",
            //            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //       );
        }
    }
}
