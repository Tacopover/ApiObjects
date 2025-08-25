using FamilyAuditorCore.Abstractions;
using FamilyAuditorCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace FamilyAuditorCore
{
    /// <summary>
    /// Main validator class that validates family information against configurable rules
    /// without direct dependencies on CAD system APIs. This is the core orchestrator
    /// for the family validation process.
    /// </summary>
    public class FamilyValidator
    {
        /// <summary>
        /// Validates a family against a collection of rules using pre-extracted family information
        /// This is the most direct validation method when family data is already available
        /// </summary>
        /// <param name="familyInfo">The abstracted family information containing all measurable properties</param>
        /// <param name="rules">The collection of validation rules to check against</param>
        /// <returns>Validation result containing success/failure status and detailed violation information</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when familyInfo or rules is null</exception>
        public ValidationResult ValidateFamily(FamilyInfo familyInfo, IEnumerable<Rule> rules)
        {
            if (familyInfo == null)
                throw new System.ArgumentNullException(nameof(familyInfo));
            if (rules == null)
                throw new System.ArgumentNullException(nameof(rules));

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
        /// Validates a family against a rules container using pre-extracted family information
        /// Convenience method that uses the rules from a RulesContainer
        /// </summary>
        /// <param name="familyInfo">The abstracted family information</param>
        /// <param name="rulesContainer">The rules container with all configured rules</param>
        /// <returns>Validation result with details of any violations</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when familyInfo or rulesContainer is null</exception>
        public ValidationResult ValidateFamily(FamilyInfo familyInfo, RulesContainer rulesContainer)
        {
            if (rulesContainer == null)
                throw new System.ArgumentNullException(nameof(rulesContainer));

            return ValidateFamily(familyInfo, rulesContainer.Rules);
        }
        
        /// <summary>
        /// Validates a family by extracting data from CAD system objects first
        /// This method bridges the gap between CAD-specific objects and the validation framework
        /// </summary>
        /// <param name="familyDocument">The family document from the CAD system (e.g., Revit Document)</param>
        /// <param name="filePath">Path to the family file on disk</param>
        /// <param name="rules">The collection of validation rules to check against</param>
        /// <param name="dataExtractor">The data extractor implementation for the specific CAD system</param>
        /// <returns>Validation result with details of any violations</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
        /// <exception cref="FamilyDataExtractionException">Thrown when data extraction fails</exception>
        public ValidationResult ValidateFamily(object familyDocument, string filePath, 
            IEnumerable<Rule> rules, IFamilyDataExtractor dataExtractor)
        {
            if (familyDocument == null)
                throw new System.ArgumentNullException(nameof(familyDocument));
            if (dataExtractor == null)
                throw new System.ArgumentNullException(nameof(dataExtractor));
            if (rules == null)
                throw new System.ArgumentNullException(nameof(rules));

            if (!dataExtractor.CanProcess(familyDocument))
            {
                var result = new ValidationResult();
                result.AddViolation(RuleType.NumberOfElements, "Cannot process family document with this extractor");
                return result;
            }
            
            var familyInfo = dataExtractor.ExtractFamilyInfo(familyDocument, filePath);
            return ValidateFamily(familyInfo, rules);
        }
        
        /// <summary>
        /// Validates a family by extracting data from CAD system objects first
        /// Convenience overload that uses a RulesContainer for the validation rules
        /// </summary>
        /// <param name="familyDocument">The family document from the CAD system</param>
        /// <param name="filePath">Path to the family file</param>
        /// <param name="rulesContainer">The rules container with all configured rules</param>
        /// <param name="dataExtractor">The data extractor for the specific CAD system</param>
        /// <returns>Validation result with details of any violations</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
        /// <exception cref="FamilyDataExtractionException">Thrown when data extraction fails</exception>
        public ValidationResult ValidateFamily(object familyDocument, string filePath, 
            RulesContainer rulesContainer, IFamilyDataExtractor dataExtractor)
        {
            if (rulesContainer == null)
                throw new System.ArgumentNullException(nameof(rulesContainer));

            return ValidateFamily(familyDocument, filePath, rulesContainer.Rules, dataExtractor);
        }
    }
}