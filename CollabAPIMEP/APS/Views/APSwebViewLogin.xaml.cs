using System;
using System.Reflection;
using System.Web;
using System.Windows;

namespace CollabAPIMEP.APS
{
    public partial class WebViewLogin : Window
    {
        #region Fields

        readonly string _callbackUrl;
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

        #endregion

        #region Properties

        public string Code { get; set; }

        #endregion

        #region Constructors

        public WebViewLogin(string url, string callbackUrl, string title = null)
        {
            InitializeComponent();

            _callbackUrl = callbackUrl;

            webView2.InitializeWebAsync().GetAwaiter();

            if (!string.IsNullOrEmpty(title))
            {
                Title = title;
            }

            if (!string.IsNullOrEmpty(url))
            {
                webView2.Source = new Uri(url);
                webView2.NavigationStarting += WebView2_NavigationStarting;
            }

            this.Closing += (s, e) =>
            {
                if (Code is null)
                {
                    tcs.SetException(new Exception("Code is null"));
                }
            };
        }
        #endregion

        #region Methods

        public Task<string> GetCodeAsync()
        {
            return tcs.Task;
        }

        public Task<string> ShowGetCodeAsync()
        {
            Show();
            return GetCodeAsync();
        }

        private void WebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            var splittedUri = e.Uri.Split('?');
            if (splittedUri.First() == _callbackUrl)
            {
                var query = splittedUri.LastOrDefault();
                var querys = HttpUtility.ParseQueryString(query);
                if (querys.Get("code") is string code)
                {
                    Code = code;
                    tcs.SetResult(code);
                    this.Close();
                }
            }

        }

        #endregion

    }
}