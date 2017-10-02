using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestfulEA.Models;

namespace RestfulEA.Controllers
{
    public class SPController : Controller
    {
        // GET: SP
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ParseIncoming()
        {



            //Get the URL that was posted.
            var fullURL = this.Url.Action();
            string url = fullURL.ToString();

            //parse the URL
            var VallueArray = url.Split('/');
  
            //Clean emptycells
            string[] CleanURL = VallueArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();


            //Workout the URL to go back           
            string backURL = "";
            for (int i = 0; i < CleanURL.Count() - 1; i++)
            {backURL = backURL + "/" + CleanURL[i];}
            ViewBag.BackUrl = backURL;

            //store the current working repository
            EA.Repository m_Repository = new EA.Repository();

            //I don't know what this is about but it can cause trouble if not caught
            //new comment
            if (CleanURL.Count() > 2)
            {
                if (CleanURL[1] == "__browserLink")
                {
                    return View();
                }
            }

            //if we have 1 or 2 parts to the url then we only want to show the SPs. i.e. each file
            if (CleanURL.Count() == 1)
            {
                List<string> ListOfSPs = new List<string>();
                ViewBag.ListOfSPs = GetListOfSPs();
                ViewBag.CurrentURL = "SP";
                return View();
            }

            //If we have a two, then we have just picked the SP.
            //and now we are showing the root packages
            else if(CleanURL.Count()==2)
            {
                List<string> StringListOfRoots = new List<string>();
                string ThingOfInterest = CleanURL[1];                
                m_Repository = getEA_Repos(ThingOfInterest);

                ViewBag.WhatHasBeenSelected = "Selected project is " + ThingOfInterest;
                ViewBag.WhatIsBelow = "Below are the root packages inside " + ThingOfInterest;

                ViewBag.CurrentURL = url;
                ViewBag.NameOfClickedElement = ThingOfInterest;
                ViewBag.ListOfPackages =   EA_Helper.GetStringListOfRoots(ThingOfInterest, m_Repository);


                return View("EA_Display");

            }

            //at this point, we have picked a root node and would like to look at the
            //the views/packages that come under that root node.
            //Don't fortget, you can have more than one root node!
            else if(CleanURL.Count() ==3)
            {
      

                List<string> StringListOfPackages = new List<string>();
                string ThingOfInterest = CleanURL[CleanURL.Count()-1];           
                m_Repository = getEA_Repos(CleanURL[1]);

                ViewBag.WhatHasBeenSelected = "Selected root node is " + ThingOfInterest;
                ViewBag.WhatIsBelow = "Below are the views inside " + ThingOfInterest;

                ViewBag.CurrentURL = url;
                ViewBag.NameOfClickedElement = ThingOfInterest;

                ViewBag.ListOfPackages = EA_Helper.GetStringListOfROOTViews(ThingOfInterest, m_Repository);

                return View("EA_Display");

            }

            //If the count is larger than 3, then the user has picked an SP, a root node and view. 
            //At this point it could be a views, diagram or element.
            else if (CleanURL.Count() >3)
            {
                string ThingOfInterest = CleanURL[CleanURL.Count() - 1];
                m_Repository = getEA_Repos(CleanURL[1]);

                ViewBag.WhatHasBeenSelected = "Selected item  is " + CleanURL[2];
                ViewBag.WhatIsBelow = "Below are the things inside " + ThingOfInterest;

                ViewBag.CurrentURL = url;
                ViewBag.NameOfClickedElement = ThingOfInterest;
                ViewBag.ThingOfInterest = ThingOfInterest;


                string ThingOfInterestType = EA_Helper.GetType(ThingOfInterest, m_Repository);


                //Depending on what type of thing we have, we will turn a different 

                if (ThingOfInterestType == "Package")
                {
                    //Show any views/packages insides the thingofinterest
                    List<string> ListOfPackages = new List<string>();
                    List<string> ListOfDiagrams = new List<string>();
                    List<string> ListOfElements = new List<string>();

                    //     EA.Package PackageToShow = EA_Helper.GetPackageObject(ThingOfInterest, m_Repository);               
                    EA.Package PackageToShow = null;

                    //Stick the diagrams and packages into string lists
                    foreach (EA.Diagram DiagramLoop in PackageToShow.Diagrams)
                    {ListOfDiagrams.Add(DiagramLoop.Name);}

                    foreach (EA.Package PackageLoop in PackageToShow.Packages)
                    {ListOfPackages.Add(PackageLoop.Name);}

                    foreach (EA.Element ElementLoop in PackageToShow.Elements)
                    {ListOfElements.Add(ElementLoop.Name);}

                    


                ViewBag.ListOfPackages = ListOfPackages;
                ViewBag.ListOfDiagrams = ListOfDiagrams;
                ViewBag.ListOfElements = ListOfElements;
                     


                return View("EA_Display_Deep");
                     
                }
                else if(ThingOfInterestType == "Diagram")
                {
                                 
                    EA.Diagram SelectedDiagram = EA_Helper.GetDiagramObject(ThingOfInterest, m_Repository);
                    ViewBag.CreatedDate = SelectedDiagram.CreatedDate;
                    ViewBag.Name = SelectedDiagram.Name;
                    ViewBag.PageHeight = SelectedDiagram.PageHeight;
                    ViewBag.PageWidth = SelectedDiagram.PageWidth;
                    ViewBag.Type = SelectedDiagram.Type;
                    ViewBag.Version = SelectedDiagram.Version;
                    ViewBag.Notes = SelectedDiagram.Notes;
                    ViewBag.ListOfElements = EA_Helper.GetStringListOfElements(SelectedDiagram, m_Repository);

                    return View("EA_Display_Diagram_Contents");

                }
                else //There are two many types of elements to list so anything that isn't a diagram or package is considered an element
                {
                    string selectedDiagram = CleanURL[CleanURL.Count() - 2];
                    EA.Element SelectedElement = EA_Helper.GetElementObject(ThingOfInterest, selectedDiagram, m_Repository);

                    ViewBag.ElementName = SelectedElement.Name;
                    ViewBag.MetaType = SelectedElement.MetaType;
                    ViewBag.SteroType = SelectedElement.Stereotype;
                    ViewBag.Notes = SelectedElement.Notes;

                    List<string> ListOfTaggedValues = new List<string>();

                    for (short i = 0; i < SelectedElement.TaggedValues.Count; i++)
                    {
                        EA.TaggedValue ob3 = (EA.TaggedValue)SelectedElement.TaggedValues.GetAt(i);
                        //Strange default tagged value called isEncapsulated. Leave it out
                        if (ob3.Name == "isEncapsulated")
                            continue;

                        ListOfTaggedValues.Add(ob3.Name + ":" + ob3.Value);
                    }

                    ViewBag.ListOfTaggedValue = ListOfTaggedValues;
                    return View("EA_Display_Element");

                }













            }

            return View();
        }

        //Gets the list of SPs
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
        //application state variabel
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