#region

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Serialization;
using LeagueSharp.Loader.Data;
using MessageBox = System.Windows.Forms.MessageBox;

#endregion

/*
    Copyright (C) 2014 LeagueSharp

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/*
    Copyright (C) 2014 Nikita Bernthaler

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace LeagueSharp.Loader.Class
{
    public class Utility
    {
        public static void MapClassToXmlFile(Type type, object obj, string path)
        {
            var serializer = new XmlSerializer(type);
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                serializer.Serialize(sw, obj);
            }
        }

        public static object MapXmlFileToClass(Type type, string path)
        {
            var serializer = new XmlSerializer(type);
            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                return serializer.Deserialize(reader);
            }
        }

        public static string ReadResourceString(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return string.Empty;
        }

        public static void CreateFileFromResource(string path, string resource, bool overwrite = false)
        {
            if (!overwrite && File.Exists(path))
            {
                return;
            }
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                        {
                            sw.Write(reader.ReadToEnd());
                        }
                    }
                }
            }
        }

        public static void ClearDirectory(string directory)
        {
            try
            {
                var dir = new DirectoryInfo(directory);
                foreach (var fi in dir.GetFiles())
                {
                    fi.Delete();
                }
                foreach (var di in dir.GetDirectories())
                {
                    ClearDirectory(di.FullName);
                    di.Delete();
                }
            }
            catch { }
        }

        public static string MakeValidFileName(string name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidRegStr, "_");
        }

        public static void Log(string status, string source, string message, Log log)
        {
            Application.Current.Dispatcher.Invoke(
                () => log.Items.Add(new LogItem { Status = status, Source = source, Message = message }));
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        }

        public static bool OverwriteFile(string file, string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (dir != null)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                try
                {
                    File.Move(file, path);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    throw;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RenameFileIfExists(string file, string path)
        {
            try
            {
                var counter = 1;
                var fileName = Path.GetFileNameWithoutExtension(file);
                var fileExtension = Path.GetExtension(file);
                var newPath = path;
                var pathDirectory = Path.GetDirectoryName(path);
                if (pathDirectory != null)
                {
                    if (!Directory.Exists(pathDirectory))
                    {
                        Directory.CreateDirectory(pathDirectory);
                    }
                    while (File.Exists(newPath))
                    {
                        var tmpFileName = string.Format("{0} ({1})", fileName, counter++);
                        newPath = Path.Combine(pathDirectory, tmpFileName + fileExtension);
                    }
                    File.Move(file, newPath);
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Returns the md5 hash from a string.
        /// </summary>
        public static string Md5Hash(string s)
        {
            var sb = new StringBuilder();
            HashAlgorithm algorithm = MD5.Create();
            var h = algorithm.ComputeHash(Encoding.UTF8.GetBytes(s));

            foreach (var b in h)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        public static string Md5Checksum(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    }
                }
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        public static string GetMultiLanguageText(string key)
        {
            return App.Current.FindResource(key).ToString();
        }

        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*",
                    SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }

        public static LeagueSharpAssembly CreateEmptyAssembly(string assemblyName)
        {
            try
            {
                var appconfig = Utility.ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.App.config");
                var assemblyInfocs = Utility.ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.AssemblyInfo.cs");
                var defaultProjectcsproj = Utility.ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.DefaultProject.csproj");
                var programcs = Utility.ReadResourceString("LeagueSharp.Loader.Resources.DefaultProject.Program.cs");

                var targetPath = Path.Combine(Directories.LocalRepoDir, assemblyName + Environment.TickCount.GetHashCode().ToString("X"));
                Directory.CreateDirectory(targetPath);

                programcs = programcs.Replace("{ProjectName}", assemblyName);
                assemblyInfocs = assemblyInfocs.Replace("{ProjectName}", assemblyName);
                defaultProjectcsproj = defaultProjectcsproj.Replace("{ProjectName}", assemblyName);
                defaultProjectcsproj = defaultProjectcsproj.Replace("{SystemDirectory}", Directories.CoreDirectory);

                File.WriteAllText(Path.Combine(targetPath, "App.config"), appconfig);
                File.WriteAllText(Path.Combine(targetPath, "AssemblyInfo.cs"), assemblyInfocs);
                File.WriteAllText(Path.Combine(targetPath, assemblyName + ".csproj"), defaultProjectcsproj);
                File.WriteAllText(Path.Combine(targetPath, "Program.cs"), programcs);

                return new LeagueSharpAssembly(assemblyName, Path.Combine(targetPath, assemblyName + ".csproj"), "");
            }
            catch (Exception)
            {               
                throw;
                return null;
            }
        }

        public static string GetLatestLeagueOfLegendsExePath(string lastKnownPath)
        {
            try 
	        {
                var dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(lastKnownPath), "..\\..\\"));
                if(Directory.Exists(dir))
                {
                    var versionPaths = Directory.GetDirectories(dir);
                    var greatestVersionString = "";
                    long greatestVersion = 0;

                    foreach (var versionPath in versionPaths)
	                {
                        Version version;
		                var isVersion = Version.TryParse(Path.GetFileName(versionPath), out version);
                        if(isVersion)
                        {
                            var test = version.Build * Math.Pow(600, 4) + version.Major * Math.Pow(600, 3) + version.Minor * Math.Pow(600, 2) + version.Revision * Math.Pow(600, 1);
                            if (test > greatestVersion)
                            {
                                greatestVersion = (long)test;
                                greatestVersionString = Path.GetFileName(versionPath);
                            }
                        }
	                }

                    if(greatestVersion != 0)
                    {
                        var exe = Directory.GetFiles(Path.Combine(dir, greatestVersionString), "League of Legends.exe", SearchOption.AllDirectories);
                        return exe.Length > 0 ? exe[0] : null;
                    }
                }
	        }
	        catch (Exception)
	        {
	        }

            return null;
        }
    }
}