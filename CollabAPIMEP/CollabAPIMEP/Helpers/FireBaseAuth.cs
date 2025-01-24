using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace CollabAPIMEP.Helpers
{
    public class FireBaseAuth
    {
        string firebaseApiKey = "AIzaSyA_TSkVyNfGfmrl1REcKJLkZ84EEY4kAbQ";
        var vm = new LogInViewModel(uiApp, new FirebaseAuthProvider(new FirebaseConfig(firebaseApiKey)));

    }
    public class LoginViewModel
    {
        private readonly FirebaseAuthProvider _firebaseAuthProvider;
        private AuthenticationStore _authenticationStore;

        private LogIn solidView;
        public LogIn View
        {
            get
            {
                if (solidView == null)
                {
                    solidView = new LogIn() { DataContext = this };
                }
                return solidView;
            }
            set
            {
                solidView = value;
                OnPropertyChanged(nameof(View));
            }
        }

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string _username;
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get
            {
                return _confirmPassword;
            }
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        private Visibility _isLogInButtonVisible;
        public Visibility IsLogInButtonVisible
        {
            get
            {
                return _isLogInButtonVisible;
            }
            set
            {
                _isLogInButtonVisible = value;
                OnPropertyChanged(nameof(IsLogInButtonVisible));
            }
        }

        private Visibility _isLogOutButtonVisible;
        public Visibility IsLogOutButtonVisible
        {
            get
            {
                return _isLogOutButtonVisible;
            }
            set
            {
                _isLogOutButtonVisible = value;
                OnPropertyChanged(nameof(IsLogOutButtonVisible));
            }
        }

        public ICommand SubmitCommand { get; }
        public ICommand NavigateLoginCommand { get; }
        public RelayCommand<object> ButtonLogOut { get; set; }

        public LogInViewModel(UIApplication uiApp, FirebaseAuthProvider firebaseAuthProvider) : base(uiApp)
        {
            _firebaseAuthProvider = firebaseAuthProvider;
            _authenticationStore = new AuthenticationStore(_firebaseAuthProvider);
            ButtonLogOut = new RelayCommand<object>(p => true, p => OnButtonLogOut());
        }


        protected override async void OnButtonRun()
        {
            try
            {
                //FirebaseAuthLink firebaseAuthLink = await _firebaseAuthProvider.SignInWithEmailAndPasswordAsync("mare2897@gmail.com", "Test123!");
                //var name = firebaseAuthLink.User.DisplayName;
                await _authenticationStore.Login(Email, Password);

                OnLoggedIn();
            }
            catch (Exception)
            {
                MessageBox.Show("Login failed. Please check your information or try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLoggedIn()
        {
            LoginState.IsLoggedIn = true;
            MessageBox.Show("Successfully logged in!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Email = _authenticationStore.CurrentUser.Email;
            Username = _authenticationStore.CurrentUser.DisplayName;
            IsLogInButtonVisible = Visibility.Hidden;
            IsLogOutButtonVisible = Visibility.Visible;
            //this.View.Close();
        }

        private void OnButtonLogOut()
        {
            MessageBox.Show("Logged out.");
            _authenticationStore.Logout();
            LoginState.IsLoggedIn = false;
            IsLogInButtonVisible = Visibility.Visible;
            IsLogOutButtonVisible = Visibility.Hidden;
        }

        protected override void Initialize()
        {
            if (LoginState.IsLoggedIn)
            {
                IsLogInButtonVisible = Visibility.Hidden;
                IsLogOutButtonVisible = Visibility.Visible;
            }
            else
            {
                IsLogInButtonVisible = Visibility.Visible;
                IsLogOutButtonVisible = Visibility.Hidden;
            }
        }
    }
}

