#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// Hotkeys.xaml.cs is part of LeagueSharp.Loader.
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

    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using LeagueSharp.Loader.Data;

    #endregion

    public partial class Hotkeys
    {
        public Hotkeys()
        {
            InitializeComponent();
        }

        private void Hotkeys_OnKeyUp(object sender, KeyEventArgs e)
        {
            var item = HotkeysDataGrid.SelectedItem;
            if (item != null)
            {
                ((HotkeyEntry) item).Hotkey = e.Key;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in HotkeysDataGrid.Items.Cast<HotkeyEntry>())
            {
                item.Hotkey = item.DefaultKey;
            }
        }
    }
}