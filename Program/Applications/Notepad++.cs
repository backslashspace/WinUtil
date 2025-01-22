using BSS.Logging;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class ApplicationsConfigWindow
    {
        private const String NOTEPADPLUSPLUS_SOURCE = "Notepad++";

        private static async Task InstallNotepadPlusPlus() => await Task.Run(() =>
        {
            try
            {
                Log.FastLog("Installing Notepad++", LogSeverity.Info, NOTEPADPLUSPLUS_SOURCE);

                Util.KillExplorer(true);

                if (!Util.Execute.Process(new(RunContextInfo.ExecutablePath + "\\assets\\Notepad++\\npp.8.7.5.Installer.x64.exe", "/S", true, true, true)).Success)
                {
                    Log.FastLog("Installer not found, aborted install", LogSeverity.Error, NOTEPADPLUSPLUS_SOURCE);
                    Util.RestartExplorerForUser();
                    return;
                }

                if (Directory.Exists(RunContextInfo.Windows.UserHomePath + "\\AppData\\Roaming\\Notepad++"))
                {
                    Directory.Delete(RunContextInfo.Windows.UserHomePath + "\\AppData\\Roaming\\Notepad++", true);
                }
                if (Directory.Exists("C:\\Program Files\\Notepad++"))
                {
                    Directory.Delete("C:\\Program Files\\Notepad++", true);
                }

                ZipFile.ExtractToDirectory(RunContextInfo.ExecutablePath + "\\assets\\Notepad++\\program.zip", "C:\\Program Files\\Notepad++");
                ZipFile.ExtractToDirectory(RunContextInfo.ExecutablePath + "\\assets\\Notepad++\\roaming.zip", RunContextInfo.Windows.UserHomePath + "\\AppData\\Roaming\\Notepad++");

                if (!File.Exists("C:\\Windows\\Fonts\\CascadiaMono.ttf"))
                {
                    File.Copy(RunContextInfo.ExecutablePath + "\\assets\\Notepad++\\CascadiaMono.ttf", "C:\\Windows\\Fonts\\CascadiaMono.ttf", false);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", "Cascadia Mono Regular (TrueType)", "CascadiaMono.ttf", RegistryValueKind.String);
                }

                Registry.SetValue(@"HKEY_CLASSES_ROOT\Applications\notepad++.exe\DefaultIcon", "", "C:\\Windows\\System32\\imageres.dll,97", RegistryValueKind.String);

                Util.RestartExplorerForUser();

                Log.FastLog("Done", LogSeverity.Info, NOTEPADPLUSPLUS_SOURCE);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to install Notepad++: " + exception.Message, LogSeverity.Error, NOTEPADPLUSPLUS_SOURCE);
            }
        });
    }
}