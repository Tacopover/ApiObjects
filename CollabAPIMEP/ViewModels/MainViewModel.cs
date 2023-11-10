using Autodesk.Revit.UI;
using CollabAPIMEP.Commands;
using System.Collections.Generic;

namespace CollabAPIMEP
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UIApplication uiApp;
        private FamilyLoadHandler familyLoadHandler;
        #region properties
        private MainWindow _mainWindow;
        public MainWindow MainWindow
        {
            get
            {
                if (_mainWindow == null)
                {
                    _mainWindow = new MainWindow() { DataContext = this };
                }
                return _mainWindow;
            }
            set
            {
                _mainWindow = value;
                OnPropertyChanged(nameof(MainWindow));
            }
        }

        private string _loaderStateText;
        public string LoaderStateText
        {
            get
            {
                if (_loaderStateText == null)
                {
                    _loaderStateText = "Test";
                }
                return _loaderStateText;
            }
            set
            {
                _loaderStateText = value;
                OnPropertyChanged(nameof(LoaderStateText));
            }
        }

        private List<Rule> _rules;
        public List<Rule> Rules
        {
            get { return _rules; }
            set
            {
                _rules = value;
                OnPropertyChanged(nameof(Rules));
            }
        }


        #endregion

        #region Commands
        public RelayCommand<object> EnableCommand { get; set; }

        #endregion
        public MainViewModel(UIApplication uiapp)
        {
            uiApp = uiapp;
            familyLoadHandler = new FamilyLoadHandler(uiapp);
            Rules = new List<Rule>();
            Rules.Add(new Rule("testrule1"));
            Rules.Add(new Rule("testrule2"));
            Rules.Add(new Rule("testrule3"));

            EnableCommand = new RelayCommand<object>(p => true, p => EnableCommandAction());

            MainWindow.ShowDialog();
        }

        private void EnableCommandAction()
        {
            if (LoaderStateText == "Disabled")
            {
                familyLoadHandler.EnableFamilyLoader();
                LoaderStateText = "Enabled";
            }
            else
            {
                familyLoadHandler.DisableFamilyLoader();
                LoaderStateText = "Disabled";
            }
        }
    }
}
