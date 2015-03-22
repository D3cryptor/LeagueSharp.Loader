using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LeagueSharp.Loader.Data;
using MahApps.Metro;

namespace LeagueSharp.Loader.Views.Settings
{
    public partial class General
    {
        private readonly string[] _accentColors =
        {
            "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal",
            "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive",
            "Steel", "Mauve", "Taupe", "Sienna"
        };

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

        private void Logout_OnClick(object sender, RoutedEventArgs e)
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
            senderBox.Items.Add("Bulgarian");
            senderBox.Items.Add("Chinese");
            senderBox.Items.Add("Dutch");
            senderBox.Items.Add("English");
            senderBox.Items.Add("French");
            senderBox.Items.Add("German");
            senderBox.Items.Add("Greek");
            senderBox.Items.Add("Hungarian");
            senderBox.Items.Add("Italian");
            senderBox.Items.Add("Korean");
            senderBox.Items.Add("Latvian");
            senderBox.Items.Add("Lithuanian");
            senderBox.Items.Add("Polish");
            senderBox.Items.Add("Portuguese");
            senderBox.Items.Add("Romanian");
            senderBox.Items.Add("Russian");
            senderBox.Items.Add("Spanish");
            senderBox.Items.Add("Swedish");
            senderBox.Items.Add("Thai");
            senderBox.Items.Add("Traditional-Chinese");
            senderBox.Items.Add("Turkish");
            senderBox.Items.Add("Vietnamese");

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

        private void Color_Loaded(object sender, RoutedEventArgs e)
        {
            var colorBox = (ComboBox)sender;

            foreach (var asccent in _accentColors)
            {
                colorBox.Items.Add(asccent);
            }

            if (Config.Instance.SelectedColor != null)
            {
                colorBox.SelectedItem = Config.Instance.SelectedColor;
            }

            if (colorBox.SelectedIndex == -1)
            {
                colorBox.SelectedIndex = colorBox.Items.IndexOf("Blue");
            }
        }

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var color = ((ComboBox)sender).SelectedValue.ToString();

            if (_accentColors.Contains(color))
            {
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(color), ThemeManager.GetAppTheme("BaseLight"));
                Config.Instance.SelectedColor = color;
            }
        }
    }
}