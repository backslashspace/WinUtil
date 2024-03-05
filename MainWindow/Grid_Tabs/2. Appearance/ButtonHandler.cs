using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
//
using BSS.Launcher;
using BSS.System.Registry;

namespace WinUtil.Grid_Tabs
{
    public partial class AppearanceGrid 
    {
        private void Set_System_Theme_Toggle(object sender, RoutedEventArgs e)
        {
            Boolean state = (Boolean)!OSTheme_ToggleButton.IsChecked;

            if (state)
            {
                MainWindow.LogBoxAdd("Setting system to dark mode", Brushes.LightBlue);
                xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, RegistryValueKind.DWord);
                xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, RegistryValueKind.DWord);
                MainWindow.LogBoxAdd("Done\n", Brushes.MediumSeaGreen);
            }
            else
            {
                MainWindow.LogBoxAdd("Setting system to light mode", Brushes.LightBlue);
                xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1, RegistryValueKind.DWord);
                xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1, RegistryValueKind.DWord);
                MainWindow.LogBoxAdd("Done\n", Brushes.MediumSeaGreen);
            }
        }

        private async void ContextMenu_ToggleButton_Handler(object sender, RoutedEventArgs e)
        {
            ContextMenu_ToggleButton.IsEnabled = false;
            ContextMenu_ToggleButton.Opacity = 0.41d;

            Boolean state = (Boolean)!ContextMenu_ToggleButton.IsChecked;

            if (state)
            {
                xRegistry.DeleteSubKeyTree(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", "InprocServer32");
            }
            else
            {
                xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);
            }

            await Common.RestartExplorer().ConfigureAwait(true);

            ContextMenu_ToggleButton.Opacity = 1.0d;
            ContextMenu_ToggleButton.IsEnabled = true;
        }

        private async void Terminal_Integration_ToggleButton_Handler(object sender, RoutedEventArgs e)
        {
            Terminal_Integration_ToggleButton.IsEnabled = false;
            Terminal_Integration_ToggleButton.Opacity = 0.41d;

            MainWindow.ActivateWorker();

            Boolean state = (Boolean)!Terminal_Integration_ToggleButton.IsChecked;

            await Task.Run(() =>
            {
                try
                {
                    if (state)
                    {
                        LogBox.Add("Resetting Windows Terminal integration", Brushes.LightBlue);

                        xRegistry.DeleteSubKeyTrees("HKEY_CLASSES_ROOT\\Directory\\Background\\shell", new String[] { "Powershell", "OpenWTHere", "OpenWTHereAsAdmin" });
                        xRegistry.DeleteSubKeyTrees("HKEY_CLASSES_ROOT\\Directory\\shell", new String[] { "Powershell", "OpenWTHere", "OpenWTHereAsAdmin" });

                        xRegistry.DeleteValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Blocked", "{9F156763-7844-4DC4-B2B1-901F640F5155}");

                        Common.RestartExplorer().Wait();

                        LogBox.Add("Done\n", Brushes.MediumSeaGreen);
                    }
                    else
                    {
                        LogBox.Add("Adding Windows Terminal to context menu", Brushes.LightBlue);

                        String[] Folders = Directory.GetDirectories("C:\\Program Files\\WindowsApps");

                        for (Int32 i = 0; i < Folders.Length; i++)
                        {
                            if (Folders[i].Contains("WindowsTerminal"))
                            {
                                goto SkipInstall;
                            }
                        }

                        //install WT and dependencies
                        LogBox.Add("Installing Windows Terminal");
                        if (Global.VerboseHashCheck(Resource_Assets.VCLibs_PathName, Resource_Assets.VCLibs_Hash)[0] && Global.VerboseHashCheck(Resource_Assets.WT_PathName, Resource_Assets.WT_Hash)[0] && Global.VerboseHashCheck(Resource_Assets.WT_License_PathName, Resource_Assets.WT_License_Hash)[0])
                        {
                            xPowershell.Run($"Add-AppxPackage -Path \"{Resource_Assets.VCLibs_PathName}\"");
                            xPowershell.Run($"Add-ProvisionedAppPackage -Online -PackagePath \"{Resource_Assets.WT_PathName}\" -LicensePath \"{Resource_Assets.WT_License_PathName}\"");

                            LogBox.Add("Done");
                        }
                        else
                        {
                            goto soft_return;
                        }

                    SkipInstall:

                        //remove old ps entry
                        if (Machine.AdminGroupName == null)
                        {
                            LogBox.Add("Waiting for program initialization", Brushes.Orange);

                            while (Machine.AdminGroupName == null)
                            {
                                Task.Delay(128).Wait();
                            }
                        }

                        LogBox.Add("Removing old 'Open PowerShell here' entry from context menu");
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\Background\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:{Machine.AdminGroupName}\" -rec Yes", waitForExit: true, hiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\Background\\shell\\Powershell\" -ot reg -actn ace -ace \"n:{Machine.AdminGroupName};p:full\" -rec Yes", waitForExit: true, hiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:{Machine.AdminGroupName}\" -rec Yes", waitForExit: true, hiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\shell\\Powershell\" -ot reg -actn ace -ace \"n:{Machine.AdminGroupName};p:full\" -rec Yes", waitForExit: true, hiddenExecute: true);
                        xRegistry.DeleteSubKeyTrees("HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory", new String[] { "Background\\shell\\Powershell", "shell\\Powershell" });

                        //put new
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere", "Extended", "", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "Extended", "", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "HasLUAShield", "", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "MUIVerb", "Open in Windows Terminal (Admin)", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere", "Extended", "", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "Extended", "", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "HasLUAShield", "", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "MUIVerb", "Open in Windows Terminal (Admin)", RegistryValueKind.String);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", "{9F156763-7844-4DC4-B2B1-901F640F5155}", "", RegistryValueKind.String);

                        Common.RestartExplorer().Wait();

                        LogBox.Add("Done, use with Shift + Right-Click\n", Brushes.MediumSeaGreen);
                    }
                }
                catch (Exception ex)
                {
                    LogBox.Add(ex.Message + "\n", Brushes.Red);
                }

            soft_return:;

            }).ConfigureAwait(true);

            MainWindow.DeactivateWorker();

            Terminal_Integration_ToggleButton.IsEnabled = true;
            Terminal_Integration_ToggleButton.Opacity = 1.0d;
        }









        
    }
}