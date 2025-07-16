using System.Collections.Generic;

namespace FamilyAuditorCore.Models
{
    /// <summary>
    /// Represents the result of a family validation against rules
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public List<RuleViolation> Violations { get; set; } = new List<RuleViolation>();
        
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
    /// Represents a specific rule violation
    /// </summary>
    public class RuleViolation
    {
        public RuleType RuleType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}