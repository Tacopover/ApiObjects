using System.Collections.Generic;

namespace FamilyAuditorCore.Models
{
    /// <summary>
    /// Represents the result of a family validation against a set of rules
    /// Contains information about whether validation passed and details of any violations
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets whether the family passed all validation rules
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of error messages from rule violations
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the detailed list of rule violations
        /// </summary>
        public List<RuleViolation> Violations { get; set; } = new List<RuleViolation>();
        
        /// <summary>
        /// Adds a rule violation to the validation result
        /// This marks the validation as failed and records the violation details
        /// </summary>
        /// <param name="ruleType">The type of rule that was violated</param>
        /// <param name="errorMessage">Descriptive error message for the violation</param>
        public void AddViolation(RuleType ruleType, string errorMessage)
        {
            IsValid = false;
            ErrorMessages.Add(errorMessage);
            Violations.Add(new RuleViolation 
            { 
                RuleType = ruleType, 
                ErrorMessage = errorMessage 
            });
        }
    }
    
    /// <summary>
    /// Represents a specific rule violation with details about what rule was broken
    /// </summary>
    public class RuleViolation
    {
        /// <summary>
        /// Gets or sets the type of rule that was violated
        /// </summary>
        public RuleType RuleType { get; set; }

        /// <summary>
        /// Gets or sets the descriptive error message explaining the violation
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}