using EXT.Launcher.Process;
using EXT.System.Registry;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//
using EXT.Launcher.Powershell;
using System.CodeDom.Compiler;

namespace WinUtil.Grid_Tabs
{
    public partial class BehaviorGrid : UserControl
    {
        public BehaviorGrid()
        {
            InitializeComponent();
        }
















        private static Boolean UX_FState = false;
        private void UX(object sender, RoutedEventArgs e)
        {
            if (UX_FState)
            {
                MainWindow.LogBoxAdd("General settings are currently being applied, please wait\n", Brushes.Orange);
                return;
            }

            UX_FState = true;

            MainWindow.ActivateWorker();

            String Version = "v1";

            Object[,] Fields = //user can change, default value, text, tooltip
            {
                { true, true,   "Disable fast startup",                                         "It is recommended to turn off the Windows 'Hybrid Shutdown' feature,\nthis will provide better compatibility and stability in the long term,\nas Hybrid Shutdown does not fully shut down the system in normal\noperation when pressing 'Shut Down PC'." },
                { true, true,   "Show file extensions",                                         null },
                { true, true,   "Set default explorer page to \"This PC\"",                     null },
                { true, true,   "Explorer process separation",                                  "This can improve system stability and prevents the desktop from\ncrashing when an Explorer window becomes unresponsive. (Each explorer window gets its own process)" },
                { true, true,   "Enable NTFS long paths",                                       "This allows windows to use paths that are longer than 260 characters." },
                { true, true,   "Set Desktop wallpaper quality to 100%",                        null },
                { true, true,   "Deactivating local security questions",                        "This improves security, answers of security questions are stored as plain text in the registry." },
                { true, true,   "Display encrypted / compressed NTFS attributes",               "This will make file or directory names show in colors.\nGreen = encrypted\nBlue = compressed" },
                { true, true,   "Properly handle multiple network adapters",                    "This allows the user to have multiple *usable* network connections at the same time.\n\nBy default, Windows tries to keep the number of network connections to a minimum, this could lead to automatic disconnects under some conditions.\n\nAn example would be being connected to a network via Wi-Fi and Ethernet at the same time, by default, Windows would always prefer the Ethernet adapter for all traffic and disconnect from the Wi-Fi network after some time, even if some resources like Internet are only accessible from the Wi-Fi network.\n(The same applies to WWAN cards)\n\n[i] IgnoreNonRoutableEthernet=1, fMinimizeConnections=0" },
                { true, true,   "Enable NumLock on startup",                                    null },
                { true, true,   "Disable Windows Update auto reboot",                           "Disable Windows Update auto reboot while users are logged in." },
                { true, true,   "Enable clipboard history",                                     "The clipboard history is accessible with 'WIN + V', it can be very useful." },
                { true, true,   "Disable Cortana",                                              null },
                { true, true,   "Disable third-party suggestions",                              null },
                { true, true,   "Disable safe search",                                          null },
                { true, true,   "Create small memory dump file on crash",                       null },
                { true, true,   "Remove dynamic user home directory links",                     "By default, the documents, video and image folders are linked to each other, this can lead to some unexpected behavior, like copying the contents of all folders when only copying the documents folder." },
                { true, true,   "Activate explorer compact mode",                               "This is most of the time the default." },
                { true, true,   "Remove old 'Open PowerShell here' entry from context menu",    null },
                { true, true,   "Explorer dont pretty path",                                    "By default, Explorer changes the case of filenames so that a file that is named c:\\iLikeTOTypeTHiS appears as c:\\Iliketotypethis." },
                { true, true,   "Show file operations details",                                 "Shows information like total files, remaining time and speed when copying or moving files." },
                { true, true,   "Hide Taskbar widgets",                                         null },
                { true, true,   "Disable lock screen tips and tricks",                          null },
                { true, true,   "Remove 'AutoLogger' file and restricting directory",           null },
                { true, true,   "Deactivate Windows widgets",                                   "Deactivates the advertising / widgets panel, which can be opened by pressing 'Win + W'." },
                { true, true,   "Deactivate system ads / suggestions & auto app installation",  null },
                { true, true,   "Deactivate search bar web search",  null }
            };

            Dynamic_Select ODS = new($"UX module {Version}",
                        "\\WND_Dialogue\\Icons\\imageres_114.ico",
                        "General settings",
                        "For more information on some points, read the tooltip.",
                        Fields);

            ODS.ShowDialog();

            if ((Boolean)ODS.Was_Cancled)
            {
                MainWindow.DeactivateWorker();
                UX_FState = false;
                return;
            }

            //

            if (!Global.VerboseHashCheck(Resource_Assets.SetACL_PathName, Resource_Assets.SetACL_Hash)[0])
            {
                MainWindow.LogBoxAdd($" - UX module {Version}\n", StayInLine: true);

                Dialogue Warn = new($"WinUtil: UX module {Version}",
                        $"\"{Resource_Assets.SetACL_PathName}\" was not found or is invalid,\nskip \"{Fields[18,2]}\"?",
                        Dialogue.Icons.Circle_Error,
                        "Continue",
                        "Cancle");

                Warn.ShowDialog();

                if (Warn.Result == 0)
                {
                    ODS.Result[18] = false;
                }
                else
                {
                    MainWindow.DeactivateWorker();
                    UX_FState = false;
                    return;
                }
            }

            //

            MainWindow.LogBoxAdd("Applying general settings", Brushes.LightBlue);

            Task.Run(() =>
            {
                try
                {
                    if (ODS.Result.Length != 27)
                    {
                        throw new ArgumentException("unexpected user length\n");
                    }

                    #region Act
                    if (ODS.Result[0])
                    {
                        MainWindow.DispatchedLogBoxAdd("Deactivating fast startup");
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[1])
                    {
                        MainWindow.DispatchedLogBoxAdd("Showing file extensions");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[2])
                    {
                        MainWindow.DispatchedLogBoxAdd("Setting default explorer page to \"This PC\"");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[3])
                    {
                        MainWindow.DispatchedLogBoxAdd("Activating Explorer process separation");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SeparateProcess", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[4])
                    {
                        MainWindow.DispatchedLogBoxAdd("Enabling NTFS long paths");
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[5])
                    {
                        MainWindow.DispatchedLogBoxAdd("Setting Desktop wallpaper quality to 100%");
                        Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[6])
                    {
                        MainWindow.DispatchedLogBoxAdd("Deactivating local security questions");
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "NoLocalPasswordResetQuestions", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[7])
                    {
                        MainWindow.DispatchedLogBoxAdd("Showing encrypted / compressed NTFS attributes");
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowEncryptCompressedColor", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[8])
                    {
                        MainWindow.DispatchedLogBoxAdd("Changing network adapter behavior");
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WcmSvc\Local", "fMinimizeConnections", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Wcmsvc", "IgnoreNonRoutableEthernet", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[9])
                    {
                        MainWindow.DispatchedLogBoxAdd("Enabling NumLock on startup");
                        Registry.SetValue("HKEY_USERS\\.DEFAULT\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 2, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[10])
                    {
                        MainWindow.DispatchedLogBoxAdd("Disabling Windows Update auto reboot");
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[11])
                    {
                        MainWindow.DispatchedLogBoxAdd("Enabling clipboard history");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Clipboard", "EnableClipboardHistory", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[12])
                    {
                        MainWindow.DispatchedLogBoxAdd("Disabling Cortana");
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[13])
                    {
                        MainWindow.DispatchedLogBoxAdd("Disabling third-party suggestions");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\CloudContent", "DisableThirdPartySuggestions", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[14])
                    {
                        MainWindow.DispatchedLogBoxAdd("Disabling safe search");
                        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\SearchSettings", "SafeSearchMode", 0, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[15])
                    {
                        MainWindow.DispatchedLogBoxAdd("Set create small memory dump file on crash");
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\CrashControl", "CrashDumpEnabled", 3, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[16])
                    {
                        MainWindow.DispatchedLogBoxAdd("Remove dynamic user home directory links");
                        xPowershell.Run("Get-ChildItem -Path \"C:\\Users\\" + Machine.UserPath + "\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \"ReparsePoint\" } | remove-item -force -ErrorAction SilentlyContinue", WaitForExit: true);
                        xPowershell.Run("Get-ChildItem -Path \"C:\\Users\\" + Machine.UserPath + "\\documents\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \"ReparsePoint\" } | remove-item -force -ErrorAction SilentlyContinue", WaitForExit: true);
                    }

                    if (ODS.Result[17])
                    {
                        MainWindow.DispatchedLogBoxAdd("Activating explorer compact mode");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[18])
                    {
                        if (Machine.AdminGroupName == null)
                        {
                            MainWindow.DispatchedLogBoxAdd("Waiting for program initialization", Brushes.Orange);

                            while (Machine.AdminGroupName == null)
                            {
                                Task.Delay(128).Wait();
                            }
                        }

                        MainWindow.DispatchedLogBoxAdd("Removing old 'Open PowerShell here' entry from context menu");
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:{Machine.AdminGroupName}\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Powershell\" -ot reg -actn ace -ace \"n:{Machine.AdminGroupName};p:full\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_CLASSES_ROOT\\Directory\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:{Machine.AdminGroupName}\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xProcess.Run(Resource_Assets.SetACL_PathName, $"-on \"HKEY_CLASSES_ROOT\\Directory\\shell\\Powershell\" -ot reg -actn ace -ace \"n:{Machine.AdminGroupName};p:full\" -rec Yes", WaitForExit: true, HiddenExecute: true);
                        xRegistry.Delete.DeleteSubKeyTree("HKEY_CLASSES_ROOT\\Directory", new String[] { "Background\\shell\\Powershell", "shell\\Powershell" });
                    }

                    if (ODS.Result[19])
                    {
                        MainWindow.DispatchedLogBoxAdd("Explorer dont pretty path");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DontPrettyPath", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[20])
                    {
                        MainWindow.DispatchedLogBoxAdd("Showing file operations details");
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager", "EnthusiastMode", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[21])
                    {
                        MainWindow.DispatchedLogBoxAdd("Hiding Taskbar widgets");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 2, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[22])
                    {
                        MainWindow.DispatchedLogBoxAdd("Disabling lock screen tips and tricks");
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\Subscriptions\338387", "SubscriptionContext", "sc-mode=0", RegistryValueKind.String);

                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true))
                        {
                            foreach (String subKeyName in key.GetSubKeyNames())
                            {
                                if (subKeyName.Contains("$lockscreenpinnedtiles$windows.data.curatedtilecollection.tilecollection"))
                                {
                                    Byte[] Bytes = new Byte[] { 0x02, 0x00, 0x00, 0x00, 0x56, 0x51, 0x4C, 0x29, 0xDC, 0x12, 0xD9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0x0A, 0x0A, 0x00, 0xCA, 0x32, 0x00, 0xCC, 0x83, 0x12, 0x03, 0x26, 0x7B, 0x00, 0x32, 0x00, 0x43, 0x00, 0x46, 0x00, 0x43, 0x00, 0x44, 0x00, 0x46, 0x00, 0x46, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x45, 0x00, 0x37, 0x00, 0x31, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x38, 0x00, 0x45, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x39, 0x00, 0x45, 0x00, 0x32, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x33, 0x00, 0x38, 0x00, 0x44, 0x00, 0x31, 0x00, 0x35, 0x00, 0x36, 0x00, 0x36, 0x00, 0x46, 0x00, 0x30, 0x00, 0x37, 0x00, 0x31, 0x00, 0x7D, 0x00, 0x26, 0x7B, 0x00, 0x44, 0x00, 0x31, 0x00, 0x38, 0x00, 0x43, 0x00, 0x45, 0x00, 0x34, 0x00, 0x43, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x43, 0x00, 0x38, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x43, 0x00, 0x31, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x41, 0x00, 0x41, 0x00, 0x36, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x32, 0x00, 0x33, 0x00, 0x33, 0x00, 0x46, 0x00, 0x46, 0x00, 0x30, 0x00, 0x44, 0x00, 0x32, 0x00, 0x41, 0x00, 0x35, 0x00, 0x43, 0x00, 0x39, 0x00, 0x7D, 0x00, 0x26, 0x7B, 0x00, 0x45, 0x00, 0x44, 0x00, 0x38, 0x00, 0x35, 0x00, 0x42, 0x00, 0x36, 0x00, 0x43, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x33, 0x00, 0x39, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x31, 0x00, 0x38, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x36, 0x00, 0x41, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x46, 0x00, 0x39, 0x00, 0x39, 0x00, 0x36, 0x00, 0x44, 0x00, 0x45, 0x00, 0x38, 0x00, 0x31, 0x00, 0x37, 0x00, 0x42, 0x00, 0x39, 0x00, 0x37, 0x00, 0x7D, 0x00, 0x00, 0xCD, 0x0A, 0x12, 0x0A, 0x03, 0x26, 0x7B, 0x00, 0x32, 0x00, 0x43, 0x00, 0x46, 0x00, 0x43, 0x00, 0x44, 0x00, 0x46, 0x00, 0x46, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x45, 0x00, 0x37, 0x00, 0x31, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x38, 0x00, 0x45, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x39, 0x00, 0x45, 0x00, 0x32, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x33, 0x00, 0x38, 0x00, 0x44, 0x00, 0x31, 0x00, 0x35, 0x00, 0x36, 0x00, 0x36, 0x00, 0x46, 0x00, 0x30, 0x00, 0x37, 0x00, 0x31, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xF3, 0xBF, 0xF3, 0xE7, 0x02, 0x24, 0x9C, 0xCE, 0x03, 0x44, 0xE4, 0x91, 0x01, 0x66, 0x9E, 0xD7, 0x8C, 0xEC, 0xD8, 0xC2, 0x99, 0xF8, 0x71, 0x00, 0xD2, 0x0A, 0x52, 0x50, 0x00, 0x7E, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x6D, 0x00, 0x75, 0x00, 0x6E, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x61, 0x00, 0x70, 0x00, 0x70, 0x00, 0x73, 0x00, 0x5F, 0x00, 0x38, 0x00, 0x77, 0x00, 0x65, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x62, 0x00, 0x33, 0x00, 0x64, 0x00, 0x38, 0x00, 0x62, 0x00, 0x62, 0x00, 0x77, 0x00, 0x65, 0x00, 0x21, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x6C, 0x00, 0x69, 0x00, 0x76, 0x00, 0x65, 0x00, 0x2E, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x61, 0x00, 0x72, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x32, 0x00, 0x00, 0x26, 0x7B, 0x00, 0x44, 0x00, 0x31, 0x00, 0x38, 0x00, 0x43, 0x00, 0x45, 0x00, 0x34, 0x00, 0x43, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x43, 0x00, 0x38, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x43, 0x00, 0x31, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x41, 0x00, 0x41, 0x00, 0x36, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x32, 0x00, 0x33, 0x00, 0x33, 0x00, 0x46, 0x00, 0x46, 0x00, 0x30, 0x00, 0x44, 0x00, 0x32, 0x00, 0x41, 0x00, 0x35, 0x00, 0x43, 0x00, 0x39, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xCA, 0xC9, 0xB3, 0x8C, 0x0D, 0x24, 0x8E, 0x19, 0x44, 0x9A, 0x98, 0x01, 0x66, 0xAA, 0xDD, 0x8D, 0xF9, 0x83, 0xDE, 0xF4, 0xD2, 0xC9, 0x01, 0x00, 0xD2, 0x0A, 0x4E, 0x50, 0x00, 0x7E, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x6D, 0x00, 0x75, 0x00, 0x6E, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x61, 0x00, 0x70, 0x00, 0x70, 0x00, 0x73, 0x00, 0x5F, 0x00, 0x38, 0x00, 0x77, 0x00, 0x65, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x62, 0x00, 0x33, 0x00, 0x64, 0x00, 0x38, 0x00, 0x62, 0x00, 0x62, 0x00, 0x77, 0x00, 0x65, 0x00, 0x21, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x6C, 0x00, 0x69, 0x00, 0x76, 0x00, 0x65, 0x00, 0x2E, 0x00, 0x6D, 0x00, 0x61, 0x00, 0x69, 0x00, 0x6C, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x31, 0x00, 0x00, 0x26, 0x7B, 0x00, 0x45, 0x00, 0x44, 0x00, 0x38, 0x00, 0x35, 0x00, 0x42, 0x00, 0x36, 0x00, 0x43, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x33, 0x00, 0x39, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x31, 0x00, 0x38, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x36, 0x00, 0x41, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x46, 0x00, 0x39, 0x00, 0x39, 0x00, 0x36, 0x00, 0x44, 0x00, 0x45, 0x00, 0x38, 0x00, 0x31, 0x00, 0x37, 0x00, 0x42, 0x00, 0x39, 0x00, 0x37, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xCB, 0xED, 0x96, 0xEC, 0x0E, 0x24, 0x94, 0xA7, 0x01, 0x44, 0x83, 0x83, 0x01, 0x66, 0x86, 0xD9, 0xE6, 0xB7, 0xE9, 0xBB, 0xE0, 0xBD, 0x97, 0x01, 0x00, 0xD2, 0x0A, 0x26, 0x50, 0x00, 0x7E, 0x00, 0x4D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x53, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x70, 0x00, 0x65, 0x00, 0x41, 0x00, 0x70, 0x00, 0x70, 0x00, 0x5F, 0x00, 0x6B, 0x00, 0x7A, 0x00, 0x66, 0x00, 0x38, 0x00, 0x71, 0x00, 0x78, 0x00, 0x66, 0x00, 0x33, 0x00, 0x38, 0x00, 0x7A, 0x00, 0x67, 0x00, 0x35, 0x00, 0x63, 0x00, 0x21, 0x00, 0x41, 0x00, 0x70, 0x00, 0x70, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x33, 0x00, 0x00, 0x00 };

                                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", Bytes, RegistryValueKind.Binary);

                                    break;
                                }
                            }

                            key.Close();
                            key.Dispose();
                        }
                    }

                    if (ODS.Result[23])
                    {
                        MainWindow.DispatchedLogBoxAdd("Removing 'AutoLogger' file and restricting directory");
                        File.Delete(@"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl");
                        xProcess.Run(@"icacls.exe", @"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\Autologger /deny SYSTEM:(OI)(CI)F", WaitForExit: true, HiddenExecute: true);
                    }

                    if (ODS.Result[24])
                    {
                        MainWindow.DispatchedLogBoxAdd("Deactivate Windows widgets");
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{2cc5ca98-6485-489a-920e-b3e88a6ccce3}", 1, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[25])
                    {
                        MainWindow.DispatchedLogBoxAdd("Deactivating system ads / suggestions & auto app installation");
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "ContentDeliveryAllowed", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "FeatureManagementEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "OemPreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEverEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContentEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
                    }

                    if (ODS.Result[26])
                    {
                        MainWindow.DispatchedLogBoxAdd("Deactivating search bar web search");
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                    }
                    #endregion

                    MainWindow.DispatchedLogBoxAdd("Restarting explorer to apply changes");
                    xProcess.Run("C:\\Windows\\System32\\taskkill.exe", "/IM explorer.exe /F", WaitForExit: true, HiddenExecute: true);
                    Task.Delay(1000).Wait();
                    xProcess.Run("C:\\Windows\\explorer.exe", null, WaitForExit: true, HiddenExecute: true);

                    MainWindow.DispatchedLogBoxAdd("Done\n", Brushes.MediumSeaGreen);
                }
                catch (Exception ex)
                {
                    MainWindow.DispatchedLogBoxAdd(ex.Message, Brushes.Red);
                }

                MainWindow.DeactivateWorker();

                UX_FState = false;
            });
        }

        private static Boolean HyperKey_UnReg_FState = false;
        private void HyperKey_UnReg(object sender, RoutedEventArgs e)
        {
            if (HyperKey_UnReg_FState)
            {
                MainWindow.LogBoxAdd("Hyper key handling is currently running, please wait\n", Brushes.Orange);

                return;
            }

            HyperKey_UnReg_FState = true;

            MainWindow.ActivateWorker();

            String Version = "v1";

            //

            if (!Global.VerboseHashCheck(Resource_Assets.HyperKey_UnReg_PathName, Resource_Assets.HyperKey_UnReg_Hash)[0])
            {
                MainWindow.LogBoxAdd($" - HyperKey_UnReg module {Version}\n", StayInLine: true);

                MainWindow.DeactivateWorker();

                HyperKey_UnReg_FState = false;

                return;
            }

            //

            Dialogue Info = new("WinUtil: Hyper keys",
                        "This will automatically deregister all\n" +
                        "Office Hyper keys and the Teams-Windows integration on every user login.\n\n" +

                        "The following combinations will be deregister:\n\n" +
                        "Office: Ctrl + Shift + Win + Alt + W, T, Y, O, P, D, L, X, N, <Space>\n" +
                        "Office-App: Ctrl + Shift + Win + Alt\n" +
                        "Teams: Win + C",
                        Dialogue.Icons.Gear,
                        "Continue",
                        "Cancle", 0);

            Info.ShowDialog();

            if (Info.Result == 1)
            {
                MainWindow.DeactivateWorker();

                HyperKey_UnReg_FState = false;

                return;
            }

            //

            MainWindow.LogBoxAdd("Setting up automatic Hyper key deregistration", Brushes.LightBlue);

            Task.Run(() =>
            {
                try
                {
                    MainWindow.DispatchedLogBoxAdd("Copying file to \"C:\\Program Files\\WinUtil\\HyperKey_UnReg.exe\"");

                    String STD = "C:\\Program Files\\WinUtil\\HyperKey_UnReg.exe";

                    if (!Directory.Exists("C:\\Program Files\\WinUtil"))
                    {
                        Directory.CreateDirectory("C:\\Program Files\\WinUtil");
                    }

                    File.Copy(Resource_Assets.HyperKey_UnReg_PathName, STD, true);

                    MainWindow.DispatchedLogBoxAdd("Adding program to global user init\n(HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\\Userinit)");

                    String GlobalUserInitString = xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "Userinit", RegistryValueKind.String);

                    if (GlobalUserInitString.Contains($"\"{STD}\""))
                    {
                        MainWindow.DispatchedLogBoxAdd(" - ", StayInLine: true);
                        MainWindow.DispatchedLogBoxAdd("already present", Brushes.LimeGreen, StayInLine: true);
                    }
                    else
                    {
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "Userinit", $"{GlobalUserInitString}, \"{STD}\",", RegistryValueKind.String);
                    }

                    MainWindow.DispatchedLogBoxAdd("Unregistering Teams and Office Hyper keys");
                    xProcess.Run(STD, WaitForExit: true);

                    MainWindow.DispatchedLogBoxAdd("Done\n", Brushes.MediumSeaGreen);
                }
                catch (Exception ex)
                {
                    MainWindow.DispatchedLogBoxAdd(ex.Message + "\n", Brushes.Red);
                }

                MainWindow.DeactivateWorker();

                HyperKey_UnReg_FState = false;
            });
        }














        private void f(object sender, RoutedEventArgs e)
        {

        }
    }
}