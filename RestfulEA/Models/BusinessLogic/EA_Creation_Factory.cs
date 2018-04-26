using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EA;
using System.IO;
using System.Xml.Linq;
using System.Text;

namespace RestfulEA.Models.BusinessLogic
{
    public class EA_Creation_Factory
    {

        //MOSSECOBJECTS
        const string BreakdownElement = "BreakdownElement";



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
 
           // DiagOb.ElementID = NewElement.oElementID;


            DiagOb.Update();


            ////
         EA.Element NewElement2 = (EA.Element)Package.Elements.AddNew("IanTheNew", "Actor");
         EA.DiagramObject DiagOb2 = (EA.DiagramObject)Diagram.DiagramObjects.AddNew("l=800;r=600;t=200;b=600;", "");
         DiagOb2.ElementID = NewElement2.ElementID;   
            
            
            
            //LINKS


        }

        public static void PostAircadiaToEA(List<Repository> m_RepositoryList, string RAW_TOI, Uos UOSObject)
        {

        
            EA.Repository m_Repository = new EA.Repository();

            //1) CHECK EAP FILE EXISTS, IF NOT CREATE IT

            string WORKING_DIR = System.Configuration.ConfigurationManager.AppSettings["EAPWORKINGDIR"];

            if (System.IO.File.Exists(WORKING_DIR + @"\" + RAW_TOI + ".eap"))
            {


             

            }
            else  //USER IS POSTING TO NON EXISTANT PROJECT SO CREATE ONE
            {
                m_Repository = new EA.Repository();    
                m_Repository.CreateModel(EA.CreateModelType.cmEAPFromBase, WORKING_DIR + @"\" + RAW_TOI + ".eap", 0);
                m_RepositoryList.Add(m_Repository);
             
             }
            
          
            //It should exist at this point.
            m_Repository.OpenFile(WORKING_DIR + @"\" + RAW_TOI + ".eap");



            //1.1 GET THE EA OBJECT UP AND RUNNING TO RECIEVE NEW OBJECTS


            //CREATE A NEW TOP LEVEL PACKAGE FOR US TO 

           Architecture architecture  = MOSSEC_HELPER.GetArchitecture(UOSObject);



            EA.Package Model = (EA.Package)m_Repository.Models.AddNew(RAW_TOI, "");
            Model.Update();
            EA.Package Package = (EA.Package)Model.Packages.AddNew(architecture.coreObjectId, "nothing");
            Package.Update();
           




            //2) POPULATE LISTS OF MOSSEC ELEMENTS 
            List<BreakdownElement> MOS_BreakdownElements = new List<global::BreakdownElement>();
            List<Breakdown> MOS_Breakdown = new List<Breakdown>();


            //TODO - MAKE OTHER LISTS TO BE POPULATED HERE

            MOSSEC_LIST_POPULATOR(UOSObject, ref MOS_BreakdownElements,ref MOS_Breakdown);



            //3) ADD EA ELEMENTS TO THE PACKAGE                  
            EA_LIST_POPULATOR(MOS_BreakdownElements, MOS_Breakdown, ref Package);



        


            //4) PUSH EA ELEMENTS INTO THE EAP FILE




        }
  
        
        //Cycle through each Mossec element and add elements to a new diagram inside the package.
        private static void EA_LIST_POPULATOR(List<BreakdownElement> mOS_BreakdownElements, List <Breakdown> mOS_Breakdowns,  ref Package packageToEnrich)
        {


            foreach (Breakdown BD in mOS_Breakdowns)
            {

                //Turn the Breakdowns into the Diagram
                EA.Diagram NewDiagram = (EA.Diagram)packageToEnrich.Diagrams.AddNew(BD.coreObjectId, "Logical");

                NewDiagram.Notes = BD.coreObjectId + "Created by " + BD.createdBy.reference + " on the " + BD.createdOn;
                NewDiagram.Author = BD.createdBy.reference;
                NewDiagram.Update();


                //Turn the Breakdownelements into elements
                List<BreakdownElement> SelectedBreakdowns = new List<BreakdownElement>();
                SelectedBreakdowns = mOS_BreakdownElements.FindAll(s => s.type.Value == BD.type.Value);
            
                EA_ElementPlacer ElementPlacer = new EA_ElementPlacer();  //Provides the coordinates of the next EA element.

                //In the mossec example, these will be Functional and Logical breakdowns
                foreach (BreakdownElement breakdownElement in SelectedBreakdowns)
                {


                    //CREATE LOGICAL ELEMENT AND DIAGRAM OBJECT
                    EA.Element NewElement = (EA.Element)packageToEnrich.Elements.AddNew(breakdownElement.names[0].text, "Class");
                    NewElement.Update();



                    EA.DiagramObject DiagOb = (EA.DiagramObject)NewDiagram.DiagramObjects.AddNew(ElementPlacer.GetNextElement(), "");
                    DiagOb.ElementID = NewElement.ElementID;
                    DiagOb.Update();



                }
            }


            packageToEnrich.Update();

        }


        //Go Through the top level MOSSEC Object and find Breakdowns
        private static void  MOSSEC_LIST_POPULATOR(Uos MossecTop,  ref List<BreakdownElement> BreakdownElements , ref List<Breakdown>Breakdowns)
        {



            //Go Through each of the Objects and look for breakdowns
            foreach ( var MossecItemLoop in MossecTop.MoSSECDataContainer[0].Items)
            {

                switch (MossecItemLoop.ToString())
                {
                    case "BreakdownElement":
                        BreakdownElement bde = (BreakdownElement)MossecItemLoop;
                        BreakdownElements.Add(bde);
                        break;
                    case "Breakdown":
                        Breakdown bd = (Breakdown)MossecItemLoop;
                        Breakdowns.Add(bd);
                        break;
                        
                }

                }
    
        }



     





    }
}