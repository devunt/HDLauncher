﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace HDLauncher
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Version.Text = "v" + Constants.VERSION;
            InstallPath.Text = Settings.FFXIVPath;
            RunAsAdministrator.IsChecked = Settings.RunAsAdministrator;
        }

        private void InstallPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.Description = "파이널 판타지 14가 설치된 경로를 선택해주세요.";

            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                InstallPath.Text = dialog.SelectedPath;
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Settings.FFXIVPath = InstallPath.Text;
            Settings.RunAsAdministrator = RunAsAdministrator.IsChecked == true;
            Settings.Save();
        }
    }
}