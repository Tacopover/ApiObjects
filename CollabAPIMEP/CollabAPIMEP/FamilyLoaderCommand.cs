using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
                FamilyLoadHandler currentLoadHandler = FamilyLoaderApplication.currentLoadHandler;

                string location = Assembly.GetExecutingAssembly().Location;
                string path = typeof(FamilyLoaderCommand).Namespace + "." + nameof(FamilyLoaderCommand);

                if (info != null)
                {

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
                else
                {
                    TaskDialog.Show("Error", "This command can only be opened in a project environment.");
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
