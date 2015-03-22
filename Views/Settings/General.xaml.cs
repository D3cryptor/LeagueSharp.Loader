#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// General.xaml.cs is part of LeagueSharp.Loader.
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

namespace LeagueSharp.Loader.Views.Settings
{
    #region

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using LeagueSharp.Loader.Data;
    using MahApps.Metro;
    #endregion

    public partial class General
    {
        string[] accentArray = { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
        int myAccent = 0;
        
        public General()
        {
            InitializeComponent();
        }

        private void GameSettingsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((DataGrid) sender).SelectedItem;
            if (item != null)
            {
                ((GameSettings) item).SelectedValue = ((GameSettings) item).SelectedValue ==
                                                      ((GameSettings) item).PosibleValues[0]
                    ? ((GameSettings) item).PosibleValues[1]
                    : ((GameSettings) item).PosibleValues[0];
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.Username = "";
            Config.Instance.Password = "";
            ((MainWindow) DataContext).MainWindow_OnClosing(null, null);

            Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0 || e.RemovedItems.Count <= 0)
            {
                return;
            }

            var selected = (string) e.AddedItems[0];

            if (Config.Instance.SelectedLanguage == selected ||
                (Config.Instance.SelectedLanguage == null && selected == "English"))
            {
                return;
            }

            Config.Instance.SelectedLanguage = selected;
            ((MainWindow) DataContext).MainWindow_OnClosing(null, null);
            Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var senderBox = (ComboBox) sender;

            senderBox.Items.Clear();
            senderBox.Items.Add("Arabic");
            senderBox.Items.Add("Chinese");
            senderBox.Items.Add("English");
            senderBox.Items.Add("German");
            senderBox.Items.Add("Greek");
            senderBox.Items.Add("Dutch");
            senderBox.Items.Add("Spanish");
            senderBox.Items.Add("Russian");
            senderBox.Items.Add("Portuguese");
            senderBox.Items.Add("Italian");
            senderBox.Items.Add("French");
            senderBox.Items.Add("Korean");
            senderBox.Items.Add("Polish");
            senderBox.Items.Add("Romanian");
            senderBox.Items.Add("Swedish");
            senderBox.Items.Add("Turkish");
            senderBox.Items.Add("Thai");
            senderBox.Items.Add("Vietnamese");
            senderBox.Items.Add("Lithuanian");

            if (Config.Instance.SelectedLanguage != null)
            {
                senderBox.SelectedItem =
                    senderBox.Items.Cast<string>()
                        .FirstOrDefault(item => item == Config.Instance.SelectedLanguage);
            }


            //English as default
            if (senderBox.SelectedIndex == -1)
            {
                senderBox.SelectedIndex = senderBox.Items.IndexOf("English");
            }
        }
            
        private void ChangeAccent_OnClick(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(accentArray[n]), ThemeManager.GetAppTheme("BaseLight"));
            myAccent = (myAccent + 1) % 23;
        }

    }
}
