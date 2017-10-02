using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EA;
using System.Xml;

namespace RestfulEA.Models
{
    public class EA_Helper
    {


        public static List<EA.Package> GetListOfRootNodes(string ThingOfInterest, EA.Repository m_Repository)
        {
            List<EA.Package> ListOfPackages = new List<Package>();


            for (short rootCounter = 0; rootCounter < m_Repository.Models.Count; rootCounter++)
            {
                //Let's open the top level root packages.
                EA.Package RootPackageLoop = (EA.Package)m_Repository.Models.GetAt(rootCounter);
                ListOfPackages.Add(RootPackageLoop);
            }

                return ListOfPackages; 

        }

        public static List<string>GetStringListOfRoots(string ThingOfInterest, EA.Repository m_Repository)
        {
            List<string> StringListOfRoots = new List<string>();


            string SP_Name = System.IO.Path.GetFileNameWithoutExtension(m_Repository.ConnectionString);

            //This signifies that the user has just selected a service provider.               
            if (ThingOfInterest == SP_Name)
            {
                for (short i = 0; i < m_Repository.Models.Count; i++)
                {
                    EA.Package TopEA_Pkg = (EA.Package)m_Repository.Models.GetAt(i);
                    StringListOfRoots.Add(TopEA_Pkg.Name);
                }
            }
            return StringListOfRoots;
        }


        public static List<string> GetStringListOfROOTPackages(string ThingOfInterest, EA.Repository m_Repository, string SP)
        {
            List<string> ListToReturn = new List<string>();
            //Go aorund all the top level models
            for (short rootCounter = 0; rootCounter < m_Repository.Models.Count; rootCounter++)
            {
                //Let's open the top level root packages.
                EA.Package RootPackageLoop = (EA.Package)m_Repository.Models.GetAt(rootCounter);
                ListToReturn.Add(RootPackageLoop.Name);
            }
            return ListToReturn;
        }

        //Get a list of views from the selected root package.
        public static List<string> GetStringListOfROOTViews(string RootPackageOfInterest, EA.Repository m_Repository)
        {
            List<string> ListOfViewsToReturn = new List<string>();

            //go through every root note
            for (short iRoot = 0; iRoot < m_Repository.Models.Count; iRoot++)
            {
                EA.Package MyRootNode = (EA.Package)m_Repository.Models.GetAt(iRoot);

                //If the root node 
                if (RootPackageOfInterest == MyRootNode.Name)
                {
                    //For every view in that root.
                    for (short iPackage = 0; iPackage < MyRootNode.Packages.Count; iPackage++)
                    {
                        //If this is the right correct root node, lets store the views.                                 
                        EA.Package MyView = (EA.Package)MyRootNode.Packages.GetAt(iPackage);
                        ListOfViewsToReturn.Add(MyView.Name);
                    }
                    //At this point we can assume we have all the views so let's exit
                    return ListOfViewsToReturn;
                }
            }

            return ListOfViewsToReturn;
        }



        private static List<EA.Package> GetObjectListOfROOTViews(string RootPackageOfInterest, EA.Repository m_Repository)
        {
            List<EA.Package> ListOfViewsToReturn = new List<EA.Package>();

            //go through every root note
            for (short iRoot = 0; iRoot < m_Repository.Models.Count; iRoot++)
            {
                EA.Package MyRootNode = (EA.Package)m_Repository.Models.GetAt(iRoot);
                //If the root node 
                if (RootPackageOfInterest == MyRootNode.Name)
                {
                    //For every view in that root.
                    for (short iPackage = 0; iPackage < MyRootNode.Packages.Count; iPackage++)
                    {
                        //If this is the right correct root node, lets store the views.                                 
                        EA.Package MyView = (EA.Package)MyRootNode.Packages.GetAt(iPackage);
                        ListOfViewsToReturn.Add(MyView);
                    }
                    return ListOfViewsToReturn;
                }              
            }

            //Will be null if it gets to here
            return ListOfViewsToReturn;
        }


