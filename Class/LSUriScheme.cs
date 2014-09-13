#region

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
using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using LeagueSharp.Loader.Data;
using LeagueSharp.Loader.Views;
using MahApps.Metro.Controls;
using Microsoft.Win32;

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

        public static void CreateRegistryKeys(bool admin)
        {
            if (admin)
            {
                try
                {
                    var lsKey = Registry.ClassesRoot.CreateSubKey(Name);
                    if (lsKey != null)
                    {
                        lsKey.SetValue("URL Protocol", "", RegistryValueKind.String);

                        var defaultIconKey = lsKey.CreateSubKey("DefaultIcon");
                        if (defaultIconKey != null)
                        {
                            defaultIconKey.SetValue(
                                "", string.Format("\"{0}\", 0", Directories.LoaderFilePath), RegistryValueKind.String);
                        }

                        var registryKey = lsKey.CreateSubKey("shell");
                        if (registryKey != null)
                        {
                            var subKey = registryKey
                                .CreateSubKey("open");
                            if (subKey != null)
                            {
                                var key = subKey
                                    .CreateSubKey("command");
                                if (key != null)
                                {
                                    key
                                        .SetValue(
                                            "", string.Format("\"{0}\" %1", Directories.LoaderFilePath), RegistryValueKind.String);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
            else
            {
                var p = new Process();
                p.StartInfo.FileName = Directories.LoaderFilePath;
                p.StartInfo.Verb = "runas";
                p.StartInfo.Arguments = "addregkey";
                p.Start();
            }
            
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