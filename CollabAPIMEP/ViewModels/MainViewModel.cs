using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CollabAPIMEP
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UIApplication uiApp;

        private Document m_doc;
        private FamilyLoadHandler familyLoadHandler;
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

        #endregion

        #region Commands
        public RelayCommand<object> EnableCommand { get; set; }

        #endregion
        public MainViewModel(UIApplication uiapp, FamilyLoadHandler _familyLoadHandler)
        {
            uiApp = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;
            familyLoadHandler = _familyLoadHandler;
            if (familyLoadHandler == null)
            {
                familyLoadHandler = new FamilyLoadHandler(uiapp);
            }

            familyLoadHandler.RulesMap = CreateRules();
            Rules = new ObservableCollection<Rule>(familyLoadHandler.RulesMap.Values.ToList());

            handler = new RequestHandler(this, familyLoadHandler);
            exEvent = ExternalEvent.Create(handler);

            EnableCommand = new RelayCommand<object>(p => true, p => EnableCommandAction());

            MainWindow.Show();
        }

        private Dictionary<string, Rule> CreateRules()
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

        private void EnableCommandAction()
        {
            MakeRequest(RequestId.ToggleFamilyLoaderEvent);
        }

        public void MakeRequest(RequestId request)
        {
            handler.Request.Make(request);
            exEvent.Raise();
        }

        //private void SaveSettings()
        //{
        //    Schema schema = Schema.Lookup(FamilyLoadHandler.Settings);
        //    Entity retrievedEntity = m_doc.ProjectInformation.GetEntity(schema);

        //    List<Rule> rules = retrievedEntity.Get<List<Rule>>(schema.GetField("FamilyLoaderRules"));


        //    if (schema == null)
        //    {
        //        SchemaBuilder schemabuilder = new SchemaBuilder(GUIDschemaPanelId);
        //        FieldBuilder fieldbuilder = schemabuilder.AddSimpleField("PanelID", typeof(ElementId));
        //        fieldbuilder.SetDocumentation("ElementID of the Electrical Schematics Panel");
        //        schemabuilder.SetSchemaName("PanelID");
        //        schema = schemabuilder.Finish();

        //    }
        //    Entity entity = new Entity(schema);
        //    Field fieldPanelID = schema.GetField("PanelID");
        //    entity.Set<ElementId>(fieldPanelID, elemIdToSTore);
        //    viewDrafting.SetEntity(entity);
        //}
    }
}
