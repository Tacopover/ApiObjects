using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Windows;

namespace CollabAPIMEP
{
    public class FamilyLoadHandler
    {
        private UIApplication uiApp;
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document m_doc;
        public FamilyLoadHandler(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_app = uiapp.Application;
            m_doc = uiApp.ActiveUIDocument.Document;
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
            // if the pathname is empty then cancel loading the family into the document
            if (pathname == "")
            {
                e.Cancel();
                MessageBox.Show("loading family canceled");
                return;

                // if this does not work then just load the family, check the family and if it does not meet the requirements delete it from doc
            }


            else
            {
                UIDocument familyUiDocument = uiApp.OpenAndActivateDocument(e.FamilyPath);
                Document familyDocument = familyUiDocument.Document;
                FilteredElementCollector eleCol = new FilteredElementCollector(familyDocument);
                var elements = eleCol.WhereElementIsNotElementType().ToElements();

                if (elements.Count > 100)
                {
                    familyDocument.Close();
                    e.Cancel();
                    MessageBox.Show("Too many elements inside family, loading family canceled");
                    return;
                }


                FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));

                IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();

                if (importsLinks.Count != 0)
                {
                    familyDocument.Close();
                    e.Cancel();
                    MessageBox.Show("CAD drawings inside families is not allowed, loading family canceled");
                    return;
                }

            }


            string result = "Family: " + famname + "\n Document: " + doctitle + "\n Path: " + pathname;
            //MessageBox.Show(result);
        }


        
    }
}