        //Takes the name of what ever was clicked and looks to find it's type.
        public static string GetType(string thingOfInterest, Repository m_Repository)
        {

            //LOOKING FOR ELEMENTS OR PACKAGES (THEY ARE IN THE SAME TABLE)
            string QueryResult = m_Repository.SQLQuery("SELECT * FROM t_object WHERE Name = '" + thingOfInterest + "'")  ;
            QueryResult  = QueryResult.Replace("\r", "").Replace("\n", "").Replace("\t", "");

            XmlReader reader = XmlReader.Create(new System.IO.StringReader(QueryResult));
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(reader); // Load the XML document from the specified file
           
            XmlNodeList TypeList = xmlDoc.GetElementsByTagName("Object_Type");          // Get elements

            if (TypeList.Count > 0)  //If we have something then we have finished looking
            return TypeList[0].InnerText;   //If this is not zero then we could have a diagram that we are looking at.

            //LOOKING FOR DIAGRAMS

            QueryResult = m_Repository.SQLQuery("SELECT * FROM t_diagram WHERE Name = '" + thingOfInterest + "'");
            QueryResult = QueryResult.Replace("\r", "").Replace("\n", "").Replace("\t", "");

            reader = XmlReader.Create(new System.IO.StringReader(QueryResult));
            xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(reader); // Load the XML document from the specified file


            TypeList = xmlDoc.GetElementsByTagName("Name");          // Get elements

            if (TypeList.Count > 0)  //If we have something then we have finished looking
            return "Diagram";   //If this is not zero then we could have a diagram that we are looking at.


            return null;

        }


        public static string GetThingType(string tOI_Name, string tOI_Type, Repository m_Repository)
        {

            //LOOKING FOR ELEMENTS OR PACKAGES (THEY ARE IN THE SAME TABLE)
            string QueryResult = m_Repository.SQLQuery("SELECT * FROM t_object WHERE Name = '" + tOI_Name + "' AND Object_Type = '" + tOI_Type + "'");
            QueryResult = QueryResult.Replace("\r", "").Replace("\n", "").Replace("\t", "");

            XmlReader reader = XmlReader.Create(new System.IO.StringReader(QueryResult));
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(reader); // Load the XML document from the specified file

            XmlNodeList TypeList = xmlDoc.GetElementsByTagName("tOI_Type");          // Get elements


            throw new NotImplementedException();
        }



        //This is a sophisticated method that goes around the entire EA model looking for what ever was clicked.
        //The rest-ful nature means we don't know what type of thing was clicked, we only have the name.
        //This will use a recursive method to loop through every single layer.
        //Prepare for headaches!
        public static void GetViewsAndDiagrams(string ThingOfInterest, EA.Repository m_Repository, string[] cleanURL, out List<string>ListOfPackages, out List<string>ListOfDiagrams)
        {
            List<string> OutStringListOfViews = new List<string>();           //The final list of views to be returned.    (As out paramter)
            List<string> OutStringListOfDiagrams = new List<string>();        //The final list of diagrams to be returned. (As out paramter)

            List<EA.Package> ListOfRootViews = new List<EA.Package>();     //Object list to store the results
            
            ListOfRootViews = GetObjectListOfROOTViews(cleanURL[2], m_Repository);  //We can always figure out the root views

            //Look around the listroot views and see if one of these
            //is what has been selected
            foreach (EA.Package EA_Package in ListOfRootViews)
            {
                //Is this the view/package that was selected?
                if (EA_Package.Name == ThingOfInterest)
                {
                  
                        //If it does, then record the sub views as a string list
                        foreach (EA.Package PackLoop in EA_Package.Packages)
                        { OutStringListOfViews.Add(PackLoop.Name); }
                  

                       //Do we have any diagrams, if so, let's record them.                  
                        foreach (EA.Diagram DiaLoop in EA_Package.Diagrams)
                        { OutStringListOfDiagrams.Add(DiaLoop.Name); }

                    

                 }



                else
                {
                    //If it isn't, we have to dive into a recursive routine and if it exists below.....        
                    //Get Packages from the deep recursiveloop
                    //  Package DeepPackage = new Package();
                    EA.Package DeepPackage = null;
                    ThePackageRecursiveLoop(EA_Package, ThingOfInterest, out DeepPackage, false);              

                    if (DeepPackage != null)
                    {
                        foreach (EA.Package PkgLoop in DeepPackage.Packages)
                        {OutStringListOfViews.Add(PkgLoop.Name);}
                   
                        foreach (EA.Diagram DiaLoop in DeepPackage.Diagrams)
                        { OutStringListOfDiagrams.Add(DiaLoop.Name); }

                        

                    }
                }
            }


            ListOfPackages = OutStringListOfViews;
            ListOfDiagrams = OutStringListOfDiagrams;


        }



