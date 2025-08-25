using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FamilyAuditorCore
{
    /// <summary>
    /// Container for a collection of validation rules associated with a document
    /// Provides methods for rule management, serialization, and default rule setup
    /// </summary>
    public class RulesContainer
    {
        /// <summary>
        /// Gets or sets the collection of validation rules
        /// </summary>
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// Gets or sets whether rule validation is enabled for this container
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the title of the document these rules apply to
        /// </summary>
        public string DocTitle { get; set; }

        /// <summary>
        /// Initializes a new instance of the RulesContainer class
        /// </summary>
        /// <param name="docTitle">The title of the document these rules apply to</param>
        public RulesContainer(string docTitle)
        {
            Rules = new List<Rule>();
            DocTitle = docTitle;
        }

        /// <summary>
        /// Sets up default validation rules with commonly used thresholds
        /// This creates a standard set of rules suitable for most family validation scenarios
        /// </summary>
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

        /// <summary>
        /// Gets a specific rule by its type
        /// </summary>
        /// <param name="ruleType">The type of rule to retrieve</param>
        /// <returns>The rule of the specified type, or null if not found</returns>
        public Rule GetRule(RuleType ruleType)
        {
            return Rules.Find(r => r.TypeOfRule == ruleType);
        }

        /// <summary>
        /// Serializes the rules container to a JSON string
        /// </summary>
        /// <returns>JSON representation of the rules container</returns>
        public string SerializeToString()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            return jsonString;
        }

        /// <summary>
        /// Deserializes a JSON string to a RulesContainer object
        /// </summary>
        /// <param name="jsonString">JSON string to deserialize</param>
        /// <returns>Deserialized RulesContainer, or null if deserialization fails</returns>
        public static RulesContainer DeserializeFromString(string jsonString)
        {
            try
            {
                RulesContainer rulesContainer = JsonConvert.DeserializeObject<RulesContainer>(jsonString);
                return rulesContainer;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
