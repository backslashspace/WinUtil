using BSS.Logging;
using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class ApplicationsConfigWindow
    {
        private const String CODECS_SOURCE = "Media";

        private static async Task Codecs() => await Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(RunContextInfo.ExecutablePath + "\\assets\\Codecs"))
                {
                    Log.FastLog(RunContextInfo.ExecutablePath + "\\assets\\Codecs not found", LogSeverity.Error, CODECS_SOURCE);
                    return;
                }

                String[] files = Directory.GetFileSystemEntries(RunContextInfo.ExecutablePath + "\\assets\\Codecs", "*.AppxBundle");

                if (files.Length == 0)
                {
                    Log.FastLog(RunContextInfo.ExecutablePath + "\\assets\\Codecs was empty!", LogSeverity.Warning, CODECS_SOURCE);
                    return;
                }

                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                        $"Install {files.Length} extensions from '{RunContextInfo.ExecutablePath}\\assets\\Codecs'?",
                        CODECS_SOURCE,
                        System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Question);

                if (result != System.Windows.Forms.DialogResult.Yes) return;

                Log.FastLog("Installing: VCLibs", LogSeverity.Info, CODECS_SOURCE);
                PowerShell.Create().AddScript($"Add-AppxPackage -Path \"{RunContextInfo.ExecutablePath + "\\assets\\Microsoft.VCLibs.140.00.UWPDesktop_14.0.33728.0_x64__8wekyb3d8bbwe.Appx"}\"")
                            .Invoke();

                for (Int32 i = 0; i < files.Length; ++i)
                {
                    Log.FastLog("Installing: " + files[i], LogSeverity.Info, CODECS_SOURCE);
                    PowerShell.Create().AddCommand("Add-AppPackage")
                                .AddParameter("-Path", files[i])
                                .Invoke();
                }

                Log.FastLog("Done", LogSeverity.Info, CODECS_SOURCE);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to install codecs: " + exception.Message, LogSeverity.Error, CODECS_SOURCE);
            }
        });
    }
}