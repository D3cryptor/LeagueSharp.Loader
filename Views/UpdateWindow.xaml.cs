#region LICENSE

// Copyright 2014 LeagueSharp.Loader
// UpdateWindow.xaml.cs is part of LeagueSharp.Loader.
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

namespace LeagueSharp.Loader.Views
{
    #region

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net;
    using System.Windows;
    using LeagueSharp.Loader.Class;
    using LeagueSharp.Loader.Data;
    using MahApps.Metro.Controls;

    #endregion

    /// <summary>
    ///     Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MetroWindow
    {
        public bool AlreadyActivated = false;
        public string UpdateUrl;
        public string ProgressText;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void UpdateWindow_OnActivated(object sender, EventArgs e)
        {
            if (AlreadyActivated)
            {
                return;
            }

            AlreadyActivated = true;

            try
            {
                var webClient = new WebClient();
                ProgressText = (string) ProgressLabel.Content;
                UpdateProgressBar.Maximum = 100;
                try
                {
                    webClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
                    webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(UpdateUrl), Updater.SetupFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Utility.GetMultiLanguageText("LoaderUpdateFailed") + ex);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Utility.GetMultiLanguageText("LoaderUpdateFailed") + ex);
                Environment.Exit(0);
            }
        }

        private void WebClientOnDownloadProgressChanged(object sender,
            DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                {
                    UpdateProgressBar.Value = downloadProgressChangedEventArgs.ProgressPercentage;
                    ProgressLabel.Content = string.Format(
                        ProgressText, downloadProgressChangedEventArgs.BytesReceived / 1024,
                        downloadProgressChangedEventArgs.TotalBytesToReceive / 1024);
                });
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (Parent != null)
            {
                ((MainWindow) Parent).MainWindow_OnClosing(null, null);
            }
            new Process
            {
                StartInfo =
                {
                    FileName = Updater.SetupFile,
                    Arguments = "/VERYSILENT /DIR=\"" + Directories.CurrentDirectory + "\""
                }
            }.Start();
            Environment.Exit(0);
        }
    }
}