# Integration Guide: Using the New FamilyAuditorCore Abstraction

## Overview

This guide explains how to integrate the new abstracted FamilyAuditorCore with the existing CollabAPIMEP projects.

## Required Changes

### 1. Create RevitFamilyDataExtractor

Create a new class in the CollabAPIMEP shared project:

```csharp
// File: CollabAPIMEP/Helpers/RevitFamilyDataExtractor.cs
using Autodesk.Revit.DB;
using FamilyAuditorCore.Abstractions;
using FamilyAuditorCore.Models;
using System.IO;

namespace CollabAPIMEP.Helpers
{
    public class RevitFamilyDataExtractor : IFamilyDataExtractor
    {
        public FamilyInfo ExtractFamilyInfo(object familyDocument, string filePath)
        {
            var doc = (Document)familyDocument;
            var familyManager = doc.FamilyManager;
            
            return new FamilyInfo
            {
                FilePath = filePath,
                FileSizeBytes = GetFileSize(filePath),
                ElementCount = GetElementCount(doc),
                ParameterCount = familyManager.Parameters.Size,
                ImportedInstanceCount = GetImportedInstanceCount(doc),
                MaterialCount = GetMaterialCount(doc),
                DetailLineCount = GetDetailLineCount(doc),
                VertexCount = GetVertexCount(doc),
                HasElementsWithoutSubcategory = HasElementsWithoutSubcategory(doc)
            };
        }
        
        public int GetParameterCount(object familyManager)
        {
            var fm = (FamilyManager)familyManager;
            return fm.Parameters.Size;
        }
        
        public bool CanProcess(object familyDocument)
        {
            return familyDocument is Document doc && doc.IsFamilyDocument;
        }
        
        // Helper methods that extract the same data as the current IRuleHandler implementations
        private long GetFileSize(string filePath)
        {
            if (filePath == "NotSaved") return 0;
            try
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch
            {
                return 0;
            }
        }
        
        private int GetElementCount(Document doc)
        {
            FilteredElementCollector collectorElements = new FilteredElementCollector(doc);
            FilterRule parRuleVisibility = ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(((int)BuiltInParameter.IS_VISIBLE_PARAM)));
            ElementParameterFilter filterVisibility = new ElementParameterFilter(parRuleVisibility);
            IList<Element> elementsWithGeometry = collectorElements.WherePasses(filterVisibility).ToElements();
            return elementsWithGeometry.Count;
        }
        
        private int GetImportedInstanceCount(Document doc)
        {
            FilteredElementCollector colImportsAll = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance));
            IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
            return importsLinks.Count;
        }
        
        private int GetMaterialCount(Document doc)
        {
            FilteredElementCollector materialCollector = new FilteredElementCollector(doc).OfClass(typeof(Material));
            return materialCollector.ToElements().Count;
        }
        
        private int GetDetailLineCount(Document doc)
        {
            FilteredElementCollector colDetailLines = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Lines).OfClass(typeof(CurveElement));
            return colDetailLines.ToElements().Count;
        }
        
        private int GetVertexCount(Document doc)
        {
            int edgeCount = 0;
            FilteredElementCollector collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
            Options options = new Options();
            foreach (Element element in collector)
            {
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
        
        private bool HasElementsWithoutSubcategory(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilterRule parRule = ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(((int)BuiltInParameter.FAMILY_ELEM_SUBCATEGORY)));
            ElementParameterFilter filter = new ElementParameterFilter(parRule);
            IList<Element> filteredElements = collector.WherePasses(filter).ToElements();
            foreach (Element element in filteredElements)
            {
                ElementId eleId = element.get_Parameter(BuiltInParameter.FAMILY_ELEM_SUBCATEGORY).AsElementId();
                if (eleId == ElementId.InvalidElementId)
                {
                    return true;
                }
            }
            return false;
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
    }
}
```

### 2. Update FamilyLoadHandler

Modify the FamilyLoadHandler to use the new abstraction:

```csharp
// In FamilyLoadHandler.cs, replace direct rule validation with:

private RevitFamilyDataExtractor _dataExtractor = new RevitFamilyDataExtractor();
private FamilyValidator _validator = new FamilyValidator();

public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
{
    // ... existing code ...
    
    // Extract family info once
    var familyInfo = _dataExtractor.ExtractFamilyInfo(family.Document, family.Document.PathName);
    
    // Validate using abstracted data
    var result = _validator.ValidateFamily(familyInfo, RulesHost);
    
    if (!result.IsValid)
    {
        string errorMessage = string.Join("\n", result.ErrorMessages);
        SimpleLog.Log(errorMessage);
        return false; // Reject family
    }
    
    return true; // Accept family
}
```

### 3. Benefits of This Change

1. **Separation of Concerns**: Data extraction is separate from validation logic
2. **Performance**: Family data is extracted once, can be validated against multiple rule sets
3. **Testability**: Core validation logic can be unit tested without Revit
4. **Maintainability**: Adding new rules only requires updating the core library
5. **Future-Proofing**: Could potentially support other CAD systems

### 4. Migration Strategy

1. **Phase 1**: Implement RevitFamilyDataExtractor alongside existing code
2. **Phase 2**: Update FamilyLoadHandler to use new abstraction
3. **Phase 3**: Remove old IRuleHandler implementations (they'll still exist in FamilyAuditorCore for reference)
4. **Phase 4**: Remove FamilyAuditor_Core project entirely

### 5. Backward Compatibility

The old IRuleHandler interface and implementations will remain in FamilyAuditorCore but should be considered deprecated. The new IFamilyRuleHandler interface provides the same functionality with better separation of concerns.

## Testing the Integration

Create unit tests that validate the data extraction:

```csharp
[Test]
public void RevitDataExtractor_ExtractsCorrectData()
{
    // This would require a test Revit document
    var extractor = new RevitFamilyDataExtractor();
    var familyInfo = extractor.ExtractFamilyInfo(testDocument, testPath);
    
    Assert.That(familyInfo.ElementCount, Is.GreaterThan(0));
    Assert.That(familyInfo.ParameterCount, Is.GreaterThan(0));
    // ... other assertions
}
```

This approach provides a clean migration path while maintaining all existing functionality.