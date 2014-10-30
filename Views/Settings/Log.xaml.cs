using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LeagueSharp.Loader.Data;
using System.IO;

namespace LeagueSharp.Loader.Views.Settings
{
    /// <summary>
    /// Interaction logic for Log.xaml
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
                System.Diagnostics.Process.Start(Directories.LogsDir);
            }
        }
    }
}
