using System;
using NUnit;
using NUnit.Framework;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Linq;
using System.Xml.Linq;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using System.Configuration.Assemblies;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Events;
using CollabAPIMEP;

[assembly: AssemblyMetadata("NUnit.Open", "true")]

namespace CollabAPIMEP.UnitTest
{



    [TestFixture]
    public class UnitTest
    {
        public FamilyLoadHandler handler = null;


        UIApplication uiapp;
        Document doc;

        const string ProjectsFolder = @"C:\Users\arjan\source\repos\MEPAPI\FamilyAuditor\CollabAPIMEP";

        const string modelPath = ProjectsFolder + @"\CollabAPIMEP\resources\TestProject.rvt";


        [OneTimeSetUp]
        public void OneTimeSetup(UIApplication uiapp)
        {

#if UNIT_TEST
            uiapp.DialogBoxShowing += UiAppOnDialogBoxShowing;
#endif
        }

        [SetUp]
        public void Setup(UIApplication uiapp)
        {
            this.uiapp = uiapp;
        }

        [TestCase(modelPath)]

        public void FamilyLoaderNullCheck()
        {


            //arrange
            uiapp.OpenAndActivateDocument(modelPath);

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
            if(handler.RulesMap == null)
            {
                Assert.Fail("RulesMap is null");

            }

        }


        [TestCase(modelPath)]
        public void FamilyLoader()
        {
            // Arrange
            uiapp.OpenAndActivateDocument(modelPath);

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

            if(errorMessages.Count == 0)
            {
                Assert.Fail($"Errors found: {string.Join(", ", errorMessages)}");
            }

        }


        [OneTimeTearDown]
        public void OneTimeTearDown(UIApplication uiapp)
        {

#if UNIT_TEST
            uiapp.DialogBoxShowing -= UiAppOnDialogBoxShowing;
#endif
        }

        public static void UiAppOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            switch (args)
            {
                // (Konrad) Dismiss Unresolved References pop-up.
                case TaskDialogShowingEventArgs args2:
                    if (args2.DialogId == "TaskDialog_Unresolved_References")
                        args2.OverrideResult(1002);
                    break;
                default:
                    return;
            }
        }













    }
}
