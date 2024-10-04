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

namespace CollabAPIMEP
{

    [TestFixture]
    public class UnitTest
    {
        public FamilyLoadHandler handler = null;


        UIApplication uiapp;
        Document doc;

        const string ProjectsFolder = @"C:\Users\arjan\source\repos\MEPAPI\FamilyAuditor\CollabAPIMEP";

        const string modelPath1 = ProjectsFolder + @"\CollabAPIMEP\resources\TestProject1.rvt";
        const string modelPath2 = ProjectsFolder + @"\CollabAPIMEP\resources\TestProject2.rvt";



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


        //isnull check
        [TestCase(modelPath1)]

        public void FamilyLoaderNullCheck(string modelPath)
        {

            //arrange
            uiapp.OpenAndActivateDocument(modelPath1);

            handler = new FamilyLoadHandler(uiapp.ActiveAddInId);

            if (!handler.GetRulesFromSchema())
            {
                handler.RulesHost.SetDefaultRules();
            }

            //assert
            if(!handler.RulesHost.Rules.Any())
            {
                Assert.Fail("RulesMap is null");

            }

        }

        //switchmodel test
        [TestCase(modelPath1, modelPath2)]

        public void SwitchModelCheck(string modelPath1, string modelPath2)
        {
            // Arrange
            UIDocument uidoc = uiapp.OpenAndActivateDocument(modelPath1);

            handler = new FamilyLoadHandler(uiapp.ActiveAddInId);

            handler.Initialize(uiapp);


            if (!handler.GetRulesFromSchema())
            {
                handler.RulesHost.SetDefaultRules();
            }

            handler.RulesHost.Rules[0].UserInput = "200";

            handler.SaveRulesToSchema();

            string jsonStringModel1 = handler.RulesHost.SerializeToString();


            uiapp.OpenAndActivateDocument(modelPath2);

            if (!handler.GetRulesFromSchema())
            {
                handler.RulesHost.SetDefaultRules();
            }

            handler.RulesHost.Rules[0].UserInput = "300";

            handler.SaveRulesToSchema();

            string jsonStringModel2 = handler.RulesHost.SerializeToString();


            uiapp.OpenAndActivateDocument(modelPath1);
            string jsonStringModel1Check = handler.RulesHost.SerializeToString();



            if (jsonStringModel1 != jsonStringModel1Check)
            {
                Assert.Fail("Switching models, rules are not correctly updated");
            }

            uiapp.OpenAndActivateDocument(modelPath2);

            string jsonStringModel2Check = handler.RulesHost.SerializeToString();

            if (jsonStringModel2 != jsonStringModel2Check)
            {
                Assert.Fail("Switching models, rules are not correctly updated");
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
