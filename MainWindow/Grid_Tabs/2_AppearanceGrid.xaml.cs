using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
//
using EXT.System.Registry;
using System.IO;
using EXT.Launcher.Process;
using EXT.Launcher.Powershell;

namespace WinUtil.Grid_Tabs
{
    public partial class AppearanceGrid : UserControl
    {
        private static Boolean Set_System_Theme_Toggle_State = true;
        private void Set_System_Theme_Toggle(object sender, RoutedEventArgs e)
        {
            if (Set_System_Theme_Toggle_State)
            {
                MainWindow.LogBoxAdd("Setting system to dark mode", Brushes.LightBlue);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, RegistryValueKind.DWord);
                MainWindow.LogBoxAdd("Done\n", Brushes.MediumSeaGreen);

                Set_System_Theme_Toggle_State = false;
            }
            else
            {
                MainWindow.LogBoxAdd("Setting system to light mode", Brushes.LightBlue);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1, RegistryValueKind.DWord);
                MainWindow.LogBoxAdd("Done\n", Brushes.MediumSeaGreen);

                Set_System_Theme_Toggle_State = true;
            }
        }

        private void InitContextButton()
        {
            if (xRegistry.TestRegValuePresense(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", ""))
            {
                Theme_Switch.Checked -= ContextMenuToggle;
                Theme_Switch.IsChecked = true;
                Theme_Switch.Checked += ContextMenuToggle;
            }
            else
            {
                MenuStateIsLegacy = false;
            }
        }
        internal void SetContextButtonVisibility()
        {
            if (Machine.UIVersion == Machine.WindowsUIVersion.Windows10)
            {
               Context_Button.IsEnabled = false;
               Context_Button_Icon.Opacity = 0.41;
               Theme_Switch.Opacity = 0.41;
               Context_Button_Head.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#747474"));
               Context_Button_Body.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#747474"));
            }

            
        }

        private void InitThemeSwitch()
        {
            Int32 v0 = xRegistry.Get.Value("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", RegistryValueKind.DWord);
            Int32 v1 = xRegistry.Get.Value("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", RegistryValueKind.DWord);

            if ((v0 == 1) || (v1 == 1))
            {
                Bright_Mode_Switch.Checked -= Set_System_Theme_Toggle;
                Bright_Mode_Switch.IsChecked = true;
                Bright_Mode_Switch.Checked += Set_System_Theme_Toggle;
            }
            else
            {
                Set_System_Theme_Toggle_State = false;
            }
        }

        private void InitTerminal_Integrator()
        {
            if (   xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "MUIVerb")

                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "HasLUAShield")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "MUIVerb")

                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "MUIVerb")

                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "HasLUAShield")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "MUIVerb"))
            {
                Terminal_Integrator_SW.Checked -= Terminal_IntegratorToggle;
                Terminal_Integrator_SW.IsChecked = true;
                Terminal_Integrator_SW.Checked += Terminal_IntegratorToggle;
            }
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        public AppearanceGrid()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitThemeSwitch();

            InitContextButton();

            InitTerminal_Integrator();

            Visibility = Visibility.Collapsed;
        }

        //#######################################################################################################

        private static Boolean MenuStateIsLegacy = true;
        private void ContextMenuToggle(object sender, RoutedEventArgs e)
        {
            if (MenuStateIsLegacy)
            {
                xRegistry.Delete.DeleteSubKeyTree(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", new String[] { "InprocServer32" });

                MenuStateIsLegacy = false;
            }
            else
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);

                MenuStateIsLegacy = true;
            }
        }

        private void Terminal_IntegratorToggle(object sender, RoutedEventArgs e)
        {
            Terminal_Integrator_SW.IsEnabled = false;
            Terminal_Integrator_SW.Opacity = 0.41;

            Boolean State = (Boolean)Terminal_Integrator_SW.IsChecked;

            MainWindow.ActivateWorker();

            Task.Run(() =>
            {
                try
                {
                    if (!State)
                    {
                        LogBox.Add("Resetting Windows Terminal integration", Brushes.LightBlue);

                        xRegistry.Delete.DeleteSubKeyTree("HKEY_CLASSES_ROOT\\Directory\\Background\\shell", new String[] { "Powershell", "OpenWTHere", "OpenWTHereAsAdmin" });
                        xRegistry.Delete.DeleteSubKeyTree("HKEY_CLASSES_ROOT\\Directory\\shell", new String[] { "Powershell", "OpenWTHere", "OpenWTHereAsAdmin" });

                        xRegistry.Delete.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Blocked", new String[] { "{9F156763-7844-4DC4-B2B1-901F640F5155}" });

                        xProcess.Run("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
                        xProcess.Run("cmd.exe", "/c \"start explorer.exe\"", true, true);

                        LogBox.Add("Done\n", Brushes.MediumSeaGreen);
                    }
                    else
                    {
                        LogBox.Add("Adding Windows Terminal to context menu", Brushes.LightBlue);

                        String[] Folders = Directory.GetDirectories("C:\\Program Files\\WindowsApps");

                        for (Int32 i = 0; i < Folders.Length; i++)
                        {
                            //Debug.Write(Folders[i]);

                            if (Folders[i].Contains("WindowsTerminal"))
                            {
                                goto SkipInstall;
                            }
                        }

                        //install WT and pedendenedtcios
                        LogBox.Add("Installing Windows Terminal");
                        if (Global.VerboseHashCheck(Resource_Assets.VCLibs_PathName, Resource_Assets.VCLibs_Hash)[0] && Global.VerboseHashCheck(Resource_Assets.WT_PathName, Resource_Assets.WT_Hash)[0] && Global.VerboseHashCheck(Resource_Assets.WT_License_PathName, Resource_Assets.WT_License_Hash)[0])
                        {
                            xPowershell.Run($"Add-AppxPackage -Path \"assets\\{Resource_Assets.VCLibs_PathName}\"");
                            xPowershell.Run($"Add-ProvisionedAppPackage -Online -PackagePath \"assets\\Windows Terminal\\{Resource_Assets.WT_PathName}\" -LicensePath \"assets\\Windows Terminal\\{Resource_Assets.WT_License_PathName}\"");

                            LogBox.Add("Done");
                        }
                        else
                        {
                            MainWindow.DeactivateWorker();

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
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\Background\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:{Machine.AdminGroupName}\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\Background\\shell\\Powershell\" -ot reg -actn ace -ace \"n:{Machine.AdminGroupName};p:full\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:{Machine.AdminGroupName}\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory\\shell\\Powershell\" -ot reg -actn ace -ace \"n:{Machine.AdminGroupName};p:full\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xRegistry.Delete.DeleteSubKeyTree("HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Directory", new String[] { "Background\\shell\\Powershell", "shell\\Powershell" });

                        //put new
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere", "Extended", "", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHere\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "Extended", "", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "HasLUAShield", "", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin", "MUIVerb", "Open in Windows Terminal (Admin)", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\Background\shell\OpenWTHereAsAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere", "Extended", "", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHere\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "Extended", "", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "HasLUAShield", "", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin", "MUIVerb", "Open in Windows Terminal (Admin)", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Directory\shell\OpenWTHereAsAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", "{9F156763-7844-4DC4-B2B1-901F640F5155}", "", RegistryValueKind.String);

                        xProcess.Run("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
                        xProcess.Run("cmd.exe", "/c \"start explorer.exe\"", true, true);

                        LogBox.Add("Done, use with Shift + Right-Click\n", Brushes.MediumSeaGreen);
                    }
                }
                catch (Exception ex)
                {
                    LogBox.Add(ex.Message + "\n", Brushes.Red);
                }

                MainWindow.DeactivateWorker();

            soft_return:

                MainWindow.Dispatcher_Static.Invoke(new Action(() =>
                {
                    Terminal_Integrator_SW.IsEnabled = true;
                    Terminal_Integrator_SW.Opacity = 1;
                }));
            });
        }







    }
}