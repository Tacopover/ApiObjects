using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FamilyAuditorCore
{
    public class RulesContainer
    {
        public List<Rule> Rules { get; set; }
        public bool IsEnabled { get; set; }

        public string DocTitle { get; set; }

        public RulesContainer(string docTitle)
        {
            Rules = new List<Rule>();
            DocTitle = docTitle;
        }

        public void SetDefaultRules()
        {
            Rule ruleElementNumber = new Rule(RuleType.NumberOfElements, 100.ToString());
            ruleElementNumber.Name = "Number of elements";
            ruleElementNumber.IsEnabled = true;

            Rule ruleImports = new Rule(RuleType.ImportedInstances, 0.ToString());
            ruleImports.Name = "Imported instances";
            ruleImports.IsEnabled = true;

            Rule ruleSubCategory = new Rule(RuleType.SubCategory);
            ruleSubCategory.Name = "Sub Category";
            ruleSubCategory.IsEnabled = true;

            Rule ruleMaterial = new Rule(RuleType.Material, 30.ToString());
            ruleMaterial.Name = "Materials";
            ruleMaterial.IsEnabled = true;

            Rule ruleParameterNumber = new Rule(RuleType.NumberOfParameters, 50.ToString());
            ruleParameterNumber.Name = "Number of Parameters";
            ruleParameterNumber.IsEnabled = true;

            Rule ruleFileSize = new Rule(RuleType.FileSize, 8.ToString());
            ruleFileSize.Name = "File Size";
            ruleFileSize.IsEnabled = true;
            ruleFileSize.Unit = "MB";

            Rule ruleDetailLines = new Rule(RuleType.DetailLines, 50.ToString());
            ruleDetailLines.Name = "Detail Lines";
            ruleDetailLines.IsEnabled = true;

            Rule ruleVertices = new Rule(RuleType.Vertices, 50.ToString());
            ruleVertices.Name = "Vertices";
            ruleVertices.IsEnabled = true;

            Rules.Add(ruleElementNumber);
            Rules.Add(ruleImports);
            Rules.Add(ruleSubCategory);
            Rules.Add(ruleMaterial);
            Rules.Add(ruleParameterNumber);
            Rules.Add(ruleFileSize);
            Rules.Add(ruleDetailLines);
            Rules.Add(ruleVertices);

        }

        public Rule GetRule(RuleType ruleType)
        {
            return Rules.Find(r => r.TypeOfRule == ruleType);
        }

        public string SerializeToString()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            return jsonString;
        }

        public static RulesContainer DeserializeFromString(string jsonString)
        {
            try
            {
                RulesContainer rulesContainer = JsonConvert.DeserializeObject<RulesContainer>(jsonString);
                return rulesContainer;
            }
            catch (JsonException ex)
            {
                return null;
            }

        }
    }
}
