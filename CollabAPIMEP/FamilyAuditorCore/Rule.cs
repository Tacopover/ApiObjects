using System;

namespace FamilyAuditorCore
{
    /// <summary>
    /// Represents a validation rule for family auditing
    /// Rules define criteria that families must meet to be considered valid
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// Gets the total number of default rules available
        /// </summary>
        public static int NumberOfDefaultRules = Enum.GetNames(typeof(RuleType)).Length;

        /// <summary>
        /// Gets or sets whether this rule is enabled for validation
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the type of rule this represents
        /// </summary>
        public RuleType TypeOfRule { get; set; }

        /// <summary>
        /// Gets or sets the display name of the rule
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of what this rule validates
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement for this rule (e.g., "MB", "count")
        /// </summary>
        public string Unit { get; set; } = "";

        private string _userInput;

        /// <summary>
        /// Gets or sets the user-defined threshold value for this rule
        /// Returns "N/A" if no value is set
        /// </summary>
        public string UserInput
        {
            get
            {
                if (_userInput == null)
                {
                    return "N/A";
                }
                else
                {
                    return _userInput;
                }
            }
            set
            {
                _userInput = value;
            }
        }

        private bool _isUserInputEnabled;

        /// <summary>
        /// Gets or sets whether user input is enabled for this rule type
        /// Some rules may have fixed criteria and don't require user input
        /// </summary>
        public bool IsUserInputEnabled
        {
            get { return _isUserInputEnabled; }
            set
            {
                _isUserInputEnabled = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Rule class
        /// </summary>
        /// <param name="ruleEnum">The type of rule to create</param>
        /// <param name="userInput">Optional user-defined threshold value</param>
        public Rule(RuleType ruleEnum, string userInput = null)
        {
            TypeOfRule = ruleEnum;
            IsEnabled = false;
            if (userInput == null)
            {
                _isUserInputEnabled = false;
            }
            else
            {
                _isUserInputEnabled = true;
            }
            _userInput = userInput;
            UpdateDescription();
        }

        /// <summary>
        /// Updates the rule description based on the rule type
        /// </summary>
        private void UpdateDescription()
        {
            switch (TypeOfRule)
            {
                case RuleType.NumberOfElements:
                    Description = $"This rule will check the number of elements in the family. If the number of elements is greater than specified, the family will not be loaded into the project.";
                    break;
                case RuleType.ImportedInstances:
                    Description = $"This rule will check the number of imported instances in the family. If the number of imported instances is greater than specified, the family will not be loaded into the project.";
                    break;
                case RuleType.SubCategory:
                    Description = "This rule will check if every piece of geometry in the family is assigned to a subcategory. If not, the family will not be loaded into the project.";
                    break;
                case RuleType.Material:
                    Description = $"This rule will check the number of materials in a family. If the number is greater than specified, the family will not be loaded into the project.";
                    break;
                case RuleType.NumberOfParameters:
                    Description = $"This rule will check the number of parameters in a family. If the number is greater than specified, the family will not be loaded into the project.";
                    break;
                case RuleType.FileSize:
                    Description = $"This rule will check the file size of the family. If the file size is greater than specified, the family will not be loaded into the project. \nThis rule does not work on families that have not been saved, i.e. Edit Family -> Load into Project";
                    break;
                case RuleType.DetailLines:
                    Description = $"This rule will check the number of (symbolic) lines in the family. If the number is greater than specified, the family will not be loaded into the project.";
                    break;
                case RuleType.Vertices:
                    Description = $"This rule will check the number of vertices in the family geometry. If the number is greater than specified, the family will not be loaded into the project.";
                    break;
            }
        }
    }

    /// <summary>
    /// Exception thrown when rule processing encounters an error
    /// </summary>
    [Serializable]
    public class RuleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the RuleException class
        /// </summary>
        public RuleException() { }

        /// <summary>
        /// Initializes a new instance of the RuleException class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public RuleException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the RuleException class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="inner">The exception that is the cause of the current exception</param>
        public RuleException(string message, Exception inner) : base(message, inner) { }
    }
}
