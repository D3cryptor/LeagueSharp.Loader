#region

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using LeagueSharp.Loader.Views;
using MahApps.Metro.Controls;

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

#endregion

namespace LeagueSharp.Loader.Class
{
    public static class LSUriScheme
    {
        public const string Name = "ls";

        public static string FullName
        {
            get { return Name + "://"; }
        }

        public static void CreateRegFile(string fileName)
        {
            var regFileContents = Utility.ReadResourceString("LeagueSharp.Loader.Resources.CreateScheme.reg");

            regFileContents = regFileContents.Replace(
                "%leaguesharploader%",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LeagueSharp.Loader.exe").Replace(@"\", @"\\"));
            regFileContents = regFileContents.Replace("%name%", Name);

            using (
                var writer = new StreamWriter(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), false, Encoding.Unicode))
            {
                writer.Write(regFileContents);
            }

            System.Diagnostics.Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
        }

        public static void HandleUrl(string url, MetroWindow window)
        {
            url = WebUtility.UrlDecode(url.Remove(0, FullName.Length));

            var r = Regex.Matches(url, "(project|profile)/([^/]*)/([^/]*)/([^/]*)/?");
            foreach (Match m in r)
            {
                var linkType = m.Groups[1].ToString();
                var gitHubUser = m.Groups[2].ToString();
                var repositoryName = m.Groups[3].ToString();
                var assemblyName = m.Groups[4].ToString();

                switch (linkType)
                {
                    case "project":
                        var w = new InstallerWindow { Owner = window };
                        w.ListAssemblies(
                            string.Format("https://github.com/{0}/{1}", gitHubUser, repositoryName), true,
                            assemblyName != "" ? m.Groups[4].ToString() : null);
                        w.ShowDialog();
                        break;

                    case "profile":
                        break;
                }
            }
        }
    }
}