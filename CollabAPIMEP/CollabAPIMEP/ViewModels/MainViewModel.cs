using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using CollabAPIMEP.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Application = Autodesk.Revit.ApplicationServices.Application;


namespace CollabAPIMEP
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UIApplication uiApp;
        private Application m_app;
        private Document m_doc;
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

        #endregion

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
        private string _loaderStateText;
        public string LoaderStateText
        {
            get
            {
                if (_loaderStateText == null)
                {
                    _loaderStateText = "Disabled";
                }
                return _loaderStateText;
            }
            set
            {
                _loaderStateText = value;
                OnPropertyChanged(nameof(LoaderStateText));
            }
        }

        private ObservableCollection<Rule> _rules;
        public ObservableCollection<Rule> Rules
        {
            get { return _rules; }
            set
            {
                _rules = value;
                OnPropertyChanged(nameof(Rules));
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
                _isLoaderEnabled = value;
                if (_isLoaderEnabled == true)
                {
                    EnabledColour = new SolidColorBrush(Colors.Green);
                    LoadingStateText = "Enabled";
                }
                else
                {
                    EnabledColour = new SolidColorBrush(Colors.CornflowerBlue);
                    LoadingStateText = "Disabled";
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
                    _enabledColour = new SolidColorBrush(Colors.Green);
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
        public RelayCommand<object> AddTestCommand { get; set; }
        public RelayCommand<object> SaveCommand { get; set; }


        #endregion
        public MainViewModel(UIApplication uiapp, FamilyLoadHandler _familyLoadHandler)
        {

            uiApp = uiapp;
            m_app = uiApp.Application;
            m_doc = uiapp.ActiveUIDocument.Document;
            DocTitle = m_doc.Title;
            _familyLoadHandler.SetHandlerAndEvent();
            this._familyLoadHandler = _familyLoadHandler;

            //not needed anymore familyloaderclass will always be passed in
            //if (FamLoadHandler == null)
            //{
            //    // this one is here for easy debugging via add-in manager
            //    FamLoadHandler = new FamilyLoadHandler(uiapp);
            //    FamLoadHandler.GetRulesFromSchema();
            //    FamLoadHandler.EnableFamilyLoading();
            //}

            _familyLoadHandler.EnableFamilyLoading();

            IsLoaderEnabled = true;
            if(FamLoadHandler.RulesMap == null)
            {
                FamLoadHandler.RulesMap = Rule.GetDefaultRules();
                FamLoadHandler.SaveRulesToSchema();

            }

            Rules = new ObservableCollection<Rule>(FamLoadHandler.RulesMap.Values.ToList());
            
            if(FamLoadHandler.RulesEnabled)
            {
                LoadingStateText = "Enabled"; 

            }

            else
            {
                LoadingStateText = "Disabled";
            }


            EnableLoadingCommand = new RelayCommand<object>(p => true, p => ToggleFamilyLoadingAction());
            AddTestCommand = new RelayCommand<object>(p => true, p => AddTestCommandAction());
            SaveCommand = new RelayCommand<object>(p => true, p => SaveAction());

            MinimizeImage = Utils.LoadEmbeddedImage("minimizeButton.png");
            MaximizeImage = Utils.LoadEmbeddedImage("maximizeButton.png");
            CloseImage = Utils.LoadEmbeddedImage("closeButton.png");

            ShowMainWindow();
            Results = new ObservableCollection<string>();
        }

        public void ShowMainWindow()
        {
            if (IsWindowClosed)
            {
                //not needed familyloaderclass will always be passed in
                //FamLoadHandler = new FamilyLoadHandler(uiApp);
                //FamLoadHandler.GetRulesFromSchema();
                //FamLoadHandler.EnableFamilyLoading();

                MainWindow = new MainWindow() { DataContext = this };
                WindowInteropHelper helper = new WindowInteropHelper(MainWindow);
                helper.Owner = uiApp.MainWindowHandle;
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
            FamLoadHandler.ExternalEvent.Dispose();
            FamLoadHandler.ExternalEvent = null;
            FamLoadHandler.Handler = null;
            IsWindowClosed = true;
            MainWindow.Closed -= MainWindow_Closed;
        }

        private void ToggleFamilyLoadingAction()
        {
            if (LoadingStateText == "Disabled")
            {
                FamLoadHandler.RequestEnableLoading(Rules.ToList());
                IsLoaderEnabled = true;
            }
            else
            {
                FamLoadHandler.RequestDisableLoading();
                IsLoaderEnabled = false;
            }
        }

        private void SaveAction()
        {
            FamLoadHandler.RequestSaveRules(Rules.ToList());
        }

        private void AddTestCommandAction()
        {
            Results.Add("test" + Results.Count.ToString());
        }


        //public void EnableFamilyLoader()
        //{
        //    m_app.FamilyLoadedIntoDocument += OnFamilyLoadedIntoDocument;
        //}

        //public void DisableFamilyLoader()
        //{
        //    m_app.FamilyLoadedIntoDocument -= OnFamilyLoadedIntoDocument;
        //}

        //public void EnableFamilyLoading()
        //{
        //    m_app.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
        //}

        //public void DisableFamilyLoading()
        //{
        //    m_app.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
        //}

        //private void OnFamilyLoadedIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadedIntoDocumentEventArgs e)
        //{
        //    Results.Add("Loaded: " + e.FamilyPath + e.FamilyName + ".rfa");
        //}


        //private void OnFamilyLoadingIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadingIntoDocumentEventArgs e)
        //{
        //    if (e.Cancellable)
        //    {
        //        //apply rules
        //        string pathname = e.FamilyPath + e.FamilyName + ".rfa";

        //        try
        //        {
        //            FamLoadHandler.ApplyRules(pathname, Rules.ToList());
        //        }
        //        catch (RuleException ex)
        //        {
        //            e.Cancel();
        //            Results.Add("Canceled: " + e.FamilyPath + e.FamilyName + ".rfa");
        //            MessageBox.Show(ex.Message);
        //        }

        //        Document familyDocument = m_app.OpenDocumentFile(pathname);
        //        foreach (Rule rule in Rules)
        //        {
        //            if (!rule.IsEnabled)
        //            {
        //                continue;
        //            }

        //            switch (rule.ID)
        //            {
        //                case "NumberOfElements":
        //                    FilteredElementCollector eleCol = new FilteredElementCollector(familyDocument);
        //                    var elements = eleCol.WhereElementIsNotElementType().ToElements();
        //                    int elementCount = elements.Count;
        //                    if (elementCount > Convert.ToInt32(rule.UserInput))
        //                    {
        //                        e.Cancel();
        //                        MessageBox.Show($"{elementCount} elements inside family, loading family canceled");
        //                        familyDocument.Close(false);
        //                    }
        //                    break;
        //                case "ImportedInstances":
        //                    FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));
        //                    IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
        //                    int importCount = importsLinks.Count;
        //                    if (importCount > 0)
        //                    {
        //                        e.Cancel();
        //                        MessageBox.Show($"{importCount} imported instances inside family, loading family canceled");
        //                        familyDocument.Close(false);
        //                    }
        //                    break;
        //                case "SubCategory":
        //                    break;
        //                case "Material":
        //                    break;
        //            }

        //        }



        //    }
        //}

    }
}
