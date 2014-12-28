#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// Log.xaml.cs is part of LeagueSharp.Loader.
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

    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using LeagueSharp.Loader.Data;

    #endregion

    /// <summary>
    ///     Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : UserControl
    {
        public Log()
        {
            InitializeComponent();
            LogsDataGrid.ItemsSource = Logs.MainLog.Items;
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Directories.LogsDir))
            {
                Process.Start(Directories.LogsDir);
            }
        }
    }
}