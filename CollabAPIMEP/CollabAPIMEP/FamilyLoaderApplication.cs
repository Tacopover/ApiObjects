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

        public static FamilyLoadHandler currentLoadHandler { get; set; }

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
            CCbuttonAdmin.ToolTip = "Family Auditor for BIM manager";

            CCbuttonAdmin.ToolTip = "Start" + assemblyTitle;
            Icon = Utils.LoadEmbeddedImage("FamilyAuditor.png");
            CCbuttonAdmin.LargeImage = Icon;
#endif

#if !ADMIN
            PushButtonData CCDataUserPopup = new PushButtonData("FL-USER",
            "Info",
            thisAssemblyPath,
            "CollabAPIMEP.UserPopup");

            PushButton CCbuttonUser = ribbonPanel.AddItem(CCDataUserPopup) as PushButton;
            CCbuttonUser.ToolTip = "Family Auditor Version and active rules";
            Icon = Utils.LoadEmbeddedImage("FamilyAuditor.png");
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

            if (!currentLoadHandler.GetRulesFromSchema())
            {
                currentLoadHandler.RulesMap = Rule.GetDefaultRules();
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
                if (currentLoadHandler == null)
                {
                    currentLoadHandler = new FamilyLoadHandler(uiapp);
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
                if (currentLoadHandler == null)
                {
                    currentLoadHandler = new FamilyLoadHandler(uiapp);
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
                if (doc.PathName == "")
                {
                    return doc.Title;
                }
                return doc.PathName;
            }


        }


    }



}

