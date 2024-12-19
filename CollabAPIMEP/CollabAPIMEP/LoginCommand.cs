using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CollabAPIMEP
{
    [Transaction(TransactionMode.Manual)]
    public class LoginCommand : IExternalCommand, IExternalCommandAvailability
    {
        private static PageView window;
        private static ServiceProvider serviceProvider;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            if (serviceProvider == null)
            {
                serviceProvider = ServiceConfigurator.ConfigureServices();
            }

            var aps = serviceProvider.GetService<APS>();

            if (window is null)
            {
                window = new PageView(new WebView2Page(aps)); // Pass the APS instance
                window.Closed += (s, e) => { window = null; };
                window.Show();
            }
            window?.Activate();

            return Result.Succeeded;
        }

        public bool IsCommandAvailable(UIApplication uiapp, CategorySet selectedCategories)
        {
            return true;
        }
    }
}
