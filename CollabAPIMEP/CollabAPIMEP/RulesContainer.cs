using CollabAPIMEP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabAPIMEP
{
    public class RulesContainer
    {
        public List<Rule> Rules { get; set; }
        public bool IsEnabled { get; set; }

        public RulesContainer()
        {
            Rules = new List<Rule>();
        }

        public void SetDefaultRules()
        {
            Rule ruleElementNumber;
            Rule ruleImports;
            Rule ruleSubCategory;
            Rule ruleMaterial;
            Rule ruleParameterNumber;
            Rule ruleFileSize;
            Rule ruleDetailLines;
            Rule ruleVertices;

            //check if the rules are already set. This might be the case if the model has been synced and the rules have not been saved yet.
            if (Rules.Count > 0)
            {
                ruleElementNumber = Rules.Find(r => r.TypeOfRule == RuleType.NumberOfElements);
                ruleImports = Rules.Find(r => r.TypeOfRule == RuleType.ImportedInstances);
                ruleSubCategory = Rules.Find(r => r.TypeOfRule == RuleType.SubCategory);
                ruleMaterial = Rules.Find(r => r.TypeOfRule == RuleType.Material);
                ruleParameterNumber = Rules.Find(r => r.TypeOfRule == RuleType.NumberOfParameters);
                ruleFileSize = Rules.Find(r => r.TypeOfRule == RuleType.FileSize);
                ruleDetailLines = Rules.Find(r => r.TypeOfRule == RuleType.DetailLines);
                ruleVertices = Rules.Find(r => r.TypeOfRule == RuleType.Vertices);
            }
            else
            {
                ruleElementNumber = new Rule(RuleType.NumberOfElements, 100.ToString());
                ruleImports = new Rule(RuleType.ImportedInstances, 0.ToString());
                ruleSubCategory = new Rule(RuleType.SubCategory);
                ruleMaterial = new Rule(RuleType.Material, 30.ToString());
                ruleParameterNumber = new Rule(RuleType.NumberOfParameters, 50.ToString());
                ruleFileSize = new Rule(RuleType.FileSize, 8.ToString());
                ruleDetailLines = new Rule(RuleType.DetailLines, 50.ToString());
                ruleVertices = new Rule(RuleType.Vertices, 50.ToString());
            }

            ruleElementNumber.Name = "Number of elements";
            ruleElementNumber.IsEnabled = true;
            Rules.Add(ruleElementNumber);

            ruleImports.Name = "Imported instances";
            ruleImports.IsEnabled = true;
            Rules.Add(ruleImports);

            ruleSubCategory.Name = "Sub Category";
            ruleSubCategory.IsEnabled = true;
            Rules.Add(ruleSubCategory);

            ruleMaterial.Name = "Materials";
            ruleMaterial.IsEnabled = true;
            Rules.Add(ruleMaterial);

            ruleParameterNumber.Name = "Number of Parameters";
            ruleParameterNumber.IsEnabled = true;
            Rules.Add(ruleParameterNumber);

            ruleFileSize.Name = "File Size";
            ruleFileSize.IsEnabled = true;
            ruleFileSize.Unit = "MB";
            Rules.Add(ruleFileSize);

            ruleDetailLines.Name = "Detail Lines";
            ruleDetailLines.IsEnabled = true;
            Rules.Add(ruleDetailLines);

            ruleVertices.Name = "Vertices";
            ruleVertices.IsEnabled = true;
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
