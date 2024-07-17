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


namespace UnitTest
{
    [TestFixture]
    public class UnitTest
    {
        public FamilyLoadHandler handler = null;

        [Test]
        [TestModel(@".\TestProject.rvt")]

        public void familyLoader()
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

            var currentRulesmap = handler.RulesMap.ToDictionary(entry => entry.Key, entry => entry.Value);

            handler.SaveRulesToSchema();

            handler.GetRulesFromSchema();

            foreach (var entry in currentRulesmap)
            {
                // Check if the key exists in the loaded rules
                Assert.IsTrue(handler.RulesMap.ContainsKey(entry.Key), $"Key {entry.Key} is missing after loading.");

                // Check if the value associated with the key is equal in both dictionaries
                Assert.IsTrue(entry.Value.Equals(handler.RulesMap[entry.Key]), $"Value for key {entry.Key} has changed after loading.");
            }



        }












    }
}
