using BSS.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class ApplicationsConfigWindow
    {
        private const String IMAGEGLASS_SOURCE = "ImageGlass";

        private static async Task ImageGlass() => await Task.Run(() =>
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("ImageGlass");
                if (processes != null && processes.Length != 0)
                {
                    System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                    $"{processes.Length} instances open, end all tasks?",
                    IMAGEGLASS_SOURCE,
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

                    if (result != System.Windows.Forms.DialogResult.Yes) return;

                    for (Int32 i = 0; i < processes.Length; ++i)
                    {
                        processes[i].Kill();
                    }
                }

                Log.FastLog("Installing " + IMAGEGLASS_SOURCE, LogSeverity.Info, IMAGEGLASS_SOURCE);

                if (!File.Exists(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\ImageGlass_x64.msi")
                || !File.Exists(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\igconfig.json")
                || !File.Exists(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\windowsdesktop-runtime-win-x64.exe"))
                {
                    Log.FastLog("Not all installer files found, aborted install (ImageGlass_x64.msi, windowsdesktop-runtime-win-x64.exe, igconfig.json)", LogSeverity.Error, IMAGEGLASS_SOURCE);
                    return;
                }

                Util.Execute.Process(new(@"C:\Windows\System32\msiexec.exe", $"/i \"{RunContextInfo.ExecutablePath}\\assets\\ImageGlass\\ImageGlass_x64.msi\" /quiet", true, true, true));
                Util.Execute.Process(new(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\windowsdesktop-runtime-win-x64.exe", "/install /quiet /norestart", true, true, true));

                if (Directory.Exists(RunContextInfo.Windows.UserHomePath + "\\AppData\\Local\\ImageGlass"))
                {
                    Directory.Delete(RunContextInfo.Windows.UserHomePath + "\\AppData\\Local\\ImageGlass", true);
                }
                Directory.CreateDirectory(RunContextInfo.Windows.UserHomePath + "\\AppData\\Local\\ImageGlass");

                File.Copy(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\igconfig.json", RunContextInfo.Windows.UserHomePath + "\\AppData\\Local\\ImageGlass\\igconfig.json");
                File.Copy(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\igconfig.json", "C:\\Program Files\\ImageGlass\\igconfig.json");

                Log.FastLog("Done", LogSeverity.Info, IMAGEGLASS_SOURCE);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to install ImageGlass: " + exception.Message, LogSeverity.Error, IMAGEGLASS_SOURCE);
            }
        });
    }
}