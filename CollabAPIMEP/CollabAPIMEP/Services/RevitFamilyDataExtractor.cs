using Autodesk.Revit.DB;
using FamilyAuditorCore.Abstractions;
using FamilyAuditorCore.Models;
using System;
using System.IO;
using System.Linq;

namespace CollabAPIMEP.Services
{
    /// <summary>
    /// Revit-specific implementation of IFamilyDataExtractor
    /// Extracts family information from Revit API objects into abstracted FamilyInfo model
    /// This is an internal implementation class that bridges Revit API with FamilyAuditorCore
    /// </summary>
    public class RevitFamilyDataExtractor : IFamilyDataExtractor
    {
        /// <summary>
        /// Extracts family information from a Revit family document
        /// </summary>
        /// <param name="familyDocument">Revit Document object</param>
        /// <param name="filePath">Path to the family file</param>
        /// <returns>Abstracted family information</returns>
        public FamilyInfo ExtractFamilyInfo(object familyDocument, string filePath)
        {
            if (!CanProcess(familyDocument))
            {
                throw new FamilyDataExtractionException($"Cannot process family document of type {familyDocument?.GetType()}");
            }

            var doc = (Document)familyDocument;
            
            try
            {
                var familyInfo = new FamilyInfo
                {
                    FilePath = filePath,
                    FileSizeBytes = GetFileSize(filePath),
                    ElementCount = GetElementCount(doc),
                    ParameterCount = GetParameterCount(doc.FamilyManager),
                    ImportedInstanceCount = GetImportedInstanceCount(doc),
                    MaterialCount = GetMaterialCount(doc),
                    DetailLineCount = GetDetailLineCount(doc),
                    VertexCount = GetVertexCount(doc),
                    HasElementsWithoutSubcategory = HasElementsWithoutSubcategory(doc)
                };

                return familyInfo;
            }
            catch (Exception ex)
            {
                throw new FamilyDataExtractionException($"Failed to extract family info from {filePath}", ex);
            }
        }

        /// <summary>
        /// Gets parameter count from a Revit FamilyManager
        /// </summary>
        /// <param name="familyManager">Revit FamilyManager object</param>
        /// <returns>Number of parameters</returns>
        public int GetParameterCount(object familyManager)
        {
            if (familyManager is FamilyManager manager)
            {
                return manager.Parameters.Size;
            }
            throw new FamilyDataExtractionException($"Cannot extract parameter count from object of type {familyManager?.GetType()}");
        }

        /// <summary>
        /// Validates if the extractor can process the given family document
        /// </summary>
        /// <param name="familyDocument">Document to validate</param>
        /// <returns>True if this is a Revit family document</returns>
        public bool CanProcess(object familyDocument)
        {
            return familyDocument is Document doc && doc.IsFamilyDocument;
        }

        #region Private Helper Methods

        private long GetFileSize(string filePath)
        {
            if (filePath == "NotSaved" || string.IsNullOrEmpty(filePath))
            {
                return 0;
            }

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
            var collectorElements = new FilteredElementCollector(doc);
            var parRuleVisibility = ParameterFilterRuleFactory.CreateHasValueParameterRule(
                new ElementId((int)BuiltInParameter.IS_VISIBLE_PARAM));
            var filterVisibility = new ElementParameterFilter(parRuleVisibility);
            var elementsWithGeometry = collectorElements.WherePasses(filterVisibility).ToElements();
            return elementsWithGeometry.Count;
        }

        private int GetImportedInstanceCount(Document doc)
        {
            var colImportsAll = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance));
            var importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
            return importsLinks.Count;
        }

        private int GetMaterialCount(Document doc)
        {
            var materialCollector = new FilteredElementCollector(doc).OfClass(typeof(Material));
            var materials = materialCollector.ToElements();
            return materials.Count;
        }

        private int GetDetailLineCount(Document doc)
        {
            var colDetailLines = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Lines)
                .OfClass(typeof(CurveElement));
            var detailLines = colDetailLines.ToElements();
            return detailLines.Count;
        }

        private int GetVertexCount(Document doc)
        {
            int edgeCount = 0;
            var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
            var options = new Options();
            
            foreach (Element element in collector)
            {
                var geometryElement = element.get_Geometry(options);
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
                var instanceGeometry = instance.GetInstanceGeometry();
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

        private bool HasElementsWithoutSubcategory(Document doc)
        {
            var collector = new FilteredElementCollector(doc);
            var parRule = ParameterFilterRuleFactory.CreateHasValueParameterRule(
                new ElementId((int)BuiltInParameter.FAMILY_ELEM_SUBCATEGORY));
            var filter = new ElementParameterFilter(parRule);
            var filteredElements = collector.WherePasses(filter).ToElements();
            
            foreach (Element element in filteredElements)
            {
                var eleId = element.get_Parameter(BuiltInParameter.FAMILY_ELEM_SUBCATEGORY).AsElementId();
                if (eleId == ElementId.InvalidElementId)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}