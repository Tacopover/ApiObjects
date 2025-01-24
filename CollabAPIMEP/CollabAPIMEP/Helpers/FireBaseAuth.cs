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
        private static readonly string ApiKey = "YOUR_FIREBASE_API_KEY";
        private static readonly string DatabaseUrl = "https://YOUR_FIREBASE_PROJECT_ID.firebaseio.com/";
        private readonly FirebaseAuthClient authClient;
        private FirebaseClient firebaseClient;
        private string userID = "";
        private static string passWord = "test";

        public FirebaseHelper(string UserID)
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = ApiKey,
                AuthDomain = "YOUR_FIREBASE_PROJECT_ID.firebaseapp.com",
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

        private void InitializeFirebaseClient(UserCredential userCredential)
        {
            firebaseClient = new FirebaseClient(
                DatabaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(userCredential.FirebaseToken)
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