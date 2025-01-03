using System;
using System.Windows;
using System.Windows.Input;

namespace CollabAPIMEP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            //fix for weird bug with xaml behaviors nuget
            var _ = new Microsoft.Xaml.Behaviors.DefaultTriggerAttribute(typeof(Trigger), typeof(Microsoft.Xaml.Behaviors.TriggerBase), null);
            //useless comment
            InitializeComponent();
        }

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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numeric input
            if (e.Text.StartsWith("0") && e.Text.Length > 1)
            {
                e.Handled = false;
                return;
            }

            e.Handled = !int.TryParse(e.Text, out _);
        }


        public void Dispose()
        {
            Close();
        }
    }
}
