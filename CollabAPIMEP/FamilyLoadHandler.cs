using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace CollabAPIMEP
{
    public class FamilyLoadHandler
    {
        public static Guid Settings = new Guid("c16f94f6-5f14-4f33-91fc-f69dd7ac0d05");
        public List<string> Pathnames = new List<string>();
        public List<string> LoadedNames = new List<string>();
        private ExternalEvent externalLoadEvent;
        private LoadFamilyHandler loadFamilyHandler;

        private UIApplication uiApp;
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document m_doc;
        public Dictionary<string, Rule> RulesMap;
        public FamilyLoadHandler(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_app = uiapp.Application;
            m_doc = uiApp.ActiveUIDocument.Document;
            loadFamilyHandler = new LoadFamilyHandler();
            externalLoadEvent = ExternalEvent.Create(loadFamilyHandler);
        }

        //public void EnableFamilyLoader()
        //{
        //    m_app.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
        //}

        //public void DisableFamilyLoader()
        //{
        //    m_app.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
        //}

        //private void OnFamilyLoadedIntoDocument(object sender, FamilyLoadedIntoDocumentEventArgs e)
        //{
        //    LoadedNames.Add(e.FamilyPath + e.FamilyName + ".rfa");

        //}
        //private void OnFamilyLoadingIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadingIntoDocumentEventArgs e)
        //{
        //    Pathnames.Add(e.FamilyPath + e.FamilyName + ".rfa");
        //    e.Cancel();
        //    return;

        //externalLoadEvent.Raise();

        //Schema schema = Schema.Lookup(Settings);

        ////Entity retrievedEntity = m_doc.ProjectInformation.GetEntity(schema);

        ////List<Rule> rules = retrievedEntity.Get<List<Rule>>(schema.GetField("FamilyLoaderRules"));



        //string doctitle = e.Document.Title;
        //string famname = e.FamilyName;
        ////if the pathname is empty then cancel loading the family into the document
        //if (pathname == "")
        //{
        //    e.Cancel();
        //    MessageBox.Show("loading family canceled");
        //    return;

        //    // if this does not work then just load the family, check the family and if it does not meet the requirements delete it from doc
        //}

        ////e.Cancel();
        ////return;

        //else
        //{

        //    //UIDocument familyUiDocument = uiApp.OpenAndActivateDocument(e.FamilyPath);
        //    //Document familyDocument = familyUiDocument.Document;
        //    Document familyDocument = m_app.OpenDocumentFile(pathname);
        //    FilteredElementCollector eleCol = new FilteredElementCollector(familyDocument);
        //    var elements = eleCol.WhereElementIsNotElementType().ToElements();



        //    foreach (Rule rule in RulesMap.Values)
        //    {
        //        if (rule.IsRuleEnabled == true)
        //        {

        //            if (rule.Name == "Number of elements")
        //            {
        //                if (elements.Count > 100)
        //                {
        //                    familyDocument.Close();
        //                    e.Cancel();
        //                    MessageBox.Show("Too many elements inside family, loading family canceled");
        //                    return;
        //                }
        //            }

        //            //    FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));


        //            FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));

        //            IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();


        //            if (rule.Name == "Imported instanced")
        //            {

        //                familyDocument.Close();
        //                e.Cancel();
        //                MessageBox.Show("CAD drawings inside families is not allowed, loading family canceled");
        //                return;

        //            }

        //        }


        //    }

        //    string result = "Family: " + famname + "\n Document: " + doctitle + "\n Path: " + pathname;
        //    MessageBox.Show(result);

        //}



        //}
    }

    public class LoadFamilyHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {

            //string pathname = @"C:\Users\taco\OneDrive - MEPover\Revit\content\connector position host.rfa";
            //Document familyDocument = app.Application.OpenDocumentFile(FamilyLoadHandler.Pathname);
            //string title = familyDocument.Title;
            //bool closed = familyDocument.Close(false);

        }

        public string GetName()
        {
            return "LoadFamilyHandler";
        }
    }
}
