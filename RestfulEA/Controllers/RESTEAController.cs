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

namespace RestfulEA.Controllers
{
    public class RESTEAController : Controller
    {
        // GET: RESTEA
        public ActionResult ParseURL()
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

            //if we have 1 or 2 parts to the url then we only want to show the SPs. i.e. each file
            if (CleanURL.Count() == 1)
            {
                List<string> ListOfSPs = new List<string>();
                ViewBag.ListOfSPs = GetListOfSPs();
                ViewBag.CurrentURL = "RESTEA";
                return View("EA_Projects");
            }

            //If we have a two, then we have just picked the SP.
            //and now we are showing the root packages
            else if (CleanURL.Count() == 2)
            {
                List<string> StringListOfRoots = new List<string>();
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



                return View("EA_Resources");
            }

            //This means we are now looking for a "thing" inside the the EA repository
            else if (CleanURL.Count() == 3)
            {
                string ThingOfInterest = CleanURL[CleanURL.Count() - 1];
                m_Repository = getEA_Repos(CleanURL[1]);
                ViewBag.CurrentURL = "/" + CleanURL[0] + "/" + CleanURL[1];
                ViewBag.URL_Prefix = "http://localhost:56901/RESTEA/" + CleanURL[1];

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
                    string header = Request.Headers.Get("Accept");
                    if (header.Contains("html"))
                    { return View("EA_Resources"); }

                    if (header.Contains("json"))
                    { return Json(PackageToShow, JsonRequestBehavior.AllowGet); }

                }


                if (TOI_Type == "otDiagram")
                {
                    List<string> ListOfElements = new List<string>();
                    List<string> ListOfElementsNames = new List<string>();
                    EA.Diagram DiagramToShow = (EA.Diagram)m_Repository.GetDiagramByGuid(TOI_GUID);

                    EA_Diagram EA_Json_Diagram = new Models.EA_Diagram();
                    EA_Json_Diagram.DiagramName = DiagramToShow.Name;
                    EA_Json_Diagram.DiagramType = DiagramToShow.Type;
                    EA_Json_Diagram.DiagramGUID = DiagramToShow.DiagramGUID;
                       



                    ViewBag.CurrentURL += "/" + DiagramToShow.Name + "|" + TOI_Type + "|" + DiagramToShow.DiagramGUID;
                    ViewBag.DiagramName = DiagramToShow.Name;

                    for (short iDO = 0; iDO < DiagramToShow.DiagramObjects.Count; iDO++)
                    {
                        EA.DiagramObject MyDO = (EA.DiagramObject)DiagramToShow.DiagramObjects.GetAt(iDO);
                        int ID = m_Repository.GetElementByID(MyDO.ElementID).ElementID;
                        EA.Element MyEle = (EA.Element)m_Repository.GetElementByID(ID);
                        ListOfElements.Add(MyEle.Name + "|" + MyEle.ObjectType + "|" + MyEle.ElementGUID);
                        ListOfElementsNames.Add(MyEle.Name);

                        //Now add stuff for jsonobject
                        EA_Json_Diagram.ElementDictionary.Add(MyEle.Name, MyEle.Type);


                    }
                    ViewBag.ListOfElements = ListOfElements;
                    ViewBag.ListOfElementsNames = ListOfElementsNames;



                    //determine if the user wants HTML OR JSON
                    string header = Request.Headers.Get("Accept");

                    if(header == null)
                    {return new HttpStatusCodeResult(406, "header string is null");}


                    if (header.Contains("html"))
                    { return View("EA_ShowDiagram"); }

                    if (header.Contains("json"))
                    {
                        //This just returned {}
                        // return Json(DiagramToShow, JsonRequestBehavior.AllowGet);


                        DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(EA_Diagram));
                        MemoryStream msObj = new MemoryStream();

                        js.WriteObject(msObj, EA_Json_Diagram);
                        msObj.Position = 0;
                        StreamReader sr = new StreamReader(msObj);
                        string jsonToReturn = sr.ReadToEnd();

                        dynamic dynobject = JObject.Parse(jsonToReturn);

                        sr.Close();
                        msObj.Close();

                        return Json(jsonToReturn, JsonRequestBehavior.AllowGet);


                    }



                }


                if (TOI_Type == "otElement")
                {

                    EA.Element MyEle = (EA.Element)m_Repository.GetElementByGuid(TOI_GUID);

                    ViewBag.Author = MyEle.Author;

                    ViewBag.ElementName = MyEle.Name;
                    ViewBag.notes = MyEle.Notes;

                //    List<string> ListOfTagValueNames = new List<string>();
                    List<string> ListOfDiagramsNames = new List<string>();

                //    List<string> ListOfTagValueValues = new List<string>();
                    List<string> ListOfDiagrams = new List<string>();

                    RestfulEA.Models.EA_TaggedValueStore myTVS = new Models.EA_TaggedValueStore();
                    myTVS.ParentElementName = MyEle.Name;
                    myTVS.ParentElementGUID = MyEle.ElementGUID;

                    //Add the tagged values in
                   for (short i = 0; i < MyEle.TaggedValues.Count; i++)
                    {
                        EA.TaggedValue TagValue = (EA.TaggedValue)MyEle.TaggedValues.GetAt(i);
                        myTVS.TaggedDictionary.Add(TagValue.Name, TagValue.Value);
                   }

                    ViewBag.TG_Store = myTVS;





              //      ViewBag.ListOfTagValueNames = ListOfTagValueNames;
              //     ViewBag.ListOfTagValueValues = ListOfTagValueValues;

                    //Add the diagrams in. (because an element can contain a diagram.




                    foreach (EA.Diagram DiagramLoop in MyEle.Diagrams)
                    {
                        ListOfDiagrams.Add(DiagramLoop.Name + "|" + DiagramLoop.ObjectType + "|" + DiagramLoop.DiagramGUID);
                        ListOfDiagramsNames.Add(DiagramLoop.Name);
                    }

                    ViewBag.TaggedSuffix = MyEle.Name + "|" + "otTaggedValue" + "|" + MyEle.ElementGUID; 


                    ViewBag.ListOfDiagramsNames = ListOfDiagramsNames;
                    ViewBag.ListOfDiagrams = ListOfDiagrams;


                    //determine if the user wants HTML OR JSON
                    string header = Request.Headers.Get("Accept");
                    if (header.Contains("html"))
                    { return View("EA_ShowElement"); }
                    if (header.Contains("json"))
                    {

                        var json = new JavaScriptSerializer().Serialize(MyEle);
                        // return Json(MyEle, JsonRequestBehavior.AllowGet);
                        return Json(json, JsonRequestBehavior.AllowGet);
                    }
                }


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
                        myTVS.TaggedDictionary.Add(TagValue.Name, TagValue.Value);
                    }


                    ViewBag.TG_Store = myTVS;


                    string header = Request.Headers.Get("Accept");
                    if (header.Contains("html"))
                    { return View("EA_ShowTaggedValues"); }
                    if (header.Contains("json"))
                    {

                        var json = new JavaScriptSerializer().Serialize(myTVS);
                       //  return Json(MyEle, JsonRequestBehavior.AllowGet);
                          return Json(json, JsonRequestBehavior.AllowGet);



                    }



                }


                return View("");
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
                string EAFileName = System.IO.Path.GetFileNameWithoutExtension(reposLoop.ConnectionString);

                if (EAFileName == pickedSP)
                    return reposLoop;
            }
            return null;
        }



    }
}