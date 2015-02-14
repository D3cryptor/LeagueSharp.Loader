#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// MainWindow.xaml.cs is part of LeagueSharp.Loader.
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

namespace LeagueSharp.Loader.Views
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interop;
    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;
    using MahApps.Metro.Controls.Dialogs;
    using Microsoft.Build.Evaluation;

    #endregion

    public partial class MainWindow : INotifyPropertyChanged
    {
        public BackgroundWorker AssembliesWorker = new BackgroundWorker();
        public BackgroundWorker UpdaterWorker = new BackgroundWorker();
        public bool AssembliesWorkerCancelled;
        public bool FirstTimeActivated = true;
        private bool _working;
        private bool _checkingForUpdates;
        private string _statusString = "?";
        private Tuple<bool, string> _loaderVersionCheckResult;

        private string _updateMessage;

        public Config Config
        {
            get { return Config.Instance; }
            set { Config.Instance = value; }
        }

        public string StatusString
        {
            get { return _statusString; }
            set
            {
                _statusString = value;
                OnPropertyChanged("StatusString");
            }
        }

        public bool CheckingForUpdates
        {
            get { return _checkingForUpdates; }
            set
            {
                _checkingForUpdates = value;
                OnPropertyChanged("CheckingForUpdates");
            }
        }

        public bool Working
        {
            get { return _working; }
            set
            {
                _working = value;
                OnPropertyChanged("Working");
            }
        }

        public Thread InjectThread { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Browser.Visibility = Visibility.Hidden;
            DataContext = this;
            GeneralSettingsItem.IsSelected = true;

            #region ContextMenu.DevMenu

            DevMenu.Visibility = Config.Instance.ShowDevOptions ? Visibility.Visible : Visibility.Collapsed;
            Config.PropertyChanged += (o, args) =>
            {
                if (args.PropertyName == "ShowDevOptions")
                {
                    DevMenu.Visibility = Config.Instance.ShowDevOptions ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            #endregion

            if (!File.Exists(Directories.CoreFilePath))
            {
                MessageBox.Show(string.Format("Couldn't find {0}", Path.GetFileName(Directories.CoreFilePath)));
                Environment.Exit(0);
            }

            Updater.MainWindow = this;
            CheckForUpdates(true, true, false);

            Updater.GetRepositories(
                delegate(List<string> list)
                {
                    if (list.Count > 0)
                    {
                        Config.Instance.KnownRepositories.Clear();
                        foreach (var repo in list)
                        {
                            Config.Instance.KnownRepositories.Add(repo);
                        }
                    }
                });

            //Try to login with the saved credentials.
            if (!Auth.Login(Config.Instance.Username, Config.Instance.Password).Item1)
            {
                ShowLoginDialog();
            }
            else
            {
                OnLogin(Config.Instance.Username);
            }

            Config.Instance.FirstRun = false;

            //Used to reload the assemblies from inside the game.
            KeyboardHook.SetHook();
            KeyboardHook.OnKeyUpTrigger += KeyboardHookOnOnKeyUpTrigger;
            KeyboardHook.HookedKeys.Add(
                KeyInterop.VirtualKeyFromKey(
                    Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "Reload").Hotkey));
            KeyboardHook.HookedKeys.Add(
                KeyInterop.VirtualKeyFromKey(
                    Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "CompileAndReload").Hotkey));

            foreach (var hk in Config.Instance.Hotkeys.SelectedHotkeys)
            {
                hk.PropertyChanged += hk_PropertyChanged;
            }

            Injection.OnInject += hwnd => Task.Factory.StartNew(
                () =>
                {
                    Injection.SendConfig(hwnd);
                    foreach (var assembly in
                        Config.Instance.SelectedProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                    {
                        Injection.LoadAssembly(hwnd, assembly);
                    }
                });

            InjectThread = new Thread(
                () =>
                {
                    while (true)
                    {
                        if (Config.Instance.Install)
                        {
                            Injection.Pulse();
                        }
                        Thread.Sleep(3000);
                    }
                });

            InjectThread.Start();
            Config.Instance.PropertyChanged += ConfigOnPropertyChanged;
            foreach (var gameSetting in Config.Instance.Settings.GameSettings)
            {
                gameSetting.PropertyChanged += GameSettingOnPropertyChanged;
            }
            NewsTabItem.Visibility = Visibility.Hidden;
            AssembliesTabItem.Visibility = Visibility.Hidden;
            SettingsTabItem.Visibility = Visibility.Hidden;
        }

        private void CheckForUpdates(bool loader, bool core, bool showDialogOnFinish)
        {
            if (CheckingForUpdates)
            {
                return;
            }
            StatusString = Utility.GetMultiLanguageText("Checking");
            _updateMessage = "";
            CheckingForUpdates = true;
            UpdaterWorker = new BackgroundWorker();

            UpdaterWorker.DoWork += delegate
            {
                if (loader)
                {
                    _loaderVersionCheckResult = Updater.CheckLoaderVersion();
                }

                if (core)
                {
                    if (Config.Instance.LeagueOfLegendsExePath != null)
                    {
                        var exe = Utility.GetLatestLeagueOfLegendsExePath(Config.Instance.LeagueOfLegendsExePath);
                        if (exe != null)
                        {
                            var updateResult = Updater.UpdateCore(exe, !showDialogOnFinish);
                            _updateMessage = updateResult.Item3;
                            switch (updateResult.Item2)
                            {
                                case true:
                                    StatusString = Utility.GetMultiLanguageText("Updated");
                                    break;
                                case false:
                                    StatusString = Utility.GetMultiLanguageText("OUTDATED");
                                    break;
                                default:
                                    StatusString = Utility.GetMultiLanguageText("Unknown");
                                    break;
                            }

                            return;
                        }
                    }
                    StatusString = Utility.GetMultiLanguageText("Unknown");
                    _updateMessage = Utility.GetMultiLanguageText("LeagueNotFound");
                }
            };

            UpdaterWorker.RunWorkerCompleted += delegate
            {
                try
                {
                    if (File.Exists(Updater.SetupFile))
                    {
                        Thread.Sleep(1000);
                        File.Delete(Updater.SetupFile);
                    }
                }
                catch
                {
                    MessageBox.Show(Utility.GetMultiLanguageText("FailedToDelete"));
                    Environment.Exit(0);
                }

                if (_loaderVersionCheckResult != null && _loaderVersionCheckResult.Item1)
                {
                    //Update the loader only when we are not injected to be able to replace the core files.
                    if (!Injection.IsInjected)
                    {
                        Updater.Updating = true;
                        Updater.UpdateLoader(_loaderVersionCheckResult);
                    }
                }

                Updater.CheckedForUpdates = true;
                CheckingForUpdates = false;
                if (showDialogOnFinish)
                {
                    ShowTextMessage(Utility.GetMultiLanguageText("UpdateStatus"), _updateMessage);
                }
            };

            UpdaterWorker.RunWorkerAsync();
        }

        private void hk_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            KeyboardHook.HookedKeys.Add(KeyInterop.VirtualKeyFromKey(((HotkeyEntry)sender).Hotkey));
            try
            {
                Utility.MapClassToXmlFile(typeof(Config), Config.Instance, Directories.ConfigFilePath);
            }
            catch { }
        }

        private void GameSettingOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            foreach (var instance in Injection.LeagueInstances)
            {
                Injection.SendConfig(instance);
            }
        }

        private void KeyboardHookOnOnKeyUpTrigger(int vKeyCode)
        {
            if (!Injection.IsInjected)
            {
                return;
            }

            var reloadVKey =
                KeyInterop.VirtualKeyFromKey(
                    Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "Reload").Hotkey);
            var reloadAndCompileVKey =
                KeyInterop.VirtualKeyFromKey(
                    Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "CompileAndReload").Hotkey);

            if (vKeyCode == reloadVKey || vKeyCode == reloadAndCompileVKey)
            {
                var targetAssemblies =
                    Config.Instance.SelectedProfile.InstalledAssemblies.Where(
                        a => a.InjectChecked || a.Type == AssemblyType.Library).ToList();

                foreach (var instance in Injection.LeagueInstances)
                {
                    foreach (var assembly in targetAssemblies)
                    {
                        Injection.UnloadAssembly(instance, assembly);
                    }
                }

                if (vKeyCode == reloadAndCompileVKey)
                {
                    //Recompile the assemblies:
                    foreach (var assembly in targetAssemblies)
                    {
                        if (assembly.Type == AssemblyType.Library)
                        {
                            assembly.Compile();
                        }
                    }

                    foreach (var assembly in targetAssemblies)
                    {
                        if (assembly.Type != AssemblyType.Library)
                        {
                            assembly.Compile();
                        }
                    }
                }

                foreach (var instance in Injection.LeagueInstances)
                {
                    foreach (var assembly in targetAssemblies)
                    {
                        Injection.LoadAssembly(instance, assembly);
                    }

                    Injection.SendConfig(instance);
                }
            }
        }

        private async void ShowLoginDialog()
        {
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
            var result =
                await
                    this.ShowLoginAsync(
                        "LeagueSharp", "Sign in",
                        new LoginDialogSettings
                        {
                            ColorScheme = MetroDialogOptions.ColorScheme,
                            NegativeButtonVisibility = Visibility.Visible
                        });

            var loginResult = new Tuple<bool, string>(false, "Cancel button pressed");
            if (result != null)
            {
                var hash = Auth.Hash(result.Password);

                loginResult = Auth.Login(result.Username, hash);
            }

            if (result != null && loginResult.Item1)
            {
                //Save the login credentials
                Config.Instance.Username = result.Username;
                Config.Instance.Password = Auth.Hash(result.Password);

                OnLogin(result.Username);
            }
            else
            {
                if (result == null)
                {
                    MainWindow_OnClosing(null, null);
                    Environment.Exit(0);
                }

                ShowAfterLoginDialog(
                    string.Format(Utility.GetMultiLanguageText("FailedToLogin"), loginResult.Item2), true);
                Utility.Log(
                    LogStatus.Error, Utility.GetMultiLanguageText("Login"),
                    string.Format(
                        Utility.GetMultiLanguageText("LoginError"), (result != null ? result.Username : "null"),
                        loginResult.Item2), Logs.MainLog);
            }
        }

        private async void ShowAfterLoginDialog(string message, bool showLoginDialog)
        {
            await this.ShowMessageAsync("Login", message);
            if (showLoginDialog)
            {
                ShowLoginDialog();
            }
        }

        public async void ShowTextMessage(string title, string message)
        {
            var visibility = Browser.Visibility;
            Browser.Visibility = Visibility.Hidden;
            await this.ShowMessageAsync(title, message);
            Browser.Visibility = (visibility == Visibility.Hidden) ? Visibility.Hidden : Visibility.Visible;
        }

        private void OnLogin(string username)
        {
            Utility.Log(LogStatus.Ok, "Login", string.Format("Succesfully signed in as {0}", username), Logs.MainLog);
            Browser.Visibility = Visibility.Visible;
            try
            {
                Utility.MapClassToXmlFile(typeof(Config), Config.Instance, Directories.ConfigFilePath);
            }
            catch
            {
                MessageBox.Show(Utility.GetMultiLanguageText("ConfigWriteError"));
            }
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new InstallerWindow { Owner = this };
            window.ShowDialog();
        }

        public void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (AssembliesWorker.IsBusy && e != null)
            {
                AssembliesWorker.CancelAsync();
                e.Cancel = true;
                Hide();
                return;
            }

            try
            {
                Utility.MapClassToXmlFile(typeof(Config), Config.Instance, Directories.ConfigFilePath);
            }
            catch
            {
                MessageBox.Show(Utility.GetMultiLanguageText("ConfigWriteError"));
            }

            KeyboardHook.UnHook();
            InjectThread.Abort();

            var allAssemblies = new List<LeagueSharpAssembly>();
            foreach (var profile in Config.Instance.Profiles)
            {
                allAssemblies.AddRange(profile.InstalledAssemblies.ToList());
            }

            Utility.ClearDirectory(Directories.AssembliesDir);
            Utility.ClearDirectory(Directories.LogsDir);
            GitUpdater.ClearUnusedRepos(allAssemblies);
        }

        private void InstalledAssembliesDataGrid_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            if (dataGrid != null)
            {
                if (dataGrid.SelectedItems.Count == 0)
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void UpdateAndCompileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count == 0)
            {
                return;
            }

            PrepareAssemblies(InstalledAssembliesDataGrid.SelectedItems.Cast<LeagueSharpAssembly>(), true, true);
        }

        public void PrepareAssemblies(IEnumerable<LeagueSharpAssembly> assemblies, bool update, bool compile, bool updateCommonLibOnly = false)
        {
            if (Working)
            {
                return;
            }

            Working = true;
            var leagueSharpAssemblies = assemblies as IList<LeagueSharpAssembly> ?? assemblies.ToList();

            AssembliesWorker = new BackgroundWorker();
            AssembliesWorker.WorkerSupportsCancellation = true;
            AssembliesWorker.DoWork += delegate
            {
                if (update || updateCommonLibOnly)
                {
                    var updateList = leagueSharpAssemblies.GroupBy(a => a.SvnUrl).Select(g => g.First());

                    Parallel.ForEach(
                        updateList.Where(a => !updateCommonLibOnly || a.SvnUrl == "https://github.com/LeagueSharp/LeagueSharpCommon" ), new ParallelOptions { MaxDegreeOfParallelism = 5 }, (assembly, state) =>
                        {
                            assembly.Update();
                            if (AssembliesWorker.CancellationPending)
                            {
                                AssembliesWorkerCancelled = true;
                                state.Break();
                            }
                        });
                }

                if (compile)
                {
                    foreach (var assembly in leagueSharpAssemblies.OrderBy(a => a.Type))
                    {
                        assembly.Compile();

                        if (AssembliesWorker.CancellationPending)
                        {
                            AssembliesWorkerCancelled = true;
                            break;
                        }
                    }
                }
            };

            AssembliesWorker.RunWorkerCompleted += delegate
            {
                ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
                Working = false;
                if (AssembliesWorkerCancelled)
                {
                    Close();
                }
            };
            AssembliesWorker.RunWorkerAsync();
        }

        private void RemoveMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var remove = InstalledAssembliesDataGrid.SelectedItems.Cast<LeagueSharpAssembly>().ToList();
            DeleteWithConfirmation(remove);
        }

        private async void DeleteWithConfirmation(IEnumerable<LeagueSharpAssembly> asemblies)
        {
            var result =
                await
                    this.ShowMessageAsync(
                        Utility.GetMultiLanguageText("Uninstall"), Utility.GetMultiLanguageText("UninstallConfirm"),
                        MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Negative)
            {
                return;
            }

            foreach (var ee in asemblies)
            {
                Config.Instance.SelectedProfile.InstalledAssemblies.Remove(ee);
            }
        }

        private void GithubItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }
            var selectedAssembly = (LeagueSharpAssembly)InstalledAssembliesDataGrid.SelectedItems[0];
            if (selectedAssembly.SvnUrl != "")
            {
                Process.Start(selectedAssembly.SvnUrl);
            }
            else if (Directory.Exists(Path.GetDirectoryName(selectedAssembly.PathToProjectFile)))
            {
                Process.Start(Path.GetDirectoryName(selectedAssembly.PathToProjectFile));
            }
        }

        private void ShareItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var stringToAppend = "";
            var count = 0;
            foreach (var selectedAssembly in InstalledAssembliesDataGrid.SelectedItems.Cast<LeagueSharpAssembly>())
            {
                if (selectedAssembly.SvnUrl.StartsWith("https://github.com", StringComparison.InvariantCultureIgnoreCase))
                {
                    var user = selectedAssembly.SvnUrl.Remove(0, 19);
                    stringToAppend += string.Format("{0}/{1}/", user, selectedAssembly.Name);
                    count++;
                }
            }

            if (count > 0)
            {
                var uri = LSUriScheme.FullName + (count == 1 ? "project" : "projectGroup") + "/" + stringToAppend;
                Clipboard.SetText(uri);
                ShowTextMessage(Utility.GetMultiLanguageText("MenuShare"), Utility.GetMultiLanguageText("ShareText"));
            }
        }

        private void LogItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)InstalledAssembliesDataGrid.SelectedItems[0];
            var logFile = Path.Combine(
                Directories.LogsDir, "Error - " + Path.GetFileName(selectedAssembly.Name + ".txt"));
            if (File.Exists(logFile))
            {
                Process.Start(logFile);
            }
            else
            {
                ShowTextMessage("Error", Utility.GetMultiLanguageText("LogNotFound"));
            }
        }

        private void NewItem_OnClick(object sender, RoutedEventArgs e)
        {
            ShowNewAssemblyDialog();
        }

        private async void ShowNewAssemblyDialog()
        {
            var assemblyName = await this.ShowInputAsync("New Project", "Enter the new project name");

            if (assemblyName != null)
            {
                assemblyName = Regex.Replace(assemblyName, @"[^A-Za-z0-9]+", "");
            }

            if (!string.IsNullOrEmpty(assemblyName))
            {
                var leagueSharpAssembly = Utility.CreateEmptyAssembly(assemblyName);
                if (leagueSharpAssembly != null)
                {
                    leagueSharpAssembly.Compile();
                    Config.SelectedProfile.InstalledAssemblies.Add(leagueSharpAssembly);
                }
            }
        }

        private void EditItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)InstalledAssembliesDataGrid.SelectedItems[0];
            if (File.Exists(selectedAssembly.PathToProjectFile))
            {
                Process.Start(selectedAssembly.PathToProjectFile);
            }
        }

        private void CloneItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }
            var selectedAssembly = (LeagueSharpAssembly)InstalledAssembliesDataGrid.SelectedItems[0];
            try
            {
                var source = Path.GetDirectoryName(selectedAssembly.PathToProjectFile);
                var destination = Path.Combine(Directories.LocalRepoDir, selectedAssembly.Name) + "_clone_" +
                                  Environment.TickCount.GetHashCode().ToString("X");
                Utility.CopyDirectory(source, destination);
                var leagueSharpAssembly = new LeagueSharpAssembly(
                    selectedAssembly.Name + "_clone",
                    Path.Combine(destination, Path.GetFileName(selectedAssembly.PathToProjectFile)), "");
                leagueSharpAssembly.Compile();
                Config.SelectedProfile.InstalledAssemblies.Add(leagueSharpAssembly);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Hidden)
            {
                Show();
                Activate();
                WindowState = WindowState.Normal;
                MenuItemLabelHide.Header = "Hide";
            }
        }

        private void TrayMenuClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TrayMenuHide_OnClick(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
                MenuItemLabelHide.Header = "Show";
            }
            else
            {
                Show();
                MenuItemLabelHide.Header = "Hide";
            }
        }

        private void TrayIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
                MenuItemLabelHide.Header = "Show";
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;
            var view = CollectionViewSource.GetDefaultView(Config.Instance.SelectedProfile.InstalledAssemblies);
            searchText = searchText.Replace("*", "(.*)");
            view.Filter = obj =>
            {
                try
                {
                    var assembly = obj as LeagueSharpAssembly;

                    if (searchText == "checked")
                    {
                        return assembly.InjectChecked;
                    }

                    var nameMatch = Regex.Match(assembly.Name, searchText, RegexOptions.IgnoreCase);
                    var displayNameMatch = Regex.Match(assembly.DisplayName, searchText, RegexOptions.IgnoreCase);
                    var svnNameMatch = Regex.Match(assembly.SvnUrl, searchText, RegexOptions.IgnoreCase);

                    return displayNameMatch.Success || nameMatch.Success || svnNameMatch.Success;
                }
                catch (Exception)
                {
                    return true;
                }
            };
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            if (FirstTimeActivated)
            {
                FirstTimeActivated = false;

                var allAssemblies = new List<LeagueSharpAssembly>();
                foreach (var profile in Config.Instance.Profiles)
                {
                    allAssemblies.AddRange(profile.InstalledAssemblies);
                }

                allAssemblies = allAssemblies.Distinct().ToList();

                GitUpdater.ClearUnusedRepos(allAssemblies);
                PrepareAssemblies(allAssemblies, Config.Instance.FirstRun || Config.Instance.UpdateOnLoad, true, true);
            }

            var text = Clipboard.GetText();
            if (text.StartsWith(LSUriScheme.FullName))
            {
                Clipboard.SetText("");
                LSUriScheme.HandleUrl(text, this);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void RemoveProfileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (Config.Instance.Profiles.Count > 1)
            {
                Config.Instance.Profiles.RemoveAt(ProfilesButton.SelectedIndex);
                Config.Instance.SelectedProfile = Config.Instance.Profiles.First();
            }
            else
            {
                Config.Instance.SelectedProfile.InstalledAssemblies = new ObservableCollection<LeagueSharpAssembly>();
                Config.Instance.SelectedProfile.Name = Utility.GetMultiLanguageText("DefaultProfile");
            }
        }

        private void NewProfileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.Profiles.Add(
                new Profile
                {
                    InstalledAssemblies = new ObservableCollection<LeagueSharpAssembly>(),
                    Name = Utility.GetMultiLanguageText("NewProfile2")
                });

            Config.Instance.SelectedProfile = Config.Instance.Profiles.Last();
        }

        private async void ShowProfileNameChangeDialog()
        {
            var result =
                await
                    this.ShowInputAsync(
                        Utility.GetMultiLanguageText("Rename"), Utility.GetMultiLanguageText("RenameText"),
                        new MetroDialogSettings { DefaultText = Config.Instance.SelectedProfile.Name, });

            if (!string.IsNullOrEmpty(result))
            {
                Config.Instance.SelectedProfile.Name = result;
            }
        }

        private void ProfilesButton_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowProfileNameChangeDialog();
        }

        private void EditProfileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ShowProfileNameChangeDialog();
        }

        private void ProfilesButton_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0 || e.RemovedItems.Count <= 0)
            {
                return;
            }

            var oldProfile = (Profile)e.RemovedItems[0];
            var newProfile = (Profile)e.AddedItems[0];

            foreach (var instance in Injection.LeagueInstances)
            {
                foreach (var assembly in oldProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                {
                    Injection.UnloadAssembly(instance, assembly);
                }

                var assembliesToLoad =
                    newProfile.InstalledAssemblies.Where(a => a.InjectChecked || a.Type == AssemblyType.Library);

                foreach (var assembly in assembliesToLoad)
                {
                    Injection.LoadAssembly(instance, assembly);
                }
            }

            TextBoxBase_OnTextChanged(null, null);
        }

        private void ConfigOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Install")
            {
                foreach (var instance in Injection.LeagueInstances)
                {
                    if (!Config.Instance.Install)
                    {
                        foreach (var assembly in
                            Config.Instance.SelectedProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                        {
                            Injection.UnloadAssembly(instance, assembly);
                        }
                    }
                    else
                    {
                        foreach (var assembly in
                            Config.Instance.SelectedProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                        {
                            Injection.LoadAssembly(instance, assembly);
                        }
                    }
                }
            }
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var name = ((TreeViewItem)((TreeView)sender).SelectedItem).Uid;
            SettingsFrame.Content = Activator.CreateInstance(null, "LeagueSharp.Loader.Views.Settings." + name).Unwrap();
        }

        private void UpdateAll_OnClick(object sender, RoutedEventArgs e)
        {
            PrepareAssemblies(Config.Instance.SelectedProfile.InstalledAssemblies, true, true);
        }

        private void CompileAll_OnClick(object sender, RoutedEventArgs e)
        {
            PrepareAssemblies(Config.Instance.SelectedProfile.InstalledAssemblies, false, true);
        }

        private void NewsButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
        }

        private void AssemblyButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
        }

        private void StatusButton_OnClick(object sender, RoutedEventArgs e)
        {
            CheckForUpdates(true, true, true);
        }

        private void GithubAssembliesItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }
            var selectedAssembly = (LeagueSharpAssembly)InstalledAssembliesDataGrid.SelectedItems[0];
            if (selectedAssembly.SvnUrl != "")
            {
                var window = new InstallerWindow { Owner = this };
                window.ListAssemblies(selectedAssembly.SvnUrl, true);
                window.ShowDialog();
            }
        }
    }
}