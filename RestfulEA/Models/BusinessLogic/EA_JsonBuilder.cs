using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulEA.Models
{

    class EA_JsonBuilder
    {



      //Creates the JSON neccesary for providing the Service Provider Catalogue
      static public JObject JsonCreateSPCList(string WholeURL, List<string> ListOfSPs)
        {
            JObject TopObject = new JObject();
            TopObject.Add("Service Provider Catalogue", "Enterprise Architect");
            JArray Jarray = new JArray();

            foreach ( string SP_Name in ListOfSPs)
            {
                Jarray.Add(WholeURL + "/" + SP_Name);             
            }     


            TopObject["ChildArray"] = Jarray;  
            return TopObject;
        }


        //Creates the JSON for showing the services of a Service Provider
        static public JObject JsonCreateServices(string WholeURL, List<string> ListOfPackages)
        {

            JObject TopObject = new JObject();
            TopObject.Add("Available Services ", WholeURL);

            JObject PackageArrayHolder = new JObject();
            JArray PackageArray = new JArray();

            foreach (String S in ListOfPackages)
            {
                PackageArray.Add(WholeURL + "/" + S);
            }

            PackageArrayHolder["PackageArray"] = PackageArray;
           // TopObject.Add(PackageArrayHolder);
            TopObject["PackageArray"] = PackageArrayHolder;


            return TopObject;
        }

        static public JObject JsonCreatePackage(string WholeURL, List<string> listOfPackages, List<string> listOfDiagrams, List<string> listOfElements)
        {

            JObject TopObject = new JObject();

            JObject PackageArrayHolder = new JObject();
            JObject DiagramsArrayHolder = new JObject();
            JObject ElementsArrayHolder = new JObject();

            JArray PackageArray = new JArray();
            JArray DiagramArray = new JArray();
            JArray ElementArray = new JArray();


            foreach (String S in listOfPackages)
            {
                PackageArray.Add(WholeURL + "/" + S);
            }
            PackageArrayHolder["Package Array"] = PackageArray;

            foreach (String S in listOfDiagrams)
            {
                DiagramArray.Add(WholeURL + "/" + S);
            }
            DiagramsArrayHolder["Diagram Array"] = DiagramArray;


            foreach (String S in listOfElements)
            {
                ElementArray.Add(WholeURL + "/" + S);
            }
            ElementsArrayHolder["Element Array"] = ElementArray;

            TopObject["Packages"] = PackageArrayHolder;
            TopObject["Diagrams"] = DiagramsArrayHolder;
            TopObject["Elements"] = ElementsArrayHolder;


            TopObject.Add("Services Provider", WholeURL);

            return TopObject;



            throw new NotImplementedException();
        }


        static public JObject JsonCreateDiagram(string WholeURL, List<string> listOfElements, Dictionary<string,string> DiagramDictionary,List<string>ListOfLinks)
        {
            JObject TopObject = new JObject();
            JObject ElementsArrayHolder = new JObject();
            JArray ElementArray = new JArray();
            JArray LinkArrary = new JArray();


            foreach (String S in listOfElements)
            {
                ElementArray.Add(WholeURL + "/" + S);
            }
            ElementsArrayHolder["Element Array"] = ElementArray;

            foreach (String S in ListOfLinks)
            {
                LinkArrary.Add(WholeURL + "/" + S);
            }
            ElementsArrayHolder["Link Array"] = LinkArrary;
        

            foreach (KeyValuePair<string, string> entry in DiagramDictionary)
            {
                TopObject.Add(entry.Key, entry.Value);           
            }

                TopObject["Elements"] = ElementsArrayHolder;
                return TopObject;

        }


        //Creates the JObject which is used to create the JSON for an EA element
        public static JObject JsonCreateElement(string BaseURI, 
                                                Dictionary<string,string> ElementDictionary ,
                                                EA_TaggedValueStore TaggedValueStore, 
                                                List<string> listOfDiagrams)
        {
            JObject TopObject = new JObject();
            JObject DiagramsArrayHolder = new JObject();
            JArray DiagramArray = new JArray();
            foreach (KeyValuePair<string, string> entry in ElementDictionary)
            {
                TopObject.Add(entry.Key, entry.Value);           
            }
           //If we have more than one tagged value then put a link to show them.
            if(TaggedValueStore.TaggedDictionary.Count > 0)
            {
                TopObject.Add("Tagged Values", BaseURI + "/" + TaggedValueStore.ParentElementName + "|otTaggedValue|" + TaggedValueStore.ParentElementGUID);
            }    
            return TopObject;
        }
 

        public static JObject JsonCreateConnector(string WholeURL, Dictionary<string, string> ConnectorDictionary)
        {
            JObject TopObject = new JObject();

            TopObject.Add("Connector URL", WholeURL);

            foreach (KeyValuePair<string, string> entry in ConnectorDictionary)
            {
                TopObject.Add(entry.Key, entry.Value);
            }

            return TopObject;

        }



        internal static JObject JsonCreateTaggedValues(EA_TaggedValueStore myTVS)
        {
            JObject TopObject = new JObject();

            TopObject.Add("Name Of container element", myTVS.ParentElementName);
            TopObject.Add("Container element GUID", myTVS.ParentElementGUID);
       
            JObject TaggedvAluesArrayHolder = new JObject();
            JArray TaggedValueArray = new JArray();    

            foreach (KeyValuePair<string, string> entry in myTVS.TaggedDictionary)
            {
             //   TopObject.Add(entry.Key, entry.Value);
                TaggedValueArray.Add(entry.Key + "-" + entry.Value);
            }

            if (myTVS.TaggedDictionary.Count > 0)
            {
                TopObject["TaggedValues"] = TaggedValueArray;
            }



            return TopObject;


            
        }
    }


    
}