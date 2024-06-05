using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;


namespace CollabAPIMEP
{
    public class FamilyLoadHandler
    {

        public static Guid Settings = new Guid("c16f94f6-5f14-4f33-91fc-f69dd7ac0d05");
        public List<string> Pathnames = new List<string>();
        public List<string> LoadedNames = new List<string>();

        private UIApplication uiApp;
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document _fl_doc;
        public Document Fl_doc
        {
            get => _fl_doc;
            set
            {
                _fl_doc = value;
                // user switches between documents, so we check if there are rules or create default ones
                if (!GetRulesFromSchema())
                {
                    RulesMap = Rule.GetDefaultRules();
                }

            }
        }
        public Document FamilyDocument;
        public static List<ElementId> AddedIds = new List<ElementId>();
        public Dictionary<string, Rule> RulesMap { get; set; }
        private List<Rule> _rules;
        public bool RulesEnabled { get; set; } = false;
        public RequestHandler Handler { get; set; }

        public ExternalEvent ExternalEvent { get; set; }



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
            Fl_doc = uiApp.ActiveUIDocument.Document;

            if (!GetRulesFromSchema())
            {
                RulesMap = Rule.GetDefaultRules();
            }
            SetHandlerAndEvent();
            EnableFamilyLoading();
        }


        public void SetHandlerAndEvent()
        {
            RequestMethods helperMethods = new RequestMethods(this);
            Handler = new RequestHandler(this, helperMethods);
            ExternalEvent = ExternalEvent.Create(Handler);
        }

        public void RemoveHandlerAndEvent()
        {
            Handler = null;
            ExternalEvent = null;
        }

        public bool GetRulesFromSchema()
        {
            // This method returns true if the document's schema is available and can fill the RulesMap with rules
            Schema schema = Schema.Lookup(Settings);

            if (schema != null)
            {
                RulesMap = new Dictionary<string, Rule>();

                Entity retrievedEntity = Fl_doc.ProjectInformation.GetEntity(schema);

                if (retrievedEntity == null || retrievedEntity.Schema == null)
                {
                    return false;
                }

                string totalString = retrievedEntity.Get<string>(schema.GetField("FamilyLoaderRules"));
                string rulesEnabled = totalString.Split(Rule.rulesEnabledSeperator).FirstOrDefault();
                string rulesString = totalString.Split(Rule.rulesEnabledSeperator).LastOrDefault();


                object value = Convert.ChangeType(rulesEnabled, typeof(bool));
                RulesEnabled = (bool)value;

                //event handlers removed and always enabled
                if (RulesEnabled == true)
                {
                    EnableFamilyLoading();
                }
                else
                {
                    DisableFamilyLoading();
                }

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

                return true;
            }

            return false;

        }

