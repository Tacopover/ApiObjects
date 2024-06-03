using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                if (info == null)
                {
                    TaskDialog.Show("Error", "This command can only be opened in a project environment.");
                    return Result.Cancelled;
                }

                string location = Assembly.GetExecutingAssembly().Location;
                string path = typeof(FamilyLoaderCommand).Namespace + "." + nameof(FamilyLoaderCommand);

                FamilyLoadHandler currentLoadHandler = FamilyLoaderApplication.currentLoadHandler;
#if DEBUG
                if (currentLoadHandler == null)
                {
                    currentLoadHandler = new FamilyLoadHandler(uiApp);
                }
#endif
                //start up logger
                startLogger();
                Log.Information("Command Start");

                //check if updater is already registered
                TypeUpdater typeUpdater_old = new TypeUpdater(commandData.Application, currentLoadHandler);
                if (UpdaterRegistry.IsUpdaterRegistered(typeUpdater_old.GetUpdaterId()))
                {
                    UpdaterRegistry.UnregisterUpdater(typeUpdater_old.GetUpdaterId());
                }

                TypeUpdater typeUpdater = new TypeUpdater(uiApp, currentLoadHandler);
                UpdaterRegistry.RegisterUpdater(typeUpdater, doc, true);
                ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
                UpdaterRegistry.AddTrigger(typeUpdater.GetUpdaterId(), familyFilter, Element.GetChangeTypeElementAddition());

                mainViewModel = new MainViewModel(uiApp, currentLoadHandler);
                uiApp.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(currentLoadHandler.OnIdling);

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

            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occurred during command execution.");
                Log.CloseAndFlush();
                string errormessage = ex.GetType().Name + " " + ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                MessageBox.Show(errormessage);
                return Result.Failed;
            }
        }
        private void startLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Revit Auditor\\FamilyAuditor.txt",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

    }
}
