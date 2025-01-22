using BSS.Logging;
using System;
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

                Log.FastLog("Done", LogSeverity.Info, IMAGEGLASS_SOURCE);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to install ImageGlass: " + exception.Message, LogSeverity.Error, IMAGEGLASS_SOURCE);
            }
        });
    }
}