        private static void ThePackageRecursiveLoop(EA.Package PackageUnderInvestigation, string ThingOfInterest, out EA.Package PkgResult,bool ResultIsReady)
        {

            if(PackageUnderInvestigation.Name==ThingOfInterest)
            {
                PkgResult = PackageUnderInvestigation;
                return;
            }

             string PkgInvestName = PackageUnderInvestigation.Name;


            foreach (EA.Package PackageLoop in PackageUnderInvestigation.Packages)
            {
                string PackageLoopName = PackageLoop.Name;

                if (PackageLoop.Name == ThingOfInterest)
                {
                    PkgResult = PackageLoop;
                    ResultIsReady = true;
                    return;
                }

                foreach (EA.Package PackageSubLoop in PackageLoop.Packages)
                {

                    string PackageSubLoopName = PackageSubLoop.Name;

                    EA.Package ResultPackage = null;
                    ThePackageRecursiveLoop(PackageSubLoop, ThingOfInterest, out ResultPackage, ResultIsReady);

                    PkgResult = ResultPackage;

                    if (PkgResult != null)
                    {
                        if (PkgResult.Name == ThingOfInterest)
                        {return;}
                    }


                }            
              
            }



  

             PkgResult = null;

   

        }


        //This is will return a diagram object search for the name.
        public static Diagram GetDiagramObject(string SelectedDiagram, Repository m_Repository)
        {    
            string x = m_Repository.SQLQuery("SELECT * FROM t_diagram where name = \"" + SelectedDiagram + "\"");
                       
            //Strip out new line characters in the sql
            string y = x.Replace("\r", "").Replace("\n", "").Replace("\t","") ;      

            XmlReader reader = XmlReader.Create(new System.IO.StringReader(y)); 
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(reader); // Load the XML document from the specified file

            // Get elements
            XmlNodeList Name = xmlDoc.GetElementsByTagName("Name");
            XmlNodeList ID = xmlDoc.GetElementsByTagName("Diagram_ID");

            //If we have nothing then return nothing here
            if(Name.Count==0)
            {
                return null;
            }
            // Display the result
            string result = Name[0].InnerText;
            string IDs = ID[0].InnerText;
            EA.Diagram MyDiagram = m_Repository.GetDiagramByID(Convert.ToInt32(IDs));

            //DumpPackage("", (EA.Package)m_Repository.Models.GetAt(idx));

            return MyDiagram;
        }

        //returns 
        public static List<string> GetStringListOfElements(Diagram selectedDiagram, Repository m_Repository)
        {

            List<string> ListOfElements = new List<string>();

            for (short iDO = 0; iDO < selectedDiagram.DiagramObjects.Count; iDO++)
            {
                EA.DiagramObject MyDO = (EA.DiagramObject)selectedDiagram.DiagramObjects.GetAt(iDO);
                //ListAdd("       " + m_Repository.GetElementByID(MyDO.ElementID).Name + "   ( ID=" + MyDO.ElementID + " )");
                ListOfElements.Add(m_Repository.GetElementByID(MyDO.ElementID).Name);

            }


            return ListOfElements; 
        }


        //returns an element
        public static EA.Element GetElementObject(string SelectedElement,string SelectedDiagram, Repository m_Repository)
        {

            string x = m_Repository.SQLQuery("SELECT * FROM t_object where name = \"" + SelectedElement + "\""); //Do a SQL Query to get the ID number of the element in question             
            string y = x.Replace("\r", "").Replace("\n", "").Replace("\t", "");     //Strip out new line characters in the sql

            XmlReader reader = XmlReader.Create(new System.IO.StringReader(y));
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(reader); // Load the XML document from the specified file

            // Get elements
            XmlNodeList Name = xmlDoc.GetElementsByTagName("Name");
            XmlNodeList ID = xmlDoc.GetElementsByTagName("Object_ID");

            //If we have nothing then return nothing here
            if (Name.Count == 0)
            {return null;}
  
            string result = Name[0].InnerText;
            string IDs = ID[0].InnerText;

            EA.Diagram SelectedDiagramObject = GetDiagramObject(SelectedDiagram, m_Repository);
            if (SelectedDiagramObject == null)
                return null;


            EA.Element ElementToReturn = m_Repository.GetElementByID(Convert.ToInt16(IDs));
        


            return ElementToReturn;
        }


    }
}