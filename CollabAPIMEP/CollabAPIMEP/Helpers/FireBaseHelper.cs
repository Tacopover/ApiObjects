using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Threading.Tasks;

namespace CollabAPIMEP
{
    public class FirebaseHelper
    {
        private static readonly string ApiKey = "AIzaSyBD0MN305SNCE7GFNBkvxIL7tu8lYPkrTc";
        private static readonly string DatabaseUrl = "https://familyauditor-f6cbe.firebaseio.com/";
        private readonly FirebaseAuthClient authClient;
        private FirebaseClient firebaseClient;
        private string userID = "";
        private static string passWord = "test";

        public FirebaseHelper(string UserID)
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = ApiKey,
                AuthDomain = "familyauditor-f6cbe.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };

            userID = UserID;
            authClient = new FirebaseAuthClient(config);
        }

        public async Task<UserCredential> CreateUserAsync()
        {
            try
            {
                // Construct an email using the userId
                string email = $"{userID}@apimep.com";

                // Create a new user with email and password
                var userCredential = await authClient.CreateUserWithEmailAndPasswordAsync(email, passWord);
                InitializeFirebaseClient(userCredential);
                return userCredential;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error creating user: {ex.Message}");
                throw;
            }
        }

        public async Task<UserCredential> SignInUserAsync()
        {
            try
            {
                // Construct an email using the userId
                string email = $"{userID}@apimep.com";

                // Sign in an existing user with email and password
                var userCredential = await authClient.SignInWithEmailAndPasswordAsync(email, passWord);
                InitializeFirebaseClient(userCredential);
                return userCredential;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error signing in user: {ex.Message}");
                throw;
            }
        }

        private async Task InitializeFirebaseClient(UserCredential userCredential)
        {
            // Get the ID token asynchronously
            var idToken = await userCredential.User.GetIdTokenAsync();

            // Initialize the FirebaseClient with the ID token
            firebaseClient = new FirebaseClient(
                DatabaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(idToken)
                });
        }

        public async Task SaveDataAsync<T>(string path, T data)
        {
            try
            {
                await firebaseClient
                    .Child(path)
                    .PutAsync(data);
                Console.WriteLine("Data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                throw;
            }
        }
    }
}