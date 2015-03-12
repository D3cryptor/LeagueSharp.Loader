#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// Config.cs is part of LeagueSharp.Loader.
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

namespace LeagueSharp.Loader.Data
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows.Input;
    using System.Xml.Serialization;
    using LeagueSharp.Loader.Class;

    #endregion

    public static class Directories
    {
        public static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\";

        public static string AppDataDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueSharp") + "\\";

        public static string RepositoryDir = Path.Combine(AppDataDirectory, "Repositories") + "\\";
        public static string AssembliesDir = Path.Combine(AppDataDirectory, "Assemblies") + "\\";
        public static readonly string CoreDirectory = Path.Combine(CurrentDirectory, "System") + "\\";
        public static readonly string LogsDir = Path.Combine(CurrentDirectory, "Logs") + "\\";

        public static readonly string LocalRepoDir = Path.Combine(CurrentDirectory, "LocalAssemblies") + "\\";
        public static readonly string LoaderFilePath = Path.Combine(CurrentDirectory, "Leaguesharp.Loader.exe");
        public static readonly string ConfigFilePath = Path.Combine(CurrentDirectory, "config.xml");
        public static readonly string CoreFilePath = Path.Combine(CoreDirectory, "Leaguesharp.Core.dll");
        public static readonly string BootstrapFilePath = Path.Combine(CoreDirectory, "Leaguesharp.Bootstrap.dll");
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Config : INotifyPropertyChanged
    {
        [XmlIgnore] public static Config Instance;
        private bool _firstRun = true;
        private Hotkeys _hotkeys;

        private bool _install = true;
        private ObservableCollection<string> _knownRepositories;
        private string _leagueOfLegendsExePath;
        private ObservableCollection<Profile> _profiles;
        private string _selectedLanguage;
        private Profile _selectedProfile;
        private ConfigSettings _settings;
        private bool _showDevOptions;
        private bool _updateOnLoad;
        private bool _tosAccepted;
        private string _appDirectory;

        public string AppDirectory
        {
            get { return _appDirectory; }
            set
            {
                _appDirectory = value;
                OnPropertyChanged("AppDirectory");
            }
        }

        public bool TosAccepted
        {
            get { return _tosAccepted; }
            set
            {
                _tosAccepted = value;
                OnPropertyChanged("TosAccepted");
            }
        }

        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged("SelectedLanguage");
            }
        }

        public string LeagueOfLegendsExePath
        {
            get { return _leagueOfLegendsExePath; }
            set
            {
                _leagueOfLegendsExePath = value;
                OnPropertyChanged("LeagueOfLegendsExePath");
            }
        }

        public bool FirstRun
        {
            get { return _firstRun; }
            set
            {
                _firstRun = value;
                OnPropertyChanged("FirstRun");
            }
        }

        public bool ShowDevOptions
        {
            get { return _showDevOptions; }
            set
            {
                _showDevOptions = value;
                OnPropertyChanged("ShowDevOptions");
            }
        }

        public bool Install
        {
            get { return _install; }
            set
            {
                _install = value;
                OnPropertyChanged("Install");
            }
        }

        public bool UpdateOnLoad
        {
            get { return _updateOnLoad; }
            set
            {
                _updateOnLoad = value;
                OnPropertyChanged("UpdateOnLoad");
            }
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public ConfigSettings Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChanged("Settings");
            }
        }

        public Hotkeys Hotkeys
        {
            get { return _hotkeys; }
            set
            {
                _hotkeys = value;
                OnPropertyChanged("Hotkeys");
            }
        }

        public Profile SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                _selectedProfile = value;
                OnPropertyChanged("SelectedProfile");
            }
        }

        [XmlArrayItem("Profiles", IsNullable = true)]
        public ObservableCollection<Profile> Profiles
        {
            get { return _profiles; }
            set
            {
                _profiles = value;
                OnPropertyChanged("Profiles");
            }
        }

        [XmlArrayItem("KnownRepositories", IsNullable = true)]
        public ObservableCollection<string> KnownRepositories
        {
            get { return _knownRepositories; }
            set
            {
                _knownRepositories = value;
                OnPropertyChanged("KnownRepositories");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    [XmlType(AnonymousType = true)]
    public class ConfigSettings : INotifyPropertyChanged
    {
        private ObservableCollection<GameSettings> _gameSettings;

        [XmlArrayItem("GameSettings", IsNullable = true)]
        public ObservableCollection<GameSettings> GameSettings
        {
            get { return _gameSettings; }
            set
            {
                _gameSettings = value;
                OnPropertyChanged("GameSettings");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class GameSettings : INotifyPropertyChanged
    {
        private string _name;

        private List<string> _posibleValues;
        private string _selectedValue;

        [XmlIgnore]
        public string DisplayName
        {
            get { return Utility.GetMultiLanguageText(_name); }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public List<string> PosibleValues
        {
            get { return _posibleValues; }
            set
            {
                _posibleValues = value;
                OnPropertyChanged("PosibleValues");
            }
        }

        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                OnPropertyChanged("SelectedValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [XmlType(AnonymousType = true)]
    public class Hotkeys : INotifyPropertyChanged
    {
        private ObservableCollection<HotkeyEntry> _selectedHotkeys;

        [XmlArrayItem("SelectedHotkeys", IsNullable = true)]
        public ObservableCollection<HotkeyEntry> SelectedHotkeys
        {
            get { return _selectedHotkeys; }
            set
            {
                _selectedHotkeys = value;
                OnPropertyChanged("Hotkeys");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class HotkeyEntry : INotifyPropertyChanged
    {
        private Key _hotkey;
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string DisplayDescription
        {
            get { return Utility.GetMultiLanguageText(Description); }
        }

        public string Description { get; set; }

        public Key Hotkey
        {
            get { return _hotkey; }
            set
            {
                _hotkey = value;
                OnPropertyChanged("Hotkey");
                OnPropertyChanged("HotkeyString");
            }
        }

        public byte HotkeyInt
        {
            get
            {
                if (Hotkey == Key.LeftShift || Hotkey == Key.RightShift)
                {
                    return 16;
                }

                if (Hotkey == Key.LeftAlt || Hotkey == Key.RightAlt)
                {
                    return 0x12;
                }

                if (Hotkey == Key.LeftCtrl || Hotkey == Key.RightCtrl)
                {
                    return 0x11;
                }

                return (byte) KeyInterop.VirtualKeyFromKey(Hotkey);
            }
            set { }
        }

        public string HotkeyString
        {
            get { return _hotkey.ToString(); }
        }

        public Key DefaultKey { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}