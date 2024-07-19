using CollabAPIMEP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using RTF;
using NUnit;
using NUnit.Framework;
using System.Runtime.InteropServices;
using RTF.Framework;
using System.Windows.Documents;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using System.Linq;
using System.Collections.Generic;


namespace UnitTest
{
    [TestFixture]
    public class UnitTest
    {
        public FamilyLoadHandler handler = null;

        [Test]
        [TestModel(@".\TestProject.rvt")]

        public void FamilyLoaderNullCheck()
        {
            //arrange
            bool result = true;

            handler = new FamilyLoadHandler();
            if (!handler.GetRulesFromSchema())
            {
                handler.RulesMap = Rule.GetDefaultRules();
            }

            //act
            if (!handler.GetRulesFromSchema())
            {
                handler.RulesMap = Rule.GetDefaultRules();
            }

            //assert
            Assert.IsNotNull(handler.RulesMap);

        }


        [Test]
        public void FamilyLoader()
        {
            // Arrange
            handler = new FamilyLoadHandler();
            handler.RulesMap = handler.GetRulesFromSchema() ? handler.RulesMap : Rule.GetDefaultRules();
            var errorMessages = new List<string>();

            // Act
            var currentRulesMap = handler.RulesMap.ToDictionary(entry => entry.Key, entry => entry.Value);
            handler.SaveRulesToSchema();
            handler.GetRulesFromSchema();

            // Assert
            foreach (var entry in currentRulesMap)
            {
                if (!handler.RulesMap.ContainsKey(entry.Key))
                {
                    errorMessages.Add($"Key {entry.Key} is missing after loading.");
                }
                else if (!entry.Value.Equals(handler.RulesMap[entry.Key]))
                {
                    errorMessages.Add($"Value for key {entry.Key} has changed after loading.");
                }
            }

            Assert.IsTrue(errorMessages.Count == 0, $"Errors found: {string.Join(", ", errorMessages)}");
        }












    }
}