        public void ApplyRules(string pathname, FamilyLoadingIntoDocumentEventArgs e)
        {

            if (RulesEnabled == true)
            {

                bool ruleViolation = false;
                string errorMessage = "";

                if (pathname != "NotSaved")
                {
                    FamilyDocument = m_app.OpenDocumentFile(pathname);
                }

                if (FamilyDocument == null)
                {
                    errorMessage = "Could not open Family Document for auditing";
                    throw new RuleException(errorMessage);
                }

                FamilyManager familyManager = FamilyDocument.FamilyManager;

                foreach (Rule rule in Rules)
                {
                    if (!rule.IsEnabled)
                    {
                        continue;
                    }

                    switch (rule.ID)
                    {

                        case "FileSize":
                            if (pathname == "NotSaved")
                            {
                                break;
                            }
                            var maxFileSizeMB = Convert.ToInt32(rule.UserInput);
                            FileInfo fileInfo = new FileInfo(pathname);
                            var fileSizeMB = fileInfo.Length / (1024 * 1024); // Convert bytes to MB
                            if (fileSizeMB > maxFileSizeMB)
                            {
                                ruleViolation = true;
                                errorMessage += $"- file size too large ({fileSizeMB} MB, only {maxFileSizeMB} MB allowed)" + System.Environment.NewLine;
                            }
                            break;

                        case "NumberOfParameters":

                            var maxParameters = Convert.ToInt32(rule.UserInput);

                            int parameterCount = familyManager.Parameters.Size;
                            if (parameterCount > maxParameters)
                            {
                                ruleViolation = true;
                                errorMessage += $"- too many parameters inside family ({parameterCount}, only {maxParameters} allowed)" + System.Environment.NewLine;
                            }
                            break;

                        case "NumberOfElements":

                            var maxElements = Convert.ToInt32(rule.UserInput);



                            FilteredElementCollector collectorElements = new FilteredElementCollector(FamilyDocument);

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
                            FilteredElementCollector colImportsAll = new FilteredElementCollector(FamilyDocument).OfClass(typeof(ImportInstance));
                            IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
                            int importCount = importsLinks.Count;
                            if (importCount > 0)
                            {
                                ruleViolation = true;

                                errorMessage += $"- too many imported instances inside family ({importCount})" + System.Environment.NewLine;
                            }
                            break;
                        case "SubCategory":

                            // Create a FilteredElementCollector to collect elements from the document
                            FilteredElementCollector collector = new FilteredElementCollector(FamilyDocument);

                            // Create a quick filter rule
                            FilterRule parRule = ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(((int)BuiltInParameter.FAMILY_ELEM_SUBCATEGORY)));


                            // Create a rule to check if the parameter has any value (not null or empty)

                            // Create a filter element with the rule
                            ElementParameterFilter filter = new ElementParameterFilter(parRule);

                            IList<Element> filteredElements = collector.WherePasses(filter).ToElements();

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

                            FilteredElementCollector materialCollector = new FilteredElementCollector(FamilyDocument).OfClass(typeof(Material));
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
                    //FamilyDocument.Close(false);
                    errorMessage = $"family: '{e.FamilyName}' load canceled because:" + System.Environment.NewLine + errorMessage;
                    throw new RuleException(errorMessage);
                }
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

            //event handlers removed and always enabled
            if (RulesEnabled == true)
            {
                EnableFamilyLoading();
            }
            else
            {
                DisableFamilyLoading();
            }

            Field familyLoader = schema.GetField("FamilyLoaderRules");
            Entity entity = new Entity(schema);
            string schemaString = RulesEnabled.ToString() + Rule.rulesEnabledSeperator;
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

            using (Transaction saveSettings = new Transaction(Fl_doc, "Save Settings"))
            {
                saveSettings.Start();
                Fl_doc.ProjectInformation.SetEntity(entity);
                saveSettings.Commit();
            }

        }


        public void RequestEnableLoading(List<Rule> rules)
        {
            Rules = rules;
            MakeRequest(RequestId.EnableLoading);
        }
        public void EnableFamilyRules()
        {
            RulesEnabled = true;
        }
        public void RequestDisableLoading()
        {
            MakeRequest(RequestId.DisableLoading);
        }


        public void EnableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
        }

