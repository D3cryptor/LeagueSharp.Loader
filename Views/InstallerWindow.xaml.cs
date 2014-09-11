using System.Collections.ObjectModel;

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
    public partial class InstallerWindow : INotifyPropertyChanged
    {
        private List<LeagueSharpAssembly> _foundAssemblies = new List<LeagueSharpAssembly>();
        private ProgressDialogController controller;
        public List<LeagueSharpAssembly> FoundAssemblies
        {
            get { return _foundAssemblies; }
            set
            {
                _foundAssemblies = value;
                OnPropertyChanged("FoundAssemblies");
            }
        }
        private bool _ableToList = true;

        public bool AbleToList
        {
            get
            {
                return _ableToList;
            }
            set
            {
                _ableToList = value;
                OnPropertyChanged("AbleToList");
            }
        }

        public InstallerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public async void ShowProgress()
        {
            controller = await this.ShowProgressAsync("Updating...", "");
            controller.SetIndeterminate();
        }

        public void ListAssemblies(string location, bool isSvn, string autoInstallName = null)
        {
            AbleToList = false;
            var bgWorker = new BackgroundWorker();

            if (!isSvn)
            {
                bgWorker.DoWork += delegate
                {
                    FoundAssemblies = LeagueSharpAssemblies.GetAssemblies(location);
                };
            }
            else
            {
                bgWorker.DoWork += delegate
                {
                    var updatedDir = SvnUpdater.Update(location, Logs.MainLog, Directories.RepositoryDir);
                    FoundAssemblies = LeagueSharpAssemblies.GetAssemblies(updatedDir, location);
                    foreach (var assembly in FoundAssemblies)
                    {
                        if (autoInstallName != null && assembly.Name.ToLower() == autoInstallName.ToLower())
                        {
                            assembly.InstallChecked = true;
                        }
                    }
                };
            }

            bgWorker.RunWorkerCompleted += delegate
            {
                AbleToList = true;
                System.Windows.Application.Current.Dispatcher.Invoke(() => installTabControl.SelectedIndex++);
                if (autoInstallName != null)
                {
                    InstallSelected();
                }

                if (controller != null)
                {
                    controller.CloseAsync();
                }
                
            };

            bgWorker.RunWorkerAsync();
        }

        public void InstallSelected()
        {
            var amount = FoundAssemblies.Count(a => a.InstallChecked);

            foreach (var assembly in FoundAssemblies)
            {
                if (assembly.InstallChecked)
                {
                    if (assembly.Compile())
                    {
                        if (((MainWindow) Owner).Config.InstalledAssemblies.All(a => a.Name != assembly.Name))
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

        private void Step1_Click(object sender, RoutedEventArgs e)
        {
            ShowProgress();
            ListAssemblies((SvnRadioButton.IsChecked == true) ? SvnComboBox.Text : PathTextBox.Text,
                (SvnRadioButton.IsChecked == true));
        }

        private void Step2_Click(object sender, RoutedEventArgs e)
        {
            InstallSelected();
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var assembly in FoundAssemblies)
            {
                assembly.InstallChecked = true;
            }
            OnPropertyChanged("FoundAssemblies");
        }
    }
}