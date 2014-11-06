using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Input;
using LeagueSharp.Loader.Class;
using LeagueSharp.Loader.Data;
using MahApps.Metro.Controls;

namespace LeagueSharp.Loader.Views
{
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
                    MessageBox.Show("Could not download the update, please update manually: " + ex);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not download the update, please update manually: " + ex);
                Environment.Exit(0);
            }
        }

        private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                UpdateProgressBar.Value = downloadProgressChangedEventArgs.ProgressPercentage;
                ProgressLabel.Content = string.Format(ProgressText, downloadProgressChangedEventArgs.BytesReceived / 1024, downloadProgressChangedEventArgs.TotalBytesToReceive / 1024);
            });
        }

        void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            new Process { StartInfo = { FileName = Updater.SetupFile, Arguments = "/VERYSILENT /DIR=\"" + Directories.CurrentDirectory + "\"" } }.Start();
            Environment.Exit(0);
        }
    }
}