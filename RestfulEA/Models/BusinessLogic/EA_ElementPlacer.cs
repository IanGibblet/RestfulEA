using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulEA.Models.BusinessLogic
{
    public class EA_ElementPlacer
    {

        private int l = 100;
        private int r = 200;
        private int t = 100;
        private int b = 200;





        public string GetNextElement()
        {
            string StringToReturn = "l=" + l.ToString() + ";r=" + r.ToString() + ";t=" + t.ToString() + ";b=" + b.ToString() + ";" ;

            l = l + 150;
            r = r + 150;
      




            return StringToReturn;



        }











    }
}