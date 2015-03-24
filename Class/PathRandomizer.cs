using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using LeagueSharp.Loader.Data;

namespace LeagueSharp.Loader.Class
{
    class PathRandomizer
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ModifyIATDelegate([MarshalAs(UnmanagedType.LPWStr)] string modulePath, [MarshalAs(UnmanagedType.LPWStr)] string newModulePath, [MarshalAs(UnmanagedType.LPStr)] string moduleName, [MarshalAs(UnmanagedType.LPStr), ] string newModuleName);

        private static ModifyIATDelegate ModifyIAT = null;

        public static void ResolveImports()
        {
            var hModule = Win32Imports.LoadLibrary(Directories.BootstrapFilePath);
            if (!(hModule != IntPtr.Zero))
            {
                return;
            }

            var procAddress = Win32Imports.GetProcAddress(hModule, "ModifyIAT");
            if (!(procAddress != IntPtr.Zero))
            {
                return;
            }

            ModifyIAT = Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ModifyIATDelegate)) as ModifyIATDelegate;
        }

        public static bool CopyFiles()
        {
            var result = true;
            if (ModifyIAT == null)
            {
                ResolveImports();
            }

            if (ModifyIAT == null)
            {
                return false;
            }

            try
            {
                //result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.dll"), LeagueSharpDllPath, true);
                //result = result && ModifyIAT(Path.Combine(Directories.CoreDirectory, "LeagueSharp.dll"), LeagueSharpDllPath, "LeagueSharp.Core.dll", LeagueSharpCoreDllName);
                result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.Core.dll"), LeagueSharpCoreDllPath, true);
                result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.Bootstrap.dll"), LeagueSharpBootstrapDllPath, true);
                result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.SandBox.dll"), LeagueSharpSandBoxDllPath, true);

                //Temp solution :^) , for some reason calling ModifyIAT() crashes the loader.
                var byteArray = File.ReadAllBytes(Path.Combine(Directories.CoreDirectory, "LeagueSharp.dll"));
                byteArray = Utility.ReplaceFilling(byteArray, Encoding.ASCII.GetBytes("LeagueSharp.Core.dll"), Encoding.ASCII.GetBytes(LeagueSharpCoreDllName));
                File.WriteAllBytes(LeagueSharpDllPath, byteArray);

                Brutal.Dev.StrongNameSigner.SigningHelper.SignAssembly(
                    LeagueSharpDllPath, Directories.CoreDirectory + "key.lnk", LeagueSharpDllPath);

                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string BaseDirectory
        {
            get { return Directories.AssembliesDir; }
        }

        public static Random RandomNumberGenerator = new Random();

        public static string GetRandomName(string oldName)
        {
            var ar1 = Utility.Md5Hash(oldName);
            var ar2 = Utility.Md5Hash(Config.Instance.Username);

            const string allowedChars = "0123456789abcdefhijkmnopqrstuvwxyz";
            var result = "";
            for (int i = 0; i < Math.Min(15, Math.Max(3, Config.Instance.Username.Length)); i++)
            {
                var j = (ar1.ToCharArray()[i] * ar2.ToCharArray()[i]) * 2;
                j = j % (allowedChars.Length - 1);
                result = result + allowedChars[j];
            }

            return result + ".dll";
        }

        private static string _leagueSharpDllName = null;
        private static string _leagueSharpCoreDllName = null;
        private static string _leagueSharpBootstrapDllName = null;
        private static string _leagueSharpSandBoxDllName = null;


        public static string LeagueSharpDllName
        {
            get
            {
                if (_leagueSharpDllName == null)
                {
                    _leagueSharpDllName = GetRandomName("LeagueSharp.dll");
                }
                return _leagueSharpDllName;
            }
        }

        public static string LeagueSharpCoreDllName
        {
            get
            {
                if (_leagueSharpCoreDllName == null)
                {
                    _leagueSharpCoreDllName = GetRandomName("LeagueSharp.Core.dll");
                }
                return _leagueSharpCoreDllName;
            }
        }

        public static string LeagueSharpBootstrapDllName
        {
            get
            {
                if (_leagueSharpBootstrapDllName == null)
                {
                    _leagueSharpBootstrapDllName = GetRandomName("LeagueSharp.Bootstrap.dll");
                }
                return _leagueSharpBootstrapDllName;
            }
        }

        public static string LeagueSharpSandBoxDllName
        {
            get
            {
              //  return "LeagueSharp.SandBox.dll";
                if (_leagueSharpSandBoxDllName == null)
                {
                    _leagueSharpSandBoxDllName = GetRandomName("LeagueSharp.SandBox.dll");
                }
                return _leagueSharpSandBoxDllName;
            }
        }

        public static string LeagueSharpDllPath {
            get { return Path.Combine(BaseDirectory, LeagueSharpDllName); }
        }

        public static string LeagueSharpCoreDllPath
        {
            get { return Path.Combine(BaseDirectory, LeagueSharpCoreDllName); }
        }

        public static string LeagueSharpBootstrapDllPath
        {
            get { return Path.Combine(BaseDirectory, LeagueSharpBootstrapDllName); }
        }

        public static string LeagueSharpSandBoxDllPath
        {
            get { return Path.Combine(BaseDirectory, LeagueSharpSandBoxDllName); }
        }
    }
}
