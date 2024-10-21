using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows;

namespace Revit2025_MinimalTest
{
    public class Revit2025Test : IExternalApplication
    {
        public static System.Windows.Media.ImageSource Icon;
        void AddRibbonPanel(UIControlledApplication application)
        {

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            string assemblyTitle = fvi.FileDescription;
            string assemblyVersion = fvi.ProductVersion;

            RibbonPanel ribbonPanel = application.CreateRibbonPanel(assemblyTitle + " " + assemblyVersion);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData CCData = new PushButtonData("FL-ADMIN",
                assemblyTitle + " (Admin)",
                thisAssemblyPath,
                "Revit2025_MinimalTest.Revit2025Command");

            PushButton CCbuttonAdmin = ribbonPanel.AddItem(CCData) as PushButton;
            //CCbuttonAdmin.ToolTip = "Family Auditor for BIM manager";

            CCbuttonAdmin.ToolTip = "Start" + assemblyTitle;
            Icon = Utils.LoadEmbeddedImage("FamilyAuditor.png");
            CCbuttonAdmin.LargeImage = Icon;




        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                AddRibbonPanel(application);

            }
            catch (Exception)
            {
                MessageBox.Show("failed");
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
