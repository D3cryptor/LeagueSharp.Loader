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

#endregion

namespace LeagueSharp.Loader.Class
{
    internal static class Updater
    {
        public delegate void RepositoriesUpdateDelegate(List<string> list);

        public const string VersionCheckURL =
            "https://raw.githubusercontent.com/joduskame/LeagueSharp/master/VersionCheck.txt";

        public static int VersionToInt(this Version version)
        {
            return version.Major * 10000000 + version.Minor * 10000 + version.Build * 100 + version.Revision;
        }

        public static Tuple<bool, string> GetVersionInfo()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var data = client.DownloadString(VersionCheckURL);
                    var updateurl = Regex.Matches(data, "<url>(.*)</url>");
                    if (updateurl.Count > 0)
                    {
                        var url = updateurl[0].Groups[1].ToString();
                        var vR = Regex.Matches(data, "<version>(.*)</version>");
                        if (vR.Count > 0)
                        {
                            var v = Version.Parse(vR[0].Groups[1].ToString());
                            if (Assembly.GetEntryAssembly().GetName().Version.VersionToInt() < v.VersionToInt())
                            {
                                return new Tuple<bool, string>(true, url);
                            }
                        }
                    }
                }
            }
            catch
            {
                return new Tuple<bool, string>(false, "");
            }

            return new Tuple<bool, string>(false, "");
        }

        public static void Update()
        {
            var result = GetVersionInfo();
            var setupFile = Path.Combine(Directories.CurrentDirectory, "LeagueSharp-setup.exe");

            try
            {
                if (File.Exists(setupFile))
                {
                    Thread.Sleep(1000);
                    File.Delete(setupFile);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not delete setup file, please delete it manually and restart LeagueSharp");
                Environment.Exit(0);
            }

            if (result.Item1)
            {
                MessageBox.Show("New version available.");

                try
                {
                    using (var webClient = new WebClient())
                    {
                        try
                        {
                            webClient.DownloadFile(result.Item2, setupFile);
                            new Process { StartInfo = { FileName = setupFile, Arguments = "/VERYSILENT" } }.Start();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Could not download the update, please update manually: " + e);
                            Environment.Exit(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Could not download the update, please update manually: " + e);
                    Environment.Exit(0);
                }
            }
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