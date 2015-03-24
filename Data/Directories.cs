#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// Directories.cs is part of LeagueSharp.Loader.
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

using System;
using System.Diagnostics;
using System.IO;

namespace LeagueSharp.Loader.Data
{
    public static class Directories
    {
        public static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string AppDataDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LS" +
            Environment.UserName.GetHashCode().ToString("X")) + "\\";

        public static readonly string RepositoryDir = Path.Combine(AppDataDirectory, "Repositories") + "\\";
        public static readonly string AssembliesDir = Path.Combine(AppDataDirectory, "1") + "\\";
        public static readonly string CoreDirectory = Path.Combine(CurrentDirectory, "System") + "\\";
        public static readonly string LogsDir = Path.Combine(CurrentDirectory, "Logs") + "\\";

        public static readonly string LocalRepoDir = Path.Combine(CurrentDirectory, "LocalAssemblies") + "\\";
        public static readonly string LoaderFilePath = Path.Combine(CurrentDirectory, Process.GetCurrentProcess().ProcessName);
        public static readonly string ConfigFilePath = Path.Combine(CurrentDirectory, "config.xml");
        public static readonly string CoreFilePath = Path.Combine(CoreDirectory, "Leaguesharp.Core.dll");
        public static readonly string BootstrapFilePath = Path.Combine(CoreDirectory, "LeagueSharp.Bootstrap.dll");
        public static readonly string SandboxFilePath = Path.Combine(CoreDirectory, "LeagueSharp.Sandbox.dll");
    }
}
