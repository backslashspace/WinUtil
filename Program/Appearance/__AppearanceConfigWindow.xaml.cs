using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Stimulator.SubWindows
{
    public sealed partial class AppearanceConfigWindow : Window
    {
        public AppearanceConfigWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            Object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Policies\\Microsoft\\Windows\\System", "DisableAcrylicBackgroundOnLogon", null);
            if (value != null && value.GetType() == typeof(Int32) && (Int32)value == 1)
            {
                LoginScreenBlurButton.IsChecked = false;
                LoginScreenBlurButton.Content = "Lock Screen\nBlur: NO";
            }
            else
            {
                LoginScreenBlurButton.IsChecked = true;
                LoginScreenBlurButton.Content = "Lock Screen\nBlur: YES";
            }

            value = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", null);
            if (value != null && value.GetType() == typeof(Int32) && (Int32)value == 1)
            {
                LockScreenButton.IsChecked = false;
                LockScreenButton.Content = "Lock Screen\nEnabled: NO";
            }
            else
            {
                LockScreenButton.IsChecked = true;
                LockScreenButton.Content = "Lock Screen\nEnabled: YES";
            }

            if (Util.IsWindows10UI())
            {
                ContextMenuButton.IsEnabled = false;
            }
            else
            {
                value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32", "", null);
                if (value != null && value.GetType() == typeof(String))
                {
                    ContextMenuButton.IsChecked = true;
                    ContextMenuButton.Content = "Legacy Context\nMenu: YES";
                }
                else
                {
                    ContextMenuButton.IsChecked = false;
                    ContextMenuButton.Content = "Legacy Context\nMenu: NO";
                }
            }

            value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\background\\shell\\WindowsTerminalAdmin", "Icon", null);
            if (value != null && value.GetType() == typeof(String))
            {
                TerminalIntegrationButton.IsChecked = true;
                TerminalIntegrationButton.Content = "Better Terminal\nIntegration: YES";
            }
            else
            {
                TerminalIntegrationButton.IsChecked = false;
                TerminalIntegrationButton.Content = "Better Terminal\nIntegration: NO";
            }
        }

        private void LoginScreenBlurButton_Click(Object sender, RoutedEventArgs e)
        {
            if ((Boolean)((ToggleButton)sender).IsChecked!)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\Microsoft\Windows\System", "DisableAcrylicBackgroundOnLogon", 0, RegistryValueKind.DWord);

                ((ToggleButton)sender).Content = "Lock Screen\nBlur: YES";

                Log.FastLog("Enabled login screen blur", LogSeverity.Info, "LockScreenBlur");
            }
            else
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\Microsoft\Windows\System", "DisableAcrylicBackgroundOnLogon", 1, RegistryValueKind.DWord);

                ((ToggleButton)sender).Content = "Lock Screen\nBlur: NO";

                Log.FastLog("Disabled login screen blur", LogSeverity.Info, "LockScreenBlur");
            }
        }

        private void ContextMenuButton_Click(Object sender, RoutedEventArgs e)
        {
            if ((Boolean)((ToggleButton)sender).IsChecked!)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);

                Util.RestartExplorerForUser();

                ((ToggleButton)sender).Content = "Legacy Context\nMenu: YES";

                Log.FastLog("Enabled old legacy context menu", LogSeverity.Info, "ContextMenu");
            }
            else
            {
                RegistryKey clsid = Registry.CurrentUser.OpenSubKey("Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", true);
                clsid.DeleteSubKeyTree("InprocServer32", false);

                ((ToggleButton)sender).Content = "Legacy Context\nMenu: NO";

                Util.RestartExplorerForUser();

                Log.FastLog("Set default context menu", LogSeverity.Info, "ContextMenu");
            }
        }

        private async void TerminalIntegrationButton_Click(Object sender, RoutedEventArgs e)
        {
            if ((Boolean)((ToggleButton)sender).IsChecked!)
            {
                ((ToggleButton)sender).IsEnabled = false;
                await Terminal(true).ConfigureAwait(true);
                ((ToggleButton)sender).IsEnabled = true;

                ((ToggleButton)sender).Content = "Better Terminal\nIntegration: YES";
            }
            else
            {
                ((ToggleButton)sender).IsEnabled = false;
                await Terminal(false).ConfigureAwait(true);
                ((ToggleButton)sender).IsEnabled = true;

                ((ToggleButton)sender).Content = "Better Terminal\nIntegration: NO";
            }
        }

        private void LockScreenButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                if ((Boolean)((ToggleButton)sender).IsChecked!)
                {
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", 0, RegistryValueKind.DWord);
                    ((ToggleButton)sender).Content = "Lock Screen\nEnabled: YES";

                    Log.FastLog("Enabled Lock Screen", LogSeverity.Info, "LockScreen");
                }
                else
                {
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", 1, RegistryValueKind.DWord);
                    ((ToggleButton)sender).Content = "Lock Screen\nEnabled: NO";

                    Log.FastLog("Disabled Lock Screen", LogSeverity.Info, "LockScreen");
                }
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to set Lock Screen config: " + exception.Message, LogSeverity.Info, "LockScreen");
            }
        }
    }
}