        public void DisableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
            RulesEnabled = false;
        }

        private void OnFamilyLoadingIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadingIntoDocumentEventArgs e)
        {
            if (!e.Cancellable)
            {
                return;
            }
            string familyPath = e.FamilyPath;
            string pathname = "NotSaved";
            if (familyPath != string.Empty)
            {
                pathname = familyPath + e.FamilyName + ".rfa";
            }


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

        public void HandleUpdater()
        {
            if (AddedIds.Any())
            {
                uiApp.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
            }
            else
            {
                uiApp.Idling -= new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
            }
        }
        public void OnIdling(object sender, IdlingEventArgs e)
        {
            if (!AddedIds.Any())
            {
                uiApp.Idling -= new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
                return;
            }


            FilteredElementCollector famCollector = new FilteredElementCollector(Fl_doc).OfClass(typeof(Family));

            using (Transaction fixDuplicatesTrans = new Transaction(Fl_doc, "Fix Duplicates"))
            {
                fixDuplicatesTrans.Start();
                int numberOfProcessedFamilies = 0;
                //TODO if there are a lof of duplicates loaded in then you will want to give the user an option to select 1 operation
                // for all families, for instance: rename all families with a suffix, or replace all families with new family, etc.
                foreach (ElementId id in AddedIds)
                {
                    Family newFamily = Fl_doc.GetElement(id) as Family;
                    if (newFamily == null)
                    {
                        continue;
                    }
                    string newFamNam = newFamily.Name;
                    // get existing family and a type
                    string existingFamName = newFamNam.Substring(0, newFamNam.Length - 1);
                    Family existingFamily = famCollector.FirstOrDefault(f => f.Name.Equals(existingFamName)) as Family;
                    if (existingFamily == null)
                    {
                        continue;
                    }
                    //ElementId existingTypeId = existingFamily.GetFamilySymbolIds().FirstOrDefault();
                    //FamilySymbol existingType = m_doc.GetElement(existingTypeId) as FamilySymbol;
                    //if (existingType == null)
                    //{
                    //    continue;
                    //}

                    //create a window to resolve the duplicates and prepare the data for the window
                    DuplicateTypeWindow duplicateTypeWindow = new DuplicateTypeWindow();
                    duplicateTypeWindow.dtViewModel.ExistingFamilyName = existingFamName;
                    duplicateTypeWindow.dtViewModel.NewFamilyName = newFamNam;
                    List<string> newFamTypeNames = new List<string>();
                    List<string> existingFamTypeNames = new List<string>();
                    foreach (ElementId symbolId in newFamily.GetFamilySymbolIds())
                    {
                        FamilySymbol symbol = Fl_doc.GetElement(symbolId) as FamilySymbol;
                        newFamTypeNames.Add(symbol.Name);
                    }
                    foreach (ElementId symbolId in existingFamily.GetFamilySymbolIds())
                    {
                        FamilySymbol symbol = Fl_doc.GetElement(symbolId) as FamilySymbol;
                        existingFamTypeNames.Add(symbol.Name);
                    }
                    existingFamTypeNames.Sort();
                    newFamTypeNames.Sort();
                    duplicateTypeWindow.dtViewModel.CreateMapping(newFamTypeNames, existingFamTypeNames);
                    duplicateTypeWindow.ShowDialog();

                    if (duplicateTypeWindow.dtViewModel.IsCanceled)
                    {
                        continue;
                    }

                    if (duplicateTypeWindow.dtViewModel.IsRenameEnabled)
                    {
                        //TODO check if the name has change at all, if not then do not rename
                        string newFamName = duplicateTypeWindow.dtViewModel.NewFamilyName;
                        string existingFamNameNew = duplicateTypeWindow.dtViewModel.ExistingFamilyName;
                        existingFamily.Name = existingFamNameNew;
                        newFamily.Name = newFamName;
                    }
                    else
                    {
                        //TODO check if new family and existing family are of the same category. It could be that a new family has been
                        // changed to a different category. In that case you cannot replace, but only rename the family
                        Dictionary<string, FamilySymbol> typeMap = new Dictionary<string, FamilySymbol>();

                        Family famToReplace;
                        Family famToRemain;
                        FamilySymbol typeToRemain;
                        FamilySymbol typeToReplace;
                        if (duplicateTypeWindow.dtViewModel.ReplaceExistingChecked)
                        {
                            famToReplace = existingFamily;
                            famToRemain = newFamily;
                        }
                        else
                        {
                            famToReplace = newFamily;
                            famToRemain = existingFamily;
                        }

                        List<FamilySymbol> typesToRemain = famToRemain.GetFamilySymbolIds().Select(i => Fl_doc.GetElement(i) as FamilySymbol).ToList();
                        foreach (var mapping in duplicateTypeWindow.dtViewModel.Mappings.ToList())
                        {
                            //create a mapping between the string value of column 1 and the family symbol that represents the string in column 2
                            //TODO item2 is column 1, which is confusing, so change this
                            typeMap[mapping.Item1] = typesToRemain.FirstOrDefault(s => s.Name.Equals(mapping.Item2));
                        }
                        //duplicateTypeWindow.dtViewModel.Mappings.ToList().ForEach(mapping =>
                        //{
                        //    FamilySymbol newType = newFamily.GetFamilySymbolIds().Select(i => m_doc.GetElement(i) as FamilySymbol)
                        //    .FirstOrDefault(symbol => symbol.Name.Equals(mapping.Item1));
                        //    FamilySymbol existingType = existingFamily.GetFamilySymbolIds().Select(id => m_doc.GetElement(id) as FamilySymbol)
                        //    .FirstOrDefault(symbol => symbol.Name.Equals(mapping.Item2));
                        //    if (newType == null || existingType == null)
                        //    {
                        //        return;
                        //    }
                        //});

                        foreach (ElementId symbolId in famToReplace.GetFamilySymbolIds())
                        {
                            FamilyInstanceFilter instanceFilter = new FamilyInstanceFilter(Fl_doc, symbolId);
                            FilteredElementCollector famInstanceCollector = new FilteredElementCollector(Fl_doc)
                        .OfClass(typeof(FamilyInstance))
                        .WherePasses(instanceFilter);
                            IList<Element> instances = famInstanceCollector.ToElements();
                            FamilySymbol symbol = Fl_doc.GetElement(symbolId) as FamilySymbol;
                            string typeName = symbol.Name;
                            FamilySymbol remainingType = typeMap.TryGetValue(typeName, out FamilySymbol type) ? type : null;
                            if (remainingType == null)
                            {
                                continue;
                            }
                            foreach (var inst in instances)
                            {
                                FamilyInstance instance = inst as FamilyInstance;
                                instance.ChangeTypeId(remainingType.Id);
                            }
                        }
                        Fl_doc.Delete(famToReplace.Id);

                    }
                    numberOfProcessedFamilies += 1;
                }
                AddedIds.Clear();

                if (numberOfProcessedFamilies == 0)
                {
                    fixDuplicatesTrans.RollBack();
                }
                else
                {
                    fixDuplicatesTrans.Commit();
                }

            }
            uiApp.Idling -= new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
        }


        public void MakeRequest(RequestId request)
        {
            Handler.Request.Make(request);
            ExternalEvent.Raise();
        }
    }

}

