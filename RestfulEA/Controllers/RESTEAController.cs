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


            //TODO - Tidy up all this array stuff
            //Get the URL that was posted.
            var UrlAppended = this.Url.Action();
            string UrlAppendage = UrlAppended.ToString();
            UrlAppendage = UrlAppendage.Remove(0, 1);
            var URLArray = UrlAppendage.Split('/'); //parse the URL
            string[] BaseURL = URLArray.Where(x => !string.IsNullOrEmpty(x)).ToArray(); //Clean emptycells

            var ThingArrary = BaseURL[2].Split('|');


            string TOI_Name = ThingArrary[0];
            string TOI_Type = ThingArrary[1];
            string TOI_GUID = ThingArrary[2];
            string SP_Root_URL = RootURL + BaseURL[0] + "/" + BaseURL[1];
        
            ViewBag.TOI_Name = TOI_Name;
            ViewBag.TOI_Type = TOI_Type;
            ViewBag.TOI_GUID = TOI_GUID;



            ViewBag.CurrentURL = BaseURL[0] + "/" + BaseURL[1];


            //Get the master repo which is needed by all things that have been clicked.
            m_Repository = getEA_Repos(URLArray[1]);
            


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
            switch (ThingArrary[1])
            {
                case "otPackage":
                    EA.Package PackageToShow = (EA.Package)m_Repository.GetPackageByGuid(ThingArrary[2]);
                    EDM.Generate_otPackage_content(PackageToShow, out ListOfPackages, out ListOfDiagrams, out ListOfElements, out ListOfPackagesNames, out ListOfDiagramsNames, out ListOfElementsNames);
                    ViewBag.ListOfPackages = ListOfPackages;
                    ViewBag.ListOfDiagrams = ListOfDiagrams;
                    ViewBag.ListOfElements = ListOfElements;
                    ViewBag.ListOfPackagesNames = ListOfPackagesNames;
                    ViewBag.ListOfDiagramsNames = ListOfDiagramsNames;
                    ViewBag.ListOfElementsNames = ListOfElementsNames;

                    break;
                case "otDiagram":

                    //HTML Content
                    EDM.Generate_otDiagram_content(m_Repository, ThingArrary[2], SP_Root_URL, out ListOfElements,out ListOfElementsNames, out ListOfConnectors, out ListOfConnectorNames, out DiagramDictionary);
                    ViewBag.ListOfElements = ListOfElements;
                    ViewBag.ListOfElementsNames = ListOfElementsNames;
                    ViewBag.ListOfLinkURLs = ListOfConnectors;
                    ViewBag.ListOfLinkNames = ListOfConnectorNames;
                    ViewBag.CurrentURL += "/" + TOI_Name + "|" + TOI_Type + "|" + TOI_GUID;
                    ViewData["CurrentURL"] = TOI_Name + "|" + TOI_Type + "|" + TOI_GUID;
                    ViewData["SP_URL"] = BaseURL[0] + "/" + BaseURL[1];
                    ViewBag.DiagramName = ThingArrary[0];

                    //JSON Conten

                    break;
                case "otElement":
                    EDM.Generate_otElement_content(m_Repository, ThingArrary[2], out ListOfDiagrams, out ListOfDiagramsNames, out ListOfConnectors, out ListOfConnectorNames, out ElementAttributes, out  myTVS);
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

            //We'll look into the header to decide if we need to return somthing different for JSON
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

            if (header.Contains("json"))
            {


                string DiagramRoot = RootURL + BaseURL[0] + "/" + BaseURL[1];
                string headerText = "application/json";

                switch (ThingArrary[1])
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





        /// <summary>
        /// /////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>

        // GET: RESTEA
        public ActionResult ParseURL()
        {



            //Get the URL that was posted.
            var fullURL = this.Url.Action();
            string url = fullURL.ToString();

            //parse the URL
            var VallueArray = url.Split('/');

            //We'll look into the header to decide if we need to return somthing different for JSON
            string header = Request.Headers.Get("Accept");

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


            ///////////


            var urlBuilder = new System.UriBuilder(Request.Url.AbsoluteUri)
            {
                Path = Url.Action("ParseURL", "RESTEA"),
                Query = null,
            };

            Uri uri = urlBuilder.Uri;
            string WholeURL = urlBuilder.ToString();




            ///////////////

            //**1** - GET THE SP CATALOG
            if (CleanURL.Count() == 1)
            {
                //List<string> ListOfSPs = new List<string>();
                //ViewBag.ListOfSPs = GetListOfSPs();
                //ViewBag.CurrentURL = "SPC";


                ////If Json is requiered, that is sorted here
                //if (header.Contains("json"))
                //{
                //    JObject JObjectToReturn = EA_JsonBuilder.JsonCreateSPCList(WholeURL, GetListOfSPs());
                //    return Content(JObjectToReturn.ToString(), "application/json");
                //}

                //return View("EA_Projects");

            }
            //**2** - GET THE SP SERVICES

            //If we have a two, then we have just picked the SP.
            //and now we are showing the root packages
            else if (CleanURL.Count() == 2)
            {
               // List<string> StringListOfRoots = new List<string>();
                string ThingOfInterest = CleanURL[1];
                m_Repository = getEA_Repos(ThingOfInterest);

                ViewBag.CurrentURL = url;
                ViewBag.SelectedSP = CleanURL[1];
                ViewBag.ThingOfInterest = ThingOfInterest;



                //GET THE PACKAGES INSIDE THE ROOT NODE
                List<EA.Package> ListOfRootPackages = EA_Helper.GetListOfRootNodes(ThingOfInterest, m_Repository);
                List<string> ListOfPackageAllData = new List<string>();
                List<string> ListOfPackageNameOnly = new List<string>();

                foreach (EA.Package PackageLoop in ListOfRootPackages)
                {
                    ListOfPackageAllData.Add(PackageLoop.Name + "|" + PackageLoop.ObjectType + "|" + PackageLoop.PackageGUID);
                    ListOfPackageNameOnly.Add(PackageLoop.Name);
                }
                ViewBag.ListOfPackages = ListOfPackageAllData;
                ViewBag.ListOfPackagesNames = ListOfPackageNameOnly;

                if (header.Contains("json"))
                {
                    JObject JObjectToReturn = EA_JsonBuilder.JsonCreateServices(WholeURL + "/" + CleanURL[1], ListOfPackageAllData);
                    return Content(JObjectToReturn.ToString(), "application/json");
                }
                return View("EA_Resources");
            }


            //**3** - GET A THING INSIDE THE EA REPO

            else if (CleanURL.Count() == 3)
            {
                string ThingOfInterest = CleanURL[CleanURL.Count() - 1];
                m_Repository = getEA_Repos(CleanURL[1]);
                ViewBag.CurrentURL = "/" + CleanURL[0] + "/" + CleanURL[1];
                ViewBag.URL_Prefix = "http://localhost:56901/SPC/" + CleanURL[1];

                ViewBag.NameOfClickedElement = ThingOfInterest;
                ViewBag.ThingOfInterest = ThingOfInterest;

                string[] UrlArray = ThingOfInterest.Split('|');

                string TOI_Name = UrlArray[0];
                string TOI_Type = UrlArray[1];
                string TOI_GUID = UrlArray[2];

                ViewBag.TOI_Name = UrlArray[0];
                ViewBag.TOI_Type = UrlArray[1];
                ViewBag.TOI_GUID = UrlArray[2];


                //If we have a package, show the details of the package and everything inside.

                //**3A** - GET A PACKAGE IN THE REPO
                if (TOI_Type == "otPackage")
                {
                    //Package type is returned as otPackage but in EA it's package
                    //not sure what is going on,
                    EA.Package PackageToShow = (EA.Package)m_Repository.GetPackageByGuid(TOI_GUID);

                    //Show any views/packages insides the thingofinterest
                    List<string> ListOfPackages = new List<string>();
                    List<string> ListOfDiagrams = new List<string>();
                    List<string> ListOfElements = new List<string>();

                    List<string> ListOfPackagesNames = new List<string>();
                    List<string> ListOfDiagramsNames = new List<string>();
                    List<string> ListOfElementsNames = new List<string>();

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

                    ViewBag.ListOfPackages = ListOfPackages;
                    ViewBag.ListOfDiagrams = ListOfDiagrams;
                    ViewBag.ListOfElements = ListOfElements;

                    ViewBag.ListOfPackagesNames = ListOfPackagesNames;
                    ViewBag.ListOfDiagramsNames = ListOfDiagramsNames;
                    ViewBag.ListOfElementsNames = ListOfElementsNames;

                    //determine if the user wants HTML OR JSON
                    //string header = Request.Headers.Get("Accept");
                    if (header.Contains("html"))
                    { return View("EA_Resources"); }

                    if (header.Contains("json"))
                    {

                        JObject JObjectToReturn = EA_JsonBuilder.JsonCreatePackage(WholeURL + "/" + CleanURL[1], ListOfPackages, ListOfDiagrams, ListOfElements);
                        return Content(JObjectToReturn.ToString(), "application/json");

                    }

                }

                //**3B** - GET A DIAGRAM FROM THE REPO
                if (TOI_Type == "otDiagram")
                {
                    //Get the elements for for viewbag
                    List<string> ListOfElementURLs = new List<string>();      //Use this for the webserivces URL (name|type|GUID)
                    List<string> ListOfElementsNames = new List<string>();

                    //Get the links for the viewbag
                    List<string> ListOfLinkURLs = new List<string>();        //Use this for the webserivces URL (name|type|GUID)
                    List<string> ListOfLinksName = new List<string>();


                    EA.Diagram DiagramToShow = (EA.Diagram)m_Repository.GetDiagramByGuid(TOI_GUID);

                    string DiagramURL = WholeURL + "/" + CleanURL[1] + "/" + CleanURL[2];
                    string ProjectURL = WholeURL + "/" + CleanURL[1];

                    EA_Diagram EA_Json_Diagram = new Models.EA_Diagram();
                    EA_Json_Diagram.DiagramName = DiagramToShow.Name;
                    EA_Json_Diagram.DiagramType = DiagramToShow.Type;
                    EA_Json_Diagram.DiagramGUID = DiagramToShow.DiagramGUID;

                    ViewBag.CurrentURL += "/" + DiagramToShow.Name + "|" + TOI_Type + "|" + DiagramToShow.DiagramGUID;
                    ViewBag.DiagramName = DiagramToShow.Name;


                    //A diagram is made of links and elements.
                    //This next bit gets the links....
                    for (short iDO = 0; iDO < DiagramToShow.DiagramLinks.Count; iDO++)
                    {

                        EA.DiagramLink MyLink = (EA.DiagramLink)DiagramToShow.DiagramLinks.GetAt(iDO);
                        int ID = m_Repository.GetConnectorByID(MyLink.ConnectorID).ConnectorID;

                        EA.Connector con;

                        try //Try and get the connector object from the repository
                        {
                            con = (EA.Connector)m_Repository.GetConnectorByID(ID);
                            ListOfLinkURLs.Add(con.Name + "|" + con.ObjectType + "|" + con.ConnectorGUID);
                            ListOfLinksName.Add(con.Name);
                        }
                        catch { }


                    }

                    //..this next bit gets the diagram objects.
                    for (short iDO = 0; iDO < DiagramToShow.DiagramObjects.Count; iDO++)
                    {
                        EA.DiagramObject MyDO = (EA.DiagramObject)DiagramToShow.DiagramObjects.GetAt(iDO);
                        int ID = m_Repository.GetElementByID(MyDO.ElementID).ElementID;
                        EA.Element MyEle = (EA.Element)m_Repository.GetElementByID(ID);
                        ListOfElementURLs.Add(MyEle.Name + "|" + MyEle.ObjectType + "|" + MyEle.ElementGUID);
                        ListOfElementsNames.Add(MyEle.Name);
                        EA_Json_Diagram.ElementDictionary.Add(MyEle.Name, MyEle.Type);

                    }
                    ViewBag.ListOfElements = ListOfElementURLs;
                    ViewBag.ListOfElementsNames = ListOfElementsNames;

                    ViewBag.ListOfLinkURLs = ListOfLinkURLs;
                    ViewBag.ListOfLinkNames = ListOfLinksName;

                    if (header == null)
                    { return new HttpStatusCodeResult(406, "header string is null"); }

                    if (header.Contains("html"))
                    { return View("EA_ShowDiagram"); }

                    if (header.Contains("json"))
                    {
                        Dictionary<string, string> DiagramDictionary = new Dictionary<string, string>();
                        DiagramDictionary.Add("Diagram Name", DiagramToShow.Name);
                        DiagramDictionary.Add("Created Data", DiagramToShow.CreatedDate.ToString());
                        DiagramDictionary.Add("Meta Type", DiagramToShow.MetaType);
                        DiagramDictionary.Add("Notes", DiagramToShow.Notes);
                        DiagramDictionary.Add("Package ID", DiagramToShow.PackageID.ToString());
                        DiagramDictionary.Add("Big Preview", DiagramURL + "/BigPreview");
                        DiagramDictionary.Add("Small Preview", DiagramURL + "/SmallPreview");

                        JObject JObjectToReturn = EA_JsonBuilder.JsonCreateDiagram(ProjectURL, ListOfElementURLs, DiagramDictionary, ListOfLinkURLs);
                        return Content(JObjectToReturn.ToString(), "application/json");

                    }

                }

                //**3C** - GET AN ELEMENT FROM THE REPO
                if (TOI_Type == "otElement")
                {

                    EA.Element MyEle = (EA.Element)m_Repository.GetElementByGuid(TOI_GUID);
                    ViewBag.Author = MyEle.Author;
                    ViewBag.ElementName = MyEle.Name;
                    ViewBag.notes = MyEle.Notes;

                    List<string> ListOfDiagramsNames = new List<string>();
                    List<string> ListOfDiagrams = new List<string>();
                    List<string> ListOfConnectors = new List<string>();
                    List<string> ListOfConnectorNames = new List<string>();

                    RestfulEA.Models.EA_TaggedValueStore myTVS = new Models.EA_TaggedValueStore();
                    myTVS.ParentElementName = MyEle.Name;
                    myTVS.ParentElementGUID = MyEle.ElementGUID;


                    foreach (EA.Connector MyConnector in MyEle.Connectors)
                    {

                        string GUID = MyConnector.ConnectorGUID;
                        string Name = MyConnector.Name;
                        string Type = MyConnector.ObjectType.ToString();


                        ListOfConnectorNames.Add(Name);
                        ListOfConnectors.Add(Name + "|otConnector|" + GUID);



                    }




                    //Add the tagged values in
                    for (short i = 0; i < MyEle.TaggedValues.Count; i++)
                    {
                        EA.TaggedValue TagValue = (EA.TaggedValue)MyEle.TaggedValues.GetAt(i);

                        //Only add the key if it does not exist
                        if (myTVS.TaggedDictionary.ContainsKey(TagValue.Name) == false)
                        {
                            myTVS.TaggedDictionary.Add(TagValue.Name, TagValue.Value);
                        }

                    }
                    ViewBag.TG_Store = myTVS;
                    foreach (EA.Diagram DiagramLoop in MyEle.Diagrams)
                    {
                        ListOfDiagrams.Add(DiagramLoop.Name + "|" + DiagramLoop.ObjectType + "|" + DiagramLoop.DiagramGUID);
                        ListOfDiagramsNames.Add(DiagramLoop.Name);
                    }
                    ViewBag.TaggedSuffix = MyEle.Name + "|" + "otTaggedValue" + "|" + MyEle.ElementGUID;
                    ViewBag.ListOfDiagramsNames = ListOfDiagramsNames;
                    ViewBag.ListOfDiagrams = ListOfDiagrams;


                    ViewBag.ListOfConnectors = ListOfConnectors;
                    ViewBag.ListOfConnectorNames = ListOfConnectorNames;

                    //determine if the user wants HTML OR JSON
                    //string header = Request.Headers.Get("Accept");
                    if (header.Contains("html"))
                    { return View("EA_ShowElement"); }
                    //Store the element paramters that we want to see in the json
                    Dictionary<string, string> ElementDictionary = new Dictionary<string, string>();
                    ElementDictionary.Add("Name", MyEle.Name);
                    ElementDictionary.Add("Author", MyEle.Author);
                    ElementDictionary.Add("DateCreated", MyEle.Created.ToString());
                    ElementDictionary.Add("Version", MyEle.Version);
                    ElementDictionary.Add("Status", MyEle.Status);
                    ElementDictionary.Add("Notes", MyEle.Notes);
                    ElementDictionary.Add("GUID", MyEle.ElementGUID);

                    //Connectors
                    foreach (EA.Connector MyCon in MyEle.Connectors)
                    {
                        //  ListOfConnectors.Add(MyCon.Name + "|otConnector|" + MyCon.ConnectorGUID);
                        //   ListOfConnectorNames.Add(MyCon.Name);

                        string ConnectorURL = MyCon.Name + "|otConnector|" + MyCon.ConnectorGUID;

                        ElementDictionary.Add("Connector:" + MyCon.Name, uri + "\\" + ConnectorURL);

                    }




                    if (header.Contains("json"))
                    {

                        JObject JObjectToReturn = EA_JsonBuilder.JsonCreateElement(WholeURL + "/" + CleanURL[1], ElementDictionary, myTVS, ListOfDiagrams);
                        return Content(JObjectToReturn.ToString(), "application/json");

                    }
                }


                //**3D** - GET A TAGGED VALUE FROM THE REPO
                if (TOI_Type == "otTaggedValue")
                {
                    //There is no native EA tagged value object
                    //One was created for the purpose of this project.
                    //The GUID that comes with tagged value is actually the GUID of the parent element 

                    EA.Element ParentElement = (EA.Element)m_Repository.GetElementByGuid(TOI_GUID);
                    RestfulEA.Models.EA_TaggedValueStore myTVS = new Models.EA_TaggedValueStore();
                    myTVS.ParentElementName = ParentElement.Name;
                    myTVS.ParentElementGUID = ParentElement.ElementGUID;

                    for (short i = 0; i < ParentElement.TaggedValues.Count; i++)
                    {
                        EA.TaggedValue TagValue = (EA.TaggedValue)ParentElement.TaggedValues.GetAt(i);

                        if (myTVS.TaggedDictionary.ContainsKey(TagValue.Name) == false)
                        {
                            myTVS.TaggedDictionary.Add(TagValue.Name, TagValue.Value);
                        }
                    }

                    ViewBag.TG_Store = myTVS;

                    // string header = Request.Headers.Get("Accept");
                    if (header.Contains("html"))
                    { return View("EA_ShowTaggedValues"); }
                    if (header.Contains("json"))
                    {
                        JObject JObjectToReturn = EA_JsonBuilder.JsonCreateTaggedValues(myTVS);
                        return Content(JObjectToReturn.ToString(), "application/json");
                    }

                }

                //**3E** - GET A CONNECTOR FROM THE REPO
                if (TOI_Type == "otConnector")
                {
                    //  EA.Diagram DiagramToShow = (EA.Diagram)m_Repository.GetDiagramByGuid(TOI_GUID);
                    Dictionary<string, string> ConnectorDictionary = new Dictionary<string, string>();
                    EA.Connector MyLink = (EA.Connector)m_Repository.GetConnectorByGuid(TOI_GUID);
                    ViewBag.TOI_Name = TOI_Name;
                    ViewBag.TOI_Type = TOI_Type;
                    ViewBag.TOI_GUID = TOI_GUID;
                    ViewBag.Alias = MyLink.Alias;
                    ViewBag.Colour = MyLink.Color;
                    ViewBag.Direction = MyLink.Direction;
                    ViewBag.Notes = MyLink.Notes;
                    ViewBag.Type = MyLink.Type;
                    ViewBag.ConnectorID = MyLink.ConnectorID;

                    ConnectorDictionary.Add("Name", TOI_Name);
                    ConnectorDictionary.Add("Type", TOI_Type);
                    ConnectorDictionary.Add("GUID", TOI_GUID);
                    ConnectorDictionary.Add("Alias", MyLink.Alias);
                    ConnectorDictionary.Add("Colour", MyLink.Color.ToString());
                    ConnectorDictionary.Add("Direction", MyLink.Notes);
                    ConnectorDictionary.Add("Connector ID", MyLink.ConnectorID.ToString());

                    if (header.Contains("html"))
                    { return View("EA_ShowConnector"); }

                    if (header.Contains("json"))
                    {
                        JObject JObjectToReturn = EA_JsonBuilder.JsonCreateConnector(WholeURL + "/" + CleanURL[1], ConnectorDictionary);
                        return Content(JObjectToReturn.ToString(), "application/json");
                    }
                }

                return View("Error, no suitable MVC view was found");
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





    }
}


    
