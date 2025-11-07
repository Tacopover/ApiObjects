using System;
using System.Windows;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;


namespace Cascad.UI
{
    public partial class FeedbackWindow : Window
    {
        private readonly Action<string> _onSubmit;

        public ImageSource LogoSource { get; }
        public string ErrorDetails { get; }

        // Added errorDetails parameter (optional)
        public FeedbackWindow(ImageSource logoSource, Action<string> onSubmit = null, string errorDetails = null)
        {
            InitializeComponent();
            LogoSource = logoSource;
            _onSubmit = onSubmit;
            ErrorDetails = errorDetails ?? string.Empty;
            DataContext = this;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string text = WhatHappenedTextBox?.Text?.Trim() ?? string.Empty;

            try
            {
                _onSubmit?.Invoke(text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Submit handler error: " + ex);
                MessageBox.Show(this, "Failed to submit crash report.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(this, "Crash report submitted. Thank you!", "Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyError_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(ErrorDetailsTextBox?.Text ?? string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CopyError failed: " + ex);
            }
        }
    }
}