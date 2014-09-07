#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
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

    [ XmlType(AnonymousType = true) ]
    public class LeagueSharpAssembly : INotifyPropertyChanged
    {
        private string _displayName = "";
        private ProjectFile _pf;
        private Project _project;
        private string _svnUrl;

        public LeagueSharpAssembly()
        {
        }

        public LeagueSharpAssembly(string name, string path, string svnUrl)
        {
            Name = name;
            PathToProjectFile = path;
            SvnUrl = svnUrl;
            Description = "";
        }

        public bool InstallChecked { get; set; }

        private bool _injectChecked;

        public bool InjectChecked
        {
            get { return _injectChecked; }
            set
            {
                bool success;

                if (value)
                {
                    success = Loader.LoadAssembly(this);
                }
                else
                {
                    success = Loader.UnLoadAssembly(this);
                }

                if (success)
                {
                    _injectChecked =  value;
                    OnPropertyChanged("InjectChecked");
                }
            }
        }

        public string DisplayName
        {
            get { return _displayName == "" ? Name : _displayName; }
            set { _displayName = value; }
        }

        public string Name { get; set; }

        private string _pathToProjectFile;

        public string PathToProjectFile
        {
            get {return _pathToProjectFile; }
            set
            {
                if (!value.Contains("%leaguesharp%"))
                {
                    _pathToProjectFile = value;
                }
                else
                {
                    _pathToProjectFile = value.Replace("%leaguesharp%", AppDomain.CurrentDomain.BaseDirectory);
                }
            }
        }

        public string PathToBinary { get { return (Type == "Library" ? Directories.LibrariesDir : Directories.AssembliesDir) + Path.GetFileName(Compiler.GetOutputFilePath(Project)); } }

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
                            PrebuildEvent = true
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

        public string Type
        {
            get
            {
                if (Project != null)
                {
                    return Project.GetPropertyValue("OutputType").ToLower() == "exe" ? "Executable" : "Library";
                }

                return "?";
            }
        }

        public string Description { get; set; }

        public string Version
        {
            get
            {
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
            SvnUpdater.Update(SvnUrl, Logs.MainLog, Directories.RepositoryDir);
            OnPropertyChanged("Type");
            OnPropertyChanged("Version");
        }

        public bool Compile()
        {
            if (Compiler.Compile(Project, Path.Combine(Directories.LogsDir,  Name + ".txt"), Logs.MainLog))
            {
                var result = Utility.OverwriteFile(
                    Compiler.GetOutputFilePath(Project),
                    (Type == "Library" ? Directories.LibrariesDir : Directories.AssembliesDir) + "\\" +
                    Path.GetFileName(Compiler.GetOutputFilePath(Project)));
                OnPropertyChanged("Version");
                return result;
            }
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