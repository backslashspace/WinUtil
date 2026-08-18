using System;
using System.IO;
using BSS.Logging;
using System.Windows;

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
        BaseConfigurationWindow baseConfigurationWindow = new();
        baseConfigurationWindow.Show();
    }

    private void Appearance_Click(Object sender, RoutedEventArgs e)
    {
        AppearanceWindow appearanceWindow = new();
        appearanceWindow.Show();
    }

    private void Miscellaneous_Click(Object sender, RoutedEventArgs e)
    {
        MiscellaneousWindow miscellaneousWindow = new();
        miscellaneousWindow.Show();
    }

    private void Privacy_and_Security_Click(Object sender, RoutedEventArgs e)
    {
        SecurityAndPrivacyWindow securityWindow = new();
        securityWindow.Show();
    }

    private void Applications_Click(Object sender, RoutedEventArgs e)
    {
        ApplicationsWindow applicationsWindow = new();
        applicationsWindow.Show();
    }

    private void Restart_Explorer_Click(Object sender, RoutedEventArgs e)
    {
        if (!Utility.KillExplorer(false)) return;

        Utility.Execute.StartInfo startInfo = new("c:\\windows\\explorer.exe");

        if (Utility.Execute.Process(startInfo).Success) Log.FastLog("Restarted Explorer", LogSeverity.Info, "MainWindow()");
    }

    // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

    private void Debug_Click(Object sender, RoutedEventArgs e)
    {
        String[] directories = Directory.GetDirectories("C:\\Windows\\WinSxS");

        for (Int32 i = 0; i < directories.Length; ++i)
        {
            if (directories[i].Contains("microsoft-windows-onedrive-setup"))
            {
                Utility.Execute.Process(new("C:\\Windows\\System32\\takeown.exe", $"/a /f \"{directories[i]}\\OneDriveSetup.exe\"", true, true, true));
                Utility.Execute.Process(new("C:\\Windows\\System32\\icacls.exe", $"\"{directories[i]}\\OneDriveSetup.exe\" /grant {RunContextInfo.Windows.AdministratorGroupName}:(F)", true, true, true));
                File.Delete(directories[i] + "\\OneDriveSetup.exe");

                break;
            }
        }
    }
}