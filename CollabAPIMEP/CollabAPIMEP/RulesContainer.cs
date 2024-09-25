using CollabAPIMEP.Models;
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
            Rule ruleElementNumber = new Rule(RuleType.NumberOfElements, 100.ToString());
            ruleElementNumber.Name = "Number of elements";
            ruleElementNumber.IsEnabled = true;
            Rules.Add(ruleElementNumber);

            Rule ruleImports = new Rule(RuleType.ImportedInstances, 0.ToString());
            ruleImports.Name = "Imported instances";
            ruleImports.IsEnabled = true;
            Rules.Add(ruleImports);

            Rule ruleSubCategory = new Rule(RuleType.SubCategory);
            ruleSubCategory.Name = "Sub Category";
            ruleSubCategory.IsEnabled = true;
            Rules.Add(ruleSubCategory);

            Rule ruleMaterial = new Rule(RuleType.Material, 30.ToString());
            ruleMaterial.Name = "Materials";
            ruleMaterial.IsEnabled = true;
            Rules.Add(ruleMaterial);

            Rule ruleParameterNumber = new Rule(RuleType.NumberOfParameters, 50.ToString());
            ruleParameterNumber.Name = "Number of Parameters";
            ruleParameterNumber.IsEnabled = true;
            Rules.Add(ruleParameterNumber);

            Rule ruleFileSize = new Rule(RuleType.FileSize, 8.ToString());
            ruleFileSize.Name = "File Size";
            ruleFileSize.IsEnabled = true;
            ruleFileSize.Unit = "MB";
            Rules.Add(ruleFileSize);

            Rule ruleDetailLines = new Rule(RuleType.DetailLines, 50.ToString());
            ruleDetailLines.Name = "Detail Lines";
            ruleDetailLines.IsEnabled = true;
            Rules.Add(ruleDetailLines);

            Rule ruleVertices = new Rule(RuleType.Vertices, 50.ToString());
            ruleVertices.Name = "Vertices";
            ruleVertices.IsEnabled = true;
            Rules.Add(ruleVertices);
        }

        public Rule GetRule(RuleType ruleType)
        {
            return Rules.Find(r => r.TypeOfRule == ruleType);
        }
    }
}
