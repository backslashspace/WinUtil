using BSS.Logging;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Stimulator.SubWindows
{
    public sealed partial class ApplicationsConfigWindow : Window
    {
        public ApplicationsConfigWindow()
        {
            InitializeComponent();

            if (RunContextInfo.Windows.IsServer == false) AzureSyncButton.IsEnabled = false;
        }

        private void EdgeButton_Click(Object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result =  System.Windows.Forms.MessageBox.Show(
                    "Uninstall Edge and all associated files?",
                    "Edge Remove Script",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

            if (result != System.Windows.Forms.DialogResult.Yes) return;

            try
            {
                if (!Util.Execute.Process(new(@"C:\Windows\System32\cmd.exe", $"/c \"{RunContextInfo.ExecutablePath + "\\assets\\Edge_Removal_24.bat"}\"", true)).Success)
                {
                    Log.FastLog("Failed to launch Edge remove script - file not found?", LogSeverity.Error, "EdgeRemove");
                    return;
                }

                Log.FastLog("Launched Edge remove script", LogSeverity.Info, "EdgeRemove");
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to launch Edge remove script: " + exception.Message, LogSeverity.Error, "EdgeRemove");
            }
        }

        private void AzureSyncButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.Execute.Process(new(@"C:\Windows\System32\cmd.exe", $"/k \"dism.exe /online /Remove-Capability /CapabilityName:AzureArcSetup~~~~\"", true)).Success)
                {
                    Log.FastLog("Failed to launche cmd.exe - file not found?", LogSeverity.Error, "AzureArcSetup");
                    return;
                }

                Log.FastLog("Executed: dism.exe /online /Remove-Capability /CapabilityName:AzureArcSetup~~~~", LogSeverity.Info, "AzureArcSetup");
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to execute dism: " + exception.Message, LogSeverity.Error, "AzureArcSetup");
            }
        }

        private async void OneDriveButton_Click(Object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                    "Uninstall OneDrive?\n\nAll explorer processes will be terminated.",
                    "OneDrive",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

            if (result != System.Windows.Forms.DialogResult.Yes) return;

            ((Button)sender).IsEnabled = false;
            await RemoveOnDrive().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void NotepadButton_Click(Object sender, RoutedEventArgs e)
        {
            if (File.Exists(@"C:\Program Files\Notepad++\notepad++.exe"))
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                    "Found Notepad++ installation.\n\nOverwrite?",
                    "Notepad++",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

                if (result != System.Windows.Forms.DialogResult.Yes) return;
            }

            ((Button)sender).IsEnabled = false;
            await InstallNotepadPlusPlus().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void CodecsButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await Codecs().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void ImageGlassButton_Click(Object sender, RoutedEventArgs e)
        {
            if (File.Exists(@"C:\Program Files\ImageGlass\ImageGlass.exe"))
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                    "Found ImageGlass installation.\n\nOverwrite?",
                    "ImageGlass",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

                if (result != System.Windows.Forms.DialogResult.Yes) return;
            }

            ((Button)sender).IsEnabled = false;
            await ImageGlass().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }
    }
}