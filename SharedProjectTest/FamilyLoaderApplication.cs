﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CollabAPIMEP
{

    public class FamilyLoaderApplication : IExternalApplication
    {
        public static System.Windows.Media.ImageSource Icon;

        public static FamilyLoadHandler LoadHandler;
        public static MainViewModel FamLoaderViewModel;

        private Autodesk.Revit.ApplicationServices.Application m_app = null;

        void AddRibbonPanel(UIControlledApplication application)
        {

            RibbonPanel ribbonPanel = application.CreateRibbonPanel("FamilyLoader");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData CCData = new PushButtonData("FL",
                "FamilyLoader",
                thisAssemblyPath,
                "CollabAPIMEP.FamilyLoaderCommand");

            PushButton CCbutton = ribbonPanel.AddItem(CCData) as PushButton;
            CCbutton.ToolTip = "Start FamilyLoader";
            Icon = Utils.LoadEmbeddedImage("fl_icon.png");
            CCbutton.LargeImage = Icon;



            PushButtonData CCDataTestReference = new PushButtonData("Test",
            "Test Reference2024",
            thisAssemblyPath,
            "CollabAPIMEP.TestReference2024");

            PushButton CCbuttonTestReference = ribbonPanel.AddItem(CCDataTestReference) as PushButton;
            CCbuttonTestReference.ToolTip = "Test";


            //        PushButtonData testButtonData = new PushButtonData("FL",
            //"FamilyLoader",
            //thisAssemblyPath,
            //"CollabAPIMEP.FamilyLoaderCommand");

            //        PushButton testButton = ribbonPanel.AddItem(testButtonData) as PushButton;


        }
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                AddRibbonPanel(application);

                application.ControlledApplication.DocumentOpened += new EventHandler
                <Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(DocumentOpened);
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

            if (FamilyLoaderApplication.FamLoaderViewModel.MainWindow != null)
            {
                if (FamilyLoaderApplication.FamLoaderViewModel.MainWindow.IsVisible)
                {
                    FamilyLoaderApplication.FamLoaderViewModel.MainWindow.Close();
                }
            }

            return Result.Succeeded;
        }

        void DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            // Sender is an Application instance:

            m_app = sender as Autodesk.Revit.ApplicationServices.Application;

            // However, UIApplication can be 
            // instantiated from Application.

            UIApplication uiapp = new UIApplication(m_app);
            Document doc = uiapp.ActiveUIDocument.Document;

            if (doc.ProjectInformation != null)
            {
                LoadHandler = new FamilyLoadHandler(uiapp);
                LoadHandler.GetRulesFromSchema();
                LoadHandler.EnableFamilyLoading();
            }

        }


        private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream(embeddedPath);
            //var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return decoder.Frames[0];
        }
    }
}
