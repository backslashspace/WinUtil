using System;
using System.IO;
using BSS.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed partial class ApplicationsWindow
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

            if (!File.Exists(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\ImageGlass_9.6.1.807_x64.msi")
            || !File.Exists(RunContextInfo.ExecutablePath + "\\assets\\ImageGlass\\igconfig.json"))
            {
                Log.FastLog("Not all installer files found, aborting install (ImageGlass_x64.msi or igconfig.json)", LogSeverity.Error, IMAGEGLASS_SOURCE);
                return;
            }

            Utility.Execute.Process(new(@"C:\Windows\System32\msiexec.exe", $"/i \"{RunContextInfo.ExecutablePath}\\assets\\ImageGlass\\ImageGlass_9.6.1.807_x64.msi\" /quiet", true, true, true));

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