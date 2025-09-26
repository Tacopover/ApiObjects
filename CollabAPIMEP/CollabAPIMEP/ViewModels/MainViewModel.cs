using Autodesk.Revit.UI;
using CollabAPIMEP.Commands;
using CollabAPIMEP.Helpers;
using FamilyAuditorCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Rule = FamilyAuditorCore.Rule;


namespace CollabAPIMEP
{
    public class MainViewModel : BaseViewModel
    {
        //CONSTANTS
        readonly String USERWARNING = "User Mode: if you want to change a rule, please contact an admin";

        private SettingsManager _settingsManager;
        public bool IsWindowClosed { get; set; } = true;

        #region images
        private System.Windows.Media.ImageSource _minimizeImage;
        public System.Windows.Media.ImageSource MinimizeImage
        {
            get { return _minimizeImage; }
            set
            {
                _minimizeImage = value;
                OnPropertyChanged(nameof(MinimizeImage));
            }
        }
        private System.Windows.Media.ImageSource _maximizeImage;
        public System.Windows.Media.ImageSource MaximizeImage
        {
            get { return _maximizeImage; }
            set
            {
                _maximizeImage = value;
                OnPropertyChanged(nameof(MaximizeImage));
            }
        }
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
        public ImageSource MepOverLogo { get; set; }

        #endregion

        #region properties
        public RequestHandler Handler { get; set; }
        public ExternalEvent ExternalEvent { get; set; }

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
        private FamilyLoadHandler _familyLoadHandler;
        public FamilyLoadHandler FamLoadHandler
        {
            get { return _familyLoadHandler; }
            set
            {
                _familyLoadHandler = value;
                OnPropertyChanged(nameof(FamLoadHandler));
            }
        }

        private string _loadingStateText;
        public string LoadingStateText
        {
            get
            {
                if (_loadingStateText == null)
                {
                    _loadingStateText = "Disabled";
                }
                return _loadingStateText;
            }
            set
            {
                _loadingStateText = value;
                OnPropertyChanged(nameof(LoadingStateText));
            }
        }
        //private string _loaderStateText;
        //public string LoaderStateText
        //{
        //    get
        //    {
        //        if (_loaderStateText == null)
        //        {
        //            _loaderStateText = "Disabled";
        //        }
        //        return _loaderStateText;
        //    }
        //    set
        //    {
        //        _loaderStateText = value;
        //        OnPropertyChanged(nameof(LoaderStateText));
        //    }
        //}

        private ObservableCollection<Rule> _rules;
        public ObservableCollection<Rule> Rules
        {
            get { return _rules; }
            set
            {
                if (_rules != value)
                {
                    _rules = value;
                    OnPropertyChanged(nameof(Rules));
                    FamLoadHandler.RulesHost.Rules = Rules.ToList();
                }
            }
        }

        private Rule _selectedRule;
        public Rule SelectedRule
        {
            get { return _selectedRule; }
            set
            {
                _selectedRule = value;
                OnPropertyChanged(nameof(SelectedRule));
                RuleDescription = _selectedRule.Description;
            }
        }


        private string _ruleDescription;
        public string RuleDescription
        {
            get
            {
                if (_ruleDescription == null)
                {
                    _ruleDescription = "Select a rule to view its description";
                }
                return _ruleDescription;
            }
            set
            {
                _ruleDescription = value;
                OnPropertyChanged(nameof(RuleDescription));
            }
        }

        private int _countElement;
        public int CountElement
        {
            get { return _countElement; }
            set
            {
                _countElement = value;
                OnPropertyChanged(nameof(CountElement));
            }
        }

        private bool _isLoaderEnabled;
        public bool IsLoaderEnabled
        {
            get { return _isLoaderEnabled; }
            set
            {
                if (value == _isLoaderEnabled)
                {
                    return;
                }
                _isLoaderEnabled = value;
                if (_isLoaderEnabled)
                {
                    LoadingStateText = "Enabled";
                    EnabledColour = new SolidColorBrush(Colors.Green);
                    FamLoadHandler.RulesHost.IsEnabled = true;
                    //FamLoadHandler.IsLoaderEnabled = true;
                    MakeRequest(RequestId.EnableLoading);
                }
                else
                {
                    LoadingStateText = "Disabled";
                    EnabledColour = new SolidColorBrush(Colors.CornflowerBlue);
                    FamLoadHandler.RulesHost.IsEnabled = false;
                    //FamLoadHandler.IsLoaderEnabled = false;
                    MakeRequest(RequestId.DisableLoading);
                }
                OnPropertyChanged(nameof(IsLoaderEnabled));
            }
        }

        private SolidColorBrush _enabledColour;
        public SolidColorBrush EnabledColour
        {
            get
            {
                if (_enabledColour == null)
                {
                    _enabledColour = new SolidColorBrush(Colors.CornflowerBlue);
                }
                return _enabledColour;
            }
            set
            {
                _enabledColour = value;
                OnPropertyChanged(nameof(EnabledColour));
            }
        }

        private ObservableCollection<string> _results;
        public ObservableCollection<string> Results
        {
            get { return new ObservableCollection<string>(FamLoadHandler.Results); }
            set
            {
                _results = value;
                OnPropertyChanged(nameof(Results));
            }
        }

        private bool _isAdminEnabled;
        public bool IsAdminEnabled
        {
            get { return _isAdminEnabled; }
            set
            {
                _isAdminEnabled = value;
                OnPropertyChanged(nameof(IsAdminEnabled));
            }
        }

