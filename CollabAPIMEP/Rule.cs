using System;

namespace CollabAPIMEP
{
    public class Rule
    {
        public bool IsEnabled { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int NumberOfElements { get; set; }

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
                if (_userInput != null)
                {
                    _userInput = value;
                    UpdateDescription();
                }
            }
        }
        public Rule(string id, string userInput = null)
        {
            ID = id;
            IsEnabled = false;
            _userInput = userInput;
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            switch (ID)
            {
                case "NumberOfElements":
                    Description = $"This rule will check the number of elements in the family. If the number of elements is greater than {UserInput}, the family will not be loaded into the project.";
                    break;
                case "ImportedInstances":
                    Description = $"This rule will check the number of imported instances in the family. If the number of imported instances is greater than {UserInput}, the family will not be loaded into the project.";
                    break;
                case "SubCategory":
                    Description = "This rule will check if every piece of geometry in the family is assigned to a subcategory. If not, the family will not be loaded into the project.";
                    break;
                case "Material":
                    Description = $"This rule will check the number of materials in a family. If the number is greater than {UserInput}, the family will not be loaded into the project.";
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
