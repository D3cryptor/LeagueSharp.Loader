#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
            url = url.Remove(0, FullName.Length);
            url = WebUtility.UrlDecode(url);

            var urlSplit = url.Split('/');
            var splitCount = url.Count(c => c == '/');

            var baseUri = "https://github.com/";
            for (var i = 1; i < splitCount; i++)
            {
                baseUri += urlSplit[i] + "/";
            }

            switch (urlSplit[0])
            {
                case "profile":
                    //dosomeprofilestuff
                    break;

                case "project":
                    var w = new InstallerWindow { Owner = window };
                    switch (splitCount)
                    {
                        case 3:
                            w.ListAssemblies(baseUri, true);
                            w.ShowDialog();
                            break;
                        case 4:
                            w.ListAssemblies(baseUri.Replace(urlSplit[3] + "/", ""), true, urlSplit[3]);
                            w.ShowDialog();
                            break;
                    }
                    break;
            }
        }
    }
}