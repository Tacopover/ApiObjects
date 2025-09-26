using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP.Views;
using CollabAPIMEP;
using Firebase.Auth;
using System.Threading.Tasks;

namespace CollabAPIMEP
{
    [Transaction(TransactionMode.Manual)]

    public class LoginCommand : IExternalCommand, IExternalCommandAvailability
    {


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            string userId =  commandData.Application.Application.LoginUserId;
            string userName = commandData.Application.Application.Username;

            FirebaseHelper firebaseHelper = new FirebaseHelper(userName, userId);

            bool result = Task.Run(async () => await firebaseHelper.Run()).Result;

            if (result)
            {
                return Result.Succeeded;
            }
            else
            {
                message = "Failed to sign in or create user.";
                return Result.Failed;
            }

        }

        public bool IsCommandAvailable(UIApplication uiapp, CategorySet selectedCategories)
        {
            return true;
        }

        public class TestObjectJson
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }   
    }
}
