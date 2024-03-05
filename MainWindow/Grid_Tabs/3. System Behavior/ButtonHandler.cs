using Microsoft.Win32;
using System;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
////
using BSS.Launcher;
using BSS.System.Registry;
using BSS.System.Service;

namespace WinUtil.Grid_Tabs
{
    public partial class BehaviorGrid
    {
        private async void UX_ButtonHandler(object sender, RoutedEventArgs e)
        {
            UX_Button.IsEnabled = false;
            UX_Button.Opacity = 0.41d;

            MainWindow.ActivateWorker();

            String moduleVersion = "v1";

            Selector.Field[] fields = new Selector.Field[]
            {
                new(true, true,     "Disable fast startup",                                           "It is recommended to turn off the Windows 'Hybrid Shutdown' feature,\nthis will provide better compatibility and stability in the long term,\nas Hybrid Shutdown does not fully shut down the system in normal\noperation when pressing 'Shut Down PC'."),
                new(true, true,     "Show file extensions",                                           null),
                new(true, true,     "Set default explorer page to \"This PC\"",                       null),
                new(true, true,     "Explorer process separation",                                    "This can improve system stability and prevents the desktop from\ncrashing when an Explorer window becomes unresponsive. (Each explorer window gets its own process)"),
                new(true, true,     "Enable NTFS long paths",                                         "This allows windows to use paths that are longer than 260 characters."),
                new(true, true,     "Set Desktop wallpaper quality to 100%",                          null),
                new(true, true,     "Deactivating local security questions",                          "This improves security, answers of security questions are stored as plain text in the registry."),
                new(true, true,     "Display encrypted / compressed NTFS attributes",                 "This will make file or directory names show in colors.\nGreen = encrypted\nBlue = compressed"),
                new(true, true,     "Properly handle multiple network adapters",                      "This allows the user to have multiple *usable* network connections at the same time.\n\nBy default, Windows tries to keep the number of network connections to a minimum, this could lead to automatic disconnects under some conditions.\n\nAn example would be being connected to a network via Wi-Fi and Ethernet at the same time, by default, Windows would always prefer the Ethernet adapter for all traffic and disconnect from the Wi-Fi network after some time, even if some resources like Internet are only accessible from the Wi-Fi network.\n(The same applies to WWAN cards)\n\n[i] IgnoreNonRoutableEthernet=1, fMinimizeConnections=0"),
                new(true, true,     "Enable NumLock on startup",                                      null),
                new(true, true,     "Disable Windows Update auto reboot",                             "Disable Windows Update auto reboot while users are logged in."),
                new(true, true,     "Enable clipboard history",                                       "The clipboard history is accessible with 'WIN + V', it can be very useful."),
                new(true, true,     "Disable Cortana",                                                null),
                new(true, true,     "Disable third-party suggestions",                                null),
                new(true, true,     "Disable safe search",                                            null),
                new(true, true,     "Create small memory dump file on crash",                         null),
                new(true, true,     "Remove dynamic user home directory links",                       "By default, the documents, video and image folders are linked to each other, this can lead to some unexpected behavior, like copying the contents of all folders when only copying the documents folder."),
                new(true, true,     "Activate explorer compact mode",                                 "This is most of the time the default."),
                new(true, true,     "Remove old 'Open PowerShell here' entry from context menu",      null),
                new(true, true,     "Explorer don't pretty path",                                     "By default, Explorer changes the case of filenames so that a file that is named c:\\iLikeTOTypeTHiS appears as c:\\Iliketotypethis."),
                new(true, true,     "Show file operations details",                                   "Shows information like total files, remaining time and speed when copying or moving files."),
                new(true, true,     "Hide Taskbar widgets",                                           null),
                new(true, true,     "Disable lock screen tips and tricks",                            null),
                new(true, true,     "Remove 'AutoLogger' file and restricting directory",             null),
                new(true, true,     "Deactivate Windows widgets",                                     "Deactivates the advertising / widgets panel, which can be opened by pressing 'Win + W'."),
                new(true, true,     "Deactivate system ads / suggestions & auto app installation",    null),
                new(true, true,     "Deactivate search bar web search",                               null),
                new(true, true,     "Start menu Settings, Network and Explorer integration",          "Adds small icons to the bottom left of the Start menu in Windows 11 / bottom right in Windows 10."),
                new(true, true,     "Deactivate StickyKeys",                                          null),
                new(true, true,     "Reduce system & UI delays",                                      null),
            };

            Selector selection = new($"UX_ButtonHandler module {moduleVersion}", "\\DialogueWindow\\Icons\\imageres_114.ico", "General settings", "For more information on some points, read the tooltip.", fields);

            selection.ShowDialog();

            if ((Boolean)selection.Was_Canceled)
            {
                PrepareExit();

                return;
            }

            //

            if (!Global.VerboseHashCheck(Resource_Assets.SetACL_PathName, Resource_Assets.SetACL_Hash)[0])
            {
                MainWindow.LogBoxAdd($" - UX_ButtonHandler module {moduleVersion}\n", StayInLine: true);

                Dialogue warn = new($"WinUtil: UX_ButtonHandler module {moduleVersion}",
                        $"\"{Resource_Assets.SetACL_PathName}\" was not found or is invalid,\nskip \"{fields[18].Fieldname}\"?",
                        Dialogue.Icons.Circle_Error,
                        "Continue",
                        "Cancel");

                warn.ShowDialog();

                if (warn.Result == 0)
                {
                    selection.Result[18] = false;
                }
                else
                {
                    PrepareExit();

                    return;
                }
            }

            //

            MainWindow.LogBoxAdd("Applying general settings", Brushes.LightBlue);

            await Task.Run(() =>
            {
                try
                {
                    if (selection.Result.Length != 30)
                    {
                        throw new ArgumentException("unexpected user length\n");
                    }

                    #region Act
                    if (selection.Result[0])
                    {
                        LogBox.Add("Deactivating fast startup");
                        xRegistry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord);
                    }

                    if (selection.Result[1])
                    {
                        LogBox.Add("Showing file extensions");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord);
                    }

                    if (selection.Result[2])
                    {
                        LogBox.Add("Setting default explorer page to \"This PC\"");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[3])
                    {
                        LogBox.Add("Activating Explorer process separation");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SeparateProcess", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[4])
                    {
                        LogBox.Add("Enabling NTFS long paths");
                        xRegistry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[5])
                    {
                        LogBox.Add("Setting Desktop wallpaper quality to 100%");
                        xRegistry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);
                    }

                    if (selection.Result[6])
                    {
                        LogBox.Add("Deactivating local security questions");
                        xRegistry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "NoLocalPasswordResetQuestions", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[7])
                    {
                        LogBox.Add("Showing encrypted / compressed NTFS attributes");
                        xRegistry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowEncryptCompressedColor", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[8])
                    {
                        LogBox.Add("Changing network adapter behavior");
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WcmSvc\Local", "fMinimizeConnections", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Wcmsvc", "IgnoreNonRoutableEthernet", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[9])
                    {
                        LogBox.Add("Enabling NumLock on startup");
                        xRegistry.SetValue("HKEY_USERS\\.DEFAULT\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 2, RegistryValueKind.DWord);
                    }

                    if (selection.Result[10])
                    {
                        LogBox.Add("Disabling Windows Update auto reboot");
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[11])
                    {
                        LogBox.Add("Enabling clipboard history");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Clipboard", "EnableClipboardHistory", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[12])
                    {
                        LogBox.Add("Disabling Cortana");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);
                    }

                    if (selection.Result[13])
                    {
                        LogBox.Add("Disabling third-party suggestions");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\CloudContent", "DisableThirdPartySuggestions", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[14])
                    {
                        LogBox.Add("Disabling safe search");
                        xRegistry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\SearchSettings", "SafeSearchMode", 0, RegistryValueKind.DWord);
                    }

                    if (selection.Result[15])
                    {
                        LogBox.Add("Set create small memory dump file on crash");
                        xRegistry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\CrashControl", "CrashDumpEnabled", 3, RegistryValueKind.DWord);
                    }

                    if (selection.Result[16])
                    {
                        LogBox.Add("Remove dynamic user home directory links");
                        xPowershell.Run("Get-ChildItem -Path \"C:\\Users\\" + Machine.UserPath + "\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \"ReparsePoint\" } | remove-item -force -ErrorAction SilentlyContinue");
                        xPowershell.Run("Get-ChildItem -Path \"C:\\Users\\" + Machine.UserPath + "\\documents\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \"ReparsePoint\" } | remove-item -force -ErrorAction SilentlyContinue");
                    }

                    if (selection.Result[17])
                    {
                        LogBox.Add("Activating explorer compact mode");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[18])
                    {
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
                    }

                    if (selection.Result[19])
                    {
                        LogBox.Add("Explorer don't pretty path");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DontPrettyPath", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[20])
                    {
                        LogBox.Add("Showing file operations details");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager", "EnthusiastMode", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[21])
                    {
                        LogBox.Add("Hiding Taskbar widgets");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 2, RegistryValueKind.DWord);
                    }

                    if (selection.Result[22])
                    {
                        LogBox.Add("Disabling lock screen tips and tricks");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\Subscriptions\338387", "SubscriptionContext", "sc-mode=0", RegistryValueKind.String);
                    }

                    if (selection.Result[23])
                    {
                        LogBox.Add("Removing 'AutoLogger' file and restricting directory");
                        File.Delete(@"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl");
                        xProcess.Run(@"icacls.exe", @"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\Autologger /deny SYSTEM:(OI)(CI)F", waitForExit: true, hiddenExecute: true);
                    }

                    if (selection.Result[24])
                    {
                        LogBox.Add("Deactivate Windows widgets");
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{2cc5ca98-6485-489a-920e-b3e88a6ccce3}", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarOpenOnHover", 0, RegistryValueKind.DWord);
                    }

                    if (selection.Result[25])
                    {
                        LogBox.Add("Deactivating system ads / suggestions & auto app installation");

                        xRegistry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord);

                        xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClient\" /Disable", waitForExit: true, hiddenExecute: true);
                        xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClientOnScenarioDownload\" /Disable", waitForExit: true, hiddenExecute: true);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "ContentDeliveryAllowed", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "FeatureManagementEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "OemPreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEverEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContentEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);

                        using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true);
                        foreach (String subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.Contains("$lockscreenpinnedtiles$windows.data.curatedtilecollection.tilecollection"))
                            {
                                Byte[] Bytes = new Byte[] { 0x02, 0x00, 0x00, 0x00, 0x56, 0x51, 0x4C, 0x29, 0xDC, 0x12, 0xD9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0x0A, 0x0A, 0x00, 0xCA, 0x32, 0x00, 0xCC, 0x83, 0x12, 0x03, 0x26, 0x7B, 0x00, 0x32, 0x00, 0x43, 0x00, 0x46, 0x00, 0x43, 0x00, 0x44, 0x00, 0x46, 0x00, 0x46, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x45, 0x00, 0x37, 0x00, 0x31, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x38, 0x00, 0x45, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x39, 0x00, 0x45, 0x00, 0x32, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x33, 0x00, 0x38, 0x00, 0x44, 0x00, 0x31, 0x00, 0x35, 0x00, 0x36, 0x00, 0x36, 0x00, 0x46, 0x00, 0x30, 0x00, 0x37, 0x00, 0x31, 0x00, 0x7D, 0x00, 0x26, 0x7B, 0x00, 0x44, 0x00, 0x31, 0x00, 0x38, 0x00, 0x43, 0x00, 0x45, 0x00, 0x34, 0x00, 0x43, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x43, 0x00, 0x38, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x43, 0x00, 0x31, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x41, 0x00, 0x41, 0x00, 0x36, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x32, 0x00, 0x33, 0x00, 0x33, 0x00, 0x46, 0x00, 0x46, 0x00, 0x30, 0x00, 0x44, 0x00, 0x32, 0x00, 0x41, 0x00, 0x35, 0x00, 0x43, 0x00, 0x39, 0x00, 0x7D, 0x00, 0x26, 0x7B, 0x00, 0x45, 0x00, 0x44, 0x00, 0x38, 0x00, 0x35, 0x00, 0x42, 0x00, 0x36, 0x00, 0x43, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x33, 0x00, 0x39, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x31, 0x00, 0x38, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x36, 0x00, 0x41, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x46, 0x00, 0x39, 0x00, 0x39, 0x00, 0x36, 0x00, 0x44, 0x00, 0x45, 0x00, 0x38, 0x00, 0x31, 0x00, 0x37, 0x00, 0x42, 0x00, 0x39, 0x00, 0x37, 0x00, 0x7D, 0x00, 0x00, 0xCD, 0x0A, 0x12, 0x0A, 0x03, 0x26, 0x7B, 0x00, 0x32, 0x00, 0x43, 0x00, 0x46, 0x00, 0x43, 0x00, 0x44, 0x00, 0x46, 0x00, 0x46, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x45, 0x00, 0x37, 0x00, 0x31, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x38, 0x00, 0x45, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x39, 0x00, 0x45, 0x00, 0x32, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x33, 0x00, 0x38, 0x00, 0x44, 0x00, 0x31, 0x00, 0x35, 0x00, 0x36, 0x00, 0x36, 0x00, 0x46, 0x00, 0x30, 0x00, 0x37, 0x00, 0x31, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xF3, 0xBF, 0xF3, 0xE7, 0x02, 0x24, 0x9C, 0xCE, 0x03, 0x44, 0xE4, 0x91, 0x01, 0x66, 0x9E, 0xD7, 0x8C, 0xEC, 0xD8, 0xC2, 0x99, 0xF8, 0x71, 0x00, 0xD2, 0x0A, 0x52, 0x50, 0x00, 0x7E, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x6D, 0x00, 0x75, 0x00, 0x6E, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x61, 0x00, 0x70, 0x00, 0x70, 0x00, 0x73, 0x00, 0x5F, 0x00, 0x38, 0x00, 0x77, 0x00, 0x65, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x62, 0x00, 0x33, 0x00, 0x64, 0x00, 0x38, 0x00, 0x62, 0x00, 0x62, 0x00, 0x77, 0x00, 0x65, 0x00, 0x21, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x6C, 0x00, 0x69, 0x00, 0x76, 0x00, 0x65, 0x00, 0x2E, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x61, 0x00, 0x72, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x32, 0x00, 0x00, 0x26, 0x7B, 0x00, 0x44, 0x00, 0x31, 0x00, 0x38, 0x00, 0x43, 0x00, 0x45, 0x00, 0x34, 0x00, 0x43, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x43, 0x00, 0x38, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x43, 0x00, 0x31, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x41, 0x00, 0x41, 0x00, 0x36, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x32, 0x00, 0x33, 0x00, 0x33, 0x00, 0x46, 0x00, 0x46, 0x00, 0x30, 0x00, 0x44, 0x00, 0x32, 0x00, 0x41, 0x00, 0x35, 0x00, 0x43, 0x00, 0x39, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xCA, 0xC9, 0xB3, 0x8C, 0x0D, 0x24, 0x8E, 0x19, 0x44, 0x9A, 0x98, 0x01, 0x66, 0xAA, 0xDD, 0x8D, 0xF9, 0x83, 0xDE, 0xF4, 0xD2, 0xC9, 0x01, 0x00, 0xD2, 0x0A, 0x4E, 0x50, 0x00, 0x7E, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x6D, 0x00, 0x75, 0x00, 0x6E, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x61, 0x00, 0x70, 0x00, 0x70, 0x00, 0x73, 0x00, 0x5F, 0x00, 0x38, 0x00, 0x77, 0x00, 0x65, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x62, 0x00, 0x33, 0x00, 0x64, 0x00, 0x38, 0x00, 0x62, 0x00, 0x62, 0x00, 0x77, 0x00, 0x65, 0x00, 0x21, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x6C, 0x00, 0x69, 0x00, 0x76, 0x00, 0x65, 0x00, 0x2E, 0x00, 0x6D, 0x00, 0x61, 0x00, 0x69, 0x00, 0x6C, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x31, 0x00, 0x00, 0x26, 0x7B, 0x00, 0x45, 0x00, 0x44, 0x00, 0x38, 0x00, 0x35, 0x00, 0x42, 0x00, 0x36, 0x00, 0x43, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x33, 0x00, 0x39, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x31, 0x00, 0x38, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x36, 0x00, 0x41, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x46, 0x00, 0x39, 0x00, 0x39, 0x00, 0x36, 0x00, 0x44, 0x00, 0x45, 0x00, 0x38, 0x00, 0x31, 0x00, 0x37, 0x00, 0x42, 0x00, 0x39, 0x00, 0x37, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xCB, 0xED, 0x96, 0xEC, 0x0E, 0x24, 0x94, 0xA7, 0x01, 0x44, 0x83, 0x83, 0x01, 0x66, 0x86, 0xD9, 0xE6, 0xB7, 0xE9, 0xBB, 0xE0, 0xBD, 0x97, 0x01, 0x00, 0xD2, 0x0A, 0x26, 0x50, 0x00, 0x7E, 0x00, 0x4D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x53, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x70, 0x00, 0x65, 0x00, 0x41, 0x00, 0x70, 0x00, 0x70, 0x00, 0x5F, 0x00, 0x6B, 0x00, 0x7A, 0x00, 0x66, 0x00, 0x38, 0x00, 0x71, 0x00, 0x78, 0x00, 0x66, 0x00, 0x33, 0x00, 0x38, 0x00, 0x7A, 0x00, 0x67, 0x00, 0x35, 0x00, 0x63, 0x00, 0x21, 0x00, 0x41, 0x00, 0x70, 0x00, 0x70, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x33, 0x00, 0x00, 0x00 };

                                xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", Bytes, RegistryValueKind.Binary);

                                break;
                            }
                        }
                    }

