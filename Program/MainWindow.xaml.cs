using BSS.Logging;
using System;
using System.IO;
using System.Windows;

namespace Stimulator
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            Loaded += OnLoaded;

            InitializeComponent();

#if DEBUG  
            Debug.IsEnabled = true;
            Debug.Visibility = Visibility.Visible;
#endif
        }

        private void System_Base_Configuration_Click(Object sender, RoutedEventArgs e)
        {
            SubWindows.BaseConfigWindow baseConfigWindow = new();
            baseConfigWindow.Show();
        }

        private void Appearance_Click(Object sender, RoutedEventArgs e)
        {
            SubWindows.AppearanceConfigWindow appearanceConfigWindow = new();
            appearanceConfigWindow.Show();
        }

        private void Miscellaneous_Click(Object sender, RoutedEventArgs e)
        {
            SubWindows.MiscellaneousConfigWindow miscellaneousConfigWindow = new();
            miscellaneousConfigWindow.Show();
        }

        private void Privacy_and_Security_Click(Object sender, RoutedEventArgs e)
        {
            SubWindows.SecurityConfigWindow securityConfigWindow = new();
            securityConfigWindow.Show();
        }

        private void Applications_Click(Object sender, RoutedEventArgs e)
        {
            SubWindows.ApplicationsConfigWindow applicationsConfigWindow = new();
            applicationsConfigWindow.Show();
        }

        private void Restart_Explorer_Click(Object sender, RoutedEventArgs e)
        {
            if (!Util.KillExplorer(false)) return;

            Util.Execute.StartInfo startInfo = new("c:\\windows\\explorer.exe");

            if (Util.Execute.Process(startInfo).Success) Log.FastLog("Restarted Explorer", LogSeverity.Info, "MainWindow()");
        }

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        private void Debug_Click(Object sender, RoutedEventArgs e)
        {
            String[] directories = Directory.GetDirectories("C:\\Windows\\WinSxS");

            for (Int32 i = 0; i < directories.Length; ++i)
            {
                if (directories[i].Contains("microsoft-windows-onedrive-setup"))
                {
                    Util.Execute.Process(new("C:\\Windows\\System32\\takeown.exe", $"/a /f \"{directories[i]}\\OneDriveSetup.exe\"", true, true, true));
                    Util.Execute.Process(new("C:\\Windows\\System32\\icacls.exe", $"\"{directories[i]}\\OneDriveSetup.exe\" /grant {RunContextInfo.Windows.AdministratorGroupName}:(F)", true, true, true));
                    File.Delete(directories[i] + "\\OneDriveSetup.exe");

                    break;
                }
            }
        }
    }
}