        private string _userText;
        public string UserText
        {
            get { return _userText; }
            set
            {
                _userText = value;
                OnPropertyChanged(nameof(UserText));
            }
        }

        private string _docTitle;
        public string DocTitle
        {
            get { return _docTitle; }
            set
            {
                _docTitle = value;
                OnPropertyChanged(nameof(DocTitle));
            }
        }

        #endregion

        #region Commands
        public RelayCommand<object> EnableLoadingCommand { get; set; }
        public RelayCommand<object> EnableLoaderCommand { get; set; }
        public RelayCommand<object> SaveCommand { get; set; }
        public RelayCommand<object> UpdateRulesCommand { get; set; }
        public RelayCommand<object> EnableUpdaterCommand { get; set; }
        public RelayCommand<object> DisableUpdaterCommand { get; set; }

        #endregion
        public MainViewModel(FamilyLoadHandler familyLoadHandler, SettingsManager settingsManager)
        {

            _familyLoadHandler = familyLoadHandler;
            _settingsManager = settingsManager;
#if ADMIN
            IsAdminEnabled = true;
            UserText = string.Empty;
#else
            UserText = USERWARNING;
#endif

            if (Handler == null)
            {
                SetHandlerAndEvent();
            }

            if (FamLoadHandler.RulesHost.IsEnabled)
            {
                IsLoaderEnabled = true;
            }
            FamLoadHandler.EnableUpdater();

            //event handlers removed and always enabled
            EnableLoadingCommand = new RelayCommand<object>(p => true, p => ToggleFamilyLoadingAction());
            SaveCommand = new RelayCommand<object>(p => true, p => SaveAction());
            UpdateRulesCommand = new RelayCommand<object>(p => true, p => UpdateRules());
            EnableUpdaterCommand = new RelayCommand<object>(p => true, p => EnableUpdater());
            DisableUpdaterCommand = new RelayCommand<object>(p => true, p => DisableUpdater());

            MinimizeImage = Utils.LoadEmbeddedImage("minimizeButton.png");
            MaximizeImage = Utils.LoadEmbeddedImage("maximizeButton.png");
            CloseImage = Utils.LoadEmbeddedImage("closeButton.png");
            MepOverLogo = Utils.LoadEmbeddedImage("Mepover logo long.png");

            FamLoadHandler.DocTitleChanged += FamLoadHandler_DocTitleChanged;
            FamLoadHandler.RulesHostChanged += FamLoadHandler_RulesHostChanged;

            Rules = new ObservableCollection<Rule>(FamLoadHandler.RulesHost.Rules);
            Results = new ObservableCollection<string>();
        }


        private void FamLoadHandler_RulesHostChanged(object sender, EventArgs e)
        {
            //when a document changes the ruleshost gets set in the FamilyLoadHandler. That is the only occasion that the FamilyLoadHandler
            //signals a change in rules to this viewmodel
            Rules = new ObservableCollection<Rule>(FamLoadHandler.RulesHost.Rules);
            IsLoaderEnabled = FamLoadHandler.RulesHost.IsEnabled;

        }

        private void FamLoadHandler_DocTitleChanged(object sender, EventArgs e)
        {
            DocTitle = FamLoadHandler.DocTitle;
        }

        public void ShowMainWindow(IntPtr mainWindowHandle)
        {
            if (IsWindowClosed)
            {
                MainWindow = new MainWindow() { DataContext = this };
                WindowInteropHelper helper = new WindowInteropHelper(MainWindow);
                helper.Owner = mainWindowHandle;
                SetHandlerAndEvent();
                MainWindow.Show();
                IsWindowClosed = false;
                MainWindow.Closed += MainWindow_Closed;
            }
            else
            {
                MainWindow.Activate();
            }
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SimpleLog.Info("Main Window Closing");
            if (ExternalEvent != null)
            {
                ExternalEvent.Dispose();
                ExternalEvent = null;
            }

            Handler = null;
            IsWindowClosed = true;
            MainWindow.Closed -= MainWindow_Closed;
            //event handlers removed and always enabled
        }

        public void SetHandlerAndEvent()
        {
            RequestMethods helperMethods = new RequestMethods(FamLoadHandler);
            Handler = new RequestHandler(FamLoadHandler, helperMethods);
            ExternalEvent = ExternalEvent.Create(Handler);
        }

        public void RemoveHandlerAndEvent()
        {
            Handler = null;
            ExternalEvent = null;
        }

        private void ToggleFamilyLoadingAction()
        {
            if (IsLoaderEnabled)
            {
                IsLoaderEnabled = false;
            }
            else
            {
                IsLoaderEnabled = true;
            }
        }

        private void SaveAction()
        {
            FamLoadHandler.SerializeRules();
            _settingsManager.SaveRulesLocal(FamLoadHandler.RulesJson);
            _settingsManager.SaveRulesOnline(FamLoadHandler.RulesJson);
            MakeRequest(RequestId.SaveRules);
        }

        private void EnableUpdater()
        {
            MakeRequest(RequestId.EnableUpdater);
        }
        private void DisableUpdater()
        {
            MakeRequest(RequestId.DisableUpdater);
        }

        private void UpdateRules()
        {
            string donothing = "do nothing";
            //foreach (Rule rule in Rules)
            //{
            //    FamLoadHandler.RulesHost.GetRule(rule.TypeOfRule).IsEnabled = rule.IsEnabled;
            //    FamLoadHandler.RulesHost.GetRule(rule.TypeOfRule).UserInput = rule.UserInput;
            //}
        }

        public void MakeRequest(RequestId request)
        {
            Handler.Request.Make(request);
            ExternalEvent.Raise();
        }

    }
}
