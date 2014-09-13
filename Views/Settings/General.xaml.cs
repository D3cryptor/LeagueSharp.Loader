using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LeagueSharp.Loader.Data;

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
            ((MainWindow)DataContext).Config.Username = "";
            ((MainWindow)DataContext).Config.Password = "";
            ((MainWindow)DataContext).MainWindow_OnClosing(null, null);
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }
    }
}
