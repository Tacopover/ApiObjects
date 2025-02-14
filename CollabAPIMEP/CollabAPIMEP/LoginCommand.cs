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

            Task.Run(async () =>
            {


                // Attempt to sign in the user
                await firebaseHelper.SignInUserAsync();


                if (firebaseHelper.UserCredential == null)
                {
                    try
                    {
                        // If sign-in fails, attempt to create a new user
                        await firebaseHelper.CreateUserAsync();
                    }
                    catch (FirebaseAuthException ex)
                    {
                        // Handle user creation failure
                    }
                }

                object testObject = new TestObjectJson
                {
                    Name = "Test",
                    Description = "This is a test object"
                };

                await firebaseHelper.SaveJsonAsync(testObject);

                var settingsFile = await firebaseHelper.LoadJsonAsync<object>();

            }).Wait();





            return Result.Succeeded;
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
