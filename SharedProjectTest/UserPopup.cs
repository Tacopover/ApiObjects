using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
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

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            string assemblyTitle = fvi.FileDescription;
            string assemblyVersion = fvi.ProductVersion;

            if (doc.ProjectInformation != null)
            {
                Schema schema = Schema.Lookup(Settings);

                if (schema != null)
                {
                    MessageBox.Show(assemblyTitle + " " + assemblyVersion + " is activated");

                }

                else
                {
                    MessageBox.Show(assemblyTitle + " " + assemblyVersion + " is deactivated");

                }

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
