#region

using System;
using System.IO;
using LeagueSharp.Loader.Data;
using System.Collections.Generic;
using LibGit2Sharp;

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

namespace LeagueSharp.Loader.Class
{
    internal class GitUpdater
    {
        public static string Update(string url, Log log, string directory)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Utility.Log(LogStatus.Skipped, "Updater", string.Format("No Url specified - {0}", url), log);
            }
            else
            {
                try
                {
                    var dir = Path.Combine(directory, url.GetHashCode().ToString("X"), "trunk");

                    if (Repository.IsValid(dir))
                    {
                        using (var repo = new Repository(dir))
                        {
                            repo.Fetch("origin");
                            repo.Checkout("origin/master", new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                        }
                    }
                    else
                    {
                        Repository.Clone(url, dir);
                    }

                    return dir;
                }
                catch (Exception ex)
                {
                    Utility.Log(LogStatus.Error, "Updater", string.Format("{0} - {1}", ex.Message, url), log);
                }
            }

            return string.Empty;
        }

        public static void ClearUnusedRepos(List<LeagueSharpAssembly> assemblyList)
        {
            try
            {
                var usedRepos = new List<string>();
                foreach (var assembly in assemblyList)
                {
                    usedRepos.Add(assembly.SvnUrl.GetHashCode().ToString("X"));
                }

                var dirs = new List<string>(Directory.EnumerateDirectories(Directories.RepositoryDir));

                foreach (var dir in dirs)
                {
                    if (!usedRepos.Contains(Path.GetFileName(dir)))
                    {
                        Utility.ClearDirectory(dir);
                        Directory.Delete(dir);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}