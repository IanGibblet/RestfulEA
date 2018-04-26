using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulEA.Models.BusinessLogic
{
    public static class MOSSEC_HELPER
    {

        internal static Architecture GetArchitecture(Uos uOSObject)
        {

            for (int i = 0; i < uOSObject.MoSSECDataContainer[0].Items.Count(); i++)
            {

                if (uOSObject.MoSSECDataContainer[0].Items[i].ToString() == "Architecture")
                {
                    //Cast the architecture and return it.
                    return (Architecture)uOSObject.MoSSECDataContainer[0].Items[i];
                }

            }
            return null;

        }
    }
}