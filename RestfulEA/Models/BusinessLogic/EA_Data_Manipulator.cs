
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



        public  string GetBaseURL(HttpRequestBase request, UrlHelper Url)
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


        public string GetSPURL(HttpRequestBase request, UrlHelper url)
        {



            var urlBuilder = new System.UriBuilder(request.Url.AbsoluteUri)
            {
                Path = url.Action("ParseURL", "RESTEA"),
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
                throw new Exception("SP Url value is null");
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

        public void Generate_otPackage_content(Package PackageToShow,
            out List<string> ListOfPackages,
            out List<string> ListOfDiagrams,
            out List<string> ListOfElements,
            out List<string> ListOfPackagesNames,
            out List<string> ListOfDiagramsNames,
            out List<string> ListOfElementsNames)

        {
            ListOfPackages = new List<string>();
            ListOfDiagrams = new List<string>();
            ListOfElements = new List<string>();
            ListOfPackagesNames = new List<string>();
            ListOfDiagramsNames = new List<string>();
            ListOfElementsNames = new List<string>();   

            //Stick the diagrams and packages into string lists
            foreach (EA.Diagram DiagramLoop in PackageToShow.Diagrams)
            {
                ListOfDiagrams.Add(DiagramLoop.Name + "|" + DiagramLoop.ObjectType + "|" + DiagramLoop.DiagramGUID);
                ListOfDiagramsNames.Add(DiagramLoop.Name);
            }
            //Put diarams intos lists  
            foreach (EA.Package PackageLoop in PackageToShow.Packages)
            {
                ListOfPackages.Add(PackageLoop.Name + "|" + PackageLoop.ObjectType + "|" + PackageLoop.PackageGUID);
                ListOfPackagesNames.Add(PackageLoop.Name);
            }
            //Put elements into lists
            foreach (EA.Element ElementLoop in PackageToShow.Elements)
            {
                ListOfElements.Add(ElementLoop.Name + "|" + ElementLoop.ObjectType + "|" + ElementLoop.ElementGUID);
                ListOfElementsNames.Add(ElementLoop.Name);
            }

          
        }



        public void Generate_otDiagram_content(EA.Repository m_Repository, string TOI_GUID,
            out List<string> listOfElements,
            out List<string> listOfElementsNames,
            out List<string> listOfLinks,
            out List<string> listOfLinkNames)
        {

            listOfElements = new List<string>();
            listOfElementsNames = new List<string>();
            listOfLinks = new List<string>();
            listOfLinkNames = new List<string>();

            EA.Diagram DiagramToShow = (EA.Diagram)m_Repository.GetDiagramByGuid(TOI_GUID);



            //STORE DIAGRAM ELEMENTS
            for (short iDO = 0; iDO < DiagramToShow.DiagramObjects.Count; iDO++)
            {
                EA.DiagramObject MyDO = (EA.DiagramObject)DiagramToShow.DiagramObjects.GetAt(iDO);
                int ID = m_Repository.GetElementByID(MyDO.ElementID).ElementID;
                EA.Element MyEle = (EA.Element)m_Repository.GetElementByID(ID);
                listOfElements.Add(MyEle.Name + "|" + MyEle.ObjectType + "|" + MyEle.ElementGUID);
                listOfElementsNames.Add(MyEle.Name);
            }



            //STORE DIAGRAM LINKS     
            for (short iDO = 0; iDO < DiagramToShow.DiagramLinks.Count; iDO++)
            {
                EA.DiagramLink MyLink = (EA.DiagramLink)DiagramToShow.DiagramLinks.GetAt(iDO);   
                int ID = m_Repository.GetConnectorByID(MyLink.ConnectorID).ConnectorID;

                EA.Connector con;

                try //Try and get the connector object from the repository
                {
                    con = (EA.Connector)m_Repository.GetConnectorByID(ID);
                    listOfLinks.Add(con.Name + "|" + con.ObjectType + "|" + con.ConnectorGUID);
                    listOfLinkNames.Add(con.Name);
                }
                catch { }




            }







        }
    }



}