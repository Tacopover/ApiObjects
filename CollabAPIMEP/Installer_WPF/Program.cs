using System;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;

namespace Installer_WPF
{
    internal class Program
    {
        const string publisher = "MEPAPI";
        const string appGuidAdmin = "537eb34b-01f8-4262-9b24-f533aee21a53";
        const string appGuidUser = "d6f7ca5f-93be-424f-8c2d-8bf4668921ca";

        static void Main()
        {
            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string executingAssemblyDirectory = System.IO.Path.GetDirectoryName(executingAssembly.Location);
            string projectDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(executingAssemblyDirectory, @"..\..\..\..\..\"));

            string buildNameAdmin = "Admin";
            string buildNameUser = "User";

            CreateInstaller(buildNameAdmin, projectDir, appGuidAdmin);
            System.Threading.Thread.Sleep(1000);

            CreateInstaller(buildNameUser, projectDir, appGuidUser);

        }

        static void CreateInstaller(string buildName, string projectDir, string guid)
        {
           

            string filedir2022_admin = System.IO.Path.Combine(projectDir, "CollabAPIMEP_2022", "bin", "Release " + buildName, "CollabAPIMEP_2022.dll");
            string filedir2023_admin = System.IO.Path.Combine(projectDir, "CollabAPIMEP_2023", "bin", "Release " + buildName, "CollabAPIMEP_2023.dll");
            string filedir2024_admin = System.IO.Path.Combine(projectDir, "CollabAPIMEP_2024", "bin", "Release " + buildName, "CollabAPIMEP_2024.dll");

            string addinFilePath2022 = System.IO.Path.Combine(projectDir, "CollabAPIMEP", "resources", "FamilyAuditor_2022.addin");
            string addinFilePath2023 = System.IO.Path.Combine(projectDir, "CollabAPIMEP", "resources", "FamilyAuditor_2023.addin");
            string addinFilePath2024 = System.IO.Path.Combine(projectDir, "CollabAPIMEP", "resources", "FamilyAuditor_2024.addin");

            var feature2022 = new Feature("Revit 2022");
            var feature2023 = new Feature("Revit 2023");
            var feature2024 = new Feature("Revit 2024");

            var project = new ManagedProject($"Family Auditor ( {buildName})",
                             new Dir(@"C:\ProgramData\Autodesk\Revit\Addins",
                                new Dir(feature2022, @"2022",
                                    new File(feature2022, addinFilePath2022),
                                    new Dir(feature2022, @"FamilyAuditor",
                                        new File(feature2022, filedir2022_admin))),
                                new Dir(@"2023",
                                    new File(feature2023, addinFilePath2023),
                                    new Dir(feature2023, @"FamilyAuditor",
                                        new File(feature2023, filedir2023_admin))),
                                new Dir(@"2024",
                                    new File(feature2024, addinFilePath2024),
                                    new Dir(feature2024, @"FamilyAuditor",
                                        new File(feature2024, filedir2024_admin)))))
            {
                Scope = InstallScope.perUser,
                GUID = new Guid(guid),
                Version = new Version("0.0.2")
            };


            //project.Package.AttributesDefinition = "InstallPrivileges=limited";
            project.ControlPanelInfo.Manufacturer = publisher;
            project.ControlPanelInfo.Contact = "Taco Pover / Arjan Noya";

            //project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
            //project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

            //custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI();


            project.ManagedUI.InstallDialogs.Add<Installer_WPF.WelcomeDialog>()
                                .Add<Installer_WPF.LicenceDialog>()
                                .Add<Installer_WPF.FeaturesDialog>()
                                .Add<Installer_WPF.ProgressDialog>()
                                .Add<Installer_WPF.ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<Installer_WPF.MaintenanceTypeDialog>()
                                           .Add<Installer_WPF.FeaturesDialog>()
                                           .Add<Installer_WPF.ProgressDialog>()
                                           .Add<Installer_WPF.ExitDialog>();



            project.MajorUpgrade = new MajorUpgrade()
            {
                Schedule = UpgradeSchedule.afterInstallInitialize,
                DowngradeErrorMessage = $"A later version of {publisher} is already installed. Setup will now exit."
            };


            project.Load += Msi_Load;
            project.BeforeInstall += Msi_BeforeInstall;
            project.AfterInstall += Msi_AfterInstall;
            //project.LicenceFile = @"C:\Users\taco\OneDrive - MEPover\Revit\License file.rtf";
            project.BannerImage = System.IO.Path.Combine(projectDir, "CollabAPIMEP", "resources", "Installer banner.png");
            //project.ValidateBackgroundImage = false;
            project.BackgroundImage = System.IO.Path.Combine(projectDir, "CollabAPIMEP", "resources", "Installer background.png");

            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            project.BuildMsi("FamilyAuditor_" + buildName + ".msi");
        }

        static void Msi_Load(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "Load");
        }

        static void Msi_BeforeInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "BeforeInstall");
        }


        static void Msi_AfterInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "AfterExecute");
        }

    }
}