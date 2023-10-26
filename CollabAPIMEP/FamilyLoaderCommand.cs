using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP;
using System;
using System.Windows;

namespace APIObjectMEPover
{
    public class FamilyLoaderCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                FamilyLoadHandler famHandler = new FamilyLoadHandler(uiApp);
                MainWindow mainWindow = new MainWindow(famHandler);
                mainWindow.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                string errormessage = ex.GetType().Name + " " + ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                MessageBox.Show(errormessage);
                return Result.Failed;
            }
        }
    }
}
