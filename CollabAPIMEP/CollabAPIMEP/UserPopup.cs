using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace CollabAPIMEP
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class UserPopup : IExternalCommand
    {
        private MainViewModel mainViewModel;

        private Document doc = null;

        public static Guid Settings = new Guid("c16f94f6-5f14-4f33-91fc-f69dd7ac0d05");

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            doc = commandData.Application.ActiveUIDocument.Document;

            UIApplication uIApp = commandData.Application;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            string assemblyTitle = fvi.FileDescription;
            string assemblyVersion = fvi.ProductVersion;

            FamilyLoadHandler currentFamilyLoadHandler = FamilyLoaderApplication.LookupFamilyLoadhandler(uIApp);

            if (doc.ProjectInformation != null && currentFamilyLoadHandler != null && currentFamilyLoadHandler.RulesEnabled == true)
            {

                string popupMessage =  "Rules applied to project:" + System.Environment.NewLine;

                foreach (Rule rule in currentFamilyLoadHandler.Rules)
                {
                    if (!rule.IsEnabled)
                    {
                        continue;
                    }

                    switch (rule.ID)
                    {


                        case "NumberOfElements":

                            var maxElements = Convert.ToInt32(rule.UserInput);

                            popupMessage += $"- maximum {maxElements} elements allowed" + System.Environment.NewLine;
                            
                            break;
                        case "ImportedInstances":
                            popupMessage += "- no imported instances allowed" + System.Environment.NewLine;
                            break;
                        case "SubCategory":
                            popupMessage += "- no geometry without allowed" + System.Environment.NewLine;  
                            break;
                        case "Material":

                            var maxMaterials = Convert.ToInt32(rule.UserInput);

                            popupMessage += $"- maximum {maxMaterials} materials allowed" + System.Environment.NewLine;
                            break;
                    }

                }

                MessageBox.Show(popupMessage, assemblyTitle + " " + assemblyVersion);

            }

            else
            {
                MessageBox.Show(assemblyTitle + " " + assemblyVersion + " is deactivated");

            }

            return Result.Succeeded;

        }

        private void testRevitReference()
        {

            Transaction trans = new Transaction(doc, "test reference");
            trans.Start();

            // Get default family symbol
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var familySymbols = collector.OfCategory(BuiltInCategory.OST_GenericModel).OfClass((typeof(FamilySymbol))).ToElements();
            FamilySymbol familySymbol = null;


            foreach (Element element in familySymbols)
            {
                FamilySymbol familySymbolTemp = element as FamilySymbol;
                Family family = familySymbolTemp.Family;
                if (familySymbolTemp != null && familySymbolTemp.Family != null)
                {
                    if (family.FamilyPlacementType == FamilyPlacementType.OneLevelBased || family.FamilyPlacementType == FamilyPlacementType.WorkPlaneBased)
                    {
                        familySymbol = familySymbolTemp;
                    }

                }
            }


            // Get default level
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            var level = levelCollector.OfClass(typeof(Level)).FirstElement() as Level;

            // Define default location
            XYZ location = new XYZ(0, 0, 0);

            doc.Create.NewFamilyInstance(location, familySymbol, level, StructuralType.NonStructural);

            trans.Commit();
        }

    }

    
}
