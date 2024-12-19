using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CollabAPIMEP.Views
{
    public class ViewModel
    {
        public static ViewModel Instance { get; } = new ViewModel();
        public Uri Uri { get; set; } = new Uri("https://www.youtube.com/watch?v=xvFZjo5PgG0");
    }

    public static class WebView2Utils
    {
        private const string UserDataFolder = "CollabAPIMEP";

        public static async Task InitializeWebAsync(this Microsoft.Web.WebView2.Wpf.WebView2 webView)
        {
            try
            {



                var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), UserDataFolder);

                var env = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: userDataFolder
                );

                await webView.EnsureCoreWebView2Async(env);
                webView.WebMessageReceived += (sender, args) =>
                {
                    Console.WriteLine(args.TryGetWebMessageAsString());
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
