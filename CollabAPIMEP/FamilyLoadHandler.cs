using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CollabAPIMEP
{
    public class FamilyLoadHandler
    {
        public static Guid Settings = new Guid("c16f94f6-5f14-4f33-91fc-f69dd7ac0d05");
        public List<string> Pathnames = new List<string>();
        public List<string> LoadedNames = new List<string>();

        private UIApplication uiApp;
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document m_doc;
        public Dictionary<string, Rule> RulesMap;
        public FamilyLoadHandler(UIApplication uiapp)
        {
            uiApp = uiapp;
            m_app = uiapp.Application;
            m_doc = uiApp.ActiveUIDocument.Document;
        }

        public void GetRulesFromSchema()
        {
            Schema schema = Schema.Lookup(Settings);

            if (schema != null)
            {
                RulesMap = new Dictionary<string, Rule>();

                Entity retrievedEntity = m_doc.ProjectInformation.GetEntity(schema);
                string rulesString = retrievedEntity.Get<string>(schema.GetField("FamilyLoaderRules"));
                List<string> rulesStrings = rulesString.Split(Rule.RuleSeparator).ToList();
                foreach (string ruleString in rulesStrings)
                {
                    Rule rule = Rule.deserializeFromSchema(ruleString);
                    RulesMap.Add(rule.ID, rule);
                }
            }
        }

        public void ApplyRules(string pathname, List<Rule> rules)
        {

            Schema schema = Schema.Lookup(Settings);
            //TODO  Load rules from project information

            if (schema != null)
            {
                Entity retrievedEntity = m_doc.ProjectInformation.GetEntity(schema);
                string rulesString = retrievedEntity.Get<string>(schema.GetField("FamilyLoaderRules"));


                Document familyDocument = m_app.OpenDocumentFile(pathname);
                foreach (Rule rule in rules)
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
                                familyDocument.Close(false);
                                throw new RuleException($"{elementCount} elements inside family, loading family canceled");
                            }
                            break;
                        case "ImportedInstances":
                            FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));
                            IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
                            int importCount = importsLinks.Count;
                            if (importCount > 0)
                            {
                                familyDocument.Close(false);
                                throw new RuleException($"{importCount} imported instances inside family, loading family canceled");
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

        public void SaveSettings(List<Rule> rules)
        {
            Schema schema = Schema.Lookup(FamilyLoadHandler.Settings);



            if (schema == null)
            {

                SchemaBuilder schemabuilder = new SchemaBuilder(FamilyLoadHandler.Settings);
                schemabuilder.SetReadAccessLevel(AccessLevel.Public);
                schemabuilder.SetWriteAccessLevel(AccessLevel.Public);
                schemabuilder.SetVendorId("MEPAPI");

                FieldBuilder fieldbuilder = schemabuilder.AddSimpleField("FamilyLoaderRules", typeof(string));

                fieldbuilder.SetDocumentation("FamilyLoader Rules");
                schemabuilder.SetSchemaName("FamilyLoader");
                schema = schemabuilder.Finish();

            }


            Field familyLoader = schema.GetField("FamilyLoaderRules");

            Entity entity = new Entity(schema);

            string schemaString = "";

            int ruleCount = 1;

            foreach (var rule in rules)
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

            Transaction saveSettings = new Transaction(m_doc, "Save Settings");
            saveSettings.Start();

            m_doc.ProjectInformation.SetEntity(entity);

            saveSettings.Commit();

        }

    }

}

