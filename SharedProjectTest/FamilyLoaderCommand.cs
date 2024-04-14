using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows;

namespace CollabAPIMEP
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class FamilyLoaderCommand : IExternalCommand
    {
        private MainViewModel mainViewModel;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {

                UIApplication uiApp = commandData.Application;
                //check if document is project environment
                Document doc = commandData.Application.ActiveUIDocument.Document;
                ProjectInfo info = doc.ProjectInformation;
                FamilyLoadHandler currentLoadHandler = FamilyLoaderApplication.LookupFamilyLoadhandler(doc);


                if (info != null)
                {

                    //create new loadhandler if it does not exist
                    if (currentLoadHandler == null)
                    {
                        currentLoadHandler = FamilyLoaderApplication.AddFamilyLoadHandler(uiApp);
                    }

                    mainViewModel = new MainViewModel(uiApp, currentLoadHandler);


                    //show main window
                    if (mainViewModel.IsWindowClosed)
                    {
                        mainViewModel.ShowMainWindow();
                    }
                    else
                    {
                        mainViewModel.MainWindow.Activate();
                    }
                    

                    return Result.Succeeded;
                }
                else
                {
                    return Result.Cancelled;
                }



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
