using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Data;

namespace CollabAPIMEP
{
    public class FamilyLoadHandler
    {
        public static Guid Settings = new Guid("c16f94f6-5f14-4f33-91fc-f69dd7ac0d05");


        private UIApplication uiApp;
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document m_doc;
        public Dictionary<string, Rule> RulesMap;
        public FamilyLoadHandler(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_app = uiapp.Application;
            m_doc = uiApp.ActiveUIDocument.Document;

            Schema schema = Schema.Lookup(Settings);

            Entity retrievedEntity = m_doc.ProjectInformation.GetEntity(schema);

            string rulesString = retrievedEntity.Get<string>(schema.GetField("FamilyLoaderRules"));

            List<string> totalRulesString = rulesString.Split(new string[] { "?" }, StringSplitOptions.None).ToList<string>();

            foreach (string ruleString in totalRulesString)
            {
                List<string> ruleProperties = rulesString.Split(new string[] { ":" }, StringSplitOptions.None).ToList<string>();

                foreach (string ruleProperty in ruleProperties)
                {
                    if (ruleProperty != "null")
                    {

                    }
                }


            }

            //FamilyLoadHandler.RulesMap

        }

        public void EnableFamilyLoader()
        {
            m_app.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
        }

        public void DisableFamilyLoader()
        {
            m_app.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
        }
        private void OnFamilyLoadingIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadingIntoDocumentEventArgs e)
        {

            string doctitle = e.Document.Title;
            string famname = e.FamilyName;
            string pathname = e.FamilyPath;
            //if the pathname is empty then cancel loading the family into the document
            if (pathname == "")
            {
                e.Cancel();
                MessageBox.Show("loading family canceled");
                return;

                // if this does not work then just load the family, check the family and if it does not meet the requirements delete it from doc
            }

            //e.Cancel();
            //return;

            else
            {

                UIDocument familyUiDocument = uiApp.OpenAndActivateDocument(e.FamilyPath);
                Document familyDocument = familyUiDocument.Document;
                FilteredElementCollector eleCol = new FilteredElementCollector(familyDocument);
                var elements = eleCol.WhereElementIsNotElementType().ToElements();

                RulesMap.TryGetValue("NumberOfElements", out Rule numberOfElements);

                if (numberOfElements.NumberOfElements > 100)
                {
                    familyDocument.Close();
                    e.Cancel();
                    MessageBox.Show("Too many elements inside family, loading family canceled");
                    return;
                }
                

                //    FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));


                FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));

                IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();

                RulesMap.TryGetValue("ImportedInstances", out Rule ImportedInstances);

                if (importsLinks.Count > ImportedInstances.NumberOfElements)
                {

                    familyDocument.Close();
                    e.Cancel();
                    MessageBox.Show("CAD drawings inside families is not allowed, loading family canceled");
                    return;

                }

                string result = "Family: " + famname + "\n Document: " + doctitle + "\n Path: " + pathname;
                MessageBox.Show(result);

            }



        }
    }
}
