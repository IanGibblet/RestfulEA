
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

        //PACKAGE CONTENT
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


        //DIAGRAM CONTENT
        public void Generate_otDiagram_content(EA.Repository m_Repository, string TOI_GUID, string SP_BaseURL,
            out List<string> listOfElements,
            out List<string> listOfElementsNames,
            out List<string> listOfLinks,
            out List<string> listOfLinkNames,
            out Dictionary<string,string> DiagramDictionary)
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



            //JSON Content
            string DiagramURL = SP_BaseURL + "/" +  DiagramToShow.Name + "|otDiagram|" + TOI_GUID; 


            DiagramDictionary = new Dictionary<string, string>();
            DiagramDictionary.Add("Diagram Name", DiagramToShow.Name);
            DiagramDictionary.Add("Created Data", DiagramToShow.CreatedDate.ToString());
            DiagramDictionary.Add("Meta Type", DiagramToShow.MetaType);
            DiagramDictionary.Add("Notes", DiagramToShow.Notes);
            DiagramDictionary.Add("Package ID", DiagramToShow.PackageID.ToString());
            DiagramDictionary.Add("Big Preview", DiagramURL + "/BigPreview");
            DiagramDictionary.Add("Small Preview", DiagramURL + "/SmallPreview");





        }



        //ELEMENT CONTENT
        public void Generate_otElement_content(
            Repository m_Repository, 
            string TOI_GUID, 
            out List<string> listOfDiagrams, 
            out List<string> listOfDiagramsNames,
            out List<string> ListOfConnectors, 
            out List<string> ListOfConnectorNames, 
            out  Dictionary<string, string> elementAttributes,
            out EA_TaggedValueStore myTVS)
        {
            listOfDiagrams = new List<string>();
            listOfDiagramsNames = new List<string>();
            ListOfConnectors = new List<string>();
            ListOfConnectorNames = new List<string>();
            elementAttributes = new Dictionary<string, string>();
            myTVS = new EA_TaggedValueStore();


            EA.Element MyEle = (EA.Element)m_Repository.GetElementByGuid(TOI_GUID);

            elementAttributes.Add("Name", MyEle.Name);
            elementAttributes.Add("Notes", MyEle.Notes);
            elementAttributes.Add("Author", MyEle.Author);
            elementAttributes.Add("DataCreated", MyEle.Created.ToString());
            elementAttributes.Add("Version", MyEle.Version);
            elementAttributes.Add("Status", MyEle.Status);
            elementAttributes.Add("GUID", MyEle.ElementGUID);


            //CONNECTORS
            foreach (EA.Connector MyConnector in MyEle.Connectors)
            {
                string GUID = MyConnector.ConnectorGUID;
                string Name = MyConnector.Name;
                string Type = MyConnector.ObjectType.ToString();

                ListOfConnectorNames.Add(Name);
                ListOfConnectors.Add(Name + "|otConnector|" + GUID);

            }


            //TAGGED VALUES
            for (short i = 0; i < MyEle.TaggedValues.Count; i++)
            {
                EA.TaggedValue TagValue = (EA.TaggedValue)MyEle.TaggedValues.GetAt(i);

                //Only add the key if it does not exist
                if (myTVS.TaggedDictionary.ContainsKey(TagValue.Name) == false)
                {
                    myTVS.TaggedDictionary.Add(TagValue.Name, TagValue.Value);
                }

            }

            myTVS.ParentElementName = MyEle.Name;
            myTVS.ParentElementGUID = MyEle.ElementGUID;




            //DIAGRAMS
            foreach (EA.Diagram DiagramLoop in MyEle.Diagrams)
            {
                listOfDiagrams.Add(DiagramLoop.Name + "|" + DiagramLoop.ObjectType + "|" + DiagramLoop.DiagramGUID);
                listOfDiagramsNames.Add(DiagramLoop.Name);
            }



        }

        public void Generate_otConnector_content(
            Repository m_Repository, 
            string TOI_GUID, 
            string TOI_Name,
            string TOI_Type,
            out EA.Connector MyLink, 
            out Dictionary<string, string> ConnectorDictionary)
        {

            EA.Package MyModel = (EA.Package)m_Repository.Models.GetAt(0);

            MyLink =  m_Repository.GetConnectorByGuid(TOI_GUID);


            

            //  EA.DiagramLink MyDiagramLink = (EA.DiagramLink)DiagramToShow.DiagramLinks.GetAt(iDO);

            ConnectorDictionary = new Dictionary<string, string>();
            MyLink = (EA.Connector)m_Repository.GetConnectorByGuid(TOI_GUID);

            ConnectorDictionary.Add("Name", TOI_Name);
            ConnectorDictionary.Add("Type", TOI_Type);
            ConnectorDictionary.Add("GUID", TOI_GUID);
            ConnectorDictionary.Add("Alias", MyLink.Alias);
            ConnectorDictionary.Add("Colour", MyLink.Color.ToString());
            ConnectorDictionary.Add("Direction", MyLink.Notes);
            ConnectorDictionary.Add("Connector ID", MyLink.ConnectorID.ToString());
            ConnectorDictionary.Add("Sterotype", MyLink.Stereotype);



            int ID = m_Repository.GetConnectorByID(MyLink.ConnectorID).ConnectorID;

            EA.Connector con;

            try //Try and get the connector object from the repository
            {
                con = (EA.Connector)m_Repository.GetConnectorByID(ID);
              //  ListOfLinkURLs.Add(con.Name + "|" + con.ObjectType + "|" + con.ConnectorGUID);
               // ListOfLinksName.Add(con.Name);
            }
            catch { }



        }

        public void Generate_otTaggedValues_content(Repository m_Repository, string TOI_GUID,string TOI_Name, out EA_TaggedValueStore myTVS)
        {
         


            EA.Element ParentElement = (EA.Element)m_Repository.GetElementByGuid(TOI_GUID);
            myTVS = new Models.EA_TaggedValueStore();
            myTVS.ParentElementName = ParentElement.Name;
            myTVS.ParentElementGUID = ParentElement.ElementGUID;


            for (short i = 0; i < ParentElement.TaggedValues.Count; i++)
            {
                EA.TaggedValue TagValue = (EA.TaggedValue)ParentElement.TaggedValues.GetAt(i);



                if (TagValue.Name == TOI_Name)
                {
                    if (myTVS.TaggedDictionary.ContainsKey(TagValue.Name) == false)
                    {
                        myTVS.TaggedDictionary.Add(TagValue.Name, TagValue.Value);
                    }
                }
            

            }









        }
    }



}