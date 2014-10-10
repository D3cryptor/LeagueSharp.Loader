using System.Windows.Data;
using System.Windows.Forms;
using Hardcodet.Wpf.TaskbarNotification;
using Clipboard = System.Windows.Clipboard;
using DataGrid = System.Windows.Controls.DataGrid;
using WebBrowser = System.Windows.Controls.WebBrowser;

#region

using System.Windows.Navigation;

#region

using System.Windows.Input;

#region

using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LeagueSharp.Loader.Class;
using LeagueSharp.Loader.Data;
using MahApps.Metro.Controls.Dialogs;

#endregion

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

namespace LeagueSharp.Loader.Views
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        public BackgroundWorker BgWorker = new BackgroundWorker();
        public bool BgWorkerCancelled;
        public bool FirstTimeActivated = true;
        private bool _working;

        public Config Config { get; set; }

        public bool Working
        {
            get { return _working; }
            set
            {
                _working = value;
                OnPropertyChanged("Working");
                InstalledAssembliesDataGrid.Items.Refresh();
            }
        }

        public Thread InjectThread { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Utility.CreateFileFromResource("config.xml", "LeagueSharp.Loader.Resources.config.xml");

            try
            {
                Config = ((Config)Utility.MapXmlFileToClass(typeof(Config), "config.xml"));
            }
            catch(Exception)
            {
                File.Delete(Path.Combine(Directories.CurrentDirectory, "config.xml"));
                System.Windows.MessageBox.Show("Couldn't load config.xml");
                Environment.Exit(0);
            }
            
            Browser.Visibility = Visibility.Hidden;
            DataContext = this;
            GeneralSettingsItem.IsSelected = true;

            if (!File.Exists(Path.Combine(Directories.CoreDirectory, "Leaguesharp.Core.dll")))
            {
                System.Windows.MessageBox.Show("Couldn't find Leaguesharp.Core.dll");
                Environment.Exit(0);
            }

            Updater.UpdateLoader();

            Updater.GetRepositories(
                delegate(List<string> list)
                {
                    if (list.Count > 0)
                    {
                        Config.KnownRepositories.Clear();
                        foreach (var repo in list)
                        {
                            Config.KnownRepositories.Add(repo);
                        }
                    }
                });

            if (Config.FirstRun)
            {
                LSUriScheme.CreateRegistryKeys(false);
            }
            
            //Try to login with the saved credentials.
            if (!Auth.Login(Config.Username, Config.Password).Item1)
            {
                ShowLoginDialog();
            }
            else
            {
                OnLogin(Config.Username);
            }

            Config.FirstRun = false;

            //Used to reload the assemblies from inside the game.
            KeyboardHook.SetHook();
            KeyboardHook.OnKeyUpTrigger += KeyboardHookOnOnKeyUpTrigger;
            KeyboardHook.HookedKeys.Add(KeyInterop.VirtualKeyFromKey(Config.Hotkeys.SelectedHotkeys.First(h => h.Name == "Reload").Hotkey));
            KeyboardHook.HookedKeys.Add(KeyInterop.VirtualKeyFromKey(Config.Hotkeys.SelectedHotkeys.First(h => h.Name == "CompileAndReload").Hotkey));

            foreach (var hk in Config.Hotkeys.SelectedHotkeys)
            {
                hk.PropertyChanged += hk_PropertyChanged;
            }

            InjectThread = new Thread(
                () =>
                {
                    while (true)
                    {
                        if (Config.Install)
                        {
                            Injection.Pulse();
                        }
                        Thread.Sleep(3000);
                    }
                });

            InjectThread.Start();
            Config.PropertyChanged += ConfigOnPropertyChanged;
            foreach (var gameSetting in Config.Settings.GameSettings)
            {
                gameSetting.PropertyChanged += GameSettingOnPropertyChanged;
            }

            SettingsTabItem.Visibility = Visibility.Hidden;
        }

        void hk_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            KeyboardHook.HookedKeys.Add(KeyInterop.VirtualKeyFromKey(((HotkeyEntry)sender).Hotkey));
        }

        private void GameSettingOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (Injection.IsInjected)
            {
                Injection.SendConfig(IntPtr.Zero, Config);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 32769)
            {
                var lhwid = wParam;
                Task.Factory.StartNew(
                    () =>
                    {
                        Injection.SendConfig(lhwid, Config);
                        Thread.Sleep(1500);
                        foreach (var assembly in Config.SelectedProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                        {
                            Injection.LoadAssembly(lhwid, assembly);
                        }
                    });
            }
            return IntPtr.Zero;
        }

        private void KeyboardHookOnOnKeyUpTrigger(int vKeyCode)
        {
            if (!Injection.IsInjected)
            {
                return;
            }

            var reloadVKey =
                KeyInterop.VirtualKeyFromKey(Config.Hotkeys.SelectedHotkeys.First(h => h.Name == "Reload").Hotkey);
            var reloadAndCompileVKey =
                KeyInterop.VirtualKeyFromKey(
                    Config.Hotkeys.SelectedHotkeys.First(h => h.Name == "CompileAndReload").Hotkey);

            if (vKeyCode == reloadVKey || vKeyCode == reloadAndCompileVKey)
            {
                var hwnd = Injection.GetLeagueWnd();
                var targetAssemblies =
                    Config.SelectedProfile.InstalledAssemblies.Where(
                        a => a.InjectChecked || a.Type == AssemblyType.Library).ToList();

                foreach (var assembly in targetAssemblies)
                {
                    Injection.UnloadAssembly(hwnd, assembly);
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

                foreach (var assembly in targetAssemblies)
                {
                    Injection.LoadAssembly(hwnd, assembly);
                }

                Injection.SendConfig(hwnd, Config);
            }
        }

        private async void ShowLoginDialog()
        {
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
            var result =
                await
                    this.ShowLoginAsync(
                        "LeagueSharp", "Sign in",
                        new LoginDialogSettings { ColorScheme = MetroDialogOptions.ColorScheme, NegativeButtonVisibility = Visibility.Visible});

            var loginResult = new Tuple<bool, string>(false, "Cancel button pressed");
            if (result != null)
            {
                var hash = Auth.Hash(result.Password);

                loginResult = Auth.Login(result.Username, hash);
            }

            if (result != null && loginResult.Item1)
            {
                //Save the login credentials
                Config.Username = result.Username;
                Config.Password = Auth.Hash(result.Password);

                OnLogin(result.Username);
            }
            else
            {
                if (result == null)
                {
                    MainWindow_OnClosing(null, null);
                    Environment.Exit(0);
                }

                ShowAfterLoginDialog(string.Format("Failed to login: {0}", loginResult.Item2), true);
                Utility.Log(
                    LogStatus.Error, "Login",
                    string.Format(
                        "Failed to sign in as {0}: {1}", (result != null ? result.Username : "null"), loginResult.Item2),
                    Logs.MainLog);
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

        private async void ShowTextMessage(string title, string message)
        {
            await this.ShowMessageAsync(title, message);
        }

        private void OnLogin(string username)
        {
            Utility.Log(LogStatus.Ok, "Login", string.Format("Succesfully signed in as {0}", username), Logs.MainLog);
            Browser.Visibility = Visibility.Visible;
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new InstallerWindow { Owner = this };
            window.ShowDialog();
        }

        public void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (BgWorker.IsBusy && e != null)
            {
                BgWorker.CancelAsync();
                e.Cancel = true;
                Hide();
                return;
            }

            Utility.MapClassToXmlFile(typeof(Config), Config, "config.xml");
            KeyboardHook.UnHook();
            InjectThread.Abort();

            var allAssemblies = new List<LeagueSharpAssembly>();
            foreach (var profile in Config.Profiles)
            {
                allAssemblies.AddRange(profile.InstalledAssemblies.ToList());
            }
            Utility.ClearDirectory(Directories.AssembliesDir);
            Utility.ClearDirectory(Directories.LogsDir);
            SvnUpdater.ClearUnusedRepos(allAssemblies);
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

        private void PrepareAssemblies(IEnumerable<LeagueSharpAssembly> assemblies, bool update, bool compile)
        {
            if (Working)
            {
                return;
            }

            Working = true;
            var leagueSharpAssemblies = assemblies as IList<LeagueSharpAssembly> ?? assemblies.ToList();

            BgWorker = new BackgroundWorker();
            BgWorker.WorkerSupportsCancellation = true;
            BgWorker.DoWork += delegate
            {
                var updatedSvnUrls = new List<string>();

                foreach (var assembly in leagueSharpAssemblies)
                {
                    if (assembly.Type == AssemblyType.Library)
                    {
                        if (update && !updatedSvnUrls.Contains(assembly.SvnUrl))
                        {
                            assembly.Update();
                            updatedSvnUrls.Add(assembly.SvnUrl);
                        }

                        if (compile)
                        {
                            assembly.Compile();
                        }
                    }
                    if (BgWorker.CancellationPending)
                    {
                        BgWorkerCancelled = true;
                        break;
                    }
                }

                foreach (var assembly in leagueSharpAssemblies)
                {
                    if (assembly.Type != AssemblyType.Library)
                    {
                        if (update && !updatedSvnUrls.Contains(assembly.SvnUrl))
                        {
                            assembly.Update();
                            updatedSvnUrls.Add(assembly.SvnUrl);
                        }

                        if (compile)
                        {
                            assembly.Compile();
                        }
                    }
                    if (BgWorker.CancellationPending)
                    {
                        BgWorkerCancelled = true;
                        break;
                    }
                }
            };

            BgWorker.RunWorkerCompleted += delegate
            {
                ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
                Working = false;
                if (BgWorkerCancelled)
                {
                    Close();
                }
            };
            BgWorker.RunWorkerAsync();
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
                        "Uninstall", "Are you sure you want to uninstall selected assemblies?",
                        MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Negative)
            {
                return;
            }

            foreach (var ee in asemblies)
            {
                Config.SelectedProfile.InstalledAssemblies.Remove(ee);
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
                System.Diagnostics.Process.Start(selectedAssembly.SvnUrl);
            }
        }

        private void ShareItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)InstalledAssembliesDataGrid.SelectedItems[0];
            if (selectedAssembly.SvnUrl.ToLower().StartsWith("https://github.com"))
            {
                var user = selectedAssembly.SvnUrl.Remove(0, 19);
                Clipboard.SetText(string.Format(LSUriScheme.FullName + "project/{0}/{1}/", user, selectedAssembly.Name));
                ShowTextMessage("Share", "Assembly url copied to the clipboard");
            }
        }

        private void LogItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (InstalledAssembliesDataGrid.SelectedItems.Count <= 0)
            {
                return;
            }

            var selectedAssembly = (LeagueSharpAssembly)InstalledAssembliesDataGrid.SelectedItems[0];
            var logFile = Path.Combine(Directories.LogsDir,
                "Error - " + Path.GetFileName(selectedAssembly.Name + ".txt"));
            if (File.Exists(logFile))
            {
                System.Diagnostics.Process.Start(logFile);
            }
            else
            {
                ShowTextMessage("Error", "Log file not found");
            }
        }

        private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Hidden)
            {
                Show();
                Activate();
                WindowState = WindowState.Normal;
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
            }
            else
            {
                Show();
            }
        }

        private void TrayIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;
            var view = CollectionViewSource.GetDefaultView(Config.SelectedProfile.InstalledAssemblies);
            searchText = searchText.Replace("*", "(.*)");
            view.Filter = obj =>
            {
                try
                {
                    var assembly = obj as LeagueSharpAssembly;
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
                PrepareAssemblies(Config.SelectedProfile.InstalledAssemblies, Config.FirstRun || Config.UpdateOnLoad, true);
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
            if (Config.Profiles.Count > 1)
            {
                Config.Profiles.RemoveAt(ProfilesButton.SelectedIndex);
                Config.SelectedProfile = Config.Profiles.First();
            }
            else
            {
                Config.SelectedProfile.InstalledAssemblies = new ObservableCollection<LeagueSharpAssembly>();
                Config.SelectedProfile.Name = "Default";
            }
        }

        private void NewProfileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Config.Profiles.Add(
                new Profile
                {
                    InstalledAssemblies = new ObservableCollection<LeagueSharpAssembly>(),
                    Name = "New profile"
                });

            Config.SelectedProfile = Config.Profiles.Last();
        }

        private async void ShowProfileNameChangeDialog()
        {
            var result = await this.ShowInputAsync("Rename", "Insert the new name for the profile", new MetroDialogSettings { 
            DefaultText = Config.SelectedProfile.Name,
            });

            if (!string.IsNullOrEmpty(result))
            {
                Config.SelectedProfile.Name = result;
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
            if (Injection.IsInjected)
            {
                var hwnd = Injection.GetLeagueWnd();

                foreach (var assembly in oldProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                {
                    Injection.UnloadAssembly(hwnd, assembly);
                }

                var assembliesToLoad =
                    newProfile.InstalledAssemblies.Where(a => a.InjectChecked || a.Type == AssemblyType.Library);

                //Recompile the assemblies:
                foreach (var assembly in assembliesToLoad)
                {
                    if (assembly.Type == AssemblyType.Library)
                    {
                        assembly.Compile();
                    }
                }

                foreach (var assembly in assembliesToLoad)
                {
                    if (assembly.Type != AssemblyType.Library)
                    {
                        assembly.Compile();
                    }
                }

                foreach (var assembly in assembliesToLoad)
                {
                    Injection.LoadAssembly(hwnd, assembly);
                }
            }
            else
            {
                PrepareAssemblies(Config.SelectedProfile.InstalledAssemblies, false, true);
            }
            TextBoxBase_OnTextChanged(null, null);
        }

        private void Browser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var script = "document.body.style.overflow ='hidden'";
            var wb = (WebBrowser)sender;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
        }

        private void ConfigOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Install")
            {
                if (Injection.IsInjected)
                {
                    var hwnd = Injection.GetLeagueWnd();
                    if (!Config.Install)
                    {
                        foreach (var assembly in Config.SelectedProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                        {
                            Injection.UnloadAssembly(hwnd, assembly);
                        }
                    }
                    else
                    {
                        foreach (var assembly in Config.SelectedProfile.InstalledAssemblies.Where(a => a.InjectChecked))
                        {
                            Injection.LoadAssembly(hwnd, assembly);
                        }
                    }
                }
            }
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var name = (string)((TreeViewItem)((System.Windows.Controls.TreeView)sender).SelectedItem).Header;
            SettingsFrame.Content = Activator.CreateInstance(null, "LeagueSharp.Loader.Views.Settings." + name).Unwrap();
        }

        private void UpdateAll_OnClick(object sender, RoutedEventArgs e)
        {
            PrepareAssemblies(Config.SelectedProfile.InstalledAssemblies, true, true);
        }

        private void CompileAll_OnClick(object sender, RoutedEventArgs e)
        {
            PrepareAssemblies(Config.SelectedProfile.InstalledAssemblies, false, true);
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
        }
    }
}