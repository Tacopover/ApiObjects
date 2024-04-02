using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CollabAPIMEP
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class TestReference2024 : IExternalCommand
    {
        private MainViewModel mainViewModel;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;

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
                if(familySymbolTemp != null && familySymbolTemp.Family != null)
                {
                    if(family.FamilyPlacementType == FamilyPlacementType.OneLevelBased || family.FamilyPlacementType == FamilyPlacementType.WorkPlaneBased)
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

            return Result.Succeeded;


        }

    }

    
}
