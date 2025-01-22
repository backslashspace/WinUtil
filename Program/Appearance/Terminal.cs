using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class AppearanceConfigWindow
    {
        private const String TERMINAL_SOURCE = "Terminal";

        private async static Task Terminal(Boolean enable)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (enable) TerminalEnable();
                    else TerminalReset();
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to with: " + exception.Message, LogSeverity.Error, TERMINAL_SOURCE);
                }
            });

            return;
        }

        // # # # # # # # # # # # # # # # # # # # # #

        private static void TerminalEnable()
        {
            Log.FastLog("Removing old 'Open PowerShell here' entry from context menu", LogSeverity.Info, TERMINAL_SOURCE);

            if (!Util.UnlockRegistryKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\background\\shell\\Powershell", RunContextInfo.Windows.AdministratorGroupName))
            {
                Log.FastLog("Failed to take ownership of 'HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\background\\shell\\Powershell'", LogSeverity.Error, TERMINAL_SOURCE);
                return;
            }
            if (!Util.UnlockRegistryKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\shell\\Powershell", RunContextInfo.Windows.AdministratorGroupName))
            {
                Log.FastLog("Failed to take ownership of 'HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\shell\\Powershell'", LogSeverity.Error, TERMINAL_SOURCE);
                return;
            }

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Directory\\background\\shell", true);
            key.DeleteSubKeyTree("Powershell", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Directory\\shell", true);
            key.DeleteSubKeyTree("Powershell", false);

        RETRY_COPY:
            Log.FastLog("Copying wt.exe", LogSeverity.Info, TERMINAL_SOURCE);
            // copy wt.exe to use its icon in the context menu
            IEnumerable<String> windowsApps = Directory.EnumerateFileSystemEntries("C:\\Program Files\\WindowsApps");
            foreach (String entry in windowsApps)
            {
                if (entry.IndexOf("Microsoft.WindowsTerminal") > 0)
                {
                    if (File.Exists(entry + "\\wt.exe"))
                    {
                        Directory.CreateDirectory("C:\\Program Files\\WinUtil");
                        File.Copy(entry + "\\wt.exe", "C:\\Program Files\\WinUtil\\wt.exe", true);
                        goto SET_REGISTRY;
                    }
                }
            }

            Log.FastLog("Unable to find windows terminal installation in 'C:\\Program Files\\WindowsApps'", LogSeverity.Info, TERMINAL_SOURCE);
            if (!InstallTerminal()) return;
            goto RETRY_COPY;

        SET_REGISTRY:
            // set new context menu entry
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminalAdmin", "Extended", "", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminalAdmin", "HasLUAShield", "", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminalAdmin", "Icon", "C:\\Program Files\\WinUtil\\wt.exe", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminalAdmin", "MUIVerb", "Open in Terminal (Admin)", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminalAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminalAdmin", "Extended", "", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminalAdmin", "HasLUAShield", "", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminalAdmin", "Icon", "C:\\Program Files\\WinUtil\\wt.exe", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminalAdmin", "MUIVerb", "Open in Terminal (Admin)", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminalAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);

            if (Util.IsWindows10UI())
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminal", "Extended", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminal", "Icon", "C:\\Program Files\\WinUtil\\wt.exe", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminal", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\WindowsTerminal\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminal", "Extended", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminal", "Icon", "C:\\Program Files\\WinUtil\\wt.exe", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminal", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\WindowsTerminal\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);
            }

            Util.RestartExplorerForUser();
            Log.FastLog("Done", LogSeverity.Info, TERMINAL_SOURCE);
        }

        private static void TerminalReset()
        {
            Log.FastLog("Resetting Terminal integration", LogSeverity.Info, TERMINAL_SOURCE);

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Directory\\background\\shell", true);
            key.DeleteSubKeyTree("WindowsTerminalAdmin", false);
            key.DeleteSubKeyTree("WindowsTerminal", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Directory\\shell", true);
            key.DeleteSubKeyTree("WindowsTerminalAdmin", false);
            key.DeleteSubKeyTree("WindowsTerminal", false);

            Util.RestartExplorerForUser();
            Log.FastLog("Done", LogSeverity.Info, TERMINAL_SOURCE);
        }

        //

        private static Boolean InstallTerminal()
        {
            Log.FastLog("Installing Windows Terminal", LogSeverity.Info, TERMINAL_SOURCE);

            if (!File.Exists(RunContextInfo.ExecutablePath + "\\assets\\Microsoft.VCLibs.140.00.UWPDesktop_14.0.33728.0_x64__8wekyb3d8bbwe.Appx"))
            {
                Log.FastLog("VCLibs not found", LogSeverity.Error, TERMINAL_SOURCE);
                return false;
            }

            if (!File.Exists(RunContextInfo.ExecutablePath + "\\assets\\Microsoft.UI.Xaml.2.8_8.2310.30001.0_x64__8wekyb3d8bbwe.Appx"))
            {
                Log.FastLog("UI.Xaml not found", LogSeverity.Error, TERMINAL_SOURCE);
                return false;
            }

            if (!File.Exists(RunContextInfo.ExecutablePath + "\\assets\\b79298bcbb3945749cf7a91baa551990.msixbundle"))
            {
                Log.FastLog("Install file not found", LogSeverity.Error, TERMINAL_SOURCE);
                return false;
            }

            if (!File.Exists(RunContextInfo.ExecutablePath + "\\assets\\b79298bcbb3945749cf7a91baa551990_License1.xml"))
            {
                Log.FastLog("License file not found", LogSeverity.Error, TERMINAL_SOURCE);
                return false;
            }

            //

            PowerShell.Create().AddScript($"Add-AppxPackage -Path \"{RunContextInfo.ExecutablePath + "\\assets\\Microsoft.VCLibs.140.00.UWPDesktop_14.0.33728.0_x64__8wekyb3d8bbwe.Appx"}\"").Invoke();
            PowerShell.Create().AddScript($"Add-AppxPackage -Path \"{RunContextInfo.ExecutablePath + "\\assets\\Microsoft.UI.Xaml.2.8_8.2310.30001.0_x64__8wekyb3d8bbwe.Appx"}\"").Invoke();
            PowerShell.Create().AddScript($"Add-ProvisionedAppPackage -Online -PackagePath \"{RunContextInfo.ExecutablePath + "\\assets\\b79298bcbb3945749cf7a91baa551990.msixbundle"}\" -LicensePath \"{RunContextInfo.ExecutablePath + "\\assets\\b79298bcbb3945749cf7a91baa551990_License1.xml"}\"").Invoke();

            return true;
        }
    }
}