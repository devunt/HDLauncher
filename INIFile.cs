﻿using System.Runtime.InteropServices;
using System.Text;

namespace HDLauncher
{
    /// <summary>
    ///     Create a New INI file to store or load data
    /// </summary>
    public class INIFile
    {
        public string path;

        /// <summary>
        ///     INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public INIFile(string INIPath)
        {
            path = INIPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal,
            int size, string filePath);

        /// <summary>
        ///     Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void WriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        /// <summary>
        ///     Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string ReadValue(string Section, string Key)
        {
            var temp = new StringBuilder(255);
            var i = GetPrivateProfileString(Section, Key, "", temp,
                255, path);
            return temp.ToString();
        }
    }
}

// This code is originally from http://www.codeproject.com/Articles/1966/An-INI-file-handling-class-using-C