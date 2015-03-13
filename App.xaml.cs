#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// App.xaml.cs is part of LeagueSharp.Loader.
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

namespace LeagueSharp.Loader
{
    #region

    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;

    #endregion

    public partial class App
    {
        private Mutex _mutex;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        protected override void OnStartup(StartupEventArgs e)
        {
            if (File.Exists(Updater.SetupFile))
            {
                Thread.Sleep(1000);
            }

            bool createdNew;
            _mutex = new Mutex(true, Utility.Md5Hash(Utility.Md5Checksum(Directories.LoaderFilePath) + Utility.Md5Hash(Environment.UserName)), out createdNew);
            if (!createdNew)
            {
                if (e.Args.Length > 0)
                {
                    var wnd = Injection.FindWindow(IntPtr.Zero, "LeagueSharp");
                    if (wnd != IntPtr.Zero)
                    {
                        Clipboard.SetText(e.Args[0]);
                        ShowWindow(wnd, 5);
                        SetForegroundWindow(wnd);
                    }
                }

                _mutex = null;
                Environment.Exit(0);
            }

            Utility.CreateFileFromResource(Directories.ConfigFilePath, "LeagueSharp.Loader.Resources.config.xml");

            var configCorrupted = false;
            try
            {
                Config.Instance = ((Config) Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath));
            }
            catch (Exception)
            {
                configCorrupted = true;
            }

            if (!configCorrupted)
            {
                try
                {
                    if (File.Exists(Directories.ConfigFilePath + ".bak"))
                    {
                        File.Delete(Directories.ConfigFilePath + ".bak");
                    }
                    File.Copy(Directories.ConfigFilePath, Directories.ConfigFilePath + ".bak");
                    File.SetAttributes(Directories.ConfigFilePath + ".bak", FileAttributes.Hidden);
                }
                catch (Exception)
                {
                    //ignore
                }
            }
            else
            {
                try
                {
                    Config.Instance = ((Config)Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath + ".bak"));
                    File.Delete(Directories.ConfigFilePath);
                    File.Copy(Directories.ConfigFilePath + ".bak", Directories.ConfigFilePath);
                    File.SetAttributes(Directories.ConfigFilePath, FileAttributes.Normal);
                }
                catch (Exception)
                {
                    File.Delete(Directories.ConfigFilePath + ".bak");
                    File.Delete(Directories.ConfigFilePath);
                    MessageBox.Show("Couldn't load config.xml.");
                    Environment.Exit(0);
                }
            }

            #region AppData randomization

            try
            {
                var oldPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), "LeagueSharp");

                var appPath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), "LeagueSharp" + Environment.UserName.GetHashCode().ToString("X"));

                if (Directory.Exists(oldPath) && !Directory.Exists(appPath))
                {
                    Directory.Move(oldPath, appPath);
                }

                if (string.IsNullOrEmpty(Config.Instance.AppDirectory) || !Directory.Exists(Config.Instance.AppDirectory))
                {
                    Config.Instance.AppDirectory = appPath;
                }
                else
                {
                    appPath = Config.Instance.AppDirectory;
                }

                Directories.AppDataDirectory = appPath + "\\";
                Directories.RepositoryDir = Path.Combine(Directories.AppDataDirectory, "Repositories") + "\\";
                Directories.AssembliesDir = Path.Combine(Directories.AppDataDirectory, "Assemblies") + "\\";
            }
            catch (Exception ex)
            {
                MessageBox.Show("AppData randomization failed.\n" + ex.Message, "Startup", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            #endregion

            //Load the language resources.
            var dict = new ResourceDictionary();

            if (Config.Instance.SelectedLanguage != null)
            {
                dict.Source = new Uri(
                    "..\\Resources\\Language\\" + Config.Instance.SelectedLanguage + ".xaml", UriKind.Relative);
            }
            else
            {
                var lid = Thread.CurrentThread.CurrentCulture.ToString().Contains("-")
                    ? Thread.CurrentThread.CurrentCulture.ToString().Split('-')[0].ToUpperInvariant()
                    : Thread.CurrentThread.CurrentCulture.ToString().ToUpperInvariant();
                switch (lid)
                {
                    case "DE":
                        dict.Source = new Uri("..\\Resources\\Language\\German.xaml", UriKind.Relative);
                        break;
                    case "AR":
                        dict.Source = new Uri("..\\Resources\\Language\\Arabic.xaml", UriKind.Relative);
                        break;
                    case "ES":
                        dict.Source = new Uri("..\\Resources\\Language\\Spanish.xaml", UriKind.Relative);
                        break;
                    case "FR":
                        dict.Source = new Uri("..\\Resources\\Language\\French.xaml", UriKind.Relative);
                        break;
                    case "IT":
                        dict.Source = new Uri("..\\Resources\\Language\\Italian.xaml", UriKind.Relative);
                        break;
                    case "KO":
                        dict.Source = new Uri("..\\Resources\\Language\\Korean.xaml", UriKind.Relative);
                        break;
                    case "NL":
                        dict.Source = new Uri("..\\Resources\\Language\\Dutch.xaml", UriKind.Relative);
                        break;
                    case "PL":
                        dict.Source = new Uri("..\\Resources\\Language\\Polish.xaml", UriKind.Relative);
                        break;
                    case "PT":
                        dict.Source = new Uri("..\\Resources\\Language\\Portuguese.xaml", UriKind.Relative);
                        break;
                    case "RO":
                        dict.Source = new Uri("..\\Resources\\Language\\Romanian.xaml", UriKind.Relative);
                        break;
                    case "RU":
                        dict.Source = new Uri("..\\Resources\\Language\\Russian.xaml", UriKind.Relative);
                        break;
                    case "SE":
                        dict.Source = new Uri("..\\Resources\\Language\\Swedish.xaml", UriKind.Relative);
                        break;
                    case "TR":
                        dict.Source = new Uri("..\\Resources\\Language\\Turkish.xaml", UriKind.Relative);
                        break;
                    case "VI":
                        dict.Source = new Uri("..\\Resources\\Language\\Vietnamese.xaml", UriKind.Relative);
                        break;
                    case "ZH":
                        dict.Source = new Uri("..\\Resources\\Language\\Chinese.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("..\\Resources\\Language\\English.xaml", UriKind.Relative);
                        break;
                }
            }

            Resources.MergedDictionaries.Add(dict);
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
            }
            base.OnExit(e);
        }
    }
}
