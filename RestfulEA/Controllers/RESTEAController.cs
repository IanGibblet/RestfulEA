using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestfulEA.Models;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using RestfulEA.Models.BusinessLogic;

namespace RestfulEA.Controllers
{
    public class RESTEAController : Controller
    {

        //INFORMATION DISPLAY

        //Get all of the repositories
        public ActionResult ServiceProviderCatalog()
        {
            
            //Get the current list of EA repositories from the session variable
            List<EA.Repository> MyListRepo = new List<EA.Repository>();
            MyListRepo = HttpContext.Application["IanList"] as List<EA.Repository>;
            EA_Data_Manipulator EDM = new EA_Data_Manipulator();

            string WholeURL = EDM.GetBaseURL(Request, Url);

            ViewBag.ListOfSPs = EDM.GetListOfSPs(MyListRepo);
            ViewBag.CurrentURL = "SPC";

            //We'll look into the header to decide if we need to return somthing different for JSON
            string header = Request.Headers.Get("Accept");

            //JSON
            if (header.Contains("json"))
            {
                JObject JObjectToReturn = EA_JsonBuilder.JsonCreateSPCList(WholeURL, GetListOfSPs());
                return Content(JObjectToReturn.ToString(), "application/json");
            }
            //HTML
            if (header.Contains("html"))
            {
                return View("EA_ServiceProviderCatalog");
            }

            return null;
        }


        //The the top layer of the repository
        public ActionResult ServiceProviderTop()
        {
            //Get the current list of EA repositories from the session variable
            List<EA.Repository> MyListRepo = new List<EA.Repository>();
            MyListRepo = HttpContext.Application["IanList"] as List<EA.Repository>;
            EA_Data_Manipulator EDM = new EA_Data_Manipulator();

          
            string BaseURL = EDM.GetBaseURL(Request, Url);

            string SP_URL = Request.Url.AbsoluteUri;


            //Get the URL that was posted.
            var UrlAppended = this.Url.Action();       
            string UrlAppendage = UrlAppended.ToString();
            UrlAppendage = UrlAppendage.Remove(0, 1);

            //parse the URL
            var VallueArray = UrlAppendage.Split('/');

            //Clean emptycells
            string[] CleanURL = VallueArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();     
            string ThingOfInterest = CleanURL[1];
  
            var TOI_Array = ThingOfInterest.Split('|');

         


            EA.Repository m_Repository = new EA.Repository();  //store the current working repository
            m_Repository = getEA_Repos(TOI_Array[0]);



            //   ViewBag.CurrentURL = BaseURL + UrlAppendage;
            ViewBag.CurrentURL = SP_URL;
            ViewBag.TOI_Name = ThingOfInterest;
            ViewBag.TOI_Type = m_Repository.ObjectType;


            List <string> ListOfPackagesWithData = EDM.GetListOfPackageWithData(ThingOfInterest, m_Repository);
            List<string> ListOfPackageNameOnly = EDM.GetListOfPackageWithName(ThingOfInterest, m_Repository);
            
         

            ViewBag.ListOfPackages = ListOfPackagesWithData; 
            ViewBag.ListOfPackagesNames = ListOfPackageNameOnly;

            //We'll look into the header to decide if we need to return somthing different for JSON
            string header = Request.Headers.Get("Accept");

            if (header.Contains("json"))
            {
                JObject JObjectToReturn = EA_JsonBuilder.JsonCreateServices(BaseURL + "/" + CleanURL[1], ListOfPackagesWithData);
                return Content(JObjectToReturn.ToString(), "application/json");
            }

            if (header.Contains("html"))
            {
                return View("EA_ServiceProviderTop");
            }

            return null;
        }


