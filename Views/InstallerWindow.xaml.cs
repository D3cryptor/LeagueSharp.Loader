#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using LeagueSharp.Loader.Class;
using LeagueSharp.Loader.Data;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TextBox = System.Windows.Controls.TextBox;

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
    public partial class InstallerWindow : MetroWindow
    {
        public List<LeagueSharpAssembly> FoundAssemblies;

        public string SearchLocation = "";

        public InstallerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Step1_Click(object sender, RoutedEventArgs e)
        {
            Step1.IsEnabled = false;

            var bgWorker = new BackgroundWorker();

            if (LocalRadioButton.IsChecked == true)
            {
                SearchLocation = PathTextBox.Text;
                bgWorker.DoWork += delegate
                {
                    FoundAssemblies = LeagueSharpAssemblies.GetAssemblies(SearchLocation);
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        () => MainDataGrid.ItemsSource = FoundAssemblies);
                };
            }
            else if (SvnRadioButton.IsChecked == true)
            {
                SearchLocation = SvnComboBox.Text;
                bgWorker.DoWork += delegate
                {
                    var updatedDir = SvnUpdater.Update(SearchLocation, Logs.MainLog, Directories.RepositoryDir);
                    FoundAssemblies = LeagueSharpAssemblies.GetAssemblies(updatedDir, SearchLocation);
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        () => MainDataGrid.ItemsSource = FoundAssemblies);
                };
            }

            bgWorker.RunWorkerCompleted += delegate
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => Step1.IsEnabled = true);
                System.Windows.Application.Current.Dispatcher.Invoke(() => installTabControl.SelectedIndex++);
            };

            bgWorker.RunWorkerAsync();
        }

        private void Step2_Click(object sender, RoutedEventArgs e)
        {
            var amount = FoundAssemblies.Count(a => a.InstallChecked);

            foreach (var assembly in FoundAssemblies)
            {
                if (assembly.InstallChecked)
                {
                    if (assembly.Compile())
                    {
                        ((MainWindow)Owner).Config.InstalledAssemblies.Add(assembly);
                        amount--;
                    }
                }
            }

            if (amount == 0)
            {
                AfterInstallMessage("Selected assemblies succesfully installed.", true);
            }
            else
            {
                AfterInstallMessage(
                    "There was an error while trying to install some of the assemblies, check the log for more details.");
            }
        }

        private async void AfterInstallMessage(string msg, bool close = false)
        {
            await this.ShowMessageAsync("Installer", msg);
            if (close)
            {
                Close();
            }
        }

        private void Step2P_Click(object sender, RoutedEventArgs e)
        {
            installTabControl.SelectedIndex--;
        }

        private void PathTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.SelectedText))
            {
                using (var folderDialog = new FolderBrowserDialog())
                {
                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        textBox.Text = folderDialog.SelectedPath;
                    }
                }
            }
        }

        private void InstallerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SvnComboBox.ItemsSource = ((MainWindow)Owner).Config.KnownRepositories;
        }

        private void SvnComboBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            SvnRadioButton.IsChecked = true;
            LocalRadioButton.IsChecked = !SvnRadioButton.IsChecked;
        }

        private void PathTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            SvnRadioButton.IsChecked = false;
            LocalRadioButton.IsChecked = !SvnRadioButton.IsChecked;
        }
    }
}