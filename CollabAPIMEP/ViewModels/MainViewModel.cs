using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP.Commands;
using System.Collections.Generic;

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

        #endregion

        #region Commands
        public RelayCommand<object> EnableCommand { get; set; }

        #endregion
        public MainViewModel(UIApplication uiapp, FamilyLoadHandler loadHandler)
        {
            uiApp = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;
            familyLoadHandler = loadHandler;
            Rules = CreateRules();

            EnableCommand = new RelayCommand<object>(p => true, p => EnableCommandAction());

            MainWindow.ShowDialog();
        }

        private List<Rule> CreateRules()
        {
            List<Rule> rules = new List<Rule>();
            string countElement = "100";
            Rule ruleElementNumber = new Rule("Number of elements", countElement);
            ruleElementNumber.Description = $"This rule will check the number of elements in the family. If the number of elements is greater than {countElement}, the family will not be loaded into the project.";
            rules.Add(ruleElementNumber);

            Rule ruleImports = new Rule("Imported instances");
            ruleImports.Description = "This rule will check the number of imported instances in the family. If the number of imported instances is greater than 0, the family will not be loaded into the project.";
            rules.Add(ruleImports);

            Rule ruleSubCategory = new Rule("Sub Category");
            ruleSubCategory.Description = "This rule will check if every piece of geometry in the family is assigned to a subcategory. If not, the family will not be loaded into the project.";
            rules.Add(ruleSubCategory);

            Rule ruleMaterial = new Rule("Material");
            ruleMaterial.Description = "This rule will check the number of materials in a family. If the number is greater than 50, the family will not be loaded into the project.";
            rules.Add(ruleMaterial);
            return rules;
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

        private void SaveSettings()
        {
            Schema schema = Schema.Lookup(FamilyLoadHandler.Settings);
            Entity retrievedEntity = m_doc.ProjectInformation.GetEntity(schema);

            List<Rule> rules = retrievedEntity.Get<List<Rule>>(schema.GetField("FamilyLoaderRules"));


            if (schema == null)
            {
                SchemaBuilder schemabuilder = new SchemaBuilder(GUIDschemaPanelId);
                FieldBuilder fieldbuilder = schemabuilder.AddSimpleField("PanelID", typeof(ElementId));
                fieldbuilder.SetDocumentation("ElementID of the Electrical Schematics Panel");
                schemabuilder.SetSchemaName("PanelID");
                schema = schemabuilder.Finish();

            }
            Entity entity = new Entity(schema);
            Field fieldPanelID = schema.GetField("PanelID");
            entity.Set<ElementId>(fieldPanelID, elemIdToSTore);
            viewDrafting.SetEntity(entity);
        }
    }
}
