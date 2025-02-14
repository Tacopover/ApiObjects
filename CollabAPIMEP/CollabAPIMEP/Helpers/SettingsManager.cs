using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using FamilyAuditorCore;
using Firebase.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CollabAPIMEP.Helpers
{
    public class SettingsManager
    {
        private readonly Guid settingsGuid = new Guid("c16f94f6-5f14-4f33-91fc-f69dd7ac0d05");


        public FirebaseHelper FireBaseHelper { get; set; }


        private bool _isUserLoggedIn;
        public bool IsUserLoggedIn
        {
            get
            {
                return _isUserLoggedIn;
            }
            set
            {
                _isUserLoggedIn = value;
            }
        }

        private string _userText;
        public string UserText
        {
            get => _userText;
            set
            {
                _userText = value;
                UserTextChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UserTextChanged;

        public SettingsManager()
        {

        }

        public void LogIn()
        {
            //log in
            IsUserLoggedIn = true;
        }

        #region save and load rules
        private string _localFolder;
        public string LocalFolder
        {
            get
            {
                if (_localFolder == null)
                {
                    string folderLocalAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                    string directoryLocalSettings = Path.Combine(folderLocalAppdata, "FamilyAuditor");
                    if (!Directory.Exists(directoryLocalSettings))
                    {
                        Directory.CreateDirectory(directoryLocalSettings);
                    }
                    _localFolder = directoryLocalSettings;
                }
                return _localFolder;
            }
        }
        private string fileNameJson => Path.Combine(LocalFolder, "Rules.json");

        public void SaveRulesOnline(string jsonString)
        {
            if (IsUserLoggedIn)
            {
                //save rules online
                UserText = string.Empty;
            }
            else
            {
                UserText = "Please log in to save rules online";
            }

        }
        public void SaveRulesLocal(string jsonString)
        {
            try
            {
                File.WriteAllText(fileNameJson, jsonString);
            }
            catch (Exception ex)
            {
                UserText = "Failed to save rules locally";
            }
        }

        //public RulesContainer LoadRulesLocal()
        //{
        //    try
        //    {
        //        if (File.Exists(fileNameJson))
        //        {
        //            string jsonString = File.ReadAllText(fileNameJson);

        //            RulesContainer rulesHost = JsonConvert.DeserializeObject<RulesContainer>(jsonString);
        //            return rulesHost;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(fileNameJson + " failed to load");
        //    }

        //}

        public RulesContainer LoadRulesOnline()
        {
            if (IsUserLoggedIn)
            {
                //load rules online
                //RulesContainer rulesHost = firebaseClass.LoadRules();
                UserText = string.Empty;
            }
            else
            {
                UserText = "Please log in to load rules online";
            }
            return null;
        }

        #endregion
    }
}
