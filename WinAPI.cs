using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HDLauncher
{
    internal class WinAPI
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetGetCookieEx(string url, string name, StringBuilder data, ref int length,
            int flags, IntPtr reserved);
    }
}