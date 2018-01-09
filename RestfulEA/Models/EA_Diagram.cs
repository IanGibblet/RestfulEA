using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulEA.Models
{
    public class EA_Diagram
    {
        public string DiagramName;    
        public string DiagramType;
        public string DiagramGUID;


        //Use this to store name and type of element
        public Dictionary<string, String> ElementDictionary = new Dictionary<string, string>();

        public Dictionary<string, string> LinkDictionary = new Dictionary<string, string>();


    }
}