#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// LSUriScheme.cs is part of LeagueSharp.Loader.
// 
// LeagueSharp.Loader is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LeagueSharp.Loader is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LeagueSharp.Loader. If not, see <http://www.gnu.org/licenses/>.

#endregion



namespace LeagueSharp.Loader.Class
{
    #region

    using System;
    using System.Collections.Generic;
    using Data;
    using System.Net;
    using System.Text.RegularExpressions;
    using Views;
    using MahApps.Metro.Controls;

    #endregion

    public static class LSUriScheme
    {
        public const string Name = "ls";

        public static string FullName
        {
            get { return Name + "://"; }
        }

        public static void HandleUrl(string url, MetroWindow window)
        {
            url = WebUtility.UrlDecode(url.Remove(0, FullName.Length));

            var r = Regex.Matches(url, "(project|projectGroup)/([^/]*)/([^/]*)/([^/]*)/?");
            foreach (Match m in r)
            {
                var linkType = m.Groups[1].ToString();
              
                switch (linkType)
                {
                    case "project":
                        var gitHubUser = m.Groups[2].ToString();
                        var repositoryName = m.Groups[3].ToString();
                        var assemblyName = m.Groups[4].ToString();

                        var w = new InstallerWindow { Owner = window };
                        w.ListAssemblies(
                            string.Format("https://github.com/{0}/{1}", gitHubUser, repositoryName), true,
                            assemblyName != "" ? m.Groups[4].ToString() : null);
                        w.ShowDialog();
                        break;

                    case "projectGroup":
                        
                        var remaining = url.Remove(0, 13);
                        var assemblies = new List<LeagueSharpAssembly>();

                        while (remaining.IndexOf("/", StringComparison.InvariantCulture) != -1)
                        {
                            var data = remaining.Split(new[] {'/'});
                            if (data.Length < 3)
                            {
                                break;
                            }
                            
                            var assembly = new LeagueSharpAssembly(data[2], "", 
                                string.Format("https://github.com/{0}/{1}", data[0], data[1]));
                            assemblies.Add(assembly);
                            for (int i = 0; i < 3; i++)
                            {
                                remaining = remaining.Remove(0, remaining.IndexOf("/", StringComparison.InvariantCulture) + 1);
                            }
                        }

                        if (assemblies.Count > 0)
                        {
                            assemblies.ForEach(
                                assembly => Config.Instance.SelectedProfile.InstalledAssemblies.Add(assembly));
                            ((MainWindow) window).PrepareAssemblies(assemblies, true, true, false);
                            ((MainWindow) window).ShowTextMessage(Utility.GetMultiLanguageText("Installer"),
                                Utility.GetMultiLanguageText("SuccessfullyInstalled"));
                        }
                        break;
                }
            }
        }
    }
}