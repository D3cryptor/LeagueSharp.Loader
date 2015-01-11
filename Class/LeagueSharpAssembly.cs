#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// LeagueSharpAssembly.cs is part of LeagueSharp.Loader.
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
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.Build.Evaluation;

    #endregion

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

    [XmlType(AnonymousType = true), Serializable]
    public class LeagueSharpAssembly : INotifyPropertyChanged
    {
        private string _displayName = "";
        private bool _injectChecked;
        private bool _installChecked;
        private string _pathToProjectFile = "";
        private string _svnUrl;
        private AssemblyType? _type = null;
        private string _pathToBinary = null;

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

        public bool InstallChecked
        {
            get { return _installChecked; }
            set
            {
                _installChecked = value;
                OnPropertyChanged("InstallChecked");
            }
        }

        public bool InjectChecked
        {
            get
            {
                if (Type == AssemblyType.Library)
                {
                    return true;
                }

                return _injectChecked;
            }
            set
            {
                if (value)
                {
                    foreach (var instance in Injection.LeagueInstances)
                    {
                        Injection.LoadAssembly(instance, this);
                    }
                }
                else
                {
                    foreach (var instance in Injection.LeagueInstances)
                    {
                        Injection.UnloadAssembly(instance, this);
                    }
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
            get
            {
                if (File.Exists(_pathToProjectFile))
                {
                    return _pathToProjectFile;
                }

                try
                {
                    var folderToSearch = Path.Combine(Directories.RepositoryDir, SvnUrl.GetHashCode().ToString("X"), "trunk");
                    var projectFile = Directory.GetFiles(folderToSearch, "*.csproj", SearchOption.AllDirectories)
                        .FirstOrDefault(file => Path.GetFileNameWithoutExtension(file) == Name);
                    if (projectFile != default(string))
                    {
                        return projectFile;
                    }
                }
                catch (Exception)
                {
                    
                }
                
                return _pathToProjectFile;
            }
            set
            {
                if (!value.Contains("%AppData%"))
                {
                    _pathToProjectFile = value;
                }
                else
                {
                    _pathToProjectFile = value.Replace(
                        "%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                }
            }
        }

        public string PathToBinary
        {
            get
            {
                if (_pathToBinary == null)
                {
                    _pathToBinary =
                        Path.Combine(
                            (Type == AssemblyType.Library ? Directories.CoreDirectory : Directories.AssembliesDir),
                            (Type == AssemblyType.Library ? "" : PathToProjectFile.GetHashCode().ToString("X")) +
                            Path.GetFileName(Compiler.GetOutputFilePath(GetProject())));
                }

                return _pathToBinary;
            }
        }

        public string Location
        {
            get { return SvnUrl == "" ? "Local" : SvnUrl; }
        }


        public AssemblyType Type
        {
            get
            {
                if (_type == null)
                {
                    var project = GetProject();
                    if (project != null)
                    {
                        _type = project.GetPropertyValue("OutputType").ToLower().Contains("exe")
                            ? AssemblyType.Executable
                            : AssemblyType.Library;
                    }
                }

                return _type ?? AssemblyType.Unknown;
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

        public override bool Equals(object obj)
        {
            if (obj is LeagueSharpAssembly)
            {
                return ((LeagueSharpAssembly) obj).PathToProjectFile == PathToProjectFile;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return PathToProjectFile.GetHashCode();
        }

        public Project GetProject()
        {
            if (File.Exists(PathToProjectFile))
            {
                try
                {
                    var pf = new ProjectFile(PathToProjectFile, Logs.MainLog)
                    {
                        Configuration = "Release",
                        PlatformTarget = "x86",
                        ReferencesPath = Directories.CoreDirectory,
                        UpdateReferences = true,
                        PostbuildEvent = true,
                        PrebuildEvent = true,
                        ResetOutputPath = true
                    };
                    pf.Change();

                   /* _pathToBinary =
                        Path.Combine(
                            (Type == AssemblyType.Library ? Directories.CoreDirectory : Directories.AssembliesDir),
                            (Type == AssemblyType.Library ? "" : PathToProjectFile.GetHashCode().ToString("X")) +
                            Path.GetFileName(Compiler.GetOutputFilePath(pf.Project)));

                    _type = pf.Project.GetPropertyValue("OutputType").ToLower().Contains("exe")
                        ? AssemblyType.Executable
                        : AssemblyType.Library;*/

                    return pf.Project;
                }
                catch (Exception e)
                {
                    Utility.Log(LogStatus.Error, "Builder", "Error: " + e, Logs.MainLog);
                }
            }
            return null;
        }

        public void Update()
        {
            if (Status == AssemblyStatus.Updating || SvnUrl == "")
            {
                return;
            }

            Status = AssemblyStatus.Updating;
            OnPropertyChanged("Version");
            try
            {
                GitUpdater.Update(SvnUrl, Logs.MainLog, Directories.RepositoryDir);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            Status = AssemblyStatus.Ready;
            OnPropertyChanged("Version");
        }

        public bool Compile()
        {
            Status = AssemblyStatus.Compiling;
            OnPropertyChanged("Version");
            var project = GetProject();

            if (Compiler.Compile(project, Path.Combine(Directories.LogsDir, Name + ".txt"), Logs.MainLog))
            {
                var result = Utility.OverwriteFile(Compiler.GetOutputFilePath(project), PathToBinary);

                Utility.ClearDirectory(Compiler.GetOutputFilePath(project));
                Utility.ClearDirectory(Path.Combine(project.DirectoryPath, "bin"));
                Utility.ClearDirectory(Path.Combine(project.DirectoryPath, "obj"));

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

        public LeagueSharpAssembly Copy()
        {
            return new LeagueSharpAssembly(Name, PathToProjectFile, SvnUrl);
        }
    }
}