        //Get a "thing" from inside the rest service
        public ActionResult ServiceProviderContent()
        {

            //Stuff we use later
            EA.Repository m_Repository = new EA.Repository();
            EA_Data_Manipulator EDM = new EA_Data_Manipulator();


            //TODO - Investigate if we really need this.
            string RootURL = EDM.GetBaseURL(Request, Url);
            


        

            string RawURL = this.Url.Action();

            string[] Spilt_URL = RawURL.Split('/');


            string URL_SPC = Spilt_URL[1];  //Will always be SPC
            string URL_SP = Spilt_URL[2];   //Name of EA file
            string URL_TOI = Spilt_URL[3];

            string[] Split_TOI = Spilt_URL[3].Split('|');


         
            string TOI_Name = Split_TOI[0];
            string TOI_Type = Split_TOI[1];
            string TOI_GUID = Split_TOI[2];



            
          


            string SP_Root_URL = RootURL + URL_SPC + "/" + URL_SP;
        
            ViewBag.TOI_Name = TOI_Name;
            ViewBag.TOI_Type = TOI_Type;
            ViewBag.TOI_GUID = TOI_GUID;
            ViewBag.CurrentURL = URL_SPC + "/" + URL_SP;
            ViewBag.CurrentFullURL = URL_SPC + "/" + URL_SP + "/" + URL_TOI;


            //Get the master repo which is needed by all things that have been clicked.
            m_Repository = getEA_Repos(URL_SP);
            
            //otPackage Lists
            List<string> ListOfPackages = new List<string>();
            List<string> ListOfDiagrams = new List<string>();
            List<string> ListOfElements = new List<string>();
            List<string> ListOfPackagesNames = new List<string>();
            List<string> ListOfDiagramsNames = new List<string>();
            List<string> ListOfElementsNames = new List<string>();

            //otDiagram Lists
            Dictionary<string, string> DiagramDictionary = new Dictionary<string, string>();
   

            //otElement Lists
            List<string> ListOfConnectors = new List<string>();
            List<string> ListOfConnectorNames = new List<string>();
            Dictionary<string, string> ElementAttributes = new Dictionary<string, string>();
            RestfulEA.Models.EA_TaggedValueStore myTVS = new Models.EA_TaggedValueStore();


            //otConnector 
            //EA.Connector MyLink = new EA.Connector();
            EA.Connector Mycon;
            Dictionary<string, string> ConnectorDictionary = new Dictionary<string, string>();


            //Objects for the JSON
            EA_Diagram EA_JSON_Diagram = new EA_Diagram();



            //Generate the content for the viewbag depending on type of object that the 
            //user last clicked on.
            switch (TOI_Type)
            {
                case "otPackage":
                    EA.Package PackageToShow = (EA.Package)m_Repository.GetPackageByGuid(TOI_GUID);
                    EDM.Generate_otPackage_content(PackageToShow, out ListOfPackages, out ListOfDiagrams, out ListOfElements, out ListOfPackagesNames, out ListOfDiagramsNames, out ListOfElementsNames);
                    ViewBag.PackageID = PackageToShow.PackageID.ToString();
                    ViewBag.ListOfPackages = ListOfPackages;
                    ViewBag.ListOfDiagrams = ListOfDiagrams;
                    ViewBag.ListOfElements = ListOfElements;
                    ViewBag.ListOfPackagesNames = ListOfPackagesNames;
                    ViewBag.ListOfDiagramsNames = ListOfDiagramsNames;
                    ViewBag.ListOfElementsNames = ListOfElementsNames;

                    break;
                case "otDiagram":

                    //HTML Content
                    EDM.Generate_otDiagram_content(m_Repository, TOI_GUID, SP_Root_URL, out ListOfElements,out ListOfElementsNames, out ListOfConnectors, out ListOfConnectorNames, out DiagramDictionary);
                    ViewBag.ListOfElements = ListOfElements;
                    ViewBag.ListOfElementsNames = ListOfElementsNames;
                    ViewBag.ListOfLinkURLs = ListOfConnectors;
                    ViewBag.ListOfLinkNames = ListOfConnectorNames;
                    ViewBag.CurrentURL += "/" + TOI_Name + "|" + TOI_Type + "|" + TOI_GUID;
                    ViewData["CurrentURL"] = TOI_Name + "|" + TOI_Type + "|" + TOI_GUID;
                    ViewData["SP_URL"] = URL_SPC + "/" + URL_SP;
                    ViewBag.DiagramName = TOI_Name;

                    //JSON Conten

                    break;
                case "otElement":
                    EDM.Generate_otElement_content(m_Repository, TOI_GUID, out ListOfDiagrams, out ListOfDiagramsNames, out ListOfConnectors, out ListOfConnectorNames, out ElementAttributes, out  myTVS);
                    ViewBag.TG_Store = myTVS;
                    ViewBag.ListOfConnectors = ListOfConnectors;
                    ViewBag.ListOfConnectorNames = ListOfConnectorNames;
                    ViewBag.ListOfDiagramsNames = ListOfDiagramsNames;
                    ViewBag.ListOfDiagrams = ListOfDiagrams;
                    ViewBag.ElementName = ElementAttributes["Name"];
                    ViewBag.notes = ElementAttributes["Name"];
                    ViewBag.Author = ElementAttributes["Author"];
                    ViewData["CurrentURL"] = TOI_Name + "|" + TOI_Type + "|" + TOI_GUID;

                    break;
                case "otTaggedValue":
                    EDM.Generate_otTaggedValues_content(m_Repository, TOI_GUID, TOI_Name, out myTVS);
                    ViewBag.TG_Store = myTVS;

                    break;


                case "otConnector":
                    EDM.Generate_otConnector_content(m_Repository,TOI_GUID,TOI_Name,TOI_Type, out Mycon, out ConnectorDictionary);

                    ViewBag.TOI_Name = TOI_Name;
                    ViewBag.TOI_Type = TOI_Type;
                    ViewBag.TOI_GUID = TOI_GUID;

                    ViewBag.Alias = Mycon.Alias;
                    ViewBag.Colour = Mycon.Color;
                    ViewBag.Direction = Mycon.Direction;
                    ViewBag.Notes = Mycon.Notes;
                    ViewBag.Type = Mycon.Type;
                    ViewBag.ConnectorID = Mycon.ConnectorID;
                    ViewBag.Sterotype = Mycon.Stereotype;


                    break;
                default:
                    break;
            }

            //RETURN HTML
            string header = Request.Headers.Get("Accept");

            if (header.Contains("html"))
            {

                switch (TOI_Type)
                {
                    case "otPackage":
                        return View("EA_ShowPackage");
                        
                    case "otDiagram":
                        return View("EA_ShowDiagram");                
                    
                    case "otElement":
                        return View("EA_ShowElement");
             
                    case "otTaggedValue":
                        return View("EA_ShowTaggedValues");

                    case "otConnector":
                        return View("EA_ShowConnector");


                    default:
                        break;
                }


            }


            //RETURN JSON
            if (header.Contains("json"))
            {


                string DiagramRoot = RootURL + URL_SPC + "/" + URL_SP;
                string headerText = "application/json";

                switch (TOI_GUID)
                {
                    case "otPackage":
                        JObject JotPackageToReturn = EA_JsonBuilder.JsonCreatePackage(DiagramRoot, ListOfPackages, ListOfDiagrams, ListOfElements);
                        return Content(JotPackageToReturn.ToString(), headerText);
                       
                    case "otDiagram":
                        JObject JotDiagramToReturn = EA_JsonBuilder.JsonCreateDiagram(DiagramRoot, ListOfElements, DiagramDictionary, ListOfConnectors);
                        return Content(JotDiagramToReturn.ToString(), headerText);

         
                    case "otElement":
                        JObject JotElemenet = EA_JsonBuilder.JsonCreateElement(DiagramRoot, ElementAttributes, myTVS, ListOfDiagrams);
                        return Content(JotElemenet.ToString(), headerText);

                    case "otTaggedValue":
                        JObject jotTaggedValue = EA_JsonBuilder.JsonCreateTaggedValues(myTVS);
                        return Content(jotTaggedValue.ToString(), headerText);

                    case "otConnector":
                        JObject jotConnector = EA_JsonBuilder.JsonCreateConnector(DiagramRoot, ConnectorDictionary);
                        return Content(jotConnector.ToString(), headerText);



                    default:
                        break;
                }




            }




            return null;
        }