                    if (selection.Result[26])
                    {
                        LogBox.Add("Deactivating search bar web search");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1, RegistryValueKind.DWord);
                    }

                    if (selection.Result[27])
                    {
                        LogBox.Add("Setting Start menu Settings, Network and Explorer integration");
                        Byte[] V = new Byte[] { 0x44, 0x81, 0x75, 0xFE, 0x0D, 0x08, 0xAE, 0x42, 0x8B, 0xDA, 0x34, 0xED, 0x97, 0xB6, 0x63, 0x94, 0xBC, 0x24, 0x8A, 0x14, 0x0C, 0xD6, 0x89, 0x42, 0xA0, 0x80, 0x6E, 0xD9, 0xBB, 0xA2, 0x48, 0x82, 0x86, 0x08, 0x73, 0x52, 0xAA, 0x51, 0x43, 0x42, 0x9F, 0x7B, 0x27, 0x76, 0x58, 0x46, 0x59, 0xD4 };
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Start", "VisiblePlaces", V, RegistryValueKind.Binary);

                        using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true);
                        foreach (String subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.Contains("$windows.data.unifiedtile.startglobalproperties"))
                            {
                                Byte[] Bytes = new Byte[] { 0x02, 0x00, 0x00, 0x00, 0xAA, 0x00, 0x9F, 0xEC, 0x94, 0x2E, 0xDA, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0xCB, 0x32, 0x0A, 0x03, 0x05, 0x86, 0x91, 0xCC, 0x93, 0x05, 0x24, 0xAA, 0xA3, 0x01, 0x44, 0xC3, 0x84, 0x01, 0x66, 0x9F, 0xF7, 0x9D, 0xB1, 0x87, 0xCB, 0xD1, 0xAC, 0xD4, 0x01, 0x00, 0x05, 0xC4, 0x82, 0xD6, 0xF3, 0x0F, 0x24, 0x8D, 0x10, 0x44, 0xAE, 0x85, 0x01, 0x66, 0x8B, 0xB5, 0xD3, 0xE9, 0xFE, 0xD2, 0xED, 0xB1, 0x94, 0x01, 0x00, 0x05, 0xBC, 0xC9, 0xA8, 0xA4, 0x01, 0x24, 0x8C, 0xAC, 0x03, 0x44, 0x89, 0x85, 0x01, 0x66, 0xA0, 0x81, 0xBA, 0xCB, 0xBD, 0xD7, 0xA8, 0xA4, 0x82, 0x01, 0x00, 0xC2, 0x3C, 0x01, 0xC2, 0x46, 0x01, 0xC5, 0x5A, 0x01, 0x00 };

                                xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", Bytes, RegistryValueKind.Binary);

                                break;
                            }
                        }
                    }

                    if (selection.Result[28])
                    {
                        LogBox.Add("Deactivating StickyKeys");
                        xRegistry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "Sound on Activation", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "Warning Sounds", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility\\Keyboard Response", "Flags", "102", RegistryValueKind.String);
                        xRegistry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility\\StickyKeys", "Flags", "26", RegistryValueKind.String);
                        xRegistry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility\\ToggleKeys", "Flags", "38", RegistryValueKind.String);
                    }

                    if (selection.Result[29])
                    {
                        LogBox.Add("System performance tweaks");
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WaitToKillAppTimeout", 5000, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\Ndu", "Start", 4, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", "IRPStackSize", 20, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 0xffffffff, RegistryValueKind.DWord);
                    }
                    #endregion

                    LogBox.Add("Restarting explorer to apply changes");

                    Common.RestartExplorer().Wait();

                    LogBox.Add("Done\n", Brushes.MediumSeaGreen);
                }
                catch (Exception ex)
                {
                    LogBox.Add(ex.Message, Brushes.Red);
                }

            }).ConfigureAwait(true);

            //

            PrepareExit();

            void PrepareExit()
            {
                MainWindow.DeactivateWorker();

                UX_Button.IsEnabled = true;
                UX_Button.Opacity = 1.0d;
            }
        }

        private void HyperKey_UnReg(object sender, RoutedEventArgs e)
        {
            HyperKey_Button.IsEnabled = false;
            HyperKey_Button.Opacity = 0.41d;

            MainWindow.ActivateWorker();

            String moduleVersion = "v1";

            //

            if (!Global.VerboseHashCheck(Resource_Assets.HyperKey_UnReg_PathName, Resource_Assets.HyperKey_UnReg_Hash)[0])
            {
                MainWindow.LogBoxAdd($" - HyperKey_UnReg module {moduleVersion}\n", StayInLine: true);

                PrepareExit();

                return;
            }

            //

            Dialogue info = new("WinUtil: Hyper keys",
                        "This will automatically deregister all\n" +
                        "Office Hyper keys, Teams-Windows integration\n" +
                        "and the Windows Help panel on every user login.\n\n" +

                        "The following combinations will be deregister:\n\n" +
                        "Office: Ctrl + Shift + Win + Alt + W, T, Y, O, P, D, L, X, N, <Space>\n" +
                        "Office-Entry: Ctrl + Shift + Win + Alt\n" +
                        "Windows Help: F1\n" +
                        "Teams: Win + C",
                        Dialogue.Icons.Gear,
                        "Continue",
                        "Cancel", 0);

            info.ShowDialog();

            if (info.Result == 1)
            {
                PrepareExit();

                return;
            }

            //

            MainWindow.LogBoxAdd("Setting up automatic Hyper key deregistration", Brushes.LightBlue);

            Task.Run(() =>
            {
                try
                {
                    LogBox.Add("Copying file to \"C:\\Program Files\\WinUtil\\HyperKey_UnReg.exe\"");

                    String std = "C:\\Program Files\\WinUtil\\HyperKey_UnReg.exe";

                    if (!Directory.Exists("C:\\Program Files\\WinUtil"))
                    {
                        Directory.CreateDirectory("C:\\Program Files\\WinUtil");
                    }

                    File.Copy(Resource_Assets.HyperKey_UnReg_PathName, std, true);

                    LogBox.Add("Adding program to global user init\n(HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\\Userinit)");

                    String GlobalUserInitString = xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "Userinit", RegistryValueKind.String);

                    if (GlobalUserInitString.Contains($"\"{std}\""))
                    {
                        LogBox.Add(" - ", StayInLine: true);
                        LogBox.Add("already present", Brushes.LimeGreen, StayInLine: true);
                    }
                    else
                    {
                        xRegistry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", "Userinit", $"{GlobalUserInitString}, \"{std}\",", RegistryValueKind.String);
                    }

                    LogBox.Add("Deregistering Teams and Office Hyper keys");
                    xProcess.Run(std, waitForExit: true);

                    LogBox.Add("Done\n", Brushes.MediumSeaGreen);
                }
                catch (Exception ex)
                {
                    LogBox.Add(ex.Message + "\n", Brushes.Red);
                }

            }).ConfigureAwait(true);

            PrepareExit();

            //

            void PrepareExit()
            {
                MainWindow.DeactivateWorker();

                HyperKey_Button.IsEnabled = true;
                HyperKey_Button.Opacity = 1.0d;
            }
        }

        private async void GameDVR_GameBar(object sender, RoutedEventArgs e)
        {
            GameDVR_Button.IsEnabled = false;
            GameDVR_Button.Opacity = 0.41d;

            MainWindow.ActivateWorker();

            MainWindow.LogBoxAdd("Deactivating GameDVR and GameBar\n", Brushes.LightBlue, null);

            try
            {
                await Task.Run(() =>
                {
                    xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);

                    xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord);
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                LogBox.Add(ex.Message + "\n", Brushes.Red);
            }

            MainWindow.DeactivateWorker();

            GameDVR_Button.IsEnabled = true;
            GameDVR_Button.Opacity = 1.0d;
        }

        private async void Boot_Policy(object sender, RoutedEventArgs e)
        {
            BootPolicy_Button.IsEnabled = false;
            BootPolicy_Button.Opacity = 0.41d;

            MainWindow.ActivateWorker();

            Dialogue Info = new("WinUtil: Boot policy editor",
                       "Setting the Windows boot policy to 'legacy' enables you\n" +
                       "to press F8 during startup, this allows you to access the old recovery menu.",
                       Dialogue.Icons.Gear,
                       "Default",
                       "Cancel",
                       "Legacy");

            Info.ShowDialog();

            if (Info.Result == 1)
            {
                PrepareExit();

                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    if (Info.Result == 0)
                    {
                        LogBox.Add("Setting Boot Policy to 'Default'", Brushes.LightBlue, null);
                        xProcess.Run("bcdedit.exe", "/set {current} bootmenupolicy Standard", waitForExit: false, hiddenExecute: true);
                    }
                    else
                    {
                        LogBox.Add("Setting Boot Policy to 'Legacy'", Brushes.LightBlue, null);
                        xProcess.Run("bcdedit.exe", "/set {current} bootmenupolicy Legacy", waitForExit: false, hiddenExecute: true);
                    }

                    LogBox.Add("Done\n", Brushes.MediumSeaGreen);

                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                LogBox.Add(ex.Message + "\n", Brushes.Red);
            }

            PrepareExit();

            //

            void PrepareExit()
            {
                MainWindow.DeactivateWorker();

                BootPolicy_Button.IsEnabled = true;
                BootPolicy_Button.Opacity = 1.0d;
            }
        }

        private void BackGroundAppsToggle(object sender, RoutedEventArgs e)
        {
            Boolean state = (Boolean)!BackGroundApps_ToggleButton.IsChecked;

            if (state)
            {
                MainWindow.LogBoxAdd("Enabling Background Apps\n", Brushes.LightBlue);
                xRegistry.DeleteValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsRunInBackground");
            }
            else
            {
                MainWindow.LogBoxAdd("Disabling Background Apps\n", Brushes.LightBlue);
                xRegistry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsRunInBackground", 2, RegistryValueKind.DWord);
            }
        }

        private async void Windows_Update(object sender, RoutedEventArgs e)
        {
            WindowsUpdate_Button.IsEnabled = false;
            WindowsUpdate_Button.Opacity = 0.41d;

            MainWindow.ActivateWorker();

            WS_Update wnd = new();
            wnd.ShowDialog();

            if (wnd.Choice == WS_Update.Button.Cancel)
            {
                PrepareExit();

                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    switch (wnd.Choice)
                    {
                        case WS_Update.Button.NoDrivers:
                            NoDrivers();
                            break;
                        case WS_Update.Button.Security_Only:
                            Security_Only();
                            break;
                        case WS_Update.Button.No_Updates:
                            No_Updates();
                            break;
                        case WS_Update.Button.Reset:
                            Reset();
                            break;
                    }

                    static void NoDrivers()
                    {
                        LogBox.Add("Excluding Drivers In Quality Updates", Brushes.LightBlue);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);
                    }

                    static void Security_Only()
                    {
                        LogBox.Add("Setting Windows Update to \"Security Updates only\"", Brushes.LightBlue);

                        LogBox.Add("Disabling driver offering through Windows Update");
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Device Metadata", "PreventDeviceMetadataFromNetwork", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DontPromptForWindowsUpdate", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DontSearchWindowsUpdate", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DriverUpdateWizardWuSearchEnabled", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);

                        LogBox.Add("Setting update policy");
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX_ButtonHandler\Settings", "BranchReadinessLevel", 16, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX_ButtonHandler\Settings", "DeferFeatureUpdatesPeriodInDays", 365, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX_ButtonHandler\Settings", "DeferQualityUpdatesPeriodInDays", 2, RegistryValueKind.DWord);
                    }

                    static void No_Updates()
                    {
                        LogBox.Add("Deactivating Windows Update", Brushes.LightBlue);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 1, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0, RegistryValueKind.DWord);

                        try { xService.SetStartupType("BITS", ServiceStartMode.Disabled); } catch { }
                        try { xService.SetStartupType("wuauserv", ServiceStartMode.Disabled); } catch { }

                        try { xService.Stop("BITS"); } catch { }
                        try { xService.Stop("wuauserv"); } catch { }
                    }

                    static void Reset()
                    {
                        LogBox.Add("Resetting Windows Update services", Brushes.LightBlue);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 0, RegistryValueKind.DWord);
                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 3, RegistryValueKind.DWord);

                        xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0, RegistryValueKind.DWord);

                        xService.SetStartupType("BITS", ServiceStartMode.Automatic);
                        xService.SetStartupType("wuauserv", ServiceStartMode.Automatic);

                        LogBox.Add("Enabling driver offering through Windows Update");
                        xRegistry.DeleteValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\Device Metadata", "PreventDeviceMetadataFromNetwork");

                        xRegistry.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DriverSearching", new String[] { "DontPromptForWindowsUpdate", "DontSearchWindowsUpdate", "DriverUpdateWizardWuSearchEnabled" });

                        xRegistry.DeleteValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate", "ExcludeWUDriversInQualityUpdate");

                        LogBox.Add("Enabling Windows Update automatic restart");
                        xRegistry.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", new String[] { "NoAutoRebootWithLoggedOnUsers", "AUPowerManagement" });


                        LogBox.Add("Enabled driver offering through Windows Update");
                        xRegistry.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\WindowsUpdate\\UX_ButtonHandler\\Settings", new String[] { "BranchReadinessLevel", "DeferFeatureUpdatesPeriodInDays", "DeferQualityUpdatesPeriodInDays" });

                        LogBox.Add("Stopping Windows Update Services");
                        xProcess.Run("net.exe", "stop \"BITS\" /y", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("net.exe", "stop \"wuauserv\" /y", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("net.exe", "stop \"appidsvc\" /y", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("net.exe", "stop \"cryptsvc\" /y", hiddenExecute: true, waitForExit: true);

                        LogBox.Add("Removing QMGR Data file");
                        try
                        {
                            Directory.Delete(@"C:\ProgramData\Application Data\Microsoft\Network\Downloader", true);
                        }
                        catch (System.IO.DirectoryNotFoundException) { }

                        LogBox.Add("Flushing DNS");
                        xProcess.Run("ipconfig.exe", "/flushdns", hiddenExecute: true, waitForExit: true);

                        LogBox.Add("Removing old Windows Update log");
                        File.Delete("C:\\Windows\\WindowsUpdate.log");

                        LogBox.Add("Resetting the Windows Update Services to default settings");
                        xProcess.Run("sc.exe", "sdset bits D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("sc.exe", "wuauserv D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", hiddenExecute: true, waitForExit: true);

                        LogBox.Add("Reregistering *some* DLLs (BITfiles + Windows Update)");
                        xProcess.Run("regsvr32.exe", "/s atl.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s urlmon.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s mshtml.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s shdocvw.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s browseui.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s jscript.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s vbscript.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s scrrun.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s msxml.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s msxml3.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s msxml6.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s actxprxy.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s softpub.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wintrust.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s dssenh.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s rsaenh.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s gpkcsp.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s sccbase.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s slbcsp.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s cryptdlg.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s oleaut32.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s ole32.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s shell32.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s initpki.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wuapi.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wuaueng.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wuaueng1.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wucltui.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wups.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wups2.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wuweb.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s qmgr.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s qmgrprxy.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wucltux.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s muweb.dll", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("regsvr32.exe", "/s wuwebv.dll", hiddenExecute: true, waitForExit: true);

                        LogBox.Add("Removing WSUS (Windows Server) client settings");
                        xRegistry.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate", new String[] { "AccountDomainSid", "PingID", "SusClientId" });

                        LogBox.Add("Resetting the WinSock");
                        xProcess.Run("netsh.exe", "winsock reset", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("netsh.exe", "winhttp reset proxy", hiddenExecute: true, waitForExit: true);

                        LogBox.Add("Delete all BITS jobs");
                        xPowershell.Run("Get-BitsTransfer | Remove-BitsTransfer");

                        LogBox.Add("Attempting to install the Windows Update Agent");
                        if (Environment.Is64BitOperatingSystem)
                        {
                            xProcess.Run("wusa.exe", "Windows8-RT-KB2937636-x64 /quiet", hiddenExecute: true, waitForExit: true);
                        }
                        else
                        {
                            xProcess.Run("wusa.exe", "Windows8-RT-KB2937636-x86 /quiet", hiddenExecute: true, waitForExit: true);
                        }

                        LogBox.Add("Reseting Windows Update policies");
                        xRegistry.DeleteSubKeyTree("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "WindowsUpdate");
                        xRegistry.DeleteSubKeyTree("HKEY_CURRENT_USER\\SOFTWARE\\Policies\\Microsoft\\Windows", "WindowsUpdate");

                        xRegistry.DeleteSubKeyTree("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", "WindowsUpdate");
                        xRegistry.DeleteSubKeyTree("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", "WindowsUpdate");

                        LogBox.Add("Starting Windows Update Services");
                        xProcess.Run("net.exe", "start \"BITS\" /y", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("net.exe", "start \"wuauserv\" /y", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("net.exe", "start \"appidsvc\" /y", hiddenExecute: true, waitForExit: true);
                        xProcess.Run("net.exe", "start \"cryptsvc\" /y", hiddenExecute: true, waitForExit: true);

                        LogBox.Add("Forcing discovery");
                        xProcess.Run("wuauclt.exe", "/resetauthorization /detectnow", hiddenExecute: true, waitForExit: true);

                        LogBox.Add("Restart your computer to complete", Brushes.Yellow, null);
                    }

                    LogBox.Add("Done\n", Brushes.MediumSeaGreen);
                }
                catch (Exception ex)
                {
                    LogBox.Add(ex.Message + "\n", Brushes.Red);
                }

            }).ConfigureAwait(true);

            PrepareExit();

            //

            void PrepareExit()
            {
                MainWindow.DeactivateWorker();

                WindowsUpdate_Button.IsEnabled = true;
                WindowsUpdate_Button.Opacity = 1.0d;
            }
        }
    }
}