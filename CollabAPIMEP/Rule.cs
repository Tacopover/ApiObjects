using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CollabAPIMEP
{
    public class Rule


    {
        public static char RuleSeparator = '|';

        public static char PropertySeparator = '_';

        public static char ValueSeparator = ':';

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

        public Rule()
        {

        }

        public static Rule deserializeFromSchema(string schemaString)
        {
            List<string> properties = schemaString.Split(Rule.PropertySeparator).ToList();
            Rule rule = new Rule();

            foreach (string propertyValue in properties)
            {
                string propertyString = propertyValue.Split(Rule.ValueSeparator).ToList().FirstOrDefault();
                string valueString = propertyValue.Split(Rule.ValueSeparator).ToList().LastOrDefault();

                PropertyInfo prop = typeof(Rule).GetProperty(propertyString);
                if (prop == null)
                {
                    continue;
                }
                object value = Convert.ChangeType(valueString, prop.PropertyType);
                prop.SetValue(rule, value);

            }

            rule.UpdateDescription();

            return rule;
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

        public static Dictionary<string, Rule> GetDefaultRules()
        {
            // if there are no rules loaded then the schema is not yet created. In that case create default rules:
            Dictionary<string, Rule> rulesMap = new Dictionary<string, Rule>();

            Rule ruleElementNumber = new Rule("NumberOfElements", 100.ToString());
            ruleElementNumber.Name = "Number of elements";
            rulesMap["NumberOfElements"] = ruleElementNumber;

            Rule ruleImports = new Rule("ImportedInstances", 1.ToString());
            ruleImports.Name = "Imported instances";
            rulesMap["ImportedInstances"] = ruleImports;

            Rule ruleSubCategory = new Rule("SubCategory");
            ruleSubCategory.Name = "Sub Category";
            rulesMap["SubCategory"] = ruleSubCategory;

            Rule ruleMaterial = new Rule("Material", 30.ToString());
            ruleMaterial.Name = "Material";
            rulesMap["Material"] = ruleMaterial;
            return rulesMap;
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