        public ActionResult OSLC_SmallPreview()
        {
            //Get the URL that was posted.
            var fullURL = this.Url.Action();
            string url = fullURL.ToString();

            //parse the URL
            var VallueArray = url.Split('/');

            //Clean emptycells
            string[] CleanURL = VallueArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //store the current working repository
            EA.Repository m_Repository = new EA.Repository();

            //I don't know what this is about but it can cause trouble if not caught
            if (CleanURL.Count() > 2)
            {
                if (CleanURL[1] == "__browserLink")
                { return View(); }
            }

            string ThingOfInterest = CleanURL[CleanURL.Count() - 2];
            string ActionOfInterest = CleanURL[CleanURL.Count() - 1];

            m_Repository = getEA_Repos(CleanURL[1]);

            string[] UrlArray = ThingOfInterest.Split('|');

            string TOI_Name = UrlArray[0];
            string TOI_Type = UrlArray[1];
            string TOI_GUID = UrlArray[2];



            List<string> ListOfElementsTypes = new List<string>();
            List<string> ListOfElementsNames = new List<string>();
            List<string> ListOfElementAuthor = new List<string>();
            List<string> ListOfElementSteroType = new List<string>();
            List<string> ListOfElementNotes = new List<string>();

            EA.Diagram DiagramToShow = (EA.Diagram)m_Repository.GetDiagramByGuid(TOI_GUID);
            ViewBag.DiagramName = DiagramToShow.Name;


            for (short iDO = 0; iDO < DiagramToShow.DiagramObjects.Count; iDO++)
            {
                EA.DiagramObject MyDO = (EA.DiagramObject)DiagramToShow.DiagramObjects.GetAt(iDO);
                int ID = m_Repository.GetElementByID(MyDO.ElementID).ElementID;
                EA.Element MyEle = (EA.Element)m_Repository.GetElementByID(ID);


                ListOfElementsNames.Add(MyEle.Name);
                ListOfElementsTypes.Add(MyEle.Type);
                ListOfElementAuthor.Add(MyEle.Author);
                ListOfElementSteroType.Add(MyEle.Stereotype);
                ListOfElementNotes.Add(MyEle.Notes);

            }

            ViewBag.ListOfElementsTypes = ListOfElementsTypes;
            ViewBag.ListOfElementsNames = ListOfElementsNames;
            ViewBag.ListOfElementAuthor = ListOfElementAuthor;
            ViewBag.ListOfElementSteroType = ListOfElementSteroType;
            ViewBag.ListOfElementNotes = ListOfElementNotes;

            return View("EA_small_preview");

        }


