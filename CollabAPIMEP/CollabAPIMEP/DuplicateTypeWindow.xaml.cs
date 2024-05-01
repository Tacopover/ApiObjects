using CollabAPIMEP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CollabAPIMEP
{
    public sealed partial class DuplicateTypeWindow : Window
    {
        public DuplicateTypeViewModel dtViewModel;
        public string Result { get; set; }
        public DuplicateTypeWindow()
        {
            InitializeComponent();
            dtViewModel = new DuplicateTypeViewModel(this);
            DataContext = dtViewModel;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            dtViewModel.IsCanceled = true;
            Close();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
