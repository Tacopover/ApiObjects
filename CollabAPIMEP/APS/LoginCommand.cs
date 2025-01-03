using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CollabAPIMEP.APS
{
    [Transaction(TransactionMode.Manual)]
    public class LoginCommand : IExternalCommand, IExternalCommandAvailability
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {

            MainWindow_APS mainWindow = new MainWindow_APS();
            mainWindow.ShowDialog();
            return Result.Succeeded;
        }

        public bool IsCommandAvailable(UIApplication uiapp, CategorySet selectedCategories)
        {
            return true;
        }
    }
}
