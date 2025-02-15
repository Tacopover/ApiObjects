using FamilyAuditorCore.Commands;
using FamilyAuditorCore.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace FamilyAuditorCore.ViewModels
{
    public class DuplicateTypeMultiViewModel : BaseViewModel
    {
        public DuplicateTypeMultiWindow DuplicateMultiWindow;
        public bool IsCanceled { get; set; }

        private System.Windows.Media.ImageSource _closeImage;
        public System.Windows.Media.ImageSource CloseImage
        {
            get { return _closeImage; }
            set
            {
                _closeImage = value;
                OnPropertyChanged(nameof(CloseImage));
            }
        }
        private bool isOkEnabled;
        public bool IsOkEnabled
        {
            get { return isOkEnabled; }
            set
            {
                isOkEnabled = value;
                OnPropertyChanged(nameof(IsOkEnabled));
            }
        }
        private ObservableCollection<DuplicateTypeHandler> duplicateTypeHandlers;
        public ObservableCollection<DuplicateTypeHandler> DuplicateTypeHandlers
        {
            get { return duplicateTypeHandlers; }
            set
            {
                duplicateTypeHandlers = value;
                OnPropertyChanged(nameof(DuplicateTypeHandlers));
            }
        }
        private string _renameSuffix;
        public string RenameSuffix
        {
            get { return _renameSuffix; }
            set
            {
                _renameSuffix = value;
                OnPropertyChanged(nameof(RenameSuffix));
            }
        }


        //private DuplicateTypeHandler _selectedTypeHandler;
        //public DuplicateTypeHandler SelectedTypeHandler
        //{
        //    get { return _selectedTypeHandler; }
        //    set
        //    {
        //        _selectedTypeHandler = value;
        //        SelectedTypeHandlerName = value.NewFamilyName;
        //        OnPropertyChanged(nameof(SelectedTypeHandler));
        //    }
        //}
        //private string _selectedTypeHandlerName;
        //public string SelectedTypeHandlerName
        //{
        //    get
        //    {
        //        if (_selectedTypeHandler == null) return "New_Family_Name";
        //        return _selectedTypeHandlerName;
        //    }
        //    set
        //    {
        //        _selectedTypeHandlerName = value;
        //        OnPropertyChanged(nameof(SelectedTypeHandlerName));
        //    }
        //}

        public RelayCommand<object> ButtonCloseCommand { get; set; }
        public RelayCommand<object> ButtonApplyCommand { get; set; }
        public RelayCommand<object> SuffixTextBoxLeaveCommand { get; set; }


        public DuplicateTypeMultiViewModel(List<DuplicateTypeHandler> dth)
        {
            DuplicateTypeHandlers = new ObservableCollection<DuplicateTypeHandler>(dth);
            RenameSuffix = "1";
            DuplicateMultiWindow = new DuplicateTypeMultiWindow();
            DuplicateMultiWindow.DataContext = this;
            CloseImage = Utils.LoadEmbeddedImage("closeButton.png");
            IsOkEnabled = true;

            ButtonCloseCommand = new RelayCommand<object>(p => true, p => CloseWindow(true));
            ButtonApplyCommand = new RelayCommand<object>(p => true, p => CloseWindow(false));
            SuffixTextBoxLeaveCommand = new RelayCommand<object>(p => true, p => UpdateNewNames());
        }

        private void CloseWindow(bool isCanceled)
        {
            IsCanceled = isCanceled;
            DuplicateMultiWindow.Close();
        }


        private void UpdateNewNames()
        {
            if (RenameSuffix == string.Empty)
            {
                IsOkEnabled = false;
            }
            else
            {
                IsOkEnabled = true;
            }
            //foreach (DuplicateTypeHandler dth in DuplicateTypeHandlers)
            //{
            //    string renameSuffixFixed = RemoveSpecialCharacters(RenameSuffix);
            //    dth.DuplicateViewModel.NewSuffix = renameSuffixFixed;

            //    if (dth.IsCustom)
            //    {
            //        continue;
            //    }
            //    dth.DuplicateViewModel.NewFamilyNameCustom = dth.DuplicateViewModel.NewFamilyNameMulti;
            //    dth.NewFamilyName = dth.DuplicateViewModel.NewFamilyNameCustom;
            //}
        }

        private string RemoveSpecialCharacters(string str)
        {

            string pattern = "[^A-Za-z0-9_-]";
            string replacement = "_";
            Regex regex = new Regex(pattern);
            return regex.Replace(str, replacement);
        }
    }
}
