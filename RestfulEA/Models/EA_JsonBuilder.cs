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
                DiagramsArrayHolder.Add(WholeURL + "/" + S);
            }
            DiagramsArrayHolder["Diagram Array"] = DiagramArray;


            foreach (String S in listOfElements)
            {
                ElementsArrayHolder.Add(WholeURL + "/" + S);
            }
            ElementsArrayHolder["Element Array"] = ElementArray;

            TopObject["Packages"] = PackageArrayHolder;
            TopObject["Diagrams"] = DiagramsArrayHolder;
            TopObject["Elements"] = ElementsArrayHolder;


            TopObject.Add("Services Provider", WholeURL);

            return TopObject;



            throw new NotImplementedException();
        }
    }


    
}