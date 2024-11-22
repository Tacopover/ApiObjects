using FamilyAuditorCore.Commands;
using FamilyAuditorCore.ViewModels;
using FamilyAuditorCore.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FamilyAuditorCore.ViewModels
{
    public class DuplicateTypeViewModel : BaseViewModel
    {
        public DuplicateTypeWindow DuplicateTypeWindow;
        private SolidColorBrush enabledColour = new SolidColorBrush(Colors.CornflowerBlue);
        private SolidColorBrush disabledColour = new SolidColorBrush(Colors.LightGray);

        public bool IsCanceled { get; set; }

        #region properties
        private bool isReplaceChecked;
        public bool IsReplaceChecked
        {
            get { return isReplaceChecked; }
            set
            {
                isReplaceChecked = value;
                IsReplaceEnabled = value;
                if (value)
                {
                    IsRenameChecked = false;
                    ReplaceColour = enabledColour;
                    RenameColour = disabledColour;
                    ResolveAction = "Replace";
                }
                OnPropertyChanged(nameof(IsReplaceChecked));
            }
        }
        private bool isRenameChecked;
        public bool IsRenameChecked
        {
            get { return isRenameChecked; }
            set
            {
                isRenameChecked = value;
                IsRenameEnabled = value;
                if (value)
                {
                    IsReplaceChecked = false;
                    RenameColour = enabledColour;
                    ReplaceColour = disabledColour;
                    ResolveAction = "Rename";
                }
                OnPropertyChanged(nameof(IsRenameChecked));
            }
        }
        private bool isReplaceEnabled;
        public bool IsReplaceEnabled
        {
            get { return isReplaceEnabled; }
            set
            {
                isReplaceEnabled = value;
                OnPropertyChanged(nameof(IsReplaceEnabled));
            }
        }
        private bool isRenameEnabled;
        public bool IsRenameEnabled
        {
            get { return isRenameEnabled; }
            set
            {
                isRenameEnabled = value;
                OnPropertyChanged(nameof(IsRenameEnabled));
            }
        }
        private string existingFamilyName;
        public string ExistingFamilyName
        {
            get { return existingFamilyName; }
            set
            {
                existingFamilyName = value;
                IsOkEnabled = CanExecuteOkCommand();
                OnPropertyChanged(nameof(ExistingFamilyName));
            }
        }
        private string newFamilyName;
        public string NewFamilyName
        {
            get { return newFamilyName; }
            set
            {
                newFamilyName = value;
                IsOkEnabled = CanExecuteOkCommand();
                OnPropertyChanged(nameof(NewFamilyName));
            }
        }
        private string _newFamilyNameShort;
        public string NewFamilyNameShort
        {
            get { return _newFamilyNameShort; }
            set
            {
                _newFamilyNameShort = value;
                OnPropertyChanged(nameof(NewFamilyNameShort));
            }
        }
        private string _newSuffix;
        public string NewSuffix
        {
            get { return _newSuffix; }
            set
            {
                _newSuffix = value;
                NewFamilyNameMulti = NewFamilyNameShort + NewSuffix;
                OnPropertyChanged(nameof(NewSuffix));
            }
        }
        private string _newFamilyNameCustom;
        public string NewFamilyNameCustom
        {
            get { return _newFamilyNameCustom; }
            set
            {
                _newFamilyNameCustom = value;
                OnPropertyChanged(nameof(NewFamilyNameCustom));
            }
        }
        public string NewFamilyNameOriginal { get; set; }
        public string NewFamilyNameMulti { get; set; }
        private bool replaceExistingChecked;
        public bool ReplaceExistingChecked
        {
            get
            {
                return replaceExistingChecked;
            }
            set
            {
                replaceExistingChecked = value;
                if (value)
                {
                    ReplaceNewChecked = false;
                    if (Column2Name == "Existing Family")
                    {
                        Column2Name = "New Family";
                        Column1Name = "Existing Family";
                        SetMapping();
                    }
                }
                OnPropertyChanged(nameof(ReplaceExistingChecked));
            }
        }
        private bool? replaceNewChecked;
        public bool ReplaceNewChecked
        {
            get
            {
                if (!replaceNewChecked.HasValue)
                {
                    replaceNewChecked = true;
                }
                return (bool)replaceNewChecked;
            }
            set
            {
                replaceNewChecked = value;
                if (value)
                {
                    ReplaceExistingChecked = false;
                    if (Column1Name == "Existing Family")
                    {
                        Column1Name = "New Family";
                        Column2Name = "Existing Family";
                        SetMapping();
                    }
                }
                OnPropertyChanged(nameof(ReplaceNewChecked));
            }
        }

        private SolidColorBrush replaceColour;
        public SolidColorBrush ReplaceColour
        {
            get
            {
                if (replaceColour == null)
                {
                    replaceColour = disabledColour;
                }
                return replaceColour;
            }
            set
            {
                replaceColour = value;
                OnPropertyChanged(nameof(ReplaceColour));
            }
        }
        private SolidColorBrush renameColour;
        public SolidColorBrush RenameColour
        {
            get
            {
                if (renameColour == null)
                {
                    renameColour = enabledColour;
                }
                return renameColour;
            }
            set
            {
                renameColour = value;
                OnPropertyChanged(nameof(RenameColour));
            }
        }
        private string column1Name;
        public string Column1Name
        {
            get
            {
                if (column1Name == null)
                {
                    column1Name = "New Family";
                }
                return column1Name;
            }
            set
            {
                column1Name = value;
                OnPropertyChanged(nameof(Column1Name));
            }
        }
        private string column2Name;
        public string Column2Name
        {
            get
            {
                if (column2Name == null)
                {
                    column2Name = "Existing Family";
                }
                return column2Name;
            }
            set
            {
                column2Name = value;
                OnPropertyChanged(nameof(Column2Name));
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
        private ImageSource _closeImage;
        public ImageSource CloseImage
        {
            get { return _closeImage; }
            set
            {
                _closeImage = value;
                OnPropertyChanged(nameof(CloseImage));
            }
        }
        ObservableCollection<string> ListNew;
        ObservableCollection<string> ListExisting;

        private ObservableCollection<string> column2List;
        public ObservableCollection<string> Column2List
        {
            get { return column2List; }
            set
            {
                if (column2List != value)
                {
                    column2List = value;
                    OnPropertyChanged(nameof(Column2List));
                }
            }
        }

        private bool lostMouseOverExisting;
        public bool LostMouseOverExisting
        {
            get { return lostMouseOverExisting; }
            set
            {
                lostMouseOverExisting = value;
                if (value)
                {
                    CanExecuteOkCommand();
                }
                OnPropertyChanged(nameof(LostMouseOverExisting));
            }
        }
        private bool lostMouseOverNew;
        public bool LostMouseOverNew
        {
            get { return lostMouseOverNew; }
            set
            {
                lostMouseOverNew = value;
                if (value)
                {
                    CanExecuteOkCommand();
                }
                OnPropertyChanged(nameof(LostMouseOverNew));
            }
        }
        public ObservableCollection<string> Column1List { get; set; }
        private ObservableCollection<Mapping> mappings;
        public ObservableCollection<Mapping> Mappings
        {
            get { return mappings; }
            set
            {
                mappings = value;
                OnPropertyChanged(nameof(Mappings));
            }
        }

        private string _resolveAction;
        public string ResolveAction
        {
            get { return _resolveAction; }
            set
            {
                _resolveAction = value;
                OnPropertyChanged(nameof(ResolveAction));
            }
        }

        private SolidColorBrush _resolveColour;
        public SolidColorBrush ResolveColour
        {
            get
            {
                if (_resolveColour == null)
                {
                    _resolveColour = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x03, 0xFF));
                }
                return _resolveColour;
            }
            set
            {
                _resolveColour = value;
                OnPropertyChanged(nameof(ResolveColour));
            }
        }

        #endregion

        public RelayCommand<object> LostMouseCommand { get; set; }
        public RelayCommand<object> ButtonCloseCommand { get; set; }



        public DuplicateTypeViewModel()
        {
            DuplicateTypeWindow = new DuplicateTypeWindow();
            DuplicateTypeWindow.DataContext = this;
            IsRenameChecked = true;
            CloseImage = Utils.LoadEmbeddedImage("closeButton.png");

            LostMouseCommand = new RelayCommand<object>(p => true, p => LostMouse());
            ButtonCloseCommand = new RelayCommand<object>(p => true, p => CloseWindow());
        }

        public void ShowWindow()
        {
            DuplicateTypeWindow = new DuplicateTypeWindow();
            DuplicateTypeWindow.DataContext = this;
            DuplicateTypeWindow.ShowDialog();
        }

        private void SetMapping()
        {
            if (ReplaceExistingChecked)
            {
                Column1List = ListExisting;
                Column2List = ListNew;
            }
            else
            {
                Column1List = ListNew;
                Column2List = ListExisting;
            }
            Mappings = new ObservableCollection<Mapping>();
            for (int i = 0; i < Column1List.Count; i++)
            {
                string item2 = i < Column2List.Count ? Column2List[i] : string.Empty;
                Mappings.Add(new Mapping(Column1List, Column2List) { Item1 = Column1List[i], Item2 = item2 });
            }
        }

        private void CloseWindow()
        {
            DuplicateTypeWindow.Close();
            IsCanceled = true;
        }
        public void CreateMapping(List<string> list1, List<string> list2)
        {
            // check if there is a family in the new family list with ' 2' at the. If that is also in the exising famnilies do nothing
            // otherwise rename the ' 2' family and add the family to the existing families
            ListNew = new ObservableCollection<string>(list1);
            ListExisting = new ObservableCollection<string>(list2);
            SetMapping();
        }


        private void LostMouse()
        {
            CanExecuteOkCommand();
        }
        private bool CanExecuteOkCommand()
        {
            if (IsRenameChecked)
            {
                if (string.IsNullOrEmpty(NewFamilyName) || string.IsNullOrEmpty(ExistingFamilyName))
                {
                    return false;
                }
                if (NewFamilyName == ExistingFamilyName)
                {
                    return false;
                }
            }
            return true;
        }
        public class Mapping_old
        {
            public string Item1 { get; set; }
            public string Item2 { get; set; }
            public ObservableCollection<string> List1 { get; set; }
            public ObservableCollection<string> List2 { get; set; }

            public Mapping_old(ObservableCollection<string> list1, ObservableCollection<string> list2)
            {
                List1 = list1;
                List2 = list2;
            }

            public void SwapLists()
            {
                string tempItem = Item1;
                Item1 = Item2;
                Item2 = tempItem;
                var tempList = List1;
                List1 = List2;
                List2 = tempList;
            }
        }

        public class Mapping : INotifyPropertyChanged
        {
            private string item1;
            public string Item1
            {
                get { return item1; }
                set
                {
                    item1 = value;
                    OnPropertyChanged(nameof(Item1));
                }
            }

            private string item2;
            public string Item2
            {
                get { return item2; }
                set
                {
                    item2 = value;
                    OnPropertyChanged(nameof(Item2));
                }
            }

            public ObservableCollection<string> List1 { get; set; }
            public ObservableCollection<string> List2 { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public Mapping(ObservableCollection<string> list1, ObservableCollection<string> list2)
            {
                List1 = list1;
                List2 = list2;
            }

            public void SwapLists()
            {
                string tempItem = Item1;
                Item1 = Item2;
                Item2 = tempItem;
                //var tempList = List1;
                //List1 = List2;
                //List2 = tempList;
            }

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
