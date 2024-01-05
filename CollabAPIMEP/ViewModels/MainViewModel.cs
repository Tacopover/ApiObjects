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
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;


namespace CollabAPIMEP
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UIApplication uiApp;
        private Application m_app;
        private Document m_doc;

        RequestHandler handler;
        ExternalEvent exEvent;
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

        private ObservableCollection<string> _results;
        public ObservableCollection<string> Results
        {
            get { return _results; }
            set
            {
                _results = value;
                OnPropertyChanged(nameof(Results));
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
            this._familyLoadHandler = _familyLoadHandler;
            if (FamLoadHandler == null)
            {
                FamLoadHandler = new FamilyLoadHandler(uiapp);
            }

            FamLoadHandler.RulesMap = LoadRules();
            Rules = new ObservableCollection<Rule>(FamLoadHandler.RulesMap.Values.ToList());

            //handler = new RequestHandler(this, FamLoadHandler);
            //exEvent = ExternalEvent.Create(handler);

            EnableLoadingCommand = new RelayCommand<object>(p => true, p => EnableLoadingAction());
            EnableLoaderCommand = new RelayCommand<object>(p => true, p => EnableLoaderAction());
            AddTestCommand = new RelayCommand<object>(p => true, p => AddTestCommandAction());
            SaveCommand = new RelayCommand<object>(p => true, p => SaveAction());

            //Results = new List<string>(tempResult);
            MainWindow.Show();
            Results = new ObservableCollection<string>();
        }

        private Dictionary<string, Rule> LoadRules()
        {
            Dictionary<string, Rule> rulesMap = new Dictionary<string, Rule>();

            Rule ruleElementNumber = new Rule("NumberOfElements", 100.ToString());
            ruleElementNumber.Name = "Number of elements";
            rulesMap["NumberOfElements"] = ruleElementNumber;

            Rule ruleImports = new Rule("ImportedInstances", 1.ToString());
            ruleImports.Name = "Imported instances";
            rulesMap["ImportedInstances"] = ruleImports;

            Rule ruleSubCategory = new Rule("SubCategory");
            ruleSubCategory.Name = "Sub Category";
            rulesMap["SubCategory"] = ruleSubCategory;

            Rule ruleMaterial = new Rule("Material", 30.ToString());
            ruleMaterial.Name = "Material";
            rulesMap["Material"] = ruleMaterial;
            return rulesMap;
        }

        private void EnableLoadingAction()
        {
            MakeRequest(RequestId.ToggleFamilyLoadingEvent);
        }
        private void EnableLoaderAction()
        {
            MakeRequest(RequestId.ToggleFamilyLoaderEvent);
        }
        private void SaveAction()
        {
            MakeRequest(RequestId.SaveRules);
        }

        private void AddTestCommandAction()
        {
            Results.Add("test" + Results.Count.ToString());
        }


        public void EnableFamilyLoader()
        {
            m_app.FamilyLoadedIntoDocument += OnFamilyLoadedIntoDocument;
        }

        public void DisableFamilyLoader()
        {
            m_app.FamilyLoadedIntoDocument -= OnFamilyLoadedIntoDocument;
        }

        public void EnableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
        }

        public void DisableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
        }

        private void OnFamilyLoadedIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadedIntoDocumentEventArgs e)
        {
            Results.Add("Loaded: " + e.FamilyPath + e.FamilyName + ".rfa");
        }

        private void OnDocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {

        }

        private void OnFamilyLoadingIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadingIntoDocumentEventArgs e)
        {
            if (e.Cancellable)
            {
                //apply rules
                string pathname = e.FamilyPath + e.FamilyName + ".rfa";

                try
                {
                    FamLoadHandler.ApplyRules(pathname, Rules.ToList());
                }
                catch (RuleException ex)
                {
                    e.Cancel();
                    Results.Add("Canceled: " + e.FamilyPath + e.FamilyName + ".rfa");
                    MessageBox.Show(ex.Message);
                }

                Document familyDocument = m_app.OpenDocumentFile(pathname);
                foreach (Rule rule in Rules)
                {
                    if (!rule.IsEnabled)
                    {
                        continue;
                    }

                    switch (rule.ID)
                    {
                        case "NumberOfElements":
                            FilteredElementCollector eleCol = new FilteredElementCollector(familyDocument);
                            var elements = eleCol.WhereElementIsNotElementType().ToElements();
                            int elementCount = elements.Count;
                            if (elementCount > Convert.ToInt32(rule.UserInput))
                            {
                                e.Cancel();
                                MessageBox.Show($"{elementCount} elements inside family, loading family canceled");
                                familyDocument.Close(false);
                            }
                            break;
                        case "ImportedInstances":
                            FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));
                            IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
                            int importCount = importsLinks.Count;
                            if (importCount > 0)
                            {
                                e.Cancel();
                                MessageBox.Show($"{importCount} imported instances inside family, loading family canceled");
                                familyDocument.Close(false);
                            }
                            break;
                        case "SubCategory":
                            break;
                        case "Material":
                            break;
                    }

                }



            }
        }

        public void MakeRequest(RequestId request)
        {
            handler.Request.Make(request);
            exEvent.Raise();
        }

    }
}
