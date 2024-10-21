using CollabAPIMEP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CollabAPIMEP
{
    public class Rule


    {
        public static int NumberOfDefaultRules = Enum.GetNames(typeof(RuleType)).Length;


        public bool IsEnabled { get; set; }
        public RuleType TypeOfRule { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; } = "";

        private string _userInput;
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
        public bool IsUserInputEnabled
        {
            get { return _isUserInputEnabled; }
            set
            {
                _isUserInputEnabled = value;
            }
        }

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

    [Serializable]
    public class RuleException : Exception
    {
        public RuleException() { }
        public RuleException(string message) : base(message) { }
        public RuleException(string message, Exception inner) : base(message, inner) { }

    }
}
