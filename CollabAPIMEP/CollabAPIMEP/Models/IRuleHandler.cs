using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CollabAPIMEP.Models
{
    public interface IRuleHandler
    {
        bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage);
    }

    public static class RuleHandlerFactory
    {
        public static IRuleHandler GetRuleHandler(RuleType ruleType)
        {
            IRuleHandler ruleHandler = null;
            switch (ruleType)
            {
                case RuleType.FileSize:
                    ruleHandler = new FileSizeRuleHandler();
                    break;
                case RuleType.NumberOfParameters:
                    ruleHandler = new NumberOfParametersRuleHandler();
                    break;
                case RuleType.NumberOfElements:
                    ruleHandler = new NumberOfElementsRuleHandler();
                    break;
                case RuleType.ImportedInstances:
                    ruleHandler = new ImportedInstancesRuleHandler();
                    break;
                case RuleType.SubCategory:
                    ruleHandler = new SubCategoryRuleHandler();
                    break;
                case RuleType.Material:
                    ruleHandler = new MaterialRuleHandler();
                    break;
                case RuleType.DetailLines:
                    ruleHandler = new DetailLinesRuleHandler();
                    break;
                case RuleType.Vertices:
                    ruleHandler = new VerticesRuleHandler();
                    break;
                default:
                    throw new NotImplementedException($"No handler implemented for rule type {ruleType}");
            }

            return ruleHandler;
        }
    }

    public class FileSizeRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (pathname == "NotSaved")
            {
                return false;
            }
            var maxFileSizeMB = Convert.ToInt32(rule.UserInput);
            FileInfo fileInfo = new FileInfo(pathname);
            var fileSizeMB = fileInfo.Length / (1024 * 1024); // Convert bytes to MB
            if (fileSizeMB > maxFileSizeMB)
            {
                errorMessage = $"- file size too large ({fileSizeMB} MB, only {maxFileSizeMB} MB allowed)";
                return true;
            }
            return false;
        }
    }

    public class NumberOfParametersRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxParameters = Convert.ToInt32(rule.UserInput);
            int parameterCount = familyManager.Parameters.Size;
            if (parameterCount > maxParameters)
            {
                errorMessage = $"- too many parameters inside family ({parameterCount}, only {maxParameters} allowed)";
                return true;
            }
            return false;
        }
    }

    public class NumberOfElementsRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxElements = Convert.ToInt32(rule.UserInput);
            FilteredElementCollector collectorElements = new FilteredElementCollector(familyDocument);
            FilterRule parRuleVisibility = ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(((int)BuiltInParameter.IS_VISIBLE_PARAM)));
            ElementParameterFilter filterVisibility = new ElementParameterFilter(parRuleVisibility);
            IList<Element> elementsWithGeometry = collectorElements.WherePasses(filterVisibility).ToElements();
            int elementCount = elementsWithGeometry.Count;
            if (elementCount > maxElements)
            {
                errorMessage = $"- too many elements inside family ({elementCount}, only {maxElements} allowed)";
                return true;
            }
            return false;
        }
    }

    public class ImportedInstancesRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            FilteredElementCollector colImportsAll = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance));
            IList<Element> importsLinks = colImportsAll.WhereElementIsNotElementType().ToElements();
            int importCount = importsLinks.Count;
            if (importCount > 0)
            {
                errorMessage = $"- too many imported instances inside family ({importCount})";
                return true;
            }
            return false;
        }
    }

    public class SubCategoryRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            FilteredElementCollector collector = new FilteredElementCollector(familyDocument);
            FilterRule parRule = ParameterFilterRuleFactory.CreateHasValueParameterRule(new ElementId(((int)BuiltInParameter.FAMILY_ELEM_SUBCATEGORY)));
            ElementParameterFilter filter = new ElementParameterFilter(parRule);
            IList<Element> filteredElements = collector.WherePasses(filter).ToElements();
            foreach (Element element in filteredElements)
            {
                ElementId eleId = element.get_Parameter(BuiltInParameter.FAMILY_ELEM_SUBCATEGORY).AsElementId();
                if (eleId == ElementId.InvalidElementId)
                {
                    errorMessage = "- elements without subcategory found";
                    return true;
                }
            }
            return false;
        }
    }

    public class MaterialRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxMaterials = Convert.ToInt32(rule.UserInput);
            FilteredElementCollector materialCollector = new FilteredElementCollector(familyDocument).OfClass(typeof(Material));
            IList<Element> materials = materialCollector.ToElements();
            if (materials.Count > maxMaterials)
            {
                errorMessage = $"- too many materials inside family ({materials.Count}, only {maxMaterials} allowed)";
                return true;
            }
            return false;
        }
    }

    public class DetailLinesRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxDetailLines = Convert.ToInt32(rule.UserInput);
            FilteredElementCollector colDetailLines = new FilteredElementCollector(familyDocument).OfCategory(BuiltInCategory.OST_Lines).OfClass(typeof(CurveElement));
            IList<Element> detailLines = colDetailLines.ToElements();
            int detailLineCount = detailLines.Count;
            if (detailLineCount > maxDetailLines)
            {
                errorMessage = $"- too many detail lines inside family ({detailLineCount}, only {maxDetailLines} allowed)";
                return true;
            }
            return false;
        }
    }

    public class VerticesRuleHandler : IRuleHandler
    {
        public bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, Document familyDocument, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxVertices = Convert.ToInt32(rule.UserInput);
            int verticesCount = CountEdges(familyDocument);
            if (verticesCount > maxVertices)
            {
                errorMessage = $"- too many vertices inside family ({verticesCount}, only {maxVertices} allowed)";
                return true;
            }
            return false;
        }

        private int CountEdges(Document familyDocument)
        {
            int edgeCount = 0;
            FilteredElementCollector collector = new FilteredElementCollector(familyDocument).WhereElementIsNotElementType();
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
