using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Windows;


namespace CollabAPIMEP
{
    public class FamilyLoadHandler
    {
        public RequestHandler handler;
        public ExternalEvent exEvent;
        public static Guid Settings = new Guid("c16f94f6-5f14-4f33-91fc-f69dd7ac0d05");
        public List<string> Pathnames = new List<string>();
        public List<string> LoadedNames = new List<string>();

        private UIApplication uiApp;
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document m_doc;
        public Dictionary<string, Rule> RulesMap { get; set; }
        private List<Rule> _rules;
        public List<Rule> Rules
        {
            get
            {
                if (_rules == null)
                {
                    _rules = RulesMap.Values.ToList();
                }
                { return _rules; }
            }
            set
            {
                _rules = value;
            }
        }
        public List<string> Results = new List<string>();
        public FamilyLoadHandler(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_app = uiapp.Application;
            m_doc = uiApp.ActiveUIDocument.Document;
            RequestMethods helperMethods = new RequestMethods(this);
            handler = new RequestHandler(this, helperMethods);
            exEvent = ExternalEvent.Create(handler);
        }

        public void GetRulesFromSchema()
        {
            Schema schema = Schema.Lookup(Settings);

            if (schema != null)
            {
                RulesMap = new Dictionary<string, Rule>();

                Entity retrievedEntity = m_doc.ProjectInformation.GetEntity(schema);
                if (!retrievedEntity.IsValid())
                {
                    RulesMap = Rule.GetDefaultRules();
                    SaveRulesToSchema();
                    return;
                }
                string rulesString = retrievedEntity.Get<string>(schema.GetField("FamilyLoaderRules"));
                List<string> rulesStrings = rulesString.Split(Rule.RuleSeparator).ToList();
                foreach (string ruleString in rulesStrings)
                {
                    Rule rule = Rule.deserializeFromSchema(ruleString);
                    if (rule.ID == null)
                    {
                        continue;
                    }
                    RulesMap.Add(rule.ID, rule);
                }
            }
            else
            {
                RulesMap = Rule.GetDefaultRules();
                SaveRulesToSchema();
            }
        }

        public void ApplyRules(string pathname, FamilyLoadingIntoDocumentEventArgs e)
        {
            if (RulesMap == null)
            {
                return;
            }

            bool ruleViolation = false;
            string errorMessage = "";
            Document familyDocument = null;

            try
            {
                familyDocument = m_app.OpenDocumentFile(pathname);

            }

            catch
            {
                errorMessage = "please save the family before loading it into a project";
                throw new RuleException(errorMessage);

            }

            FamilyManager familyManager = familyDocument.FamilyManager;


            foreach (Rule rule in Rules)
            {
                if (!rule.IsEnabled)
                {
                    continue;
                }

                switch (rule.ID)
                {


                    case "NumberOfElements":

                        var maxElements = Convert.ToInt32(rule.UserInput);



                        FilteredElementCollector collectorElements = new FilteredElementCollector(familyDocument);

                        // get nested families and modeled geometry
                        FilterRule parRuleVisibility = ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(((int)BuiltInParameter.IS_VISIBLE_PARAM)));

                        ElementParameterFilter filterVisibility = new ElementParameterFilter(parRuleVisibility);

                        IList<Element> elementsWithGeometry = collectorElements.WherePasses(filterVisibility).ToElements();

                        int elementCount = elementsWithGeometry.Count;
                        if (elementCount > maxElements)
                        {
                            ruleViolation = true;
                            errorMessage += $"- too many elements inside family ({elementCount}, only {maxElements} allowed)" + System.Environment.NewLine;
                        }
                        break;
                    case "ImportedInstances":
                        FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));
                        IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
                        int importCount = importsLinks.Count;
                        if (importCount > 0)
                        {
                            ruleViolation = true;

                            errorMessage += $"- too many imported instances inside family ({importCount})" + System.Environment.NewLine;
                        }
                        break;
                    case "SubCategory":

                        bool ruleViolationSubCategory = false;

                        // Create a FilteredElementCollector to collect elements from the document
                        FilteredElementCollector collector = new FilteredElementCollector(familyDocument);

                        // Create a quick filter rule
                        FilterRule parRule = ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(((int)BuiltInParameter.FAMILY_ELEM_SUBCATEGORY)));


                        // Create a rule to check if the parameter has any value (not null or empty)

                        // Create a filter element with the rule
                        ElementParameterFilter filter = new ElementParameterFilter(parRule);

