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

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using LeagueSharp.Loader.Data;
using LeagueSharp.Loader.Views;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

#endregion

namespace LeagueSharp.Loader.Class
{
    internal static class Updater
    {
        public delegate void RepositoriesUpdateDelegate(List<string> list);

        public const string VersionCheckURL = "http://www.joduska.me/deploy/update.php?action=vcheck";
        public const string CoreVersionCheckURL = "http://www.joduska.me/deploy/update.php?action=lcheck&lmd5={0}";
        public static string UpdateZip = Path.Combine(Directories.CoreDirectory, "update.zip");
        public static string SetupFile = Path.Combine(Directories.CurrentDirectory, "LeagueSharp-update.exe");

        public static int LastCoreUpdateTry = 0;

        public static int VersionToInt(this Version version)
        {
            return version.Major * 10000000 + version.Minor * 10000 + version.Build * 100 + version.Revision;
        }

        [DataContract]
        internal class UpdateInfo
        {
            [DataMember]
            internal bool valid;

            [DataMember]
            internal string url;

            [DataMember]
            internal string version;
        }

        public static Tuple<bool, string> GetLoaderVersionInfo()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var data = client.DownloadData(VersionCheckURL);
                    var ser = new DataContractJsonSerializer(typeof(UpdateInfo));
                    var updateInfo = (UpdateInfo)ser.ReadObject(new MemoryStream(data));
                    var v = Version.Parse(updateInfo.version);
                    if (Assembly.GetEntryAssembly().GetName().Version.VersionToInt() < v.VersionToInt())
                    {
                       return new Tuple<bool, string>(true, updateInfo.url);
                    }
                }
            }
            catch
            {
                return new Tuple<bool, string>(false, "");
            }

            return new Tuple<bool, string>(false, "");
        }

        public static void UpdateLoader()
        {
            var result = GetLoaderVersionInfo();

            try
            {
                if (File.Exists(SetupFile))
                {
                    Thread.Sleep(1000);
                    File.Delete(SetupFile);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not delete update file, please delete it manually and restart LeagueSharp");
                Environment.Exit(0);
            }

            if (result.Item1)
            {
                var window = new UpdateWindow();
                window.UpdateUrl = result.Item2;
                window.ShowDialog();
            }
        }

        public static bool UpdateCore(string LeagueOfLegendsFilePath)
        {
            if (Environment.TickCount - LastCoreUpdateTry < 30000)
            {
                return false;
            }

            LastCoreUpdateTry = Environment.TickCount;

            try
            {
                var leagueMd5 = Utility.Md5Checksum(LeagueOfLegendsFilePath);
                var wr = WebRequest.Create(string.Format(CoreVersionCheckURL, leagueMd5));
                wr.Timeout = 2000;
                wr.Method = "GET";
                var response = wr.GetResponse();

                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        var ser = new DataContractJsonSerializer(typeof(UpdateInfo));
                        var updateInfo = (UpdateInfo)ser.ReadObject(stream);
                            
                        if(updateInfo.version == "0")
                        {
                            MessageBox.Show("\"League Of Legends.exe\" version not supported " + leagueMd5);
                            return false;
                        }

                        if (updateInfo.version != Utility.Md5Checksum(Path.Combine(Directories.CoreDirectory, "LeagueSharp.Core.dll"))) //Update needed
                        {
                            try
                            {
                                if (File.Exists(UpdateZip))
                                {
                                    File.Delete(UpdateZip);
                                    Thread.Sleep(500);
                                }

                                using (var webClient = new WebClient())
                                {
                                    webClient.DownloadFile(updateInfo.url, UpdateZip);
                                    using (var archive = ZipFile.OpenRead(UpdateZip))
                                    {
                                        foreach (var entry in archive.Entries)
                                        {
                                             entry.ExtractToFile(Path.Combine(Directories.CoreDirectory, entry.FullName), true);
                                         }
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                MessageBox.Show("Failed to download: " + e);
                                return false;
                            }
                            finally
                            {
                                if (File.Exists(UpdateZip))
                                {
                                    File.Delete(UpdateZip);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                //MessageBox.Show(e.ToString());
                return File.Exists(Path.Combine(Directories.CoreDirectory, "LeagueSharp.Core.dll"));
            }

            return File.Exists(Path.Combine(Directories.CoreDirectory, "LeagueSharp.Core.dll"));
        }

        public static void GetRepositories(RepositoriesUpdateDelegate del)
        {
            var wb = new WebClient();

            wb.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
            {
                var result = new List<string>();
                var matches = Regex.Matches(args.Result, "<repo>(.*)</repo>");
                foreach (Match match in matches)
                {
                    result.Add(match.Groups[1].ToString());
                }
                del(result);
            };

            wb.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/LeagueSharp/LeagueSharpLoader/master/Updates/Repositories.txt"));
        }
    }
}