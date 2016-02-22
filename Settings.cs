using Microsoft.Win32;
using System;
using System.IO;

namespace HDLauncher
{
    class Settings
    {
        private static string _password = "";
        private static INIFile iniFile;

        public enum DataCenters { Moogle, Chocobo };

        public static string FFXIVPath { get; set; } = "";
        public static string Username { get; set; } = "";
        public static DataCenters DataCenter { get; set; } = DataCenters.Moogle;
        public static bool SavePassword { get; set; } = false;

        public static string Password {
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
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Constants.REG_UNINSTALL_KEYPATH1))
            using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(Constants.REG_UNINSTALL_KEYPATH2))
            {
                try
                {
                    FFXIVPath = (string)key.GetValue(Constants.REG_PATH_VALNAME, "");
                }
                catch (NullReferenceException) { }
                try
                {
                    FFXIVPath = (string)key2.GetValue(Constants.REG_PATH_VALNAME, "");
                }
                catch (NullReferenceException) { }
            }
        }

        public static void Load()
        {
            iniFile = new INIFile(Constants.SETTINGS_FILENAME);
            if (!File.Exists(Constants.SETTINGS_FILENAME))
            {
                Init();
                Save();
            }
            else
            {
                FFXIVPath = iniFile.ReadValue("ffxiv", "path");
                Username = iniFile.ReadValue("account", "username");
                _password = iniFile.ReadValue("account", "password");
                DataCenter = (iniFile.ReadValue("preferences", "datacenter") == "c") ? DataCenters.Chocobo : DataCenters.Moogle;
                SavePassword = iniFile.ReadValue("preferences", "savepassword") != "0";
            }
        }

        public static void Save()
        {
            iniFile.WriteValue("ffxiv", "path", FFXIVPath);
            iniFile.WriteValue("account", "username", Username);
            iniFile.WriteValue("account", "password", _password);
            iniFile.WriteValue("preferences", "datacenter", (DataCenter == DataCenters.Chocobo) ? "c" : "m");
            iniFile.WriteValue("preferences", "savepassword", SavePassword ? "1" : "0");
        }
    }
}