                        IList<Element> filteredElements = collector.WherePasses(filter).ToElements();
                        int elementsWithCat = 0;

                        foreach (Element element in filteredElements)
                        {
                            ElementId eleId = element.get_Parameter(BuiltInParameter.FAMILY_ELEM_SUBCATEGORY).AsElementId();
                            if (eleId == ElementId.InvalidElementId)
                            {
                                errorMessage += "- elements without subcategory found" + System.Environment.NewLine;
                                ruleViolation = true;
                                break;
                            }
                        }

                        break;
                    case "Material":

                        var maxMaterials = Convert.ToInt32(rule.UserInput);

                        FilteredElementCollector materialCollector = new FilteredElementCollector(familyDocument).OfClass(typeof(Material));
                        IList<Element> materials = materialCollector.ToElements();

                        if (materials.Count > Convert.ToInt32(rule.UserInput))
                        {
                            ruleViolation = true;
                            errorMessage += $"- too many materials inside family ({materials.Count}, only {maxMaterials} allowed)" + System.Environment.NewLine;
                        }


                        break;
                }

            }

            if (ruleViolation == true)
            {
                //familyDocument.Close(false);
                errorMessage = $"family: '{e.FamilyName}' load canceled because:" + System.Environment.NewLine + errorMessage;
                throw new RuleException(errorMessage);

            }


        }


        public void RequestSaveRules(List<Rule> rules)
        {
            Rules = rules;
            MakeRequest(RequestId.SaveRules);
        }
        public void SaveRulesToSchema()
        {
            Schema schema = Schema.Lookup(FamilyLoadHandler.Settings);
            if (schema == null)
            {
                SchemaBuilder schemabuilder = new SchemaBuilder(FamilyLoadHandler.Settings);
                schemabuilder.SetReadAccessLevel(AccessLevel.Public);
                schemabuilder.SetWriteAccessLevel(AccessLevel.Public);
                schemabuilder.SetVendorId("APIMEP");

                FieldBuilder fieldbuilder = schemabuilder.AddSimpleField("FamilyLoaderRules", typeof(string));

                fieldbuilder.SetDocumentation("FamilyLoader Rules");
                schemabuilder.SetSchemaName("FamilyLoader");
                schema = schemabuilder.Finish();
            }

            Field familyLoader = schema.GetField("FamilyLoaderRules");
            Entity entity = new Entity(schema);
            string schemaString = "";
            int ruleCount = 1;

            foreach (var rule in Rules)
            {
                string ruleString = "";
                Type ruleType = rule.GetType();
                int propertyCount = 1;
                var properties = ruleType.GetProperties();

                foreach (PropertyInfo prop in properties)
                {
                    string propertyString = "";
                    propertyString += prop.Name;
                    propertyString += Rule.ValueSeparator;
                    propertyString += prop.GetValue(rule);
                    if (propertyCount != properties.Count())
                    {
                        propertyString += Rule.PropertySeparator;
                    }

                    ruleString += propertyString;
                    propertyCount++;
                }

                if (ruleCount != properties.Count())
                {
                    ruleString += Rule.RuleSeparator;
                }

                schemaString += ruleString;
                ruleCount++;
            }

            entity.Set<string>(familyLoader, schemaString);

            using (Transaction saveSettings = new Transaction(m_doc, "Save Settings"))
            {
                saveSettings.Start();
                m_doc.ProjectInformation.SetEntity(entity);
                saveSettings.Commit();
            }
        }

        public void RequestEnableLoading(List<Rule> rules)
        {
            Rules = rules;
            MakeRequest(RequestId.EnableLoading);
        }
        public void EnableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
        }
        public void RequestDisableLoading()
        {
            MakeRequest(RequestId.DisableLoading);
        }

        public void DisableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
        }

        private void OnFamilyLoadingIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadingIntoDocumentEventArgs e)
        {
            if (!e.Cancellable)
            {
                return;
            }

            string pathname = e.FamilyPath + e.FamilyName + ".rfa";


            //dictionary check moet hier komen!!


            try
            {
                ApplyRules(pathname, e);
            }


            catch (RuleException ex)
            {
                e.Cancel();
                Results.Add("Canceled: " + e.FamilyPath + e.FamilyName + ".rfa");
                MessageBox.Show(ex.Message);
                e.Cancel();

            }


        }

        public void MakeRequest(RequestId request)
        {
            handler.Request.Make(request);
            exEvent.Raise();
        }
    }

}

