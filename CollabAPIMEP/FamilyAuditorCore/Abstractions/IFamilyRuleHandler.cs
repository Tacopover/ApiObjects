using FamilyAuditorCore.Models;

namespace FamilyAuditorCore.Abstractions
{
    /// <summary>
    /// Interface for rule handlers that work with abstracted family data
    /// instead of direct Revit API objects
    /// </summary>
    public interface IFamilyRuleHandler
    {
        bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage);
    }
    
    /// <summary>
    /// Factory for creating rule handlers that work with abstracted data
    /// </summary>
    public static class FamilyRuleHandlerFactory
    {
        public static IFamilyRuleHandler GetRuleHandler(RuleType ruleType)
        {
            switch (ruleType)
            {
                case RuleType.FileSize:
                    return new FileSizeFamilyRuleHandler();
                case RuleType.NumberOfParameters:
                    return new NumberOfParametersFamilyRuleHandler();
                case RuleType.NumberOfElements:
                    return new NumberOfElementsFamilyRuleHandler();
                case RuleType.ImportedInstances:
                    return new ImportedInstancesFamilyRuleHandler();
                case RuleType.SubCategory:
                    return new SubCategoryFamilyRuleHandler();
                case RuleType.Material:
                    return new MaterialFamilyRuleHandler();
                case RuleType.DetailLines:
                    return new DetailLinesFamilyRuleHandler();
                case RuleType.Vertices:
                    return new VerticesFamilyRuleHandler();
                default:
                    throw new System.NotImplementedException($"No handler implemented for rule type {ruleType}");
            }
        }
    }
}