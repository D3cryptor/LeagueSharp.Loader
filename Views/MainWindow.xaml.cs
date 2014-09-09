using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Application = System.Windows.Application;
using DataGrid = System.Windows.Controls.DataGrid;

#region

using System.Collections.Generic;
using Microsoft.Build.Evaluation;

#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LeagueSharp.Loader.Class;
using LeagueSharp.Loader.Data;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

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
    public partial class MainWindow : MetroWindow
    {
        public Config Config { get; set; }
        public bool Working { get; set; }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Utility.CreateFileFromResource("config.xml", "LeagueSharp.Loader.Resources.config.xml");
            Config = ((Config)Utility.MapXmlFileToClass(typeof(Config), "config.xml"));

            Browser.Visibility = Visibility.Hidden;
            DataContext = this;

            LogsDataGrid.ItemsSource = Logs.MainLog.Items;

            //Try to login with the saved credentials.
            if (!Auth.Login(Config.Username, Config.Password).Item1)
            {
                ShowLoginDialog();
            }
            else
            {
                OnLogin(Config.Username);
            }

            PrepareAssemblies(Config.InstalledAssemblies, Config.UpdateOnLoad, true);

            //Used to reload the assemblies from inside the game.
            KeyboardHook.SetHook();
            KeyboardHook.OnKeyUpTrigger += KeyboardHookOnOnKeyUpTrigger;
        }

        private void KeyboardHookOnOnKeyUpTrigger(int vKeyCode)
        {
            
        }

        private async void ShowLoginDialog()
        {
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
            var result =
                await
                    this.ShowLoginAsync(
                        "LeagueSharp", "Sign in",
                        new LoginDialogSettings { ColorScheme = MetroDialogOptions.ColorScheme });
            
            var loginResult = new Tuple<bool, string>(false, "Cancel button pressed");
            if (result != null)
            {
                var hash = Auth.Hash(result.Password);

                Config.Username = result.Username;
                Config.Password = hash;

                loginResult = Auth.Login(result.Username, hash);
            }

            if (result != null && loginResult.Item1)
            {
                OnLogin(result.Username);
            }
            else
            {
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

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Utility.MapClassToXmlFile(typeof(Config), Config, "config.xml");
            KeyboardHook.UnHook();
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

            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += delegate
            {
                var updatedSvnUrls = new List<string>();

                var leagueSharpAssemblies = assemblies as IList<LeagueSharpAssembly> ?? assemblies.ToList();
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
                }
            };

            bgWorker.RunWorkerCompleted += delegate
            {
                ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
                Working = false;
            };

            bgWorker.RunWorkerAsync();
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
            var result = await this.ShowMessageAsync("Uninstall", "Are you sure you want to uninstall selected assemblies?", MessageDialogStyle.AffirmativeAndNegative);
            
            if (result == MessageDialogResult.Negative)
            {
                return;
            }

            foreach (var ee in asemblies)
            {
                Config.InstalledAssemblies.Remove(ee);
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Config.Username = "";
            Config.Password = "";
            MainWindow_OnClosing(null, null);
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
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

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void TrayMenuClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
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
            if (SearchTextBox.Text.Trim() != "")
            {
                var searchAssemblies = new List<LeagueSharpAssembly>();

                foreach (var assembly in Config.InstalledAssemblies)
                {
                    var nameMatch = Regex.Match(assembly.Name, SearchTextBox.Text, RegexOptions.IgnoreCase);
                    var displayNameMatch = Regex.Match(assembly.DisplayName, SearchTextBox.Text, RegexOptions.IgnoreCase);

                    if (displayNameMatch.Success || nameMatch.Success)
                    {
                        searchAssemblies.Add(assembly);
                    }
                }

                InstalledAssembliesDataGrid.ItemsSource = searchAssemblies;
            }
            else
            {
                InstalledAssembliesDataGrid.ItemsSource = Config.InstalledAssemblies;
            }
            
        }
    }
}