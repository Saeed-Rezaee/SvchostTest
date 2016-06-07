using System;

namespace Svchost
{
    static class Property
    {
        public static string SessionName
        { get { return Environment.UserName; } }

        public static string AppPath
        { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        public static string AssemblyName
        { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe"; } }

        public static string FakeAssemblyName
        { get { return "Kimya Dawson - Tire Swing"; } }
    }
}
