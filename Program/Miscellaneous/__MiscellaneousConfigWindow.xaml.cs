using BSS.Logging;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Stimulator.SubWindows
{
    public sealed partial class MiscellaneousConfigWindow : Window
    {
        public MiscellaneousConfigWindow() => InitializeComponent();

        private void MASButtonButton_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.Execute.Process(new(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", $"-c \"irm https://get.activated.win | iex; Write-Output 'Closing in 5 seconds'; Start-Sleep 5\"", true)).Success)
                {
                    Log.FastLog("Failed to launch MAS online script - file not found?", LogSeverity.Error, "MAS");
                    return;
                }

                Log.FastLog("Launched MAS online script", LogSeverity.Info, "MAS");
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to launch MAS online script: " + exception.Message, LogSeverity.Error, "MAS");
            }
        }

        private async void NGENButton_Click(Object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await NGEN().ConfigureAwait(true);
            ((Button)sender).IsEnabled = true;
        }
    }
}