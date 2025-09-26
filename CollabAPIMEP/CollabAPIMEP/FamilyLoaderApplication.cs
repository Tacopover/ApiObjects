using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CollabAPIMEP.Helpers;
using FamilyAuditorCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CollabAPIMEP
{

    public class FamilyLoaderApplication : IExternalApplication
    {
        public static System.Windows.Media.ImageSource Icon;

        public static FamilyLoadHandler currentLoadHandler { get; set; }
        public static MainViewModel ViewModel { get; set; }
        public static SettingsManager SettingsManager { get; set; }

        private Autodesk.Revit.ApplicationServices.Application m_app = null;


        void AddRibbonPanel(UIControlledApplication application)
        {

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            string assemblyTitle = fvi.FileDescription;
            string assemblyVersion = fvi.ProductVersion;

            RibbonPanel ribbonPanel = application.CreateRibbonPanel(assemblyTitle + " " + assemblyVersion);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;



#if ADMIN

            PushButtonData CCData = new PushButtonData("FL-ADMIN",
                assemblyTitle + " (Admin)",
                thisAssemblyPath,
                "CollabAPIMEP.FamilyLoaderCommand");

            PushButton CCbuttonAdmin = ribbonPanel.AddItem(CCData) as PushButton;
            //CCbuttonAdmin.ToolTip = "Family Auditor for BIM manager";

            CCbuttonAdmin.ToolTip = "Start" + assemblyTitle;
            Icon = Utils.LoadEmbeddedImage("FamilyAuditor.png");
            CCbuttonAdmin.LargeImage = Icon;

            //create login button
            PushButtonData CCDataLogin = new PushButtonData("FL-LOGIN",
                "Login",
                thisAssemblyPath,
                "CollabAPIMEP.LoginCommand");

            PushButton CCbuttonLogin = ribbonPanel.AddItem(CCDataLogin) as PushButton;
            CCbuttonLogin.ToolTip = "Login to Family Auditor";
            CCbuttonLogin.LargeImage = Icon;




#elif USER
            PushButtonData CCDataUserPopup = new PushButtonData("FL-USER",
            assemblyTitle + " (User)",
            thisAssemblyPath,
            "CollabAPIMEP.FamilyLoaderCommand");

            PushButton CCbuttonUser = ribbonPanel.AddItem(CCDataUserPopup) as PushButton;
            CCbuttonUser.ToolTip = "Start" + assemblyTitle;

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

                application.ControlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(OnFailuresProcessing);

                //TypeUpdater typeUpdater = new TypeUpdater(application.ActiveAddInId);
                //UpdaterRegistry.RegisterUpdater(typeUpdater, true);
                //ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
                //UpdaterRegistry.AddTrigger(typeUpdater.GetUpdaterId(), familyFilter, Element.GetChangeTypeElementAddition());

                currentLoadHandler = new FamilyLoadHandler(application.ActiveAddInId);
                SettingsManager = new SettingsManager();
                SettingsManager.LogIn();
                //currentLoadHandler.EnableUpdater();
                //TypeUpdater typeUpdater = new TypeUpdater(application.ActiveAddInId, currentLoadHandler);
                //UpdaterRegistry.RegisterUpdater(typeUpdater, true);
                //ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
                //UpdaterRegistry.AddTrigger(typeUpdater.GetUpdaterId(), familyFilter, Element.GetChangeTypeElementAddition());

                SimpleLog.SetLogFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Family Auditor", "FA_Log_");

            }
            catch (Exception)
            {
                MessageBox.Show("failed");
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        private void OnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {

            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            FailureHandlingOptions options = failuresAccessor.GetFailureHandlingOptions();
            options.SetClearAfterRollback(true);

            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
            if (failureMessages.Count == 0)
            {
                return;
            }
            foreach (FailureMessageAccessor failureMessage in failureMessages)
            {
                FailureDefinitionId failId = failureMessage.GetFailureDefinitionId();
                if (failId == BuiltInFailures.DocumentFailures.DUMisbehavingUpdater)
                {
                    string failureTxt = failureMessage.GetDescriptionText();
                    if (failureTxt.ToLower().Contains("typeupdater"))
                    {
                        SimpleLog.Warning("TypeUpdater disabled by Revit");
                        //failuresAccessor.DeleteWarning(failureMessage);
                        e.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);

                    }
                }
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            TypeUpdater typeUpdater = new TypeUpdater(application.ActiveAddInId, currentLoadHandler);
            UpdaterRegistry.UnregisterUpdater(typeUpdater.GetUpdaterId());
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

            currentLoadHandler.GetRulesFromSchema();

            currentLoadHandler.EnableUpdater();
        }

        void DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            // Sender is an Application instance:
            m_app = sender as Autodesk.Revit.ApplicationServices.Application;

            UIApplication uiapp = new UIApplication(m_app);


            if (uiapp.ActiveUIDocument == null)
            {
                return;
            }

            // if there is no document opened yet, enable the OnViewActivated event
            if (currentLoadHandler.Fl_doc == null)
            {
                currentLoadHandler.Fl_doc = uiapp.ActiveUIDocument.Document;
                uiapp.ViewActivated += currentLoadHandler.OnViewActivated;
                SettingsManager.LoadRulesLocal();
            }

            if (uiapp.Application.LoginUserId != "")
            {
                FirebaseHelper firebaseHelper = new FirebaseHelper(uiapp.Application.Username, uiapp.Application.LoginUserId);

                Task.Run(async () =>
                {
                    try
                    {
                        // Attempt to sign in the user
                        await firebaseHelper.SignInUserAsync();

                        if (SettingsManager.FireBaseHelper.UserCredential != null)
                        {
                            Console.WriteLine($"User signed in successfully: {firebaseHelper.UserCredential.User.Uid}");
                        }
                        else
                        {
                            Console.WriteLine("Sign-in failed: UserCredential is null");

                            // Attempt to create a new user
                            try
                            {
                                await firebaseHelper.CreateUserAsync();
                                Console.WriteLine($"User created successfully: {firebaseHelper.UserCredential.User.Uid}");
                            }
                            catch (Exception createEx)
                            {
                                Console.WriteLine($"Error creating user: {createEx.Message}");
                            }
                        }
                    }
                    catch (Exception signInEx)
                    {

                        Console.WriteLine($"Failed to sign in: {signInEx.Message}");

                    }
                });


            }

            //enable the updater in case it has been disabled by Revit
            currentLoadHandler.EnableUpdater();
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

            // if there is no document opened yet, enable the OnViewActivated event
            if (currentLoadHandler.Fl_doc == null)
            {
                currentLoadHandler.Fl_doc = uiapp.ActiveUIDocument.Document;
                uiapp.ViewActivated += currentLoadHandler.OnViewActivated;
            }

            //enable the updater in case it has been disabled by Revit
            currentLoadHandler.EnableUpdater();
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

