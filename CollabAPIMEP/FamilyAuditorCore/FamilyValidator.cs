using FamilyAuditorCore.Abstractions;
using FamilyAuditorCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace FamilyAuditorCore
{
    /// <summary>
    /// Main validator class that validates family information against rules
    /// without direct dependencies on Revit API
    /// </summary>
    public class FamilyValidator
    {
        /// <summary>
        /// Validates a family against a collection of rules
        /// </summary>
        /// <param name="familyInfo">The abstracted family information</param>
        /// <param name="rules">The rules to validate against</param>
        /// <returns>Validation result with details of any violations</returns>
        public ValidationResult ValidateFamily(FamilyInfo familyInfo, IEnumerable<Rule> rules)
        {
            var result = new ValidationResult();
            
            foreach (var rule in rules.Where(r => r.IsEnabled))
            {
                try
                {
                    var handler = FamilyRuleHandlerFactory.GetRuleHandler(rule.TypeOfRule);
                    if (handler.IsRuleViolated(rule, familyInfo, out string errorMessage))
                    {
                        result.AddViolation(rule.TypeOfRule, errorMessage);
                    }
                }
                catch (System.Exception ex)
                {
                    result.AddViolation(rule.TypeOfRule, $"Error validating rule {rule.TypeOfRule}: {ex.Message}");
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Validates a family against a rules container
        /// </summary>
        /// <param name="familyInfo">The abstracted family information</param>
        /// <param name="rulesContainer">The rules container with all rules</param>
        /// <returns>Validation result with details of any violations</returns>
        public ValidationResult ValidateFamily(FamilyInfo familyInfo, RulesContainer rulesContainer)
        {
            return ValidateFamily(familyInfo, rulesContainer.Rules);
        }
    }
}