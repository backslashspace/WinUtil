using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Stimulator.SubWindows
{
    public sealed partial class SecurityConfigWindow : Window
    {
        public SecurityConfigWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            Object value;

            if (RunContextInfo.Windows.IsServer)
            {
                UACToggleButton.IsEnabled = false;
            }
            else
            {
                value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "ConsentPromptBehaviorAdmin", null);
                if (value != null && value.GetType() == typeof(Int32) && (Int32)value == 1)
                {
                    UACToggleButton.IsChecked = true;
                    UACToggleButton.Content = "UAC always prompt\nfor Credentials: YES";
                }
                else
                {
                    UACToggleButton.IsChecked = false;
                    UACToggleButton.Content = "UAC always prompt\nfor Credentials: NO";
                }
            }

            if (RunContextInfo.Windows.IsServer)
            {
                UACToggleButton.IsEnabled = false;

                SafeDesktopToggleButton.IsChecked = true;
                SafeDesktopToggleButton.IsEnabled = false;
                SafeDesktopToggleButton.Content = "Require\nCTRL + ALT + DEL\nin login screen: YES";
            }
            else
            {
                value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "DisableCAD", null);
                if (value != null && value.GetType() == typeof(Int32) && (Int32)value == 0)
                {
                    SafeDesktopToggleButton.IsChecked = true;
                    SafeDesktopToggleButton.Content = "Require\nCTRL + ALT + DEL\nin login screen: YES";
                }
                else
                {
                    SafeDesktopToggleButton.IsChecked = false;
                    SafeDesktopToggleButton.Content = "Require\nCTRL + ALT + DEL\nin login screen: NO";
                }
            }

            if (RunContextInfo.Windows.IsHomeEdition) AttackSurfaceReductionButton.IsEnabled = false;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private void UACToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            if ((Boolean)((ToggleButton)sender).IsChecked!)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 1, RegistryValueKind.DWord);

                ((ToggleButton)sender).Content = "UAC always prompt\nfor Credentials: YES";

                Log.FastLog("Enabled verbose UAC", LogSeverity.Info, "UAC");
            }
            else
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 5, RegistryValueKind.DWord);

                ((ToggleButton)sender).Content = "UAC always prompt\nfor Credentials: NO";

                Log.FastLog("Disabled verbose UAC", LogSeverity.Info, "UAC");
            }
        }

        private void SafeDesktopToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            if ((Boolean)((ToggleButton)sender).IsChecked!)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 0, RegistryValueKind.DWord);

                ((ToggleButton)sender).Content = "Require\nCTRL + ALT + DEL\nin login screen: YES";

                Log.FastLog("Enabled 'require CTRL + ALT + DEL'", LogSeverity.Info, "SafeDesktop");
            }
            else
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 1, RegistryValueKind.DWord);

                ((ToggleButton)sender).Content = "Require\nCTRL + ALT + DEL\nin login screen: NO";

                Log.FastLog("Deactivated 'require CTRL + ALT + DEL'", LogSeverity.Info, "SafeDesktop");
            }
        }

        private void LoginTimeoutButton_Click(Object sender, RoutedEventArgs e)
        {
            Boolean thresholdIsValid = UInt16.TryParse(LoginTimeoutTextBoxThreshold.Text, out UInt16 threshold);
            Boolean windowIsValid = UInt32.TryParse(LoginTimeoutTextBoxWindow.Text, out UInt32 window);

            if (!thresholdIsValid || threshold > 999)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Login threshold was invalid, expected a '0' for off or any number between 1 to 999.\nInput was: " + LoginTimeoutTextBoxThreshold.Text,
                    "Invalid Input",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);

                return;
            }

            if (!windowIsValid || window > 99_999)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Timeout value was invalid, expected a '0' for manual unlock or any number between 1 to 99999.\nInput was: " + LoginTimeoutTextBoxWindow.Text,
                    "Invalid Input",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);

                return;
            }
            else if (thresholdIsValid && threshold == 0)
            {
                Util.Execute.Process(new(@"C:\Windows\System32\net.exe", $"accounts /lockoutthreshold:0", true, true, true));
                Log.FastLog($"Deactivated failed login timeout", LogSeverity.Info, "User-Timeout");
                return;
            }

            //

            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", $"accounts /lockoutthreshold:{threshold}", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", $"accounts /lockoutwindow:{window}", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", $"accounts /lockoutduration:{window}", true, true, true));

            Log.FastLog($"Set failed login timeout to 'after {threshold} failed attempts, lock {(window == 0 ? "until manually unlocked" : "for " + window + " minutes")}'", LogSeverity.Info, "User-Timeout");
        }

        private void SystemInactivityButton_Click(Object sender, RoutedEventArgs e)
        {
            Boolean inactivityIsValid = UInt16.TryParse(SystemInactivityTextBox.Text, out UInt16 inactivity);

            if (!inactivityIsValid || !(inactivity == 0 || (inactivity < 961 && inactivity >9)))
            {
                System.Windows.Forms.MessageBox.Show(
                    "System inactivity time was invalid, expected a '0' for off or any number between 10 to 960.\nInput was: " + SystemInactivityTextBox.Text,
                    "Invalid Input",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);

                return;
            }

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "InactivityTimeoutSecs", inactivity, RegistryValueKind.DWord);

            if (inactivity == 0) Log.FastLog("Deactivated auto system lock after inactivity", LogSeverity.Info, "Inactivity-Lock");
            else Log.FastLog($"Locking system after {inactivity} seconds", LogSeverity.Info, "Inactivity-Lock");
        }

        private void FailToRebootButton_Click(Object sender, RoutedEventArgs e)
        {
            Boolean attemptsIsValid = UInt16.TryParse(FailToRebootTextBox.Text, out UInt16 attempts);

            if (!attemptsIsValid || !(attempts == 0 || (attempts < 1000 && attempts > 3)))
            {
                System.Windows.Forms.MessageBox.Show(
                    "Fail to reboot had invalid input, expected a '0' for off or any number between 4 to 999.\nInput was: " + FailToRebootTextBox.Text,
                    "Invalid Input",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);

                return;
            }

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "MaxDevicePasswordFailedAttempts", attempts, RegistryValueKind.DWord);

            if (attempts == 0) Log.FastLog("Deactivated FailToReboot", LogSeverity.Info, "FailToReboot");
            else Log.FastLog($"Rebooting system after {attempts} failed attempts", LogSeverity.Info, "FailToReboot");
        }

        private async void AttackSurfaceReductionButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await AttackSurfaceReduction().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void PrivacyButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await Privacy().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private void SystemSecurityButton_Click(Object sender, RoutedEventArgs e)
        {
            SystemSecurity systemSecurity = new();
            systemSecurity.Show();
        }

        private void MSINFO32(Object sender, RoutedEventArgs e)
        {
            Util.Execute.Process(new("c:\\windows\\system32\\msinfo32.exe"));
        }

        private void OnOLaunchButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                Util.Execute.Result result = Util.Execute.Process(new(RunContextInfo.ExecutablePath + "\\assets\\OOSU10.exe"));

                if (!result.Success)
                {
                    Log.FastLog($"Failed to launch OOSU10", LogSeverity.Error, "OOSU10");
                    return;
                }
            }
            catch (Exception exception)
            {
                Log.FastLog($"Failed to launch OOSU10: " + exception.Message, LogSeverity.Error, "OOSU10");
            }            
        }
    }
}