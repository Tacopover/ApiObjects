﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;


namespace FamilyAuditorCore.Views
{
    public partial class DuplicateTypeMultiWindow : Window
    {
        public DuplicateTypeMultiWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}