        //This is used for extra commands such as doing the preview.  
        public ActionResult OSLC_BigPreview()
        {
            //Get the URL that was posted.
            var fullURL = this.Url.Action();
            string url = fullURL.ToString();

            //parse the URL
            var VallueArray = url.Split('/');

            //Clean emptycells
            string[] CleanURL = VallueArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //store the current working repository
            EA.Repository m_Repository = new EA.Repository();

            //I don't know what this is about but it can cause trouble if not caught
            if (CleanURL.Count() > 2)
            {
                if (CleanURL[1] == "__browserLink")
                { return View(); }
            }



            string ThingOfInterest = CleanURL[CleanURL.Count() - 2];
            string ActionOfInterest = CleanURL[CleanURL.Count() - 1];

            m_Repository = getEA_Repos(CleanURL[1]);

            string[] UrlArray = ThingOfInterest.Split('|');

            string TOI_Name = UrlArray[0];
            string TOI_Type = UrlArray[1];
            string TOI_GUID = UrlArray[2];



            string path = AppDomain.CurrentDomain.BaseDirectory + "images";

            //If the path does exist then create it.
            if (System.IO.Directory.Exists(path) == false)
            { System.IO.Directory.CreateDirectory(path); }

            EA.Project eaProject = m_Repository.GetProjectInterface();
            eaProject.PutDiagramImageToFile(TOI_GUID, path + "\\" + TOI_Name + ".jpg", 1);

            return base.File(path + "\\" + TOI_Name + ".jpg", "image/jpeg");

        }

        //DISPLAY SUPPORT

        private List<string> GetListOfSPs()
        {
            List<string> ListOfSPs = new List<string>();
            //Retreive the EA object from from the application object
            List<EA.Repository> MyListRepo = new List<EA.Repository>();
            MyListRepo = HttpContext.Application["IanList"] as List<EA.Repository>;
            foreach (EA.Repository repos in MyListRepo)
            {
                string SPFileName = System.IO.Path.GetFileNameWithoutExtension(repos.ConnectionString);
                ListOfSPs.Add(SPFileName);
            }
            return ListOfSPs;
        }


        //Get the stored EA reposiory from the application state
        //This can't goto the Help class because the repository is held in the 
        //application state variable
        private EA.Repository getEA_Repos(string pickedSP)
        {

            List<EA.Repository> MyListRepo = new List<EA.Repository>();
            MyListRepo = HttpContext.Application["IanList"] as List<EA.Repository>;
            foreach (EA.Repository reposLoop in MyListRepo)
            {
                try
                {
                    string EAFileName = System.IO.Path.GetFileNameWithoutExtension(reposLoop.ConnectionString);
                    if (EAFileName == pickedSP)
                        return reposLoop;

                }
                catch
                {

                }
            }
            return null;
        }




        //FORM SUBMISSION

       [HttpPost]
        public ActionResult  ServiceProviderContent(string textBoxFormData)
        {

            string RawURL = this.Url.Action();
            string[] SpiltURL = RawURL.Split('/');
            string[] RAW_TOI = SpiltURL[3].Split('|');



            string ServiceProvider = SpiltURL[2];
            string TOI_Name = RAW_TOI[0];
            string TOI_Type = RAW_TOI[1];
            string TOI_GUID = RAW_TOI[2];        


            EA.Repository m_Repository = new EA.Repository();
            m_Repository = getEA_Repos(SpiltURL[2]);




            EA_Creation_Factory.CreateDiagram(m_Repository, TOI_GUID, textBoxFormData);





            return ServiceProviderContent();
        }







    }
}


    
