using System;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;

namespace CollabAPIMEP.Views
{
    public partial class WebView2Page : Page
    {
        private APS _aps;
        private const string TokenFilePath = "tokens.json";

        public WebView2Page(APS aps)
        {
            InitializeComponent();
            this.Dispatcher.Invoke(async () => await InitializeWebAsync());
            this.DataContext = ViewModel.Instance;
            _aps = aps; // Use the injected APS instance
        }

        private async Task InitializeWebAsync()
        {
            try
            {
                await webView.EnsureCoreWebView2Async();
                var authUrl = _aps.GetAuthorizationURL();
                webView.CoreWebView2.Navigate(authUrl);
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
            }
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var uri = new Uri(webView.Source.ToString());
            if (uri.AbsolutePath == "/callback") // Adjust this to match your callback path
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var code = query["code"];
                if (!string.IsNullOrEmpty(code))
                {
                    HandleOAuthCallback(code);
                }
            }
        }

        private async void HandleOAuthCallback(string code)
        {
            var tokens = await _aps.GenerateTokens(code);
            SaveTokens(tokens);
            System.Windows.MessageBox.Show("Authentication successful!");
        }

        private void SaveTokens(Tokens tokens)
        {
            var json = JsonSerializer.Serialize(tokens);
            File.WriteAllText(TokenFilePath, json);
        }

        private Tokens LoadTokens()
        {
            if (File.Exists(TokenFilePath))
            {
                var json = File.ReadAllText(TokenFilePath);
                return JsonSerializer.Deserialize<Tokens>(json);
            }
            return null;
        }

        private async Task<Tokens> RefreshTokensIfNeeded(Tokens tokens)
        {
            if (tokens.ExpiresAt < DateTime.Now.ToUniversalTime())
            {
                tokens = await _aps.RefreshTokens(tokens);
                SaveTokens(tokens);
            }
            return tokens;
        }
    }
}