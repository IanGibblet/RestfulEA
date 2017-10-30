using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulEA.Models
{

    class EA_JsonBuilder
    {


      static public JObject GetJObject()
        {

            string Json = "@{ CPU: 'Intel', Drives: ['DVD read/writer','500 gigabyte hard drive'] } ";
            JObject o = JObject.Parse(Json);

            return o;


        }


    }


    
}