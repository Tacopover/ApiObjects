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
                Rules.Add(ruleElementNumber);
                Rules.Add(ruleImports);
                Rules.Add(ruleSubCategory);
                Rules.Add(ruleMaterial);
                Rules.Add(ruleParameterNumber);
                Rules.Add(ruleFileSize);
                Rules.Add(ruleDetailLines);
                Rules.Add(ruleVertices);
            }

            ruleElementNumber.Name = "Number of elements";
            ruleElementNumber.IsEnabled = true;

            ruleImports.Name = "Imported instances";
            ruleImports.IsEnabled = true;

            ruleSubCategory.Name = "Sub Category";
            ruleSubCategory.IsEnabled = true;

            ruleMaterial.Name = "Materials";
            ruleMaterial.IsEnabled = true;

            ruleParameterNumber.Name = "Number of Parameters";
            ruleParameterNumber.IsEnabled = true;

            ruleFileSize.Name = "File Size";
            ruleFileSize.IsEnabled = true;
            ruleFileSize.Unit = "MB";

            ruleDetailLines.Name = "Detail Lines";
            ruleDetailLines.IsEnabled = true;

            ruleVertices.Name = "Vertices";
            ruleVertices.IsEnabled = true;

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
