
/// <summary>
/// This class does the data crunching for the RestEAController
/// </summary>




using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EA;

namespace RestfulEA.Models.BusinessLogic
{
    public class EA_Data_Manipulator
    {


        //Returns a list of service provider for the service provider catalog
        //Take a list of the EA repositories and converts them to a list of names.
      
        public List<string> GetListOfSPs(List<EA.Repository> EA_List )
        {
        
            List<string> ListOfSPs = new List<string>();   //List to return
       
            List<EA.Repository> MyListRepo = new List<EA.Repository>();  
     
            foreach (EA.Repository Repos in EA_List)
            {
                string SPFileName = System.IO.Path.GetFileNameWithoutExtension(Repos.ConnectionString);
                ListOfSPs.Add(SPFileName);
            }

            return ListOfSPs;

        }



        public  string GetWholeURL(HttpRequestBase request, UrlHelper Url)
        {
            var urlBuilder = new System.UriBuilder(request.Url.AbsoluteUri)
            {
                Path = Url.Action("ParseURL", "RESTEA"),
                Query = null,
            };

            Uri uri = urlBuilder.Uri;
            string WholeURL = urlBuilder.ToString();




            if (WholeURL != null)
            {
                return WholeURL;
            }
            else
            {
                throw new Exception("Whole Url value is null"); 
            }


        }

        public List<string> GetListOfPackageWithData(string thingOfInterest, Repository m_Repository)
        {
         
            List<EA.Package> ListOfRootPackages = EA_Helper.GetListOfRootNodes(thingOfInterest, m_Repository);   //GET THE PACKAGES INSIDE THE ROOT NODE
            List<string> ListOfPackageAllData = new List<string>();

            foreach (EA.Package PackageLoop in ListOfRootPackages)
            {
                ListOfPackageAllData.Add(PackageLoop.Name + "|" + PackageLoop.ObjectType + "|" + PackageLoop.PackageGUID);
             //   ListOfPackageNameOnly.Add(PackageLoop.Name);
            }


            return ListOfPackageAllData;
        }


        public List<string> GetListOfPackageWithName(string thingOfInterest, Repository m_Repository)
        {
            List<EA.Package> ListOfRootPackages = EA_Helper.GetListOfRootNodes(thingOfInterest, m_Repository);  //GET THE PACKAGES INSIDE THE ROOT NODE
            List<string> ListOfPackageNameOnly = new List<string>();
          
            foreach (EA.Package PackageLoop in ListOfRootPackages)
            {
                ListOfPackageNameOnly.Add(PackageLoop.Name);
            }
            return ListOfPackageNameOnly;     

        }

        internal void Generate_otPackage_content(Package packageToShow)
        {
            throw new NotImplementedException();
        }
    }



}