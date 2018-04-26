using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EA;
using System.IO;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace RestfulEA.App_Start
{
    public class StartUp
    {

        
        //private const string WorkspacePathDirectory = @"C:\Landfill\";
       // static string  pathForEAObject = @"C:\Landfill\EA_Cache.txt";

        public static void Init()
        {
           ParseEAProjects();

        }


        //Open all of the .eap file and load them into a json text file for upcoming sessions to use.
        public  static List<EA.Repository> ParseEAProjects()
        {
            //Get the list of EA project files
            List<string> strListOfProjects = new List<string>();
            strListOfProjects = GetListOfProjects();
            List<EA.Repository> ListOfRepositories = new List<Repository>();
         
            foreach (string strFilename in strListOfProjects)
            {

            
                try
                {
                   EA.Repository m_Repository = new EA.Repository();
                    m_Repository.OpenFile(strFilename);
                    ListOfRepositories.Add(m_Repository);

                }

                catch(Exception e)
                {
                    string message = e.Message;




                }

       


            }



            return ListOfRepositories;

            //Now let's store them for later use.
            //Create a json string
            // var EAjson = new JavaScriptSerializer().Serialize(ListOfRepositories);






            //If it exists, wipe it.
            // if (System.IO.File.Exists(pathForEAObject))
            // {
            //     System.IO.File.WriteAllText(pathForEAObject, string.Empty);
            //  }

            //Write JSON to file      
            // System.IO.File.WriteAllText(pathForEAObject, EAjson.ToString());


        }

        private static List<string> GetListOfProjects()
        {

            List<string> ListToReturn = new List<string>();


          
            string[] fileEntries = Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings["EAPWORKINGDIR"]);



            //Get all files with the extension .eap
            foreach (string File in fileEntries)
            {
                string Extension = Path.GetExtension(File);
                Extension = Extension.ToLower();

                if (Extension != ".eap")
                    continue;


                ListToReturn.Add(File);

            }


                return ListToReturn;


        }


        //Open a directory and look for EAP
        public void PopulateFromWorkSpace(string WorkspacePathDirectory)

        {
            string[] fileEntries = Directory.GetFiles(WorkspacePathDirectory);

            //Get all files with the extension .eap
            foreach (string File in fileEntries)
            {

                string Extension = Path.GetExtension(File);
                //If file is nothing to do with EA, then ignore.

                Extension = Extension.ToLower();

                if (Extension != ".eap")
                    continue;

            //    EA_SP NewSP = new EA_SP();
            //    NewSP.FullPath = File;
            //    NewSP.FileName = Path.GetFileNameWithoutExtension(File);




             //   ListOfSPs.Add(NewSP);


            }
        }
    }


}