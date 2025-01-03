
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;



namespace CollabAPIMEP.APS
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Window window;
        private readonly APS_Service ApsService;

        public MainViewModel(Window window, APS_Service apsService)
        {
            this.window = window;
            ApsService = apsService;
            User.Name = "Login";

            if (apsService.IsLoggedIn())
            {
                var task = Task.Run(async () =>
                {
                    var userInfo = await ApsService.GetUserInfoAsync();
                    User.Name = userInfo.Name;
                    User.Image = userInfo.Picture;
                });
            }

        }

        public User User { get; set; } = new User();

        [RelayCommand]
        public async Task Login()
        {
            if (User.Image is not null)
            {
                User.Image = null;
                User.Name = "Login";
                await ApsService.Logout();
                return;
            }

            var view = new WebViewLogin(ApsService.Authorize(), ApsService.CallbackUrl, "Login with Autodesk Account.");
            view.Owner = window;
            try
            {
                var code = await view.ShowGetCodeAsync();
                await ApsService.GetPKCEThreeLeggedTokenAsync(code);
                var userInfo = await ApsService.GetUserInfoAsync();
                User.Name = userInfo.Name;
                User.Image = userInfo.Picture;
            }
            catch { }
        }
    }
}