using FamilyAuditorCore.Models;
using System;

namespace FamilyAuditorCore.Abstractions
{
    /// <summary>
    /// Interface for rule handlers that validate family information against specific rule criteria
    /// Implementations work with abstracted family data instead of direct CAD API objects
    /// </summary>
    public interface IFamilyRuleHandler
    {
        /// <summary>
        /// Determines if a rule is violated based on the provided family information
        /// </summary>
        /// <param name="rule">The rule to validate against</param>
        /// <param name="familyInfo">The family information to check</param>
        /// <param name="errorMessage">Output parameter containing the error message if rule is violated</param>
        /// <returns>True if the rule is violated, false if the rule passes</returns>
        bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage);
    }
    
    /// <summary>
    /// Factory for creating rule handlers that work with abstracted family data
    /// Provides a centralized way to get appropriate handlers for different rule types
    /// </summary>
    public static class FamilyRuleHandlerFactory
    {
        /// <summary>
        /// Gets the appropriate rule handler for the specified rule type
        /// </summary>
        /// <param name="ruleType">The type of rule that needs validation</param>
        /// <returns>A rule handler capable of validating the specified rule type</returns>
        /// <exception cref="NotImplementedException">Thrown when no handler is available for the specified rule type</exception>
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
                    throw new NotImplementedException($"No handler implemented for rule type {ruleType}");
            }
        }
    }
}