using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Stimulator.SubWindows
{
    public sealed partial class BaseConfigWindow : Window
    {
        public BaseConfigWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            Object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsRunInBackground", null);
            if (value == null)
            {
                BackgroundAppsToggleButton.IsChecked = false;
                BackgroundAppsToggleButton.Foreground = Brushes.Red;
                BackgroundAppsToggleButton.Content = "Allow Background Apps: YES";
            }
            else
            {
                BackgroundAppsToggleButton.IsChecked = true;
                BackgroundAppsToggleButton.Foreground = Brushes.Green;
                BackgroundAppsToggleButton.Content = "Allow Background Apps: NO";
            }
        }

        //

        private void ToggleBackgroundApps(Object sender, RoutedEventArgs e)
        {
            if ((Boolean)((ToggleButton)sender).IsChecked!)
            {
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsRunInBackground", 2, RegistryValueKind.DWord);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsActivateWithVoice", 2, RegistryValueKind.DWord);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsActivateWithVoiceAboveLock", 2, RegistryValueKind.DWord);

                ((ToggleButton)sender).Foreground = Brushes.Green;
                ((ToggleButton)sender).Content = "Allow Background Apps: NO";

                Log.FastLog("Set background apps to: disabled", LogSeverity.Info, "Background Apps");
            }
            else
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", true);
                registryKey.DeleteValue("LetAppsRunInBackground");

                ((ToggleButton)sender).Foreground = Brushes.Red;
                ((ToggleButton)sender).Content = "Allow Background Apps: YES";

                Log.FastLog("Set background apps to: enabled", LogSeverity.Info, "Background Apps");
            }
        }

        private async void Stability_Reliability_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await StabilityReliability().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void UnAnnoy_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await Pacify().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void TCPSettingsButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await TCP().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void WindowsUpdateButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await WindowsUpdate().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }
    }
}