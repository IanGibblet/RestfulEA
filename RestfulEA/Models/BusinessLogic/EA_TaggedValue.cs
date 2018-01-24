using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulEA.Models
{
    public class EA_TaggedValueStore
    {
        public string ParentElementName;
        public string ParentElementGUID;
        public Dictionary<string, String> TaggedDictionary = new Dictionary<string, string>();  
    }
}