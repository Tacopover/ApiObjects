using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CollabAPIMEP
{

    public class FamilyLoaderApplication : IExternalApplication
    {
        public static System.Windows.Media.ImageSource Icon;

        public static Dictionary<string, FamilyLoadHandler> FamilyLoadHandlers = new Dictionary<string, FamilyLoadHandler>();
        public static MainViewModel FamLoaderViewModel;
        private Autodesk.Revit.ApplicationServices.Application m_app = null;


        void AddRibbonPanel(UIControlledApplication application)
        {

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            string assemblyTitle = fvi.FileDescription;
            string assemblyVersion = fvi.ProductVersion;





            RibbonPanel ribbonPanel = application.CreateRibbonPanel(assemblyTitle + " " + assemblyVersion);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

#if !USER

            PushButtonData CCData = new PushButtonData("FL-ADMIN",
                assemblyTitle + " (Admin)",
                thisAssemblyPath,
                "CollabAPIMEP.FamilyLoaderCommand");
            PushButton CCbuttonAdmin = ribbonPanel.AddItem(CCData) as PushButton;
            CCbuttonAdmin.ToolTip = "Start" + assemblyTitle;
            Icon = Utils.LoadEmbeddedImage("fl_icon.png");
            CCbuttonAdmin.LargeImage = Icon;
#endif

#if !ADMIN
            PushButtonData CCDataUserPopup = new PushButtonData("FL-USER",
            "Info",
            thisAssemblyPath,
            "CollabAPIMEP.UserPopup");

            PushButton CCbuttonUser = ribbonPanel.AddItem(CCDataUserPopup) as PushButton;
            CCbuttonUser.ToolTip = "Family Check Version and active rules";
            Icon = Utils.LoadEmbeddedImage("fl_icon.png");
            CCbuttonUser.LargeImage = Icon;
#endif


        }
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                AddRibbonPanel(application);

                application.ControlledApplication.DocumentOpened += new EventHandler
                <Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(DocumentOpened);

                application.ControlledApplication.DocumentCreated += new EventHandler
                    <Autodesk.Revit.DB.Events.DocumentCreatedEventArgs>(DocumentCreated);

                application.ControlledApplication.DocumentSynchronizedWithCentral += new EventHandler
                    <DocumentSynchronizedWithCentralEventArgs>(DocumentSynced);

                //TypeUpdater typeUpdater = new TypeUpdater(application.ActiveAddInId);
                //UpdaterRegistry.RegisterUpdater(typeUpdater, true);
                //ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
                //UpdaterRegistry.AddTrigger(typeUpdater.GetUpdaterId(), familyFilter, Element.GetChangeTypeElementAddition());

            }
            catch (Exception)
            {
                return Result.Failed;
                MessageBox.Show("failed");
            }
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            //TypeUpdater typeUpdater = new TypeUpdater(application, FamilyLoadHandlers.Values.FirstOrDefault());
            //UpdaterRegistry.UnregisterUpdater(typeUpdater.GetUpdaterId());
            return Result.Succeeded;
        }

        void DocumentSynced(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            // Sender is an Application instance:

            m_app = sender as Autodesk.Revit.ApplicationServices.Application;

            // However, UIApplication can be 
            // instantiated from Application.

            UIApplication uiapp = new UIApplication(m_app);

            if (uiapp.ActiveUIDocument == null)
            {
                return;
            }

            Document doc = uiapp.ActiveUIDocument.Document;

            if (doc.ProjectInformation != null)
            {
                FamilyLoadHandler currentLoadHandler = LookupFamilyLoadhandler(doc);
                if (currentLoadHandler == null)
                {
                    currentLoadHandler = AddFamilyLoadHandler(uiapp);
                }

                else
                {
                    currentLoadHandler.GetRulesFromSchema();

                }

            }

        }

        void DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {


            // Sender is an Application instance:

            m_app = sender as Autodesk.Revit.ApplicationServices.Application;

            // However, UIApplication can be 
            // instantiated from Application.

            UIApplication uiapp = new UIApplication(m_app);

            if (uiapp.ActiveUIDocument == null)
            {
                return;
            }

            Document doc = uiapp.ActiveUIDocument.Document;

            if (doc.ProjectInformation != null)
            {
                FamilyLoadHandler currentLoadHandler = LookupFamilyLoadhandler(doc);


                if (currentLoadHandler == null)
                {
                    currentLoadHandler = AddFamilyLoadHandler(uiapp);
                }

                else
                {
                    currentLoadHandler.GetRulesFromSchema();

                }

            }




        }

        void DocumentCreated(object sender, DocumentCreatedEventArgs e)
        {
            // Sender is an Application instance:

            m_app = sender as Autodesk.Revit.ApplicationServices.Application;

            // However, UIApplication can be 
            // instantiated from Application.

            UIApplication uiapp = new UIApplication(m_app);

            if (uiapp.ActiveUIDocument == null)
            {
                return;
            }

            Document doc = uiapp.ActiveUIDocument.Document;

            if (doc.ProjectInformation != null)
            {
                FamilyLoadHandler currentLoadHandler = LookupFamilyLoadhandler(doc);
                if (currentLoadHandler == null)
                {
                    currentLoadHandler = AddFamilyLoadHandler(uiapp);
                }

                else
                {
                    currentLoadHandler.GetRulesFromSchema();

                }


            }

        }



        public static string GetDocPath(Document doc)
        {

            string path = "";

            if (doc.IsWorkshared == true)
            {
                ModelPath modelPath = doc.GetWorksharingCentralModelPath();
                return ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

            }

            else
            {
                return doc.PathName;
            }


        }

        public static FamilyLoadHandler LookupFamilyLoadhandler(Document doc)
        {

            string docPath = GetDocPath(doc);
            FamilyLoadHandler currentFamilyLoadHandler = null;

            try
            {
                FamilyLoadHandlers.TryGetValue(docPath, out currentFamilyLoadHandler);
                return currentFamilyLoadHandler;

            }
            catch (Exception)
            {
                return null;
            }



        }

        public static FamilyLoadHandler AddFamilyLoadHandler(UIApplication uiApp)
        {
            FamilyLoadHandler currentLoadHandler = new FamilyLoadHandler(uiApp);

            if (currentLoadHandler.GetRulesFromSchema() == true)
            {
                //for testing
                currentLoadHandler.RulesEnabled = true;

                if (currentLoadHandler.RulesEnabled == true)
                {
                    currentLoadHandler.EnableFamilyLoading();

                }
            }
            FamilyLoadHandlers[GetDocPath(uiApp.ActiveUIDocument.Document)] = currentLoadHandler;
            return currentLoadHandler;
        }

    }



}

