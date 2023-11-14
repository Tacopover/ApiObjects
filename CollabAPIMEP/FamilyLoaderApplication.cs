using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CollabAPIMEP
{

    public class FamilyLoaderApplication : IExternalApplication
    {
        public static System.Windows.Media.ImageSource Icon;

        public static FamilyLoadHandler LoadHandler;

        void AddRibbonPanel(UIControlledApplication application)
        {
            //string tabname = "dontneedit";
            //application.CreateRibbonTab(tabname);
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("FamilyLoader");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData CCData = new PushButtonData("FL",
                "FamilyLoader",
                thisAssemblyPath,
                "CollabAPIMEP.FamilyLoaderCommand");

            PushButton CCbutton = ribbonPanel.AddItem(CCData) as PushButton;
            CCbutton.ToolTip = "Start FamilyLoader";
            Icon = PngImageSource("CollabAPIMEP.resources.fl_icon.png");
            CCbutton.LargeImage = Icon;

        }
        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonPanel(application);

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        void OnApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            // Sender is an Application instance:

            Application app = sender as Application;

            // However, UIApplication can be 
            // instantiated from Application.

            UIApplication uiapp = new UIApplication(app);

            LoadHandler = new FamilyLoadHandler(uiapp);

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
