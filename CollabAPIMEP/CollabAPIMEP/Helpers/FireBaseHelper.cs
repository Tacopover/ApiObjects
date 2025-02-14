using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CollabAPIMEP
{
    public class FirebaseHelper
    {
        private static readonly string ApiKey = "AIzaSyBD0MN305SNCE7GFNBkvxIL7tu8lYPkrTc";
        private static readonly string DatabaseUrl = "https://familyauditor-f6cbe-default-rtdb.europe-west1.firebasedatabase.app/";
        private readonly FirebaseAuthClient authClient;
        public FirebaseClient firebaseClient;
        public UserCredential UserCredential { get; set; }
        private string userName = "";
        private string userID = "";
        private static string passWord = "wijwillengraageenuseraanmaken";

        public FirebaseHelper(string userName, string userID)
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

            this.userID = userID;
            this.userName = userName;
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
                await InitializeFirebaseClient();
                userCredential.User.Info.DisplayName = userName;
                userCredential.User.Info.Email = email;

                return userCredential;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error creating user: {ex.Message}");
                throw;
            }
        }

        public async Task SignInUserAsync()
        {
            try
            {
                // Construct an email using the userId
                string email = $"{userID}@apimep.com";

                // Sign in an existing user with email and password
                UserCredential = await authClient.SignInWithEmailAndPasswordAsync(email, passWord);
                await InitializeFirebaseClient();

            }
            catch (FirebaseAuthHttpException ex)
            {
                // Handle FirebaseAuthHttpException
                Console.WriteLine($"FirebaseAuthHttpException: {ex.Message}");
                UserCredential = null;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Error signing in user: {ex.Message}");
                UserCredential = null;
            }
        }

        private async Task InitializeFirebaseClient()
        {
            // Get the ID token asynchronously
            var idToken = await UserCredential.User.GetIdTokenAsync();

            // Initialize the FirebaseClient with the ID token
            firebaseClient = new FirebaseClient(
                DatabaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(idToken)
                });
        }

        public async Task SaveDataAsync<T>(T data)
        {
            try
            {
                await firebaseClient
                    .Child(userID)
                    .PutAsync(data);
                Console.WriteLine("Data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                throw;
            }
        }

        public async Task SaveJsonAsync(object data)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(data);
                await SaveDataAsync(jsonData);
                Console.WriteLine("JSON data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving JSON data: {ex.Message}");
                throw;
            }
        }

        public async Task<T> LoadJsonAsync<T>()
        {
            try
            {
                var jsonData = await firebaseClient
                    .Child(userID)
                    .OnceSingleAsync<string>();

                T data = JsonConvert.DeserializeObject<T>(jsonData);
                Console.WriteLine("JSON data loaded successfully.");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading JSON data: {ex.Message}");
                throw;
            }
        }
    }
}