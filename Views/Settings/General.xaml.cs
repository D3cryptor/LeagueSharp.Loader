using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LeagueSharp.Loader.Data;
using System.IO;

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


namespace LeagueSharp.Loader.Views.Settings
{
    public partial class General : UserControl 
    {
        public General()
        {
            InitializeComponent();
        }

        private void GameSettingsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((DataGrid)sender).SelectedItem;
            if (item != null)
            {
                ((GameSettings)item).SelectedValue = ((GameSettings)item).SelectedValue ==
                ((GameSettings)item).PosibleValues[0]
                ? ((GameSettings)item).PosibleValues[1]
                : ((GameSettings)item).PosibleValues[0];
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.Username = "";
            Config.Instance.Password = "";
            ((MainWindow)DataContext).MainWindow_OnClosing(null, null);

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (string)e.AddedItems[0];
            File.WriteAllText(Directories.LanguageFileFilePath, selected);
            ((MainWindow)DataContext).MainWindow_OnClosing(null, null);
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).Items.Clear();
            ((ComboBox)sender).Items.Add("Chinese");
            ((ComboBox)sender).Items.Add("English");
            ((ComboBox)sender).Items.Add("German");
			((ComboBox)sender).Items.Add("Greek");
            ((ComboBox)sender).Items.Add("Dutch");
            ((ComboBox)sender).Items.Add("Spanish");
            ((ComboBox)sender).Items.Add("Russian");
            ((ComboBox)sender).Items.Add("Portuguese");
            ((ComboBox)sender).Items.Add("Italian");
            ((ComboBox)sender).Items.Add("French");
            ((ComboBox)sender).Items.Add("Korean");
            ((ComboBox)sender).Items.Add("Polish");
            ((ComboBox)sender).Items.Add("Romanian");
            ((ComboBox)sender).Items.Add("Swedish");
            ((ComboBox)sender).Items.Add("Turkish");
            ((ComboBox)sender).Items.Add("Thai");
            ((ComboBox)sender).Items.Add("Vietnamese");
        }
    }
}
