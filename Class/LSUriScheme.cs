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

namespace LeagueSharp.Loader.Class
{
    public static class LSUriScheme
    {
        public const string Name = "leaguesharp";
        public static string FullName { get { return Name + "://"; } }

        public static void CreateRegFile(string fileName)
        {
            var regFileContents = Utility.ReadResourceString("LeagueSharp.Loader.Resources.CreateScheme.reg");
            regFileContents = regFileContents.Replace("%leaguesharploader%", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LeagueSharp.Loader.exe").Replace(@"\", @"\\"));
            regFileContents = regFileContents.Replace("%name%", Name);
            using (var writer = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), false, Encoding.Unicode))
            {
                writer.Write(regFileContents);
            }

            System.Diagnostics.Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
        }

        public static void HandleUrl(string url, MetroWindow window)
        {
            //Remove the KeyStart
            url = url.Remove(0, FullName.Length);

            if (url.Count(c => c == '/') >= 3)
            {
                url = WebUtility.UrlDecode(url);
                var splittedUrl = url.Split('/');
                var gitHubUser = splittedUrl[0];
                var repoName = splittedUrl[1];
                var assemblyName = splittedUrl[2];

                var w = new InstallerWindow { Owner = window };
                w.ListAssemblies(string.Format("https://github.com/{0}/{1}", gitHubUser, repoName), true, assemblyName != "List" ? assemblyName : null);
                w.ShowDialog();
            }
        }
    }
}
