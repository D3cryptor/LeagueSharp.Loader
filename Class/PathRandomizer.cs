using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LeagueSharp.Loader.Data;

namespace LeagueSharp.Loader.Class
{
    class PathRandomizer
    {
        public static bool CopyFiles()
        {
            var result = true;
            result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.dll"), LeagueSharpDllPath, true);
            result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.Core.dll"), LeagueSharpCoreDllPath, true);
            result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.Bootstrap.dll"), LeagueSharpBootstrapDllPath, true);
            result = result && Utility.OverwriteFile(Path.Combine(Directories.CoreDirectory, "LeagueSharp.AppDomainManager.dll"), LeagueSharpSandBoxDllPath, true);
            return result;
        }

        public static string BaseDirectory
        {
            get { return Directories.AssembliesDir; }
        }

        public static Random RandomNumberGenerator = new Random();

        public static string GetRandomName(string oldName)
        {
            return oldName; //kappa
            var ar1 = Utility.Md5Hash(oldName);
            var ar2 = Utility.Md5Hash(Config.Instance.Username);

            var allowedChars = "0123456789abcdefghijklmnopqrstuvwxyz";
            var result = "";
            for (int i = 0; i < RandomNumberGenerator.Next(5, 10); i++)
            {
                var j = (int)(ar1.ToCharArray()[i] * ar2.ToCharArray()[i]) * 2;
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
                if (_leagueSharpSandBoxDllName == null)
                {
                    _leagueSharpSandBoxDllName = GetRandomName("LeagueSharp.AppDomainManager.dll");
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
