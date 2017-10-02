using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestfulEA
{
    public class MvcApplication : System.Web.HttpApplication
    {

     

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);


            //Kill old ea process

            Process[] _proceses = null;
            _proceses = Process.GetProcesses();
            foreach (Process proces in _proceses)
            {
                if(proces.ProcessName=="EA")
                {

                    try

                    {
                        proces.Kill();
                    }
                    catch
                    {

                    }
                    
                }


             
            }



            //This will create some new EA processes
            List<EA.Repository> MyListRepo  = App_Start.StartUp.ParseEAProjects();
            Application["IanList"] = MyListRepo;



            

        }


  


    }
}
