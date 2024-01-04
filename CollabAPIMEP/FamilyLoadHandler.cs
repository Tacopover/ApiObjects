﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

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

        public void ApplyRules(string pathname, List<Rule> rules)
        {
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

}

