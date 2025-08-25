using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CollabAPIMEP.Views;
using CollabAPIMEP.Services;
using FamilyAuditorCore;
using FamilyAuditorCore.Abstractions;
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
using Rule = FamilyAuditorCore.Rule;

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
        private bool IsViewMonitoringActive;

        // Event-driven architecture - removed direct ViewModel reference
        // The ViewModel will subscribe to these events instead
        public event EventHandler RulesHostChanged;
        public event EventHandler DocTitleChanged;

        private string _rulesJson;
        public string RulesJson
        {
            get => _rulesJson;
        }

        private RulesContainer _rulesHost;
        public RulesContainer RulesHost
        {
            get
            {
                if (_rulesHost == null)
                {
                    _rulesHost = new RulesContainer(Fl_doc?.Title ?? "Unknown");
                }
                return _rulesHost;
            }
            set
            {
                _rulesHost = value;
                RulesHostChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _docTitle;
        public string DocTitle
        {
            get => _docTitle;
            set
            {
                if (_docTitle != value)
                {
                    _docTitle = value;
                    DocTitleChanged?.Invoke(this, EventArgs.Empty);
                }
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
                    _fl_doc = value;
                    //user switches between documents, so we check if there are rules in memory, in schema or create default ones
                    if (_fl_doc != null)
                    {
                        RulesContainer docRules;
                        ModelRulesMap.TryGetValue(_fl_doc.Title, out docRules);
                        if (docRules != null)
                        {
                            // if the rules were modified in the current session, use the modified rules
                            RulesHost = docRules;
                        }
                        else
                        {
                            if (!GetRulesFromSchema())
                            {
                                // new model, so create a new RulesHost with default rules
                                RulesHost = new RulesContainer(Fl_doc.Title);
                                RulesHost.SetDefaultRules();
                            }
                        }
                        // Update DocTitle which will fire the event
                        DocTitle = _fl_doc.Title;
                    }
                }
            }
        }

        public Document FamilyDocument;
        public static List<ElementId> AddedIds = new List<ElementId>();
        public RequestHandler Handler { get; set; }
        public ExternalEvent ExternalEvent { get; set; }
        public List<string> Results = new List<string>();

        private IFamilyDataExtractor _familyDataExtractor;
        private FamilyValidator _familyValidator;

        public FamilyLoadHandler(AddInId activeAddInId)
        {
            this.activeAddInId = activeAddInId;
            _familyDataExtractor = new RevitFamilyDataExtractor();
            _familyValidator = new FamilyValidator();
        }

        public void InitializeApp(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_app = uiApp.Application;
        }
        public void Initialize(UIApplication uiapp)
        {
            InitializeApp(uiapp);

            ActivateDocInit();
            if (!IsViewMonitoringActive)
            {
                uiApp.ViewActivated += OnViewActivated;
            }

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
                if (pathname != "NotSaved")
                {
                    FamilyDocument = m_app.OpenDocumentFile(pathname);
                }

                if (FamilyDocument == null)
                {
                    string errorMessage = "Could not open Family Document for auditing";
                    throw new RuleException(errorMessage);
                }

                try
                {
                    // Use the new abstracted validation system
                    var validationResult = _familyValidator.ValidateFamily(
                        FamilyDocument, 
                        pathname, 
                        RulesHost, 
                        _familyDataExtractor);

                    if (!validationResult.IsValid)
                    {
                        string errorMessage = $"family: '{e.FamilyName}' load canceled because:" + 
                                            Environment.NewLine + string.Join(Environment.NewLine, validationResult.ErrorMessages);
                        throw new RuleException(errorMessage);
                    }
                }
                catch (FamilyDataExtractionException ex)
                {
                    string errorMessage = $"Failed to extract family data: {ex.Message}";
                    throw new RuleException(errorMessage);
                }

                //TODO closing the family will cause the family to load in and bypass all the rules somehow, need to check this out
                //FamilyDocument.Close(false);
            }
        }

        public void RequestSaveRules(List<Rule> rules)
        {
            RulesHost.Rules = rules;
            MakeRequest(RequestId.SaveRules);
        }

        public void SerializeRules()
        {
            //event handlers removed and always enabled
            if (RulesHost.IsEnabled == true)
            {
                EnableFamilyLoading();
            }
            else
            {
                DisableFamilyLoading();
            }
            _rulesJson = RulesHost.SerializeToString();
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
            //List<UpdaterInfo> updaterInfos = UpdaterRegistry.GetRegisteredUpdaterInfos(Fl_doc).ToList();
            //foreach (UpdaterInfo updaterInfo in updaterInfos)
            //{
            //    if (updaterInfo.UpdaterName != "TypeUpdater")
            //    {
            //        continue;
            //    }
            //    try
            //    {
            //        TypeUpdater typeUpdater_old = new TypeUpdater(activeAddInId, this);
            //        if (UpdaterRegistry.IsUpdaterRegistered(typeUpdater_old.GetUpdaterId()))
            //        {
            //            if (UpdaterRegistry.IsUpdaterEnabled(typeUpdater_old.GetUpdaterId()))
            //            {
            //                return;
            //            }
            //            UpdaterRegistry.EnableUpdater(typeUpdater_old.GetUpdaterId());
            //            return;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        SimpleLog.Error("Failed to unregister TypeUpdater");
            //        SimpleLog.Log(ex);
            //    }
            //}
            //TypeUpdater typeUpdater = new TypeUpdater(activeAddInId, this);
            //UpdaterRegistry.RegisterUpdater(typeUpdater, true);
            //ElementClassFilter familyFilter = new ElementClassFilter(typeof(Family));
            //UpdaterRegistry.AddTrigger(typeUpdater.GetUpdaterId(), familyFilter, Element.GetChangeTypeElementAddition());
        }

        public void RequestDisableUpdater()
        {
            //currently not being used, but if we want to disable the updater from the UI at some point, we can use this method
            MakeRequest(RequestId.DisableUpdater);
        }

        public void DisableUpdater()
        {
            //List<UpdaterInfo> updaterInfos = UpdaterRegistry.GetRegisteredUpdaterInfos(Fl_doc).ToList();
            //foreach (UpdaterInfo updaterInfo in updaterInfos)
            //{
            //    if (updaterInfo.UpdaterName != "TypeUpdater")
            //    {
            //        continue;
            //    }
            //    try
            //    {
            //        TypeUpdater typeUpdater_old = new TypeUpdater(activeAddInId, this);
            //        if (UpdaterRegistry.IsUpdaterRegistered(typeUpdater_old.GetUpdaterId()))
            //        {
            //            UpdaterRegistry.UnregisterUpdater(typeUpdater_old.GetUpdaterId());
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        SimpleLog.Error("Failed to unregister TypeUpdater");
            //        SimpleLog.Log(ex);
            //    }
            //}
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
            //if (!AddedIds.Any())
            //{
            //    uiApp.Idling -= new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
            //    return;
            //}

            //FilteredElementCollector famCollector = new FilteredElementCollector(Fl_doc).OfClass(typeof(Family));

            //using (Transaction fixDuplicatesTrans = new Transaction(Fl_doc, "Fix Duplicates"))
            //{
            //    fixDuplicatesTrans.Start();
            //    List<DuplicateTypeHandler> typeHandlers = new List<DuplicateTypeHandler>();
            //    foreach (ElementId id in AddedIds)
            //    {
            //        Family newFamily = Fl_doc.GetElement(id) as Family;
            //        if (newFamily == null)
            //        {
            //            continue;
            //        }
            //        string newFamNam = newFamily.Name;
            //        // get existing family and a type
            //        string existingFamName = newFamNam.Substring(0, newFamNam.Length - 1);
            //        Family existingFamily = famCollector.FirstOrDefault(f => f.Name.Equals(existingFamName)) as Family;
            //        if (existingFamily == null)
            //        {
            //            continue;
            //        }

            //        DuplicateTypeHandler typeHandler = new DuplicateTypeHandler(newFamily, existingFamily, Fl_doc);
            //        typeHandlers.Add(typeHandler);
            //    }

            //    if (typeHandlers.Count == 0)
            //    {
            //        fixDuplicatesTrans.RollBack();
            //    }
            //    else if (typeHandlers.Count == 1)
            //    {
            //        //show normal window
            //        typeHandlers.First().ShowWindow();
            //        typeHandlers.First().ResolveFamily(Fl_doc);
            //        fixDuplicatesTrans.Commit();
            //    }
            //    else
            //    {
            //        //show window for handling multiple duplicates
            //        DuplicateTypeMultiViewModel duplicateMultiViewModel = new DuplicateTypeMultiViewModel(typeHandlers);
            //        duplicateMultiViewModel.DuplicateMultiWindow.ShowDialog();
            //        if (duplicateMultiViewModel.IsCanceled)
            //        {
            //            //if canceled just let Revit handle it like it normally would
            //            fixDuplicatesTrans.RollBack();
            //        }
            //        else
            //        {
            //            typeHandlers = duplicateMultiViewModel.DuplicateTypeHandlers.ToList();
            //            // and then process each handler according to its own stored settings
            //            foreach (DuplicateTypeHandler dth in typeHandlers)
            //            {
            //                dth.ResolveFamily(Fl_doc);
            //            }
            //            fixDuplicatesTrans.Commit();
            //        }
            //    }

            //    AddedIds.Clear();
            //}
            //uiApp.Idling -= new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
        }

        public void OnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            IsViewMonitoringActive = true;
            ActivateDocInit();
            IsViewMonitoringActive = false;
        }

        private void ActivateDocInit()
        {
            if (Fl_doc == null)
            {
                if (uiApp.ActiveUIDocument.Document.IsFamilyDocument)
                {
                    FamilyDocument = uiApp.ActiveUIDocument.Document;
                    return;
                }
                Fl_doc = uiApp.ActiveUIDocument.Document;
            }
            else if (!Fl_doc.Equals(uiApp.ActiveUIDocument.Document))
            {
                //before switching to a new document, save the rules of the current document
                Document newdoc = uiApp.ActiveUIDocument.Document;
                if (newdoc.IsFamilyDocument)
                {
                    FamilyDocument = newdoc;
                    return;
                }
                else
                {
                    string oldDocTitle = Fl_doc.Title;
                    ModelRulesMap[oldDocTitle] = RulesHost;
                    // setting the Fl_doc will trigger the FamLoadHandler to load saved rules or create new ones
                    Fl_doc = newdoc;
                }
            }
            // DocTitle is now set automatically in Fl_doc setter, which fires the event
        }

        public void MakeRequest(RequestId request)
        {
            Handler.Request.Make(request);
            ExternalEvent.Raise();
        }
    }
}

