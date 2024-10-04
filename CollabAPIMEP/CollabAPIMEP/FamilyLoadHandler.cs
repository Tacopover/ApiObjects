using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CollabAPIMEP.Models;
using CollabAPIMEP.ViewModels;
using CollabAPIMEP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private AddInId activeAddInId;

        private MainViewModel _viewModel;
        public MainViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = new MainViewModel(this);
                }
                return _viewModel;
            }
            set
            {
                _viewModel = value;
            }
        }
        private RulesContainer _rulesHost;
        public RulesContainer RulesHost
        {
            get
            {
                if (_rulesHost == null)
                {
                    _rulesHost = new RulesContainer();
                }
                return _rulesHost;
            }
            set
            {
                _rulesHost = value;
            }
        }

        private Dictionary<string, RulesContainer> _modelRulesMap;
        public Dictionary<string, RulesContainer> ModelRulesMap
        {
            get
            {
                if (_modelRulesMap == null)
                {
                    _modelRulesMap = new Dictionary<string, RulesContainer>();
                }
                return _modelRulesMap;
            }
            set => _modelRulesMap = value;
        }

        private Document _fl_doc;
        public Document Fl_doc
        {
            get => _fl_doc;
            set
            {
                if (_fl_doc != value)
                {
                    //fl_doc_old = value;
                    _fl_doc = value;
                    //user switches between documents, so we check if there are rules in memory, in schema or create default ones
                    RulesContainer docRules;
                    ModelRulesMap.TryGetValue(_fl_doc.Title, out docRules);
                    if (docRules != null)
                    {
                        // if the rules were modified in the current session, use the modified rules
                        ViewModel.Rules = new ObservableCollection<Rule>(docRules.Rules);
                        ViewModel.IsLoaderEnabled = docRules.IsEnabled;
                        RulesHost = docRules;
                    }
                    else
                    {
                        if (!GetRulesFromSchema())
                        {
                            // new model, so create a new RulesHost with default rules
                            RulesHost = new RulesContainer();
                            RulesHost.SetDefaultRules();
                        }
                        // if the model has not been opened yet, use the rules from the schema
                        ViewModel.Rules = new ObservableCollection<Rule>(RulesHost.Rules);
                        ViewModel.IsLoaderEnabled = RulesHost.IsEnabled;
                    }

                }


            }
        }
        public Document FamilyDocument;
        public static List<ElementId> AddedIds = new List<ElementId>();
        public RequestHandler Handler { get; set; }

        public ExternalEvent ExternalEvent { get; set; }

        public List<string> Results = new List<string>();
        public FamilyLoadHandler(AddInId activeAddInId)
        {
            this.activeAddInId = activeAddInId;
        }

        public void Initialize(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_app = uiApp.Application;
            Fl_doc = uiApp.ActiveUIDocument.Document;

            uiApp.ViewActivated += OnViewActivated;
            //if (!GetRulesFromSchema())
            //{
            //    RulesHost.SetDefaultRules();
            //}
            SetHandlerAndEvent();
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
                Entity retrievedEntity = Fl_doc.ProjectInformation.GetEntity(schema);

                if (retrievedEntity == null || retrievedEntity.Schema == null)
                {
                    return false;
                }

                string totalString = retrievedEntity.Get<string>(schema.GetField("FamilyLoaderRules"));
                RulesContainer rulesHost = RulesContainer.DeserializeFromString(totalString);
                if (rulesHost == null)
                {
                    return false;
                }
                RulesHost = rulesHost;
                //event handlers removed and always enabled
                if (RulesHost.IsEnabled == true)
                {
                    EnableFamilyLoading();
                }
                else
                {
                    DisableFamilyLoading();
                }

                return true;
            }

            return false;

        }

        public void ApplyRules(string pathname, FamilyLoadingIntoDocumentEventArgs e)
        {

            if (RulesHost.IsEnabled == true)
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

                foreach (Rule rule in RulesHost.Rules)
                {
                    if (!rule.IsEnabled)
                    {
                        continue;
                    }

                    switch (rule.TypeOfRule)
                    {

                        case RuleType.FileSize:
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

                        case RuleType.NumberOfParameters:

                            var maxParameters = Convert.ToInt32(rule.UserInput);

                            int parameterCount = familyManager.Parameters.Size;
                            if (parameterCount > maxParameters)
                            {
                                ruleViolation = true;
                                errorMessage += $"- too many parameters inside family ({parameterCount}, only {maxParameters} allowed)" + System.Environment.NewLine;
                            }
                            break;

                        case RuleType.NumberOfElements:

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
                        case RuleType.ImportedInstances:
                            FilteredElementCollector colImportsAll = new FilteredElementCollector(FamilyDocument).OfClass(typeof(ImportInstance));
                            IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
                            int importCount = importsLinks.Count;
                            if (importCount > 0)
                            {
                                ruleViolation = true;

                                errorMessage += $"- too many imported instances inside family ({importCount})" + System.Environment.NewLine;
                            }
                            break;
                        case RuleType.SubCategory:

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
                        case RuleType.Material:

                            var maxMaterials = Convert.ToInt32(rule.UserInput);

                            FilteredElementCollector materialCollector = new FilteredElementCollector(FamilyDocument).OfClass(typeof(Material));
                            IList<Element> materials = materialCollector.ToElements();

                            if (materials.Count > Convert.ToInt32(rule.UserInput))
                            {
                                ruleViolation = true;
                                errorMessage += $"- too many materials inside family ({materials.Count}, only {maxMaterials} allowed)" + System.Environment.NewLine;
                            }


                            break;

                        case RuleType.DetailLines:


                            var maxDetailLines = Convert.ToInt32(rule.UserInput);


                            FilteredElementCollector colDetailLines = new FilteredElementCollector(FamilyDocument).OfCategory(BuiltInCategory.OST_Lines).OfClass(typeof(CurveElement));
                            IList<Element> detailLines = colDetailLines.ToElements();

                            int detailLineCount = detailLines.Count;


                            if (detailLineCount > maxDetailLines)
                            {
                                ruleViolation = true;
                                errorMessage += $"- too many detail lines inside family ({detailLineCount}, only {maxDetailLines} allowed)" + System.Environment.NewLine;
                            }
                            break;

                        case RuleType.Vertices:


                            var maxVertices = Convert.ToInt32(rule.UserInput);

                            int verticesCount = CountEdges(FamilyDocument);


                            if (verticesCount > maxVertices)
                            {
                                ruleViolation = true;
                                errorMessage += $"- too many vertices inside family ({verticesCount}, only {maxVertices} allowed)" + System.Environment.NewLine;
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


        private int CountEdges(Document familyDocument)
        {
            int edgeCount = 0;

            // Collect all elements in the family document
            FilteredElementCollector collector = new FilteredElementCollector(familyDocument).WhereElementIsNotElementType();
            Options options = new Options();
            foreach (Element element in collector)
            {
                // Get the geometry of the element                
                GeometryElement geometryElement = element.get_Geometry(options);

                if (geometryElement != null)
                {
                    foreach (GeometryObject geometryObject in geometryElement)
                    {
                        edgeCount += CountEdgesInGeometryObject(geometryObject);
                    }
                }
            }

            return edgeCount;
        }

        private int CountEdgesInGeometryObject(GeometryObject geometryObject)
        {
            int edgeCount = 0;

            if (geometryObject is GeometryInstance instance)
            {
                GeometryElement instanceGeometry = instance.GetInstanceGeometry();
                foreach (GeometryObject obj in instanceGeometry)
                {
                    edgeCount += CountEdgesInGeometryObject(obj);
                }
            }
            else if (geometryObject is Solid solid)
            {
                edgeCount += solid.Edges.Size;
            }

            return edgeCount;
        }



        private int CountDetailLines(Document familyDocument)
        {
            FilteredElementCollector colDetailLines = new FilteredElementCollector(familyDocument).OfCategory(BuiltInCategory.OST_Lines).OfClass(typeof(CurveElement));
            IList<Element> detailLines = colDetailLines.ToElements();

            int detailLineCount = detailLines.Count;
            return detailLineCount;

        }


        public void RequestSaveRules(List<Rule> rules)
        {
            RulesHost.Rules = rules;
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
            if (RulesHost.IsEnabled == true)
            {
                EnableFamilyLoading();
            }
            else
            {
                DisableFamilyLoading();
            }

            Field familyLoader = schema.GetField("FamilyLoaderRules");
            Entity entity = new Entity(schema);
            string schemaString = RulesHost.SerializeToString();
            entity.Set<string>(familyLoader, schemaString);

            using (Transaction saveSettings = new Transaction(Fl_doc, "Save Settings"))
            {
                saveSettings.Start();
                Fl_doc.ProjectInformation.SetEntity(entity);
                saveSettings.Commit();
            }
        }

        public void RequestEnableUpdater()
        {
            //currently not being used, but if we want to enable the updater from the UI at some point, we can use this method
            MakeRequest(RequestId.EnableUpdater);
        }
        public void EnableUpdater()
        {
            List<UpdaterInfo> updaterInfos = UpdaterRegistry.GetRegisteredUpdaterInfos(Fl_doc).ToList();
            foreach (UpdaterInfo updaterInfo in updaterInfos)
            {
                if (updaterInfo.UpdaterName != "TypeUpdater")
                {
                    continue;
                }
                try
                {
                    TypeUpdater typeUpdater_old = new TypeUpdater(activeAddInId, this);
                    if (UpdaterRegistry.IsUpdaterRegistered(typeUpdater_old.GetUpdaterId()))
                    {
                        if (UpdaterRegistry.IsUpdaterEnabled(typeUpdater_old.GetUpdaterId()))
                        {
                            return;
                        }
                        UpdaterRegistry.EnableUpdater(typeUpdater_old.GetUpdaterId());
                        return;
                    }
                }
                catch (Exception ex)
                {
                    SimpleLog.Error("Failed to unregister TypeUpdater");
                    SimpleLog.Log(ex);
                }
            }
            TypeUpdater typeUpdater = new TypeUpdater(activeAddInId, this);
            UpdaterRegistry.RegisterUpdater(typeUpdater, true);
            ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
            UpdaterRegistry.AddTrigger(typeUpdater.GetUpdaterId(), familyFilter, Element.GetChangeTypeElementAddition());
        }
        public void RequestDisableUpdater()
        {
            //currently not being used, but if we want to disable the updater from the UI at some point, we can use this method
            MakeRequest(RequestId.DisableUpdater);
        }
        public void DisableUpdater()
        {
            List<UpdaterInfo> updaterInfos = UpdaterRegistry.GetRegisteredUpdaterInfos(Fl_doc).ToList();
            foreach (UpdaterInfo updaterInfo in updaterInfos)
            {
                if (updaterInfo.UpdaterName != "TypeUpdater")
                {
                    continue;
                }
                try
                {
                    TypeUpdater typeUpdater_old = new TypeUpdater(activeAddInId, this);
                    if (UpdaterRegistry.IsUpdaterRegistered(typeUpdater_old.GetUpdaterId()))
                    {
                        UpdaterRegistry.UnregisterUpdater(typeUpdater_old.GetUpdaterId());
                    }
                }
                catch (Exception ex)
                {
                    SimpleLog.Error("Failed to unregister TypeUpdater");
                    SimpleLog.Log(ex);
                }
            }
        }

        public void RequestEnableLoading(List<Rule> rules)
        {
            RulesHost.Rules = rules;
            MakeRequest(RequestId.EnableLoading);
        }

        public void RequestDisableLoading()
        {
            MakeRequest(RequestId.DisableLoading);
        }


        public void EnableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument += OnFamilyLoadingIntoDocument;
            RulesHost.IsEnabled = true;
        }

        public void DisableFamilyLoading()
        {
            m_app.FamilyLoadingIntoDocument -= OnFamilyLoadingIntoDocument;
            RulesHost.IsEnabled = false;
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
                List<DuplicateTypeHandler> typeHandlers = new List<DuplicateTypeHandler>();
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

                    DuplicateTypeHandler typeHandler = new DuplicateTypeHandler(newFamily, existingFamily, Fl_doc);
                    typeHandlers.Add(typeHandler);
                }

                if (typeHandlers.Count == 0)
                {
                    fixDuplicatesTrans.RollBack();
                }

                else if (typeHandlers.Count == 1)
                {
                    //show normal window
                    typeHandlers.First().ShowWindow();
                    typeHandlers.First().ResolveFamily(Fl_doc);
                    fixDuplicatesTrans.Commit();
                }

                else
                {
                    //show window for handling multiple duplicates
                    DuplicateTypeMultiViewModel duplicateMultiViewModel = new DuplicateTypeMultiViewModel(typeHandlers);
                    duplicateMultiViewModel.DuplicateMultiWindow.ShowDialog();
                    if (duplicateMultiViewModel.IsCanceled)
                    {
                        //if canceled just let Revit handle it like it normally would
                        fixDuplicatesTrans.RollBack();
                    }
                    else
                    {
                        typeHandlers = duplicateMultiViewModel.DuplicateTypeHandlers.ToList();
                        // and then process each handler according to its own stored settings
                        foreach (DuplicateTypeHandler dth in typeHandlers)
                        {
                            dth.ResolveFamily(Fl_doc);
                        }
                        fixDuplicatesTrans.Commit();
                    }

                }

                AddedIds.Clear();

            }
            uiApp.Idling -= new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
        }

        public void OnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            if (Fl_doc == null) return;
            if (!Fl_doc.Equals(e.CurrentActiveView.Document))
            {
                //before switching to a new document, save the rules of the current document
                Document newdoc = e.CurrentActiveView.Document;
                if (newdoc.IsFamilyDocument)
                {
                    FamilyDocument = newdoc;
                }
                else
                {
                    string oldDocTitle = Fl_doc.Title;
                    ModelRulesMap[oldDocTitle] = RulesHost;
                    // setting the Fl_doc will trigger the FamLoadHandler to load saved rules or create new ones
                    Fl_doc = newdoc;
                }
            }
            ViewModel.DocTitle = Fl_doc.Title;
        }

        public void MakeRequest(RequestId request)
        {
            Handler.Request.Make(request);
            ExternalEvent.Raise();
        }
    }

}

