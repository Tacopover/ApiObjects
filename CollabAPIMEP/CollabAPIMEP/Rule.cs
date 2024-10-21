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
        public static char RuleSeparator = '|';

        public static char PropertySeparator = '_';

        public static char ValueSeparator = ':';

        public static char rulesEnabledSeperator = '*';

        public static int NumberOfDefaultRules = Enum.GetNames(typeof(RuleType)).Length;


        public bool IsEnabled { get; set; }
        //public string ID { get; set; }
        public RuleType TypeOfRule { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; } = "";

        //public int NumberOfElements { get; set; }

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
                //if (_userInput != null)
                //{
                _userInput = value;
                //UpdateDescription();
                //}
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


        //public Rule(string id, string userInput = null)
        //{
        //    ID = id;
        //    IsEnabled = false;
        //    if (userInput == null)
        //    {
        //        _isUserInputEnabled = false;
        //    }
        //    else
        //    {
        //        _isUserInputEnabled = true;
        //    }
        //    _userInput = userInput;
        //    UpdateDescription();
        //}

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

        public Rule()
        {
            // empty constructor for serialization
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
                object value;
                if (prop.PropertyType.IsEnum)
                {
                    value = Enum.Parse(prop.PropertyType, valueString);
                }
                else
                {
                    value = Convert.ChangeType(valueString, prop.PropertyType);
                }

                prop.SetValue(rule, value);

            }

            rule.UpdateDescription();

            return rule;
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
                    Description = $"This rule will check the number of detail lines in the family. If the number is greater than specified, the family will not be loaded into the project.";
                    break;
                case RuleType.Vertices:
                    Description = $"This rule will check the number of vertices in the family geometry. If the number is greater than specified, the family will not be loaded into the project.";
                    break;
            }
        }

        public static Dictionary<string, Rule> GetDefaultRules()
        {
            // if there are no rules loaded then the schema is not yet created. In that case create default rules:
            Dictionary<string, Rule> rulesMap = new Dictionary<string, Rule>();

            Rule ruleElementNumber = new Rule(RuleType.NumberOfElements, 100.ToString());
            ruleElementNumber.Name = "Number of elements";
            ruleElementNumber.IsEnabled = true;
            rulesMap["NumberOfElements"] = ruleElementNumber;

            Rule ruleImports = new Rule(RuleType.ImportedInstances, 0.ToString());
            ruleImports.Name = "Imported instances";
            ruleImports.IsEnabled = true;
            rulesMap["ImportedInstances"] = ruleImports;

            Rule ruleSubCategory = new Rule(RuleType.SubCategory);
            ruleSubCategory.Name = "Sub Category";
            ruleSubCategory.IsEnabled = true;
            rulesMap["SubCategory"] = ruleSubCategory;

            Rule ruleMaterial = new Rule(RuleType.Material, 30.ToString());
            ruleMaterial.Name = "Materials";
            ruleMaterial.IsEnabled = true;
            rulesMap["Materials"] = ruleMaterial;

            Rule ruleParameterNumber = new Rule(RuleType.NumberOfParameters, 50.ToString());
            ruleParameterNumber.Name = "Number of Parameters";
            ruleParameterNumber.IsEnabled = true;
            rulesMap["NumberOfParameters"] = ruleParameterNumber;

            Rule ruleFileSize = new Rule(RuleType.FileSize, 8.ToString());
            ruleFileSize.Name = "File Size";
            ruleFileSize.IsEnabled = true;
            ruleFileSize.Unit = "MB";
            rulesMap["FileSize"] = ruleFileSize;

            Rule ruleDetailLines = new Rule(RuleType.DetailLines, 50.ToString());
            ruleDetailLines.Name = "Detail Lines";
            ruleDetailLines.IsEnabled = true;
            rulesMap["DetailLines"] = ruleDetailLines;

            Rule ruleVertices = new Rule(RuleType.Vertices, 50.ToString());
            ruleVertices.Name = "Vertices";
            ruleVertices.IsEnabled = true;
            rulesMap["Vertices"] = ruleVertices;

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
