using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EA;

namespace RestfulEA.Models.BusinessLogic
{
    public class EA_Creation_Factory
    {


  

        public static void CreateDiagram(Repository m_Repository, string TOI_GUID, string Content)
        {
            EA.Package Package = m_Repository.GetPackageByGuid(TOI_GUID);


            //DIAGRAM
            EA.Diagram Diagram = (EA.Diagram)Package.Diagrams.AddNew(Content,"Block");
            Diagram.Notes = "Hello Test";
            Diagram.Update();



            //ELEMENTS
            EA.Element NewElement = (EA.Element)Package.Elements.AddNew("ReferenceType", "Class");
            NewElement.Update();




            EA.DiagramObject DiagOb = (EA.DiagramObject)Diagram.DiagramObjects.AddNew("l=200;r=400;t=200;b=600;", "");
  



            DiagOb.ElementID = NewElement.ElementID;


            DiagOb.Update();


            ////
         EA.Element NewElement2 = (EA.Element)Package.Elements.AddNew("IanTheNew", "Actor");
         EA.DiagramObject DiagOb2 = (EA.DiagramObject)Diagram.DiagramObjects.AddNew("l=800;r=600;t=200;b=600;", "");
         DiagOb2.ElementID = NewElement2.ElementID;   
            
            
            
            //LINKS







        }
    }
}