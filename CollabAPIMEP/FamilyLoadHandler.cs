using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace CollabAPIMEP
{
    public class FamilyLoadHandler
    {
        private UIApplication uiApp;
        private Document m_doc;
        private UIControlledApplication uiCtrlApp;
        private ControlledApplication ctrlApp;
        public FamilyLoadHandler(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_doc = uiApp.ActiveUIDocument.Document;
            uiCtrlApp = FamilyLoaderApplication.uiCtrlApp;
            ctrlApp = uiCtrlApp.ControlledApplication;
        }

        public void EnableFamilyLoader()
        {
            ctrlApp.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
        }

        public void DisableFamilyLoader()
        {
            ctrlApp.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
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
            string result = "Family: " + famname + "\n Document: " + doctitle + "\n Path: " + pathname;
            MessageBox.Show(result);
        }
    }
}
