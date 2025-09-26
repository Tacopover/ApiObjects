using FamilyAuditorCore.Models;
using System;

namespace FamilyAuditorCore.Abstractions
{
    public class FileSizeFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (familyInfo.FilePath == "NotSaved" || familyInfo.FileSizeBytes == 0)
            {
                return false;
            }
            var maxFileSizeMB = Convert.ToInt32(rule.UserInput);
            var fileSizeMB = familyInfo.FileSizeBytes / (1024 * 1024); // Convert bytes to MB
            if (fileSizeMB > maxFileSizeMB)
            {
                errorMessage = $"- file size too large ({fileSizeMB} MB, only {maxFileSizeMB} MB allowed)";
                return true;
            }
            return false;
        }
    }

    public class NumberOfParametersFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxParameters = Convert.ToInt32(rule.UserInput);
            if (familyInfo.ParameterCount > maxParameters)
            {
                errorMessage = $"- too many parameters inside family ({familyInfo.ParameterCount}, only {maxParameters} allowed)";
                return true;
            }
            return false;
        }
    }

    public class NumberOfElementsFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxElements = Convert.ToInt32(rule.UserInput);
            if (familyInfo.ElementCount > maxElements)
            {
                errorMessage = $"- too many elements inside family ({familyInfo.ElementCount}, only {maxElements} allowed)";
                return true;
            }
            return false;
        }
    }

    public class ImportedInstancesFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (familyInfo.ImportedInstanceCount > 0)
            {
                errorMessage = $"- too many imported instances inside family ({familyInfo.ImportedInstanceCount})";
                return true;
            }
            return false;
        }
    }

    public class SubCategoryFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (familyInfo.HasElementsWithoutSubcategory)
            {
                errorMessage = "- elements without subcategory found";
                return true;
            }
            return false;
        }
    }

    public class MaterialFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxMaterials = Convert.ToInt32(rule.UserInput);
            if (familyInfo.MaterialCount > maxMaterials)
            {
                errorMessage = $"- too many materials inside family ({familyInfo.MaterialCount}, only {maxMaterials} allowed)";
                return true;
            }
            return false;
        }
    }

    public class DetailLinesFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxDetailLines = Convert.ToInt32(rule.UserInput);
            if (familyInfo.DetailLineCount > maxDetailLines)
            {
                errorMessage = $"- too many detail lines inside family ({familyInfo.DetailLineCount}, only {maxDetailLines} allowed)";
                return true;
            }
            return false;
        }
    }

    public class VerticesFamilyRuleHandler : IFamilyRuleHandler
    {
        public bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            var maxVertices = Convert.ToInt32(rule.UserInput);
            if (familyInfo.VertexCount > maxVertices)
            {
                errorMessage = $"- too many vertices inside family ({familyInfo.VertexCount}, only {maxVertices} allowed)";
                return true;
            }
            return false;
        }
    }
}