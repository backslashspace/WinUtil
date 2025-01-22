using BSS.Logging;
using Microsoft.Win32;
using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class ApplicationsConfigWindow
    {
        private const String ONEDRIVE_SOURCE = "OneDrive";

        private static async Task RemoveOnDrive() => await Task.Run(() =>
        {
            try
            {
                if (!(File.Exists(RunContextInfo.Windows.UserHomePath + "\\AppData\\Local\\Microsoft\\OneDrive\\OneDrive.exe")
                || File.Exists("C:\\Windows\\SysWOW64\\OneDriveSetup.exe")
                || File.Exists("C:\\Windows\\System32\\OneDriveSetup.exe")))
                {
                    Log.FastLog("No OneDrive installation detected", LogSeverity.Info, ONEDRIVE_SOURCE);

                    return;
                }

                Log.FastLog("Removing + Disabling OneDrive", LogSeverity.Info, ONEDRIVE_SOURCE);

                Util.Execute.Process(new("c:\\windows\\system32\\taskkill.exe", "/f /im OneDriveSetup.exe", true, true, true));
                Util.Execute.Process(new("c:\\windows\\system32\\taskkill.exe", "/f /im OneDrive.exe", true, true, true));

                if (File.Exists("C:\\Windows\\System32\\OneDriveSetup.exe"))
                {
                    Util.Execute.Process(new("C:\\Windows\\system32\\OneDriveSetup.exe", "/uninstall", true, true, true));
                }

                if (File.Exists("C:\\Windows\\SysWOW64\\OneDriveSetup.exe"))
                {
                    Util.Execute.Process(new("C:\\Windows\\SysWOW64\\OneDriveSetup.exe", "/uninstall", true, true, true));
                }

                Util.KillExplorer(true);
                Util.Execute.Process(new("C:\\Windows\\System32\\takeown.exe", $"/f \"{RunContextInfo.Windows.UserHomePath}\\OneDrive\"", true, true, true));
                Util.Execute.Process(new("C:\\Windows\\System32\\cacls.exe", $"\"{RunContextInfo.Windows.UserHomePath}\\OneDrive\" /E /g {RunContextInfo.Windows.AdministratorGroupName}:f", true, true, true));
                PowerShell.Create().AddScript($"Remove-Item \"{RunContextInfo.Windows.UserHomePath}\\OneDrive\" -Recurse -Force").Invoke();
                if (Directory.Exists(RunContextInfo.Windows.UserHomePath + "\\AppData\\Local\\Microsoft\\OneDrive")) Directory.Delete(RunContextInfo.Windows.UserHomePath + "\\AppData\\Local\\Microsoft\\OneDrive", true);
                if (Directory.Exists(@"C:\ProgramData\Microsoft OneDrive")) Directory.Delete(@"C:\ProgramData\Microsoft OneDrive", true);
                if (Directory.Exists("C:\\OneDriveTemp")) Directory.Delete("C:\\OneDriveTemp", true);

                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"CLSID", true);
                key.DeleteSubKeyTree("{018D5C66-4533-4307-9B53-224DE2ED1FE6}", false);
                key = Registry.ClassesRoot.OpenSubKey(@"Wow6432Node\CLSID", true);
                key.DeleteSubKeyTree("{018D5C66-4533-4307-9B53-224DE2ED1FE6}", false);
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace", true);
                key.DeleteSubKeyTree("{018D5C66-4533-4307-9B53-224DE2ED1FE6}", false);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);

                Util.Execute.Process(new("C:\\Windows\\System32\\takeown.exe", $"/f \"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\"", true, true, true));
                Util.Execute.Process(new("C:\\Windows\\System32\\cacls.exe", $"\"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\" /E /g {RunContextInfo.Windows.AdministratorGroupName}:f", true, true, true));
                Util.Execute.Process(new("C:\\Windows\\System32\\takeown.exe", $"/f \"C:\\Windows\\System32\\OneDriveSetup.exe\"", true, true, true));
                Util.Execute.Process(new("C:\\Windows\\System32\\cacls.exe", $"\"C:\\Windows\\System32\\OneDriveSetup.exe\" /E /g {RunContextInfo.Windows.AdministratorGroupName}:f", true, true, true));

                Log.Debug("1", ONEDRIVE_SOURCE);

                File.Delete("C:\\Windows\\SysWOW64\\OneDriveSetup.exe");
                File.Delete("C:\\Windows\\System32\\OneDriveSetup.exe");

                Log.Debug("2", ONEDRIVE_SOURCE);

                String[] directories = Directory.GetDirectories("C:\\Windows\\WinSxS");

                Log.Debug("3", ONEDRIVE_SOURCE);

                for (Int32 i = 0; i < directories.Length; ++i)
                {
                    if (directories[i].Contains("microsoft-windows-onedrive-setup"))
                    {
                        Log.Debug("4", ONEDRIVE_SOURCE);

                        Util.Execute.Process(new("C:\\Windows\\System32\\takeown.exe", $"/a /f /d:Y \"{directories[i]}\\OneDriveSetup.exe\"", true, true, true));
                        Util.Execute.Process(new("C:\\Windows\\System32\\icacls.exe", $"\"{directories[i]}\\OneDriveSetup.exe\" /grant {RunContextInfo.Windows.AdministratorGroupName}:(F)", true, true, true));
                        File.Delete(directories[i] + "\\OneDriveSetup.exe");

                        break;
                    }
                }

                Log.Debug("5", ONEDRIVE_SOURCE);

                Util.RestartExplorerForUser();

                Log.FastLog("Done", LogSeverity.Info, ONEDRIVE_SOURCE);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to remove OneDrive: " + exception.Message, LogSeverity.Error, ONEDRIVE_SOURCE);
            }
        });
    }
}