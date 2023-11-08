using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
                UIDocument familyDocument = uiApp.OpenAndActivateDocument(e.FamilyPath);
                FilteredElementCollector eleCol = new FilteredElementCollector(familyDocument.Document);
                var elements = eleCol.WhereElementIsNotElementType().ToElements();

                if(elements.Count > 100)
                {
                    e.Cancel();
                    MessageBox.Show("Too many elements inside family, loading family canceled");
                    return;
                }
            }


            //string result = "Family: " + famname + "\n Document: " + doctitle + "\n Path: " + pathname;
            //MessageBox.Show(result);
        }


        
    }
}
