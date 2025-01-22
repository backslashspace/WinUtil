using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Stimulator.SubWindows
{
    public sealed partial class SystemSecurity : Window
    {
        public SystemSecurity() => InitializeComponent();

        private async void SMBButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await SMB().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private async void VBSButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await VBS().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private void LsassPplButton_Click(Object sender, RoutedEventArgs e)
        {
            Boolean useUEFILock = false;

            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                   "Enable with UEFI lock?\n",
                   "Virtualization based security",
                   System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                   System.Windows.Forms.MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Cancel) return;
            else if (result == System.Windows.Forms.DialogResult.Yes) useUEFILock = true;

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "LsaCfgFlags", useUEFILock ? 1 : 2, RegistryValueKind.DWord);

            Log.FastLog($"Set LSASS.exe to run as protected process light {(useUEFILock ? "with" : "without")} UEFI lock, a restart is needed to apply the changes", LogSeverity.Info, "LSASS");
        }

        private async void PageFileButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await PageFile().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }

        private void BSIHostsFileButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                Util.Execute.Result result = Util.Execute.Process(new(RunContextInfo.ExecutablePath + "\\assets\\SiSyPHuS_Win10_2022_Host_INIT.exe", "e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9", true, true, true));
            
                if (!result.Success)
                {
                    Log.FastLog($"Failed to launch BSI hosts file editor", LogSeverity.Error, "BSI-Hosts");
                    return;
                }

                if (result.ExitCode == -1) Log.FastLog($"No lines were added to the host file", LogSeverity.Info, "BSI-Hosts");
                else Log.FastLog($"{result.ExitCode} lines were added to the host file", LogSeverity.Info, "BSI-Hosts");
            }
            catch (Exception exception)
            {
                Log.FastLog($"Failed to launch BSI hosts file editor: " + exception.Message, LogSeverity.Error, "BSI-Hosts");
            }
        }

        private async void HardenButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await Harden().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }
    }
}