using BSS.Logging;
using System;
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
            SubWindows.SystemSecurity systemSecurity = new();

            systemSecurity.Show();
        }
    }
}