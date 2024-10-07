using CollabAPIMEP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;


namespace CollabAPIMEP.Views
{
    public sealed partial class DuplicateTypeWindow : Window
    {
        public string Result { get; set; }
        public DuplicateTypeWindow()
        {
            InitializeComponent();
            //dtViewModel = new DuplicateTypeViewModel(this);
            //DataContext = dtViewModel;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
