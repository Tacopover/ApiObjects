﻿using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;

namespace WixSharp_Installer
{
    internal class Program
    {
        const string publisher = "MEPAPI";
        const string appGuid = "537eb34b-01f8-4262-9b24-f533aee21a53";
        static void Main()
        {
            string filedir = @"C:\Users\taco\source\repos\MepoverRevit\bin\Release\";

            var project = new ManagedProject("CollabAPIMEP",
                             new Dir(@"C:\ProgramData\Autodesk\Revit\Addins",
                                new Dir(@"2022",
                                    new File(@"CollabAPIMEP\resources\ProjectMonitor_2022.addin"),
                                    new Dir(@"MEPAPI",
                                        new File(filedir + "CollabAPIMEP.2022.dll"))),
                                new Dir(@"2023",
                                    new File(@"CollabAPIMEP\resources\ProjectMonitor_2023.addin"),
                                    new Dir(@"MEPAPI",
                                        new File(filedir + "CollabAPIMEP.2023.dll"))),
                                new Dir(@"2024",
                                    new File(@"CollabAPIMEP\resources\ProjectMonitor_2024.addin"),
                                    new Dir(@"MEPAPI",
                                        new File(filedir + "CollabAPIMEP.2024.dll")))))
            {
                Scope = InstallScope.perUser,
                GUID = new Guid(appGuid),
                Version = new Version("0.1.0")
            };

            //project.Package.AttributesDefinition = "InstallPrivileges=limited";
            project.ControlPanelInfo.Manufacturer = publisher;
            project.ControlPanelInfo.Contact = "Taco Pover / Arjan Noya";

            //project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
            //project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

            //custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add(Dialogs.Welcome)
                                            .Add(Dialogs.Licence)
                                            //.Add(Dialogs.SetupType)
                                            //.Add(Dialogs.Features)
                                            //.Add(Dialogs.InstallDir)
                                            .Add(Dialogs.Progress)
                                            .Add(Dialogs.Exit);

            //project.ManagedUI.ModifyDialogs.Add(Dialogs.MaintenanceType)
            //                               .Add(Dialogs.Features)
            //                               .Add(Dialogs.Progress)
            //                               .Add(Dialogs.Exit);

            project.MajorUpgrade = new MajorUpgrade()
            {
                Schedule = UpgradeSchedule.afterInstallInitialize,
                DowngradeErrorMessage = $"A later version of {publisher} is already installed. Setup will now exit."
            };

            project.Load += Msi_Load;
            project.BeforeInstall += Msi_BeforeInstall;
            project.AfterInstall += Msi_AfterInstall;
            //project.LicenceFile = @"C:\Users\taco\OneDrive - MEPover\Revit\License file.rtf";
            //project.BannerImage = @"C:\Users\taco\OneDrive - MEPover\Taco\dev\resources\Icons\MEPover banner.png";
            //project.ValidateBackgroundImage = false;
            //project.BackgroundImage = @"C:\Users\taco\OneDrive - MEPover\Taco\dev\resources\Icons\MEPover background image.png";

            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            project.BuildMsi();
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