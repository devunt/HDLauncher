﻿using System;
using System.IO;
using Microsoft.Win32;

namespace HDLauncher
{
    internal class Settings
    {
        public enum GraphicApis
        {
            DX_9,
            DX_11
        }

        private static string _password = "";
        private static INIFile iniFile;

        public static string FFXIVPath { get; set; } = "";
        public static string Username { get; set; } = "";
        public static GraphicApis GraphicApi { get; set; } = GraphicApis.DX_11;
        public static bool SavePassword { get; set; }
        public static bool RunAsAdministrator { get; set; }

        public static string UOtpId { get; set; }
        public static string UOtpSeed { get; set; }

        public static string Password
        {
            get
            {
                if (string.IsNullOrEmpty(_password)) return "";
                return Cryptography.Decrypt(_password, Constants.PASSWORD_CRYPT_KEY);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) _password = "";
                _password = Cryptography.Encrypt(value, Constants.PASSWORD_CRYPT_KEY);
            }
        }

        private static void Init()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(Constants.REG_UNINSTALL_KEYPATH1))
            using (var key2 = Registry.LocalMachine.OpenSubKey(Constants.REG_UNINSTALL_KEYPATH2))
            {
                try
                {
                    FFXIVPath = (string)key.GetValue(Constants.REG_PATH_VALNAME, "");
                }
                catch (NullReferenceException)
                {
                }
                try
                {
                    FFXIVPath = (string)key2.GetValue(Constants.REG_PATH_VALNAME, "");
                }
                catch (NullReferenceException)
                {
                }
            }
        }

        public static void Load()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APPNAME, Constants.SETTINGS_FILEPATH);

            iniFile = new INIFile(path);
            if (!File.Exists(path))
            {
                Init();
                Save();
            }
            else
            {
                FFXIVPath = iniFile.ReadValue("ffxiv", "path");
                Username = iniFile.ReadValue("account", "username");
                _password = iniFile.ReadValue("account", "password");
                GraphicApi = iniFile.ReadValue("preferences", "graphicapi") == "9" ? GraphicApis.DX_9 : GraphicApis.DX_11;
                SavePassword = iniFile.ReadValue("preferences", "savepassword") == "1";
                RunAsAdministrator = iniFile.ReadValue("preferences", "runasadministrator") == "1";
                UOtpId = iniFile.ReadValue("uotp", "oid");
                UOtpSeed = iniFile.ReadValue("uotp", "seed");
            }
        }

        public static void Save()
        {
            iniFile.WriteValue("ffxiv", "path", FFXIVPath);
            iniFile.WriteValue("account", "username", Username);
            iniFile.WriteValue("account", "password", _password);
            iniFile.WriteValue("preferences", "graphicapi", GraphicApi == GraphicApis.DX_9 ? "9" : "11");
            iniFile.WriteValue("preferences", "savepassword", SavePassword ? "1" : "0");
            iniFile.WriteValue("preferences", "runasadministrator", RunAsAdministrator ? "1" : "0");
            iniFile.WriteValue("uotp", "oid", UOtpId);
            iniFile.WriteValue("uotp", "seed", UOtpSeed);
        }
    }
}