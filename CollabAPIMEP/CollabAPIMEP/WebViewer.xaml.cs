using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace CollabAPIMEP
{
    /// <summary>
    /// Interaction logic for WebViewer.xaml
    /// </summary>
    public partial class WebViewer : Window, IDisposable
    {

        public WebViewer()
        {
            InitializeComponent();
            this.Loaded += WebViewer_Loaded;
        }

        private async void WebViewer_Loaded(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate("https://www.google.com");
        }

        public string Code { get; internal set; }


        private void Window_Activated(object sender, System.EventArgs e)
        {
            MainViewModel viewModel = DataContext as MainViewModel;
            var results = viewModel.Results;
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                // Get the working area of the screen (excluding the taskbar)
                var workingArea = System.Windows.SystemParameters.WorkArea;

                // Adjust the window size to fit within the working area
                MaxHeight = workingArea.Height;

                WindowState = WindowState.Maximized;
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }



        public void Dispose()
        {
            Close();
        }
    }
}
