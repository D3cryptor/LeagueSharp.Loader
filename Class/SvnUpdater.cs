#region

using System.Collections.Generic;

#region

using System;
using System.IO;
using LeagueSharp.Loader.Data;
using SharpSvn;

#endregion

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
    internal class SvnUpdater
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
                    var dir = Path.Combine(directory, url.GetHashCode().ToString("X"));
                    using (var client = new SvnClient())
                    {
                        var cleanUp = false;
                        client.Conflict +=
                            delegate(object sender, SvnConflictEventArgs eventArgs)
                            {
                                eventArgs.Choice = SvnAccept.TheirsFull;
                            };
                        client.Status(
                            dir, new SvnStatusArgs { ThrowOnError = false },
                            delegate(object sender, SvnStatusEventArgs args)
                            {
                                if (args.Wedged)
                                {
                                    cleanUp = true;
                                }
                            });

                        if (cleanUp)
                        {
                            client.CleanUp(dir);
                        }

                        /*try
                        {
                            if (Directory.Exists(dir))
                            {
                                SvnInfoEventArgs remoteVersion;
                                var b1 = client.GetInfo(new Uri(url), out remoteVersion);

                                SvnInfoEventArgs localVersion;
                                var b2 = client.GetInfo(dir, out localVersion);

                                if (b1 && b2 && remoteVersion.Revision == localVersion.Revision)
                                {
                                    Utility.Log(
                                        LogStatus.Ok, "Updater", string.Format("Update not needed - {0}", url), log);
                                    return dir;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Utility.Log(LogStatus.Error, "Updater", string.Format("{0} - {1}", ex, url), log);
                        }*/

                        client.CheckOut(new Uri(url), dir);
                        client.Update(dir);
                        Utility.Log(LogStatus.Ok, "Updater", string.Format("Updated - {0}", url), log);
                    }
                    return dir;
                }
                catch (SvnException ex)
                {
                    Utility.Log(LogStatus.Error, "Updater", string.Format("{0} - {1}", ex.RootCause, url), log);
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