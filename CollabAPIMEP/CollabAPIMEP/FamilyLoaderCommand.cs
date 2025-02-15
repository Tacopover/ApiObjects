using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP.Helpers;
using FamilyAuditorCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace CollabAPIMEP
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class FamilyLoaderCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                //check if document is project environment
                Document doc = commandData.Application.ActiveUIDocument.Document;

                ProjectInfo info = doc.ProjectInformation;
                if (info == null)
                {
                    TaskDialog.Show("Error", "This command can only be opened in a project environment.");
                    return Result.Cancelled;
                }

                string location = Assembly.GetExecutingAssembly().Location;
                string path = typeof(FamilyLoaderCommand).Namespace + "." + nameof(FamilyLoaderCommand);

                FamilyLoadHandler currentLoadHandler = FamilyLoaderApplication.currentLoadHandler;
                SettingsManager settingsManager = FamilyLoaderApplication.SettingsManager;

#if DEBUG
                if (currentLoadHandler == null)
                {
                    currentLoadHandler = new FamilyLoadHandler(uiApp.ActiveAddInId);
                    currentLoadHandler.Fl_doc = uiApp.ActiveUIDocument.Document;
                    uiApp.ViewActivated += currentLoadHandler.OnViewActivated;
                }

                //check if updater is already registered
                //currentLoadHandler.EnableUpdater();


                SimpleLog.SetLogFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Family Auditor", "FA_Log_");
#endif

                currentLoadHandler.Initialize(uiApp);
                if (FamilyLoaderApplication.ViewModel == null)
                {
                    FamilyLoaderApplication.ViewModel = new MainViewModel(currentLoadHandler, FamilyLoaderApplication.SettingsManager);
                }

                //List<UpdaterInfo> updaterInfos2 = UpdaterRegistry.GetRegisteredUpdaterInfos(doc).ToList();
                //start up logger
                SimpleLog.Info("Command Start");

                uiApp.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(currentLoadHandler.OnIdling);

                //show main window
                if (FamilyLoaderApplication.ViewModel.IsWindowClosed)
                {
                    FamilyLoaderApplication.ViewModel.ShowMainWindow(uiApp.MainWindowHandle);
                }
                else
                {
                    FamilyLoaderApplication.ViewModel.MainWindow.Activate();
                }


                return Result.Succeeded;

            }

            catch (Exception ex)
            {
                SimpleLog.Error("Command Exception");
                SimpleLog.Log(ex);
                SimpleLog.SetLogFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Family Auditor", "FA_Log_");

                //TypeUpdater typeUpdater = new TypeUpdater(commandData.Application.ActiveAddInId, currentLoadHandler);
                //UpdaterRegistry.UnregisterUpdater(typeUpdater.GetUpdaterId());

                string errormessage = ex.GetType().Name + " " + ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                MessageBox.Show(errormessage);
                return Result.Failed;
            }
        }


    }
}
