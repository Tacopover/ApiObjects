using System.IO;

namespace CollabAPIMEP.APS
{
    public static class WebView2Utils
    {
        public static async Task InitializeWebAsync(this Microsoft.Web.WebView2.Wpf.WebView2 webView)
        {
            var userDataFolderName = typeof(WebView2Utils).Assembly.GetName().Name;
            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), userDataFolderName);

            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                userDataFolder: userDataFolder
            );

            await webView.EnsureCoreWebView2Async(env);
        }
    }
}