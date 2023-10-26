using Autodesk.Revit.UI;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CollabAPIMEP
{
    public class FamilyLoaderApplication : IExternalApplication
    {
        public static System.Windows.Media.ImageSource Icon;
        public static UIControlledApplication uiCtrlApp;
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
            Icon = PngImageSource("VoidManager.resources.fl_icon.png");
            CCbutton.LargeImage = Icon;

        }
        public Result OnStartup(UIControlledApplication application)
        {
            uiCtrlApp = application;
            AddRibbonPanel(application);
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
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
