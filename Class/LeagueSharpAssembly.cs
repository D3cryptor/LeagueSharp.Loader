using System.Windows;

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using LeagueSharp.Loader.Data;
using Microsoft.Build.Evaluation;

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
    public static class LeagueSharpAssemblies
    {
        public static List<LeagueSharpAssembly> GetAssemblies(string directory, string url = "")
        {
            var projectFiles = new List<string>();
            var foundAssemblies = new List<LeagueSharpAssembly>();

            try
            {
                projectFiles.AddRange(Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories));
                foreach (var projectFile in projectFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(projectFile);
                    foundAssemblies.Add(new LeagueSharpAssembly(name, projectFile, url));
                }
            }
            catch (Exception e)
            {
                Utility.Log(LogStatus.Error, "Updater", e.ToString(), Logs.MainLog);
            }

            return foundAssemblies;
        }
    }

    public enum AssemblyType
    {
        Library,
        Executable,
        Unknown,
    }

    public enum AssemblyStatus
    {
        Ready,
        Updating,
        UpdatingError,
        CompilingError,
        Compiling,
    }

    [ XmlType(AnonymousType = true) ]
    public class LeagueSharpAssembly : INotifyPropertyChanged
    {
        private string _displayName = "";
        private bool _injectChecked;
        private string _pathToProjectFile;
        private ProjectFile _pf;
        private Project _project;
        private string _svnUrl;

        public LeagueSharpAssembly()
        {
            Status = AssemblyStatus.Ready;
        }

        public LeagueSharpAssembly(string name, string path, string svnUrl)
        {
            Name = name;
            PathToProjectFile = path;
            SvnUrl = svnUrl;
            Description = "";
            Status = AssemblyStatus.Ready;
        }

        private bool _installChecked;

        public bool InstallChecked
        {
            get
            {
                return _installChecked;
            }
            set
            {
                _installChecked = value;
                OnPropertyChanged("InstallChecked");
            }
        }

        public bool InjectChecked
        {
            get { return _injectChecked; }
            set
            {
                if (value)
                {
                    Injection.LoadAssembly(Injection.GetLeagueWnd(), this);
                }
                else
                {
                    Injection.UnloadAssembly(Injection.GetLeagueWnd(), this);
                }

                _injectChecked = value;
                OnPropertyChanged("InjectChecked");
            }
        }

        public AssemblyStatus Status { get; set; }

        public string DisplayName
        {
            get { return _displayName == "" ? Name : _displayName; }
            set { _displayName = value; }
        }

        public string Name { get; set; }

        public string PathToProjectFile
        {
            get { return _pathToProjectFile; }
            set
            {
                if (!value.Contains("%AppData%"))
                {
                    _pathToProjectFile = value;
                }
                else
                {
                    _pathToProjectFile = value.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                }
            }
        }

        public string PathToBinary
        {
            get
            {
                return (Type == AssemblyType.Library ? Directories.LibrariesDir : Directories.AssembliesDir) +
                       Path.GetFileName(Compiler.GetOutputFilePath(Project));
            }
        }

        public string Location
        {
            get { return SvnUrl == "" ? "Local" : SvnUrl; }
        }

        public Project Project
        {
            get
            {
                if (_project != null)
                {
                    return _project;
                }

                if (File.Exists(PathToProjectFile))
                {
                    try
                    {
                        _pf = new ProjectFile(PathToProjectFile, Logs.MainLog)
                        {
                            Configuration = "Release",
                            PlatformTarget = "x86",
                            ReferencesPath = Directories.LibrariesDir,
                            UpdateReferences = true,
                            PostbuildEvent = true,
                            PrebuildEvent = true,
                            ResetOutputPath = true
                        };
                        _pf.Change();
                        _project = _pf.Project;
                    }
                    catch (Exception e)
                    {
                        Utility.Log(LogStatus.Error, "Builder", "Error: " + e, Logs.MainLog);
                    }
                }

                return _project;
            }
        }

        public AssemblyType Type
        {
            get
            {
                if (Project != null)
                {
                    return Project.GetPropertyValue("OutputType").ToLower() == "exe"
                        ? AssemblyType.Executable
                        : AssemblyType.Library;
                }

                return AssemblyType.Unknown;
            }
        }

        public string Description { get; set; }

        public string Version
        {
            get
            {
                if (Status != AssemblyStatus.Ready)
                {
                    return Status.ToString();
                }

                if (!string.IsNullOrEmpty(PathToBinary) && File.Exists(PathToBinary))
                {
                    return AssemblyName.GetAssemblyName(PathToBinary).Version.ToString();
                }
                return "?";
            }
        }

        public string SvnUrl
        {
            get { return _svnUrl; }
            set
            {
                _svnUrl = value;
                OnPropertyChanged("SvnUrl");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Update()
        {
            if (Status == AssemblyStatus.Updating || SvnUrl == "")
            {
                return;
            }

            Status = AssemblyStatus.Updating;
            OnPropertyChanged("Version");

            SvnUpdater.Update(SvnUrl, Logs.MainLog, Directories.RepositoryDir);

            Status = AssemblyStatus.Ready;
        }

        public bool Compile()
        {
            Status = AssemblyStatus.Compiling;
            OnPropertyChanged("Version");

            if (Compiler.Compile(Project, Path.Combine(Directories.LogsDir, Name + ".txt"), Logs.MainLog))
            {
                var result = Utility.OverwriteFile(
                    Compiler.GetOutputFilePath(Project),
                    (Type == AssemblyType.Library ? Directories.LibrariesDir : Directories.AssembliesDir) + "\\" +
                    Path.GetFileName(Compiler.GetOutputFilePath(Project)));

                Utility.ClearDirectory(Compiler.GetOutputFilePath(Project));
                Utility.ClearDirectory(Path.Combine(Project.DirectoryPath, "bin"));
                Utility.ClearDirectory(Path.Combine(Project.DirectoryPath, "obj"));

                if (result)
                {
                    Status = AssemblyStatus.Ready;
                }
                else
                {
                    Status = AssemblyStatus.CompilingError;
                }

                OnPropertyChanged("Version");
                OnPropertyChanged("Type");
                return result;
            }

            Status = AssemblyStatus.CompilingError;
            OnPropertyChanged("Version");
            return false;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
