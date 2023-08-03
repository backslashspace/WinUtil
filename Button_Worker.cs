using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Windows.Forms;
using ManagedNativeWifi;
using System.Windows.Media;
using System.Threading.Tasks;
//libs
using CustomWinMessageBox;
using ProgramLauncher;
using static PowershellHelper.PowershellHelper;
using RegistryTools;
using ServiceTools;
using WinUser;

namespace WinUtil_Main
{
    internal class Button_Worker : MainWindow
    {
        public String[] getfileagrs = { null };

        ///<summary>Activates Windows.</summary>
        ///<remarks>Uses the official MAS<br/>- HWID for Client<br/>- Protected KMS38 for Server</remarks>
        public void ActivateWindows()
        {
            if (ActivationClicks > 0)
            {
                ActivationClicks = 0;

                var result0 = System.Windows.Forms.MessageBox.Show(
                "Re-Check Windows license status?",
                "WMI",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

                if (result0 == System.Windows.Forms.DialogResult.No)
                {
                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                    ThreadIsAlive.ActivateWindows = false;
                    return;
                }

                try
                {
                    DispatchedLogBoxAdd("Re-Check Windows license status, this might take a while\n\n", Brushes.Yellow, null);

                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", new String[] { "Windows Activation Status" });

                    

                    PowerShell("$test = Get-CimInstance SoftwareLicensingProduct -Filter \"Name like 'Windows%'\" | where { $_.PartialProductKey } | select LicenseStatus; $test = $test -replace \"@{LicenseStatus=\"; $test = $test -replace \"}\"; reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil\" /v \"Windows Activation Status\" /t reg_dword /d $test /f");

                    DispatchedLogBoxAdd("[Info] Updated license information\n\n", Brushes.Cyan, null);
                }
                catch
                {
                    DispatchedLogBoxAdd("Error [F-L:50]: Couldn't get license information\n\n", Brushes.Red, null);
                }

                ThreadIsAlive.ActivateWindows = false;
                return;
            }

            Boolean Wait()
            {
                DispatchedLogBoxAdd("Waiting for backgroundtask to finish\n\n", Brushes.Yellow);

                Int16 I = 0;

                while (true)
                {
                    if (I < 99)
                    {
                        break;
                    }

                    switch (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Windows Activation Status", RegistryValueKind.DWord, false))
                    {
                        case -1:
                            ActivationClicks++;
                            RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", new String[] { "Windows Activation Status" });
                            DispatchedLogBoxAdd("Couldn't get license information, Click Activate again\n\n", Brushes.Yellow, null);
                            return true;

                        case null:
                            Task.Delay(450).Wait();

                            break;
                        default:
                            return false;
                    }

                    I++;
                }

                ActivationClicks++;
                RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", new String[] { "Windows Activation Status" });
                DispatchedLogBoxAdd("Couldn't get license information, Click Activate again\n\n", Brushes.Yellow);
                return true;
            }

            void Activate(Int32 Status)
            {
                if (ThisMachine.OSType is OSType.Server)
                {
                    DispatchedLogBoxAdd("Activating Server (P-KMS38)..\n", Brushes.DarkGreen);
                    //[iqudhiwuo)T/&/(RR]
                    //InternExtract("temp\\", "KMS38_Activation.cmd", "embeded_resources.mas.KMS38_Activation.cmd");

                    Execute.EXE("cmd.exe", "/c \"temp\\KMS38_Activation.cmd\"", true, true);
                    File.Delete("temp\\KMS38_Activation.cmd");
                }
                else
                {
                    DispatchedLogBoxAdd("Activating Client (HWID)..\n", Brushes.DarkGreen);

                    //InternExtract("temp\\", "HWID_Activation.cmd", "embeded_resources.mas.HWID_Activation.cmd");
                    Execute.EXE("cmd.exe", "/c \"temp\\HWID_Activation.cmd\"", true, true);
                    File.Delete("temp\\HWID_Activation.cmd");

                    if (!InternetIsAvailable())
                    {
                        DispatchedLogBoxAdd("Connect to Internet to finalize Activation\n", Brushes.DarkCyan);
                    }
                }

                DispatchedLogBoxAdd(Status + " --> 1\n", Brushes.DarkGreen);

                DispatchedLogBoxAdd("Activated Windows\n\n", Brushes.DarkCyan);
            }

            //#############################################

            Boolean B = true;

            while (B)
            {
                var License = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Windows Activation Status", RegistryValueKind.DWord, true);

                switch (License)
                {
                    case 1:
                        DispatchedLogBoxAdd("Windows is already licensed\n\n", Brushes.DarkCyan, null);
                        ActivationClicks++;
                        B = false;
                        break;

                    case null:
                        if (Wait())
                        {
                            ThreadIsAlive.ActivateWindows = false;
                            return;
                        }
                        else
                        {
                            continue;
                        }

                    default:
                        Activate(License);
                        B = false;
                        break;
                }
            }

            ThreadIsAlive.ActivateWindows = false;
        }

        public void General()
        {
            try
            {
                DispatchedLogBoxAdd("Running general settings\n", Brushes.DarkGreen, null);

                Execute.EXE("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Activating Windows Defender Sandbox\n", Brushes.Gray, null);
                Execute.EXE("cmd.exe", "/c \"setx /M MP_FORCE_USE_SANDBOX 1\"", true, false);

                DispatchedLogBoxAdd("Setting VeraCrypt as Trusted Process\n", Brushes.Gray, null);
                PowerShell("Add-MpPreference -ExclusionProcess \"C:\\Program Files\\VeraCrypt\\VeraCrypt.exe\"");

                DispatchedLogBoxAdd("Properly handle Ethernet connections (IgnoreNonRoutableEthernet=1)\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Wcmsvc", "IgnoreNonRoutableEthernet", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Preventing Wi-Fi disconnecting when Ethernet\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WcmSvc\Local", "fMinimizeConnections", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Showing encrypted or compressed NTFS files in color\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowEncryptCompressedColor", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Deactivating Local Security Questions\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "NoLocalPasswordResetQuestions", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Enabling NumLock on startup\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 2, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling auto reboot (while users are logged in)\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Enabling NTFSLongpaths\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Setting Desktop-background-quality to 100%\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Setting automatic crash dump creation to 3 (small memory dump file)\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\CrashControl", "CrashDumpEnabled", 3, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling SafeSearchMode\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\SearchSettings", "SafeSearchMode", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling HibernationBoot\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Enabling clipboard history\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Clipboard", "EnableClipboardHistory", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Cortana\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling thirdParty suggestions\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\CloudContent", "DisableThirdPartySuggestions", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Showing filename extensions\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Setting default explorer page to \"This PC\"\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Enabling Explorer Process separation\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SeparateProcess", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Hiding Taskbar Icons\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord);

                if (ThisMachine.UIVersion is WindowsUIVersion.Windows10)
                {
                    DispatchedLogBoxAdd("Opening Explorer Ribbon\n", Brushes.Gray, null);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOn", 0, RegistryValueKind.DWord);

                    //

                    Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 1, RegistryValueKind.DWord);
                }
                else
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord);
                }

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 2, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling lockscreen tips and tricks\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "ContentDeliveryAllowed", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\Subscriptions\338387", "SubscriptionContext", "sc-mode=0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Lock Screen", "SlideshowEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 1, RegistryValueKind.DWord);
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

                //deactivated dû´^é to windows bug
                //DispatchedLogBoxAdd("Setting NTP Server to pool.ntp.org\n", Brushes.Gray, null);
                //Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DateTime\Servers", "", "0", RegistryValueKind.String);
                //Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DateTime\Servers", "0", "pool.ntp.org", RegistryValueKind.String);

                DispatchedLogBoxAdd("Deactivating Explorer Compact Mode\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", 1, RegistryValueKind.DWord);

                RegistryIO.DeleteSubKeyTree("HKEY_CLASSES_ROOT\\Directory", new String[] { "Background\\shell\\Powershell", "shell\\Powershell" });

                DispatchedLogBoxAdd("Removing DynLinks\n", Brushes.Gray, null);

                PowerShell("Get-ChildItem -Path \"C:\\Users\\" + UserPath + "\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \"ReparsePoint\" } | remove-item -force -ErrorAction SilentlyContinue");
                PowerShell("Get-ChildItem -Path \"C:\\Users\\" + UserPath + "\\documents\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \"ReparsePoint\" } | remove-item -force -ErrorAction SilentlyContinue");

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DontPrettyPath", 1, RegistryValueKind.DWord);

                try
                {
                    if (Int32.Parse(RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuild", RegistryValueKind.String, false)) <= 22557)
                    {
                        DispatchedLogBoxAdd("Showing task manager details\n", Brushes.Gray, null);
                        Execute.EXE("cmd.exe", "/c \"Taskmgr.exe\n", true, false);
                        do
                        {
                            Task.Delay(128).Wait();
                        }
                        while ((Byte[])Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\TaskManager", "Preferences", null) == null);
                        Execute.EXE("taskkill.exe", "/f /im Taskmgr.exe", true, true);
                        Byte[] temp = (Byte[])Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\TaskManager", "Preferences", null);
                        temp[28] = 0;
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\TaskManager", "Preferences", temp, RegistryValueKind.Binary);
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Task Manager patch not run in builds 22557+ due to bug / updated UI\n", Brushes.Gray, null);
                    }
                }
                catch
                {
                    DispatchedLogBoxAdd("Task Manager patch not run in builds 22557+ due to bug / updated UI\n", Brushes.Gray, null);
                }

                DispatchedLogBoxAdd("Showing file operations details\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager", "EnthusiastMode", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Setting various services to manual..\n", Brushes.Gray, null);
                String[] services = {
                "diagnosticshub.standardcollector.service",
                "DiagTrack",
                "DPS",
                "edgeupdate",
                "edgeupdatem",
                "LMS",
                "NPSMSvc_3acd1",
                "RstMwService",
                "AESMService",
                //"VMAuthdService",
                //"VMUSBArbService",
                "RemoteRegistry",
                "DusmSvc",
                "uhssvc",
                "ClickToRunSvc",
                "TrkWks",
                "WMPNetworkSvc",
                "WSearch",
                "XblAuthManager",
                "XblGameSave",
                "XboxNetApiSvc",
                "XboxGipSvc",
                "WerSvc",
                "fhsvc",
                "gupdate",
                "gupdatem",
                "stisvc",
                "MSDTC",
                "WpcMonSvc",
                "PcaSvc",
                "WPDBusEnum",
                "SysMain",
                "StorSvc",
                "lmhosts",
                "wisvc",
                "FontCache",
                "RetailDemo",
                "ALG",
                "Browser",
                "edgeupdate",
                "MicrosoftEdgeElevationService",
                "edgeupdatem",
                "cbdhsvc_48486de",
                "QWAVE",
                "HPAppHelperCap",
                "HPDiagsCap",
                "HPNetworkCap",
                "HPSysInfoCap",
                "HpTouchpopointAnalyticsService",
                "HvHost",
                "vmickvpexchange",
                "vmicguestinterface",
                "vmicshutdown",
                "vmicheartbeat",
                "vmicvmsession",
                "vmicrdv",
                "vmictimesync"};
                for (Int32 i = 0; i < services.Length; i++)
                {
                    DispatchedLogBoxAdd(services[i] + "\n", Brushes.DarkGray, null);
                    Service.SetStartupType(services[i], ServiceStartMode.Manual);
                }

                //will break msstore
                //var deactivateMSSTORE = new ServiceController("StorSvc");
                //SetServiceStartType.ChangeStartMode(deactivateMSSTORE, ServiceStartMode.Manual);

                DispatchedLogBoxAdd("*Various performance tweaks*\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 10, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 10, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control", "WaitToKillServiceTimeout", 2000, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WaitToKillAppTimeout", 5000, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WaitToKillServiceTimeout", 2000, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\Ndu", "Start", 4, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", "IRPStackSize", 20, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Removing AutoLogger file and restricting directory\n", Brushes.Gray, null);
                File.Delete(@"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl");
                Execute.EXE(@"icacls.exe", @"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\Autologger /deny SYSTEM:(OI)(CI)F", true, false);

                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.General = false;
        }

        public void DeacTelemetry()
        {
            try
            {
                DispatchedLogBoxAdd("Deactivating telemetry\n", Brushes.DarkGreen, null);

                //get tasksearchbar mode (O&O resets it)
                Int32 TBSB = RegistryIO.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", RegistryValueKind.DWord, false);

                DispatchedLogBoxAdd("Running ShutUp10\n", Brushes.Gray, null);
                if (!(VerboseHashCheck("temp\\" + Const.su10exe, Const.su10exeHash)[0]) || !(VerboseHashCheck("temp\\" + Const.su10settings, Const.su10settingsHash)[0]))
                {//[iqudhiwuo)T/&/(RR]
                    //InternExtract("temp\\", Const.su10exe, "embeded_resources.shutup10." + Const.su10exe);
                    //InternExtract("temp\\", Const.su10settings, "embeded_resources.shutup10." + Const.su10settings);
                }
                Execute.EXE("temp\\" + Const.su10exe, "temp\\" + Const.su10settings + " /nosrp /quiet", true, true);

                DispatchedLogBoxAdd("Deactivating GameDVR\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Storage Sense\n", Brushes.Gray, null);
                Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", false);

                DispatchedLogBoxAdd("Disabling Microsoft Geolocation service\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny", RegistryValueKind.String);

                DispatchedLogBoxAdd("Disabling system telemetry\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /Disable", true, false);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\ProgramDataUpdater\" /Disable", true, false);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Autochk\\Proxy\" /Disable", true, false);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Disable", true, false);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /Disable", true, false);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\DiskDiagnostic\\Microsoft-Windows-DiskDiagnosticDataCollector\" /Disable", true, false);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Application suggestions\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "OemPreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEverEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353698Enabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Feedback\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord);
                Execute.EXE("reg.exe", "add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v \"DoNotShowFeedbackNotifications\" /t reg_dword /d 1 /f", true, false);

                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClient\" /Disable", true, false);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClientOnScenarioDownload\" /Disable", true, false);

                DispatchedLogBoxAdd("Disabling Tailored Experiences\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableTailoredExperiencesWithDiagnosticData", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Advertising ID\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Tailored Experiences\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, RegistryValueKind.DWord);
                Execute.EXE("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Windows Error Reporting\\QueueReporting\" /Disable", true, false);

                DispatchedLogBoxAdd("Stopping and disabling Diagnostics Tracking Service\n", Brushes.Gray, null);
                Service.SetStartupType("DiagTrack", ServiceStartMode.Disabled);

                try
                {
                    DispatchedLogBoxAdd("Stopping and disabling Intel ME service\n", Brushes.Gray, null);
                    Service.SetStartupType("WMIRegistrationService", ServiceStartMode.Disabled);
                }
                catch { }

                DispatchedLogBoxAdd("Stopping and disabling Superfetch service\n", Brushes.Gray, null);
                Service.SetStartupType("SysMain", ServiceStartMode.Disabled);

                DispatchedLogBoxAdd("Disabling News and Interests\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Wi-Fi Sense\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting", "Value", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots", "Value", 0, RegistryValueKind.DWord);

                //restore
                if (TBSB != -1)
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", TBSB, RegistryValueKind.DWord);
                }

                if (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Blocked Telemetry IPs", RegistryValueKind.DWord, false) == 0 || RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Blocked Telemetry IPs", RegistryValueKind.DWord, false) == null)      //suiedzhfgbwuehfbzub
                {
                    DispatchedLogBoxAdd("Excluding 'hosts' file from Windows Defender\n", Brushes.DarkGray, null);
                    PowerShell("Add-MpPreference -ExclusionPath C:\\Windows\\System32\\drivers\\etc\\hosts");

                    Task.Delay(3200).Wait();

                    DispatchedLogBoxAdd("Adding blacklisted Domains..\n", Brushes.DarkGray, null);

                    using (StreamWriter sw = File.AppendText("C:\\Windows\\System32\\drivers\\etc\\hosts"))
                    {
                        sw.WriteLine(
                         "\n127.0.0.1 a-0001.a-msedge.net"
                         + "\n127.0.0.1 a.ads1.msn.com"
                         + "\n127.0.0.1 a.ads2.msn.com"
                         + "\n127.0.0.1 ad.doubleclick.net"
                         + "\n127.0.0.1 adnexus.net"
                         + "\n127.0.0.1 adnxs.com"
                         + "\n127.0.0.1 ads.msn.com"
                         + "\n127.0.0.1 ads1.msads.net"
                         + "\n127.0.0.1 ads1.msn.com"
                         + "\n127.0.0.1 az361816.vo.msecnd.net"
                         + "\n127.0.0.1 dmd.metaservices.microsoft.com"
                         + "\n127.0.0.1 ca.telemetry.microsoft.com"
                         + "\n127.0.0.1 eu-mobile.events.data.microsoft.com"
                         + "\n127.0.0.1 cache.datamart.windows.com"
                         + "\n127.0.0.1 choice.microsoft.com"
                         + "\n127.0.0.1 statics-marketingsites-neu-ms-com.akamaized.net"
                         + "\n127.0.0.1 choice.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 choice.microsoft.com.nstac.net"
                         + "\n127.0.0.1 compatexchange.cloudapp.net"
                         + "\n127.0.0.1 corp.sts.microsoft.com"
                         + "\n127.0.0.1 corpext.msitadfs.glbdns2.microsoft.com"
                         + "\n127.0.0.1 cs1.wpc.v0cdn.net"
                         + "\n127.0.0.1 db3wns2011111.wns.windows.com"
                         + "\n127.0.0.1 df.telemetry.microsoft.com"
                         + "\n127.0.0.1 diagnostics.support.microsoft.com"
                         + "\n127.0.0.1 fe2.update.microsoft.com.akadns.net"
                         + "\n127.0.0.1 fe3.delivery.dsp.mp.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 feedback.microsoft-hohm.com"
                         + "\n127.0.0.1 feedback.search.microsoft.com"
                         + "\n127.0.0.1 feedback.windows.com"
                         + "\n127.0.0.1 i1.services.social.microsoft.com"
                         + "\n127.0.0.1 i1.services.social.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 msnbot-207-46-194-33.search.msn.com"
                         + "\n127.0.0.1 oca.telemetry.microsoft.com"
                         + "\n127.0.0.1 oca.telemetry.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 pre.footprintpredict.com"
                         + "\n127.0.0.1 preview.msn.com"
                         + "\n127.0.0.1 rad.msn.com"
                         + "\n127.0.0.1 redir.metaservices.microsoft.com"
                         + "\n127.0.0.1 reports.wes.df.telemetry.microsoft.com"
                         + "\n127.0.0.1 s0.2mdn.net"
                         + "\n127.0.0.1 services.wes.df.telemetry.microsoft.com"
                         + "\n127.0.0.1 settings-sandbox.data.microsoft.com"
                         + "\n127.0.0.1 settings-win.data.microsoft.com"
                         + "\n127.0.0.1 settings.data.microsof.com"
                         + "\n127.0.0.1 sls.update.microsoft.com.akadns.net"
                         + "\n127.0.0.1 spynet2.microsoft.com"
                         + "\n127.0.0.1 spynetalt.microsoft.com"
                         + "\n127.0.0.1 sqm.df.telemetry.microsoft.com"
                         + "\n127.0.0.1 sqm.telemetry.microsoft.com"
                         + "\n127.0.0.1 sqm.telemetry.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 ssw.live.com"
                         + "\n127.0.0.1 statsfe1.ws.microsoft.com"
                         + "\n127.0.0.1 statsfe2.update.microsoft.com.akadns.net"
                         + "\n127.0.0.1 statsfe2.ws.microsoft.com"
                         + "\n127.0.0.1 survey.watson.microsoft.com"
                         + "\n127.0.0.1 telecommand.telemetry.microsoft.com"
                         + "\n127.0.0.1 telecommand.telemetry.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 telemetry.appex.bing.net"
                         + "\n127.0.0.1 telemetry.microsoft.com"
                         + "\n127.0.0.1 telemetry.urs.microsoft.com"
                         + "\n127.0.0.1 v10.vortex-win.data.microsoft.com"
                         + "\n127.0.0.1 view.atdmt.com"
                         + "\n127.0.0.1 vortex-sandbox.data.microsoft.com"
                         + "\n127.0.0.1 vortex-win.data.microsoft.com"
                         + "\n127.0.0.1 vortex.data.microsoft.com"
                         + "\n127.0.0.1 watson.live.com"
                         + "\n127.0.0.1 watson.microsoft.com"
                         + "\n127.0.0.1 watson.ppe.telemetry.microsoft.com"
                         + "\n127.0.0.1 watson.telemetry.microsoft.com"
                         + "\n127.0.0.1 watson.telemetry.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 wes.df.telemetry.microsoft.com"
                         + "\n127.0.0.1 win10.ipv6.microsoft.com"
                         + "\n127.0.0.1 www-google-analytics.l.google.com"
                         + "\n127.0.0.1 www.google-analytics.com"
                         + "\n127.0.0.1 a.ads2.msads.net"
                         + "\n127.0.0.1 a.rad.msn.com"
                         + "\n127.0.0.1 ac3.msn.com"
                         + "\n127.0.0.1 aidps.atdmt.com"
                         + "\n127.0.0.1 aka-cdn-ns.adtech.de"
                         + "\n127.0.0.1 apps.skype.com"
                         + "\n127.0.0.1 b.ads1.msn.com"
                         + "\n127.0.0.1 b.ads2.msads.net"
                         + "\n127.0.0.1 b.rad.msn.com"
                         + "\n127.0.0.1 bs.serving-sys.com"
                         + "\n127.0.0.1 c.atdmt.com"
                         + "\n127.0.0.1 c.msn.com"
                         + "\n127.0.0.1 cdn.atdmt.com"
                         + "\n127.0.0.1 cds26.ams9.msecn.net"
                         + "\n127.0.0.1 cy2.vortex.data.microsoft.com.akadns.net"
                         + "\n127.0.0.1 db3aqu.atdmt.com"
                         + "\n127.0.0.1 ec.atdmt.com"
                         + "\n127.0.0.1 flex.msn.com"
                         + "\n127.0.0.1 g.msn.com"
                         + "\n127.0.0.1 h1.msn.com"
                         + "\n127.0.0.1 h2.msn.com"
                         + "\n127.0.0.1 live.rads.msn.com"
                         + "\n127.0.0.1 m.adnxs.com"
                         + "\n127.0.0.1 modern.watson.data.microsoft.com.akadns.net"
                         + "\n127.0.0.1 msftncsi.com"
                         + "\n127.0.0.1 msntest.serving-sys.com"
                         + "\n127.0.0.1 pricelist.skype.com"
                         + "\n127.0.0.1 rad.live.com"
                         + "\n127.0.0.1 s.gateway.messenger.live.com"
                         + "\n127.0.0.1 secure.adnxs.com"
                         + "\n127.0.0.1 secure.flashtalking.com"
                         + "\n127.0.0.1 sO.2mdn.net"
                         + "\n127.0.0.1 static.2mdn.net"
                         + "\n127.0.0.1 telemetry.appex.bing.net443"
                         + "\n127.0.0.1 ui.skype.com"
                         + "\n127.0.0.1 www.msftncsi.com"
                         + "\n127.0.0.1 cc-api-data.adobe.io"
                         + "\n127.0.0.1 ic.adobe.io"
                         + "\n127.0.0.1 www.microsoft.com"
                         + "\n127.0.0.1 microsoft.com"
                         + "\n127.0.0.1 wns.notify.windows.com.akadns.net"
                         + "\n127.0.0.1 v10-win.vortex.data.microsoft.com.akadns.net"
                         + "\n127.0.0.1 us.vortex-win.data.microsoft.com"
                         + "\n127.0.0.1 us-v10.events.data.microsoft.com"
                         + "\n127.0.0.1 urs.microsoft.com.nsatc.net"
                         + "\n127.0.0.1 vsgallery.com"
                         + "\n127.0.0.1 telemetry.remoteapp.windowsazure.com\n");

                        sw.Close();
                        sw.Dispose();
                    }

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "Blocked Telemetry IPs", 1, RegistryValueKind.DWord);
                }
                else
                {
                    DispatchedLogBoxAdd("Already added blacklisted Domains. Skipping.\n", Brushes.Yellow, null);
                }

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.N0Telemetry = false;
        }

        public void DarkMode()
        {
            DispatchedLogBoxAdd("Setting system to darkmode\n", Brushes.DarkGreen, null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, RegistryValueKind.DWord);
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.SystemTheme = false;
        }

        public void Backgroundappsno()
        {
            DispatchedLogBoxAdd("Disabling Background Apps\n", Brushes.DarkGreen, null);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsRunInBackground", 2, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.Backgroundapps = false;
        }

        public void Backgroundappsyes()
        {
            DispatchedLogBoxAdd("Enabling Background Apps\n", Brushes.DarkGreen, null);

            RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", new String[] { "LetAppsRunInBackground" });

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.Backgroundapps = false;
        }

        public void LightMode()
        {
            DispatchedLogBoxAdd("Setting system to lightmode\n", Brushes.DarkGreen, null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1, RegistryValueKind.DWord);
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.SystemTheme = false;
        }

        public void Restart_Explorer()
        {
            DispatchedLogBoxAdd("Restarting Explorer\n", Brushes.DarkGreen, null);

            Execute.EXE("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, false);

            Task.Delay(1000).Wait();

            Process.Start("explorer.exe");

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.Restart_Explorer = false;
        }

        public void BetterWT()
        {
            try
            {
                if (ThisMachine.UIVersion is WindowsUIVersion.Windows10)
                {
                    Boolean IsInstalled = false;

                    String[] Folders = Directory.EnumerateDirectories("C:\\Program Files\\WindowsApps").ToArray();

                    for (Int32 i = 0; i < Folders.Length; i++)
                    {
                        if (Folders[i].Contains("WindowsTerminal"))
                        {
                            IsInstalled = true;
                        }
                    }

                    if (!IsInstalled)
                    {
                        DispatchedLogBoxAdd("Installing Windows Terminal\n", Brushes.DarkGreen, null);

                        if (VerboseHashCheck("assets\\" + Const.VCLibsName, Const.VCLibs)[0] && VerboseHashCheck("assets\\Windows Terminal\\" + Const.WTName, Const.WT)[0] && VerboseHashCheck("assets\\Windows Terminal\\" + Const.WTLicenseName, Const.WTLicenseHash)[0])
                        {
                            PowerShell("Add-AppxPackage -Path \"assets\\" + Const.VCLibsName + "\"");

                            PowerShell("Add-ProvisionedAppPackage -Online -PackagePath \"assets\\Windows Terminal\\" + Const.WTName + "\" -LicensePath \"assets\\Windows Terminal\\" + Const.WTLicenseName + "\"");

                            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                        }
                        else
                        {
                            ThreadIsAlive.WT = false;

                            DispatchedLogBoxAdd("\nError\n\n", Brushes.Yellow, null);
                        }
                    }
                }

                DispatchedLogBoxAdd("Integrating Windows Terminal into extended Context menus\n", Brushes.DarkGreen, null);

                if (!(VerboseHashCheck("temp\\SetACL.exe", Const.ACLHash)[0]))
                {//[iqudhiwuo)T/&/(RR]
                    //InternExtract("temp\\", "SetACL.exe", "embeded_resources.SetACL.exe");
                }

                String temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:" + AdminGroupName + "\" -rec Yes";
                Execute.EXE("temp\\SetACL.exe", temp, true, true);

                temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Powershell\" -ot reg -actn ace -ace \"n:" + AdminGroupName + ";p:full\" -rec Yes";
                Execute.EXE("temp\\SetACL.exe", temp, true, true);

                temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:" + AdminGroupName + "\" -rec Yes";
                Execute.EXE("temp\\SetACL.exe", temp, true, true);

                temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\shell\\Powershell\" -ot reg -actn ace -ace \"n:" + AdminGroupName + ";p:full\" -rec Yes";
                Execute.EXE("temp\\SetACL.exe", temp, true, true);

                RegistryIO.DeleteSubKeyTree("HKEY_CLASSES_ROOT\\Directory", new String[] { "Background\\shell\\Powershell", "shell\\Powershell" });

                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "Extended", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);

                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "Extended", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "HasLUAShield", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "MUIVerb", "Open in Windows Terminal (Admin)", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);


                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "Extended", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "MUIVerb", "Open in Windows Terminal", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere\command", "", "wt.exe -d \"%V\"", RegistryValueKind.String);

                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "Extended", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "HasLUAShield", "", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "Icon", "imageres.dll,-5323", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "MUIVerb", "Open in Windows Terminal (Admin)", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin\command", "", "cmd /c start /min powershell.exe -WindowStyle Hidden Start-Process -Verb RunAs wt.exe -ArgumentList @('-d', '\"\"\"%V\"\"\"')", RegistryValueKind.String);

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", "{9F156763-7844-4DC4-B2B1-901F640F5155}", "", RegistryValueKind.String);

                Execute.EXE("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done, use with Shift + Right-Click\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.WT = false;
        }

        public void NormalWT()
        {
            try
            {
                DispatchedLogBoxAdd("Resetting Windows Terminal integration\n", Brushes.DarkGreen, null);

                RegistryIO.DeleteSubKeyTree("HKEY_CLASSES_ROOT\\Directory\\Background\\shell", new String[] { "Powershell", "OpenWTHere", "OpenWTHereAsAdmin" });
                RegistryIO.DeleteSubKeyTree("HKEY_CLASSES_ROOT\\Directory\\shell", new String[] { "Powershell", "OpenWTHere", "OpenWTHereAsAdmin" });

                RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Blocked", new String[] { "{9F156763-7844-4DC4-B2B1-901F640F5155}" });

                Execute.EXE("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.WT = false;
        }

        public void BlockTCP()
        {
            try
            {
                MessageBoxManager.Yes = "Block both";
                MessageBoxManager.No = "Only VMRDP";
                MessageBoxManager.Register();

                var result0 = System.Windows.Forms.MessageBox.Show(
                                "Blocking WSDAPI will prevent devices in the network from finding this machine via Network Discovery.",
                                "Firewall",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Information);

                MessageBoxManager.Unregister();

                if (result0 == System.Windows.Forms.DialogResult.Yes)
                {
                    DispatchedLogBoxAdd("Blocking external TCP access to port 2179 & 5357 (VMRDP and WSDAPI)\n", Brushes.DarkGreen, null);
                    PowerShell("New-NetFirewallRule -DisplayName \"Block VMRDP & WSDAPI\" -Direction Inbound -LocalPort 2179,5357 -Protocol TCP -Action Block -Description \"Blocks external access of TCP 5357 and 2179, created by WinUtil.\"");
                }
                else if (result0 == System.Windows.Forms.DialogResult.No)
                {
                    DispatchedLogBoxAdd("Blocking external TCP access to port 2179 (VMRDP)\n", Brushes.DarkGreen, null);
                    PowerShell("New-NetFirewallRule -DisplayName \"Block VMRDP\" -Direction Inbound -LocalPort 2179 -Protocol TCP -Action Block -Description \"Blocks external access of TCP 2179, created by WinUtil.\"");
                }
                else
                {
                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                    ThreadIsAlive.BlockTCP = false;
                    return;
                }

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.BlockTCP = false;
        }

        public void ClearTaskBar()
        {
            DispatchedLogBoxAdd("Clearing Taskbar\n", Brushes.DarkGreen, null);

            Execute.EXE("cmd.exe", "/c \"DEL /F /S /Q /A \"%AppData%\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar\\*\"\"", true, false);
            RegistryIO.DeleteSubKeyTree(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", new String[] { "Taskband" });

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0, RegistryValueKind.DWord);

            Execute.EXE("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
            Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.ClearTaskBar = false;
        }

        public void Nano()
        {
            try
            {
                if (File.Exists("C:\\Program Files\\Nano\\nano.exe") && File.Exists("C:\\Program Files\\Nano\\libncursesw6.dll"))
                {
                    DispatchedLogBoxAdd("Nano already installed\n\n", Brushes.DarkCyan, null);

                    ThreadIsAlive.Nano = false;
                    return;
                }
                else
                {
                    if (VerboseHashCheck("assets\\Nano.zip", Const.Nano)[0])
                    {
                        if (!(VerboseHashCheck("temp\\" + "7z.dll", Const.zip7dll)[0]) || !(VerboseHashCheck("temp\\" + "7z.exe", Const.zip7exe)[0]))
                        {//[iqudhiwuo)T/&/(RR]
                            //InternExtract("temp\\", "7z.dll", "embeded_resources._7zip.7z.dll");
                            //InternExtract("temp\\", "7z.exe", "embeded_resources._7zip.7z.exe");
                        }

                        DispatchedLogBoxAdd("Installing Nano..\n", Brushes.DarkGreen, null);

                        Execute.EXE("temp\\7z.exe", "x \"assets\\Nano.zip\" -o\"C:\\Program Files\\Nano\" -y", true, true);

                        Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) + ";C:\\Program Files\\Nano;", EnvironmentVariableTarget.Machine);

                        DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

                        ThreadIsAlive.Nano = false;
                        return;
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                    }

                    ThreadIsAlive.Nano = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }
        }

        public void NotepadPlusPlus()
        {
            try
            {
                if (File.Exists("C:\\Program Files\\Notepad++\\notepad++.exe"))
                {
                    DispatchedLogBoxAdd("Notepad++ already installed\n\n", Brushes.DarkCyan, null);

                    ThreadIsAlive.WGetAction = false;
                    return;
                }
                else
                {
                    if (WinGetIsInstalled(true) == 1)
                    {
                        if (InternetIsAvailable())
                        {
                            DispatchedLogBoxAdd("Installing Notepad++ (via WinGet)..\n", Brushes.DarkGreen, null);

                            Execute.EXE("winget.exe", "install notepad++ --accept-package-agreements --accept-source-agreements --scope machine", true, true);

                            if (File.Exists("C:\\Program Files\\Notepad++\\notepad++.exe"))
                            {
                                var result0 = System.Windows.Forms.MessageBox.Show(
                                "Associate the following file types with Notepad++?\n*.json *.config *.conf *.cfg *.txt",
                                "Notepad--",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);

                                if (result0 == System.Windows.Forms.DialogResult.Yes)
                                {
                                    DispatchedLogBoxAdd(".config\n", Brushes.DarkGray, null);
                                    Execute.EXE("cmd.exe", "/c assoc .config=configfile", false, true);
                                    Execute.EXE("cmd.exe", "/c ftype configfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);

                                    DispatchedLogBoxAdd(".txt\n", Brushes.DarkGray, null);
                                    Execute.EXE("cmd.exe", "/c assoc .txt=txtfile", false, true);
                                    Execute.EXE("cmd.exe", "/c ftype txtfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);
                                    Execute.EXE("cmd.exe", "/c assoc .txt=txtfile", false, true);

                                    DispatchedLogBoxAdd(".conf\n", Brushes.DarkGray, null);
                                    Execute.EXE("cmd.exe", "/c assoc .conf=conffile", false, true);
                                    Execute.EXE("cmd.exe", "/c ftype conffile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);

                                    DispatchedLogBoxAdd(".cfg\n", Brushes.DarkGray, null);
                                    Execute.EXE("cmd.exe", "/c assoc .cfg=cfgfile", false, true);
                                    Execute.EXE("cmd.exe", "/c ftype cfgfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);

                                    DispatchedLogBoxAdd(".json\n", Brushes.DarkGray, null);
                                    Execute.EXE("cmd.exe", "/c assoc .json=jsonfile", false, true);
                                    Execute.EXE("cmd.exe", "/c ftype jsonfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);
                                }

                                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

                                ThreadIsAlive.WGetAction = false;
                                return;
                            }
                            else
                            {
                                if (!InternetIsAvailable())
                                {
                                    DispatchedLogBoxAdd("Finished with errors, lost connection?\n\n", Brushes.Red, null);

                                    ThreadIsAlive.WGetAction = false;
                                    return;
                                }
                                else
                                {
                                    DispatchedLogBoxAdd("Error\n\n", Brushes.Red, null);

                                    ThreadIsAlive.WGetAction = false;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            DispatchedLogBoxAdd("No internet connection detected\n\n", Brushes.Red, null);

                            ThreadIsAlive.WGetAction = false;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.WGetAction = false;
        }

        public void RemWebView()
        {
            try
            {
                if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\EdgeWebView"))
                {
                    DispatchedLogBoxAdd("Removing msedgewebview2\n", Brushes.DarkGreen, null);

                    Execute.EXE("taskkill.exe", "/f /im msedgewebview2.exe", true, true);

                    Task.Delay(1000).Wait();

                    Execute.EXE("taskkill.exe", "/f /im msedgewebview2.exe", true, true);

                    try
                    {
                        Execute.EXE("taskkill.exe", "/f /im msedgewebview2.exe", true, true);
                        Execute.EXE(Directory.GetDirectories(@"C:\Program Files (x86)\Microsoft\EdgeWebView\Application")[0] + @"\Installer\setup.exe", "--uninstall --msedgewebview --system-level --verbose-logging", true, true);

                        if (File.Exists(Directory.GetDirectories(@"C:\Program Files (x86)\Microsoft\EdgeWebView\Application")[0] + @"\Installer\setup.exe"))
                        {
                            throw new Exception("to new");
                        }
                    }
                    catch
                    {
                        static void StopEdgeWeb()
                        {
                            while (true)
                            {
                                Execute.EXE("taskkill.exe", "/f /im msedgewebview2.exe", true, true);
                            }
                        }

                        Thread thread = new(() => StopEdgeWeb());
                        thread.Start();

                        DispatchedLogBoxAdd("Program setup file missing or incompatible, manual deinstallation\n", Brushes.Yellow, null);
                        DispatchedLogBoxAdd("Removing program files\n", Brushes.Gray, null);
                        Execute.EXE("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\EdgeWebView\"", true, true);
                        Execute.EXE("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\EdgeWebView\" /E /g " + AdminGroupName + ":f", true, true);

                        try
                        {
                            Directory.Delete("C:\\Program Files (x86)\\Microsoft\\EdgeWebView", true);
                        }
                        catch
                        {
                            if (Directory.EnumerateDirectories(Directory.GetDirectories(@"C:\Program Files (x86)\Microsoft\EdgeWebView\Application")[0]).ToArray().Length != 1)
                            {
                                throw new Exception("error removing files");
                            }
                        }

                        thread.Abort();

                        DispatchedLogBoxAdd("Removing from Windows-App list\n", Brushes.Gray, null);
                        try
                        {
                            using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft", true);
                            key.DeleteSubKeyTree("EdgeWebView");
                            key.Close();
                        }
                        catch { }
                    }

                    DispatchedLogBoxAdd("Deactivating auto installation*\n", Brushes.Gray, null);
                    //Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\EdgeUpdate", "Update{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\EdgeUpdate", "Install{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}", 0, RegistryValueKind.DWord);

                    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                }
                else
                {
                    DispatchedLogBoxAdd("No EdgeWebView installation found, skipping\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.WebView = false;
        }

        public void Debloat()
        {
            try
            {
                DispatchedLogBoxAdd("Removing Bloatware\n", Brushes.DarkGreen, null);

                String[] Bloatware = new String[] {
                "Microsoft.3DBuilder"
                , "Microsoft.PowerAutomateDesktop"
                , "Microsoft.YourPhone"
                , "MicrosoftCorporationII.MicrosoftFamily"
                , "MicrosoftCorporationII.QuickAssist"
                , "Clipchamp.Clipchamp"
                , "MicrosoftTeams"
                , "Microsoft.AppConnector"
                , "Microsoft.BingFinance"
                , "Microsoft.BingNews"
                , "Microsoft.BingSports"
                , "Microsoft.BingTranslator"
                , "Microsoft.BingFoodAndDrink"
                , "Microsoft.BingHealthAndFitness"
                , "Microsoft.BingTravel"
                , "Microsoft.MinecraftUWP"
                , "Microsoft.GamingServices"
                , "Microsoft.GetHelp"
                , "Microsoft.Getstarted"
                , "Microsoft.Messaging"
                , "Microsoft.MicrosoftSolitaireCollection"
                , "Microsoft.NetworkSpeedTest"
                , "Microsoft.News"
                , "Microsoft.Office.Lens"
                , "Microsoft.Office.Sway"
                , "Microsoft.Office.OneNote"
                , "Microsoft.OneConnect"
                , "Microsoft.People"
                , "Microsoft.Todos"
                , "Microsoft.SkypeApp"
                , "Microsoft.Wallet"
                , "Microsoft.Whiteboard"
                , "Microsoft.WindowsAlarms"
                , "Microsoft.windowscommunicationsapps"
                , "Microsoft.WindowsFeedbackHub"
                , "Microsoft.WindowsMaps"
                , "Microsoft.WindowsSoundRecorder"
                , "Microsoft.ConnectivityStore"
                , "Microsoft.CommsPhone"
                , "Microsoft.MixedReality.Portal"
                , "Microsoft.Getstarted"
                , "Microsoft.MicrosoftOfficeHub"
                , "EclipseManager"
                , "ActiproSoftwareLLC"
                , "AdobeSystemsIncorporated.AdobePhotoshopExpress"
                , "Duolingo-LearnLanguagesforFree"
                , "PandoraMediaInc"
                , "CandyCrush"
                , "BubbleWitch3Saga"
                , "Wunderlist"
                , "Flipboard"
                , "Twitter"
                , "Facebook"
                , "Royal Revolt"
                , "Sway"
                , "Speed Test"
                , "Dolby"
                , "Viber"
                , "ACGMediaPlayer"
                , "Netflix"
                , "OneCalendar"
                , "LinkedInforWindows"
                , "HiddenCityMysteryofShadows"
                , "Hulu"
                , "HiddenCity"
                , "AdobePhotoshopExpress"
                , "HotspotShieldFreeVPN"
                , "Microsoft.Advertising.Xaml"};

                DispatchedLogBoxAdd("Removing UWP bloat..\n", Brushes.Gray, null);

                for (Int32 i = 0; i < Bloatware.Length; i++)
                {
                    DispatchedLogBoxAdd("Removing " + Bloatware[i] + " | ", Brushes.DarkGray, null);
                    try
                    {
                        PowerShell("Get-AppxPackage -Allusers -Name " + Bloatware[i] + " | Remove-AppxPackage -Allusers");

                        DispatchedLogBoxAdd("×\n", Brushes.Green, null);
                    }
                    catch (Exception)
                    {
                        DispatchedLogBoxAdd("×\n", Brushes.Red, null);
                    }
                }

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }
            ThreadIsAlive.Debloat = false;
        }

        public void RemoveXbox()
        {
            try
            {
                DispatchedLogBoxAdd("Removing XBox bloatware\n", Brushes.DarkGreen, null);

                String[] Bloatware = new String[] {
                    "Microsoft.XboxApp"
                    , "Microsoft.XboxIdentityProvider"
                    , "Microsoft.Xbox.TCUI"
                    , "Microsoft.XboxGamingOverlay"
                    , "Microsoft.GamingApp"
                    , "Microsoft.XboxGameOverlay"
                    , "Microsoft.XboxSpeechToTextOverlay"};

                for (Int32 i = 0; i < Bloatware.Length; i++)
                {
                    DispatchedLogBoxAdd("Removing " + Bloatware[i] + " | ", Brushes.DarkGray, null);
                    try
                    {
                        PowerShell("Get-AppxPackage -Allusers -Name " + Bloatware[i] + " | Remove-AppxPackage -Allusers");

                        DispatchedLogBoxAdd("×\n", Brushes.Green, null);
                    }
                    catch (Exception)
                    {
                        DispatchedLogBoxAdd("×\n", Brushes.Red, null);
                    }
                }

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.RemoveXbox = false;
        }

        public void RMOneDrive()
        {
            try
            {
                if (File.Exists(@"C:\Users\" + UserPath + @"\AppData\Local\Microsoft\OneDrive\OneDrive.exe") || File.Exists("C:\\Windows\\SysWOW64\\OneDriveSetup.exe") || File.Exists("C:\\Windows\\System32\\OneDriveSetup.exe"))
                {

                    DispatchedLogBoxAdd("Removing + Disabling OneDrive\n", Brushes.DarkGreen, null);

                    Execute.EXE("taskkill.exe", "/f /im OneDriveSetup.exe", true, true);
                    Execute.EXE("taskkill.exe", "/f /im OneDrive.exe", true, true);

                    if (File.Exists("C:\\Windows\\System32\\OneDriveSetup.exe"))
                    {
                        Execute.EXE("C:\\Windows\\System32\\OneDriveSetup.exe", "/uninstall", true, true);
                    }

                    if (File.Exists(@"C:\Windows\SysWOW64\OneDriveSetup.exe"))
                    {
                        Execute.EXE(@"C:\Windows\SysWOW64\OneDriveSetup.exe", "/uninstall", true, true);
                    }

                    Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);

                    Execute.EXE("takeown.exe", "/f \"C:\\Users\\" + UserPath + "\\OneDrive\"", true, true);
                    Execute.EXE("cacls.exe", "\"C:\\Users\\" + UserPath + "\\OneDrive\" /E /g " + AdminGroupName + ":f", true, true);
                    Execute.EXE("powershell.exe", "\"Remove-Item \"C:\\Users\\" + UserPath + "\\OneDrive\" -Recurse -Force\"", true, true);

                    Directory.Delete(@"C:\Users\" + UserPath + @"\AppData\Local\Microsoft\OneDrive", true);
                    Directory.Delete(@"C:\ProgramData\Microsoft OneDrive", true);

                    Execute.EXE("powershell.exe", "\"Remove-Item \"C:\\OneDriveTemp\" -Recurse -Force\"", true, true);

                    RegistryIO.DeleteSubKeyTree(@"HKEY_CLASSES_ROOT\CLSID", new String[] { "{018D5C66-4533-4307-9B53-224DE2ED1FE6}" });
                    RegistryIO.DeleteSubKeyTree(@"HKEY_CLASSES_ROOT\Wow6432Node\CLSID", new String[] { "{018D5C66-4533-4307-9B53-224DE2ED1FE6}" });

                    Execute.EXE("takeown.exe", "/f \"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\"", true, true);
                    Execute.EXE("cacls.exe", "\"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\" /E /g " + AdminGroupName + ":f", true, true);
                    Execute.EXE("takeown.exe", "/f \"C:\\Windows\\System32\\OneDriveSetup.exe\"", true, true);
                    Execute.EXE("cacls.exe", "\"C:\\Windows\\System32\\OneDriveSetup.exe\" /E /g " + AdminGroupName + ":f", true, true);
                    Execute.EXE("powershell.exe", "\"Remove-Item \"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\" -Recurse -Force\"", true, true);
                    Execute.EXE("powershell.exe", "\"Remove-Item \"C:\\Windows\\System32\\OneDriveSetup.exe\" -Recurse -Force\"", true, true);

                    foreach (String s in Directory.GetDirectories("C:\\Windows\\WinSxS"))
                    {
                        if (s.Contains("microsoft-windows-onedrive-setup"))
                        {
                            Execute.EXE("takeown.exe", "/a /f \"" + s + "\\OneDriveSetup.exe", true, true);

                            Execute.EXE("icacls.exe", "\"" + s + "\\OneDriveSetup.exe\" /GRANT " + AdminGroupName + ":f", true, true);

                            Execute.EXE("powershell.exe", "\"Remove-Item \'" + s + "\\OneDriveSetup.exe\' -Force\"", true, true);

                            break;
                        }
                    }

                    Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                }
                else
                {
                    DispatchedLogBoxAdd("No OneDrive installation detected, skipping\n\n", Brushes.Yellow, null);
                }

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
                RegistryIO.DeleteSubKeyTree(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace", new String[] { "{018D5C66-4533-4307-9B53-224DE2ED1FE6}" });
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.RMOneDrive = false;
        }

        public void NoAppFilehistory()
        {
            try
            {
                DispatchedLogBoxAdd("Dectivate File/app History\n", Brushes.DarkGreen, null);

                if (ThisMachine.UIVersion is not WindowsUIVersion.Windows10)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true))
                    {
                        foreach (String subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.Contains("windows.data.unifiedtile.startglobalproperties"))
                            {
                                Byte[] Bytes = new Byte[] { 0x02, 0x00, 0x00, 0x00, 0xE0, 0x45, 0xDF, 0x00, 0xFF, 0x0E, 0xD9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0xC2, 0x14, 0x01, 0xC2, 0x3C, 0x01, 0xC5, 0x5A, 0x02, 0x00 };

                                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", Bytes, RegistryValueKind.Binary);

                                break;
                            }
                        }
                    }

                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0, RegistryValueKind.DWord);

                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowCloudFilesInQuickAccess", 0, RegistryValueKind.DWord);
                }
                else
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowCloudFilesInQuickAccess", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "HideRecentlyAddedApps", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRecentDocsHistory", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\FileHistory", "Disabled", 1, RegistryValueKind.DWord);
                }

                try
                {
                    using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs", true);
                    key.DeleteValue("MRUListEx", false);
                }
                catch (System.ArgumentException) { }

                Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);
                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.AppFilehistory = false;
        }

        public void YesAppFilehistory()
        {
            try
            {
                DispatchedLogBoxAdd("Activate File/app History\n", Brushes.DarkGreen, null);

                if (ThisMachine.UIVersion is not WindowsUIVersion.Windows10)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true))
                    {
                        foreach (String subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.Contains("windows.data.unifiedtile.startglobalproperties"))
                            {
                                Byte[] Bytes = new Byte[] { 0x02, 0x00, 0x00, 0x00, 0xF9, 0xDE, 0x68, 0xD8, 0x05, 0x0F, 0xD9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0xC2, 0x3C, 0x01, 0xC5, 0x5A, 0x02, 0x00 };

                                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", Bytes, RegistryValueKind.Binary);

                                break;
                            }
                        }
                    }

                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 1, RegistryValueKind.DWord);

                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "Start_TrackProgs", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "Start_TrackProgs", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowCloudFilesInQuickAccess", 1, RegistryValueKind.DWord);
                }
                else
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowCloudFilesInQuickAccess", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "HideRecentlyAddedApps", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRecentDocsHistory", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\FileHistory", "Disabled", 0, RegistryValueKind.DWord);
                }

                Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);
                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.AppFilehistory = false;
        }

        public void LegacyCmen()
        {
            if (ThisMachine.UIVersion is not WindowsUIVersion.Windows10)
            {
                DispatchedLogBoxAdd("Setting Legacy Context Menue\n", Brushes.DarkGreen, null);

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);

                Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);
                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Nothing changed, setting only available on Windows 11 or newer.\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.Cmen = false;
        }

        public void DefaultCmen()
        {
            if (ThisMachine.UIVersion is not WindowsUIVersion.Windows10)
            {
                DispatchedLogBoxAdd("Setting default context menue\n", Brushes.DarkGreen, null);

                RegistryIO.DeleteSubKeyTree(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", new String[] { "InprocServer32" });

                Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);
                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Nothing changed, setting only available on Windows 11 or newer.\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.Cmen = false;
        }

        public void LegacyRibbon()
        {
            try
            {
                if (ThisMachine.UIVersion is not WindowsUIVersion.Windows10)
                {
                    try
                    {
                        if (ThisMachine.OSMajorVersion > 22000)
                        {
                            DispatchedLogBoxAdd("'Feature' not available on newer versions.\n\n", Brushes.DarkGray, null);
                        }
                        else
                        {
                            DispatchedLogBoxAdd("Setting Legacy Explorer Ribbon\n", Brushes.DarkGreen, null);

                            Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);

                            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 0, RegistryValueKind.DWord);
                            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", "{e2bf9676-5f8f-435c-97eb-11607a5bedf7}", "", RegistryValueKind.String);
                            Task.Delay(400).Wait();

                            Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                            Task.Delay(400).Wait();

                            Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                            Task.Delay(400).Wait();

                            Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);

                            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 0, RegistryValueKind.DWord);
                            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", "{e2bf9676-5f8f-435c-97eb-11607a5bedf7}", "", RegistryValueKind.String);
                            Task.Delay(400).Wait();

                            Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                        }
                    }
                    catch { }
                }
                else
                {
                    DispatchedLogBoxAdd("Nothing changed, setting only available on Windows 11 or newer.\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.Ribbon = false;
        }

        public void DefaultRibbon()
        {
            if (ThisMachine.UIVersion is not WindowsUIVersion.Windows10)
            {
                DispatchedLogBoxAdd("Setting default Explorer Ribbon\n", Brushes.DarkGreen, null);

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 1, RegistryValueKind.DWord);

                RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Blocked", new String[] { "{e2bf9676-5f8f-435c-97eb-11607a5bedf7}" });

                Execute.EXE("taskkill.exe", "/f /im explorer.exe", true, true);
                Execute.EXE("cmd.exe", "/c \"start explorer.exe\"", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Nothing changed, setting only available on Windows 11 or newer.\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.Ribbon = false;
        }

        public void Firefox()
        {
            try
            {
                if (!Directory.Exists("assets\\Firefox"))
                {
                    DispatchedLogBoxAdd("Subdirectory \"assets\\Firefox\" not found, skipping\n\n", Brushes.Red, null);

                    ThreadIsAlive.Install = false;
                    return;
                }

                //profile validator
                Boolean CheckInput(String input)
                {
                    switch (input)
                    {
                        case "0":
                            if (VerboseHashCheck("assets\\Firefox\\00.zip", Const.FSH00)[0])
                            {
                                return true;
                            }
                            break;

                        case "00":
                            if (VerboseHashCheck("assets\\Firefox\\00.zip", Const.FSH00)[0])
                            {
                                return true;
                            }
                            break;

                        case "10":
                            if (VerboseHashCheck("assets\\Firefox\\10.zip", Const.FSH10)[0])
                            {
                                return true;
                            }
                            break;

                        case "11":
                            if (VerboseHashCheck("assets\\Firefox\\11.zip", Const.FSH11)[0])
                            {
                                return true;
                            }
                            break;

                        case "20":
                            if (VerboseHashCheck("assets\\Firefox\\20.zip", Const.FSH20)[0])
                            {
                                return true;
                            }
                            break;

                        case "21":
                            if (VerboseHashCheck("assets\\Firefox\\21.zip", Const.FSH21)[0])
                            {
                                return true;
                            }
                            break;

                        case "30":
                            if (VerboseHashCheck("assets\\Firefox\\30.zip", Const.FSH30)[0])
                            {
                                return true;
                            }
                            break;

                        case "31":
                            if (VerboseHashCheck("assets\\Firefox\\31.zip", Const.FSH31)[0])
                            {
                                return true;
                            }
                            break;

                        case "40":
                            if (VerboseHashCheck("assets\\Firefox\\41.zip", Const.FSH41)[0])
                            {
                                return true;
                            }
                            break;

                        case "41":
                            if (VerboseHashCheck("assets\\Firefox\\41.zip", Const.FSH41)[0])
                            {
                                return true;
                            }
                            break;
                    }

                    return false;
                }

                Boolean Softinstall = false;
                Boolean valide = false;
                String profileid = "";
                Boolean onlineinstall = false;

                //check if already installed
                if (File.Exists(@"C:\Program Files\Mozilla Firefox\firefox.exe"))
                {
                    var result0 = System.Windows.Forms.MessageBox.Show(
                    "Firefox installation found, only apply profile to user?",
                    "",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                    if (result0 == System.Windows.Forms.DialogResult.Yes)
                    {
                        DispatchedLogBoxAdd("Applying Firefox profiles\n", Brushes.DarkGreen, null);
                        Softinstall = true;
                    }
                    else if (result0 == System.Windows.Forms.DialogResult.No)
                    {
                        DispatchedLogBoxAdd("Re-Installing Firefox\n", Brushes.DarkGreen, null);
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.Install = false;
                        return;
                    }
                }

                if (!Softinstall)
                {
                    //pre checks
                    if (InternetIsAvailable())
                    {
                        var result1 = System.Windows.Forms.MessageBox.Show(
                        "Download newest version via internet?",
                        "",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                        if (result1 == System.Windows.Forms.DialogResult.Yes)
                        {
                            onlineinstall = true;
                        }
                    }
                }

                //get profile
                do
                {
                    profileid = Microsoft.VisualBasic.Interaction.InputBox("example:\n\n31     =>     3 = lvl || 1 = re-arm settings on launch\n\n0     =      default\n1     =      0 + no autofill\n2     =     1 + secure search\n3     =      2 + Secure DNS,no WebGL\n4     =      3 + resist fingerprinting (letterboxing)",
                               "Select Profile",
                               "",
                               0,
                               0);

                    valide = CheckInput(profileid);

                    if (profileid == "")
                    {
                        DispatchedLogBoxAdd("Nothing changed, canceling\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.Install = false;
                        return;
                    }
                    else if (valide)
                    {
                        if (profileid == "0")
                        {
                            profileid = "00";
                        }
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Invalide input\n", Brushes.Yellow, null);

                        var result0 = System.Windows.Forms.MessageBox.Show(
                            "Possible inputs:\n00, 10, 11, 20, 21, 30, 31, 41",
                            "Invalide Input",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                while (!valide);

                //install
                if (!Softinstall)
                {
                    if (onlineinstall)
                    {
                        if (WinGetIsInstalled(true) == 1)
                        {
                            if (InternetIsAvailable())
                            {
                                DispatchedLogBoxAdd("Installing Firefox (via WinGet)..\n", Brushes.DarkGreen, null);

                                Execute.EXE("winget.exe", "install Mozilla.Firefox --accept-package-agreements --accept-source-agreements --scope machine --custom \"INSTALL_MAINTENANCE_SERVICE=false /quiet\"", true, true);

                                if (!InternetIsAvailable())
                                {
                                    DispatchedLogBoxAdd("Finished with errors, lost connection?\n\n", Brushes.Red, null);

                                    ThreadIsAlive.Install = false;
                                    return;
                                }
                            }
                            else
                            {
                                DispatchedLogBoxAdd("No internet connection detected\n\n", Brushes.Red, null);

                                ThreadIsAlive.Install = false;
                                return;
                            }
                        }
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Installing Firefox..\n", Brushes.DarkGreen, null);

                        if (VerboseHashCheck("assets\\Firefox\\" + Const.FirefoxImageName, Const.FirefoxImageHash)[0])
                        {
                            Execute.EXE("assets\\Firefox\\" + Const.FirefoxImageName, "/s /MaintenanceService={false}", true, true);
                        }
                        else
                        {
                            var result = System.Windows.Forms.MessageBox.Show(
                            "Invalid or missing Firefox setup file, select manual?",
                            "Verification Error",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Error);

                            if (result == System.Windows.Forms.DialogResult.Yes)
                            {
                                using System.Windows.Forms.OpenFileDialog openFileDialog = new();
                                openFileDialog.InitialDirectory = "assets\\Firefox\\";
                                openFileDialog.RestoreDirectory = true;
                                openFileDialog.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
                                openFileDialog.FilterIndex = 0;
                                openFileDialog.Title = "Firefox Setup File";

                                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    DispatchedLogBoxAdd("\n(" + openFileDialog.FileName + ") ", Brushes.DarkGray, null);

                                    DispatchedLogBoxAdd("Installing with custom file..\n", Brushes.Yellow, null);

                                    Execute.EXE(openFileDialog.FileName, "/s /MaintenanceService={false}", true, true);
                                }
                                else
                                {
                                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                                    ThreadIsAlive.Install = false;
                                    return;
                                }
                            }
                            else
                            {
                                DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                                ThreadIsAlive.Install = false;
                                return;
                            }
                        }
                    }

                    if (CheckInput(profileid) != true)
                    {
                        if (Softinstall)
                        {
                            DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                        }
                        else
                        {
                            DispatchedLogBoxAdd("Finished with errors, skipping profile\n\n", Brushes.Yellow, null);
                        }

                        ThreadIsAlive.Install = false;
                        return;
                    }
                }

                //apple e profile
                DispatchedLogBoxAdd("Using profile " + profileid + " \n", Brushes.Gray, null);

                if (!(VerboseHashCheck("temp\\" + "7z.dll", Const.zip7dll)[0]) || !(VerboseHashCheck("temp\\" + "7z.exe", Const.zip7exe)[0]))
                {//[iqudhiwuo)T/&/(RR]
                    //InternExtract("temp\\", "7z.dll", "embeded_resources._7zip.7z.dll");
                    //InternExtract("temp\\", "7z.exe", "embeded_resources._7zip.7z.exe");
                }

                Execute.EXE("temp\\7z.exe", "x \"assets\\Firefox\\" + profileid + ".zip\" -o\"C:\\Users\\" + UserPath + "\\AppData\\Roaming\\Mozilla\" -y", true, true);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.Install = false;

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void RMEdge()
        {
            try
            {
                DispatchedLogBoxAdd("Removing Edge\n", Brushes.DarkGreen, null);

                try
                {
                    if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\EdgeUpdate") || Directory.Exists(@"C:\Users\" + UserPath + @"\AppData\Local\Microsoft\EdgeUpdate"))
                    {
                        DispatchedLogBoxAdd("Removing EdgeUpdate\n", Brushes.Gray, null);
                        try
                        {
                            Execute.EXE("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\EdgeUpdate\"", true, true);
                            Execute.EXE("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\EdgeUpdate\" /E /g " + AdminGroupName + ":f", true, true);
                            Execute.EXE("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                            Task.Delay(1001).Wait();
                            Directory.Delete("C:\\Program Files (x86)\\Microsoft\\EdgeUpdate", true);
                        }
                        catch (Exception) { }

                        try
                        {
                            PowerShell("Unregister-ScheduledTask -TaskName MicrosoftEdgeUpdateTaskMachineCore -Confirm $false");

                            PowerShell("Unregister-ScheduledTask -TaskName MicrosoftEdgeUpdateTaskMachineUA -Confirm $false");

                            Execute.EXE("cacls.exe", "\"C:\\Users\\" + UserPath + "\\AppData\\Local\\Microsoft\\EdgeUpdate\" /E /g " + AdminGroupName + ":f", true, true);
                            Execute.EXE("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                            Task.Delay(500).Wait();
                            Directory.Delete("C:\\Users\\" + UserPath + "\\AppData\\Local\\Microsoft\\EdgeUpdate", true);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        DispatchedLogBoxAdd("No EdgeUpdate installation found\n", Brushes.Yellow, null);
                    }

                    if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\Edge"))
                    {
                        DispatchedLogBoxAdd("Removing Microsoft Edge\n", Brushes.Gray, null);

                        RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Microsoft Edge", new String[] { "NoRemove" });

                        RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\ClientState\\{56EB18F8-B008-4CBD-B6D2-8C97FE7E9062}", new String[] { "experiment_control_labels" });

                        try
                        {
                            Execute.EXE("taskkill.exe", "/f /im msedge.exe", true, true);
                            Execute.EXE(Directory.GetDirectories(@"C:\Program Files (x86)\Microsoft\Edge\Application")[0] + @"\Installer\setup.exe", " --uninstall --msedge --system-level --verbose-logging", false, false);
                        }
                        catch (Exception ex) when (ex is System.IO.DirectoryNotFoundException || ex is System.ComponentModel.Win32Exception)
                        {
                            DispatchedLogBoxAdd("Program setup file missing, removing files\n", Brushes.Yellow, null);
                            DispatchedLogBoxAdd("\nRemoving program files\n\n", Brushes.Gray, null);

                            if (!FEdge())
                            {
                                DispatchedLogBoxAdd("An error occured while removing edge\n\n", Brushes.Yellow, null);
                            }
                        }

                        if (!(VerboseHashCheck("temp\\SetACL.exe", Const.ACLHash)[0]))
                        {
                            //[iqudhiwuo)T/&/(RR]
                            //InternExtract("temp\\", "SetACL.exe", "embeded_resources.SetACL.exe");
                        }

                        String temp = "-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\InboxApplications\" -ot reg -actn setowner -ownr \"n:" + AdminGroupName + "\" -rec Yes";
                        Execute.EXE("temp\\SetACL.exe", temp, true, true);

                        temp = "-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\InboxApplications\" -ot reg -actn ace -ace \"n:" + AdminGroupName + ";p:full\" -rec Yes";
                        Execute.EXE("temp\\SetACL.exe", temp, true, true);

                        using RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications", true);

                        foreach (String subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.Contains("neutral__8wekyb3d8bbwe") && !subKeyName.Contains("Microsoft.MicrosoftEdgeDevToolsClient"))
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications\" + subKeyName, false);

                                break;
                            }
                        }

                        key.Close();
                        key.Dispose();
                    }
                    else
                    {
                        DispatchedLogBoxAdd("No Edge installation found, skipping\n", Brushes.Yellow, null);
                    }

                    while (File.Exists(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe") && Process.GetProcessesByName("setup").Length != 0)
                    {
                        Task.Delay(256).Wait();
                    }

                    Task.Delay(2560).Wait();

                    Execute.EXE("taskkill.exe", "/f /im setup.exe", true, true);

                    if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\Edge"))
                    {
                        if (File.Exists(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"))
                        {
                            DispatchedLogBoxAdd("(Not supported by newer versions)\n", Brushes.Yellow, null);
                            DispatchedLogBoxAdd("Manaually removing Edge\n", Brushes.Yellow, null);
                        }

                        if (!FEdge(true))
                        {
                            DispatchedLogBoxAdd("An error occured while removing edge\n\n", Brushes.Yellow, null);
                        }
                    }

                    DispatchedLogBoxAdd("Making Microsoft Edge User-unistallable\n", Brushes.Gray, null);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge", "NoRemove", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update", "NoRemove", 0, RegistryValueKind.DWord);

                    DispatchedLogBoxAdd("Preventing Microsoft Edge from reinstalling via Windows Update\n", Brushes.Gray, null);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\EdgeUpdate", "DoNotUpdateToEdgeWithChromium", 1, RegistryValueKind.DWord);

                    if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\EdgeCore"))
                    {
                        DispatchedLogBoxAdd("Removing EdgeCore\n", Brushes.Gray, null);

                        Execute.EXE("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\EdgeCore\"", true, true);
                        Execute.EXE("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\EdgeCore\" /E /g " + AdminGroupName + ":f", true, true);
                        Execute.EXE("taskkill.exe", "/f /im msedge.exe", true, true);
                        Execute.EXE("taskkill.exe", "/f /im msedgewebview2.exe", true, true);
                        Task.Delay(1000).Wait();
                        Directory.Delete("C:\\Program Files (x86)\\Microsoft\\EdgeCore", true);
                    }
                    else
                    {
                        DispatchedLogBoxAdd("No EdgeCore installation found, skipping\n", Brushes.Yellow, null);
                    }

                    try
                    {
                        //deactivate service
                        Service.SetStartupType("edgeupdate", ServiceStartMode.Disabled);
                        Service.SetStartupType("edgeupdatem", ServiceStartMode.Disabled);
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    DispatchedLogBoxAdd("ERROR: " + ex.ToString(), Brushes.Red, null);

                    ThreadIsAlive.RMEdge = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.RMEdge = false;

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

            // ### fang ###

            Boolean FEdge(Boolean X86Folder = false)
            {
                for (Byte B = 0; B < 3; B++)
                {
                    try
                    {
                        Execute.EXE("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                        Execute.EXE("taskkill.exe", "/f /im msedge.exe", true, true);

                        if (X86Folder)
                        {
                            Execute.EXE("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\Edge\"", true, true);
                            Execute.EXE("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\Edge\" /E /g " + AdminGroupName + ":f", true, true);
                            Execute.EXE("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                            Execute.EXE("taskkill.exe", "/f /im msedge.exe", true, true);
                            Task.Delay(1000).Wait();
                            Directory.Delete("C:\\Program Files (x86)\\Microsoft\\Edge", true);
                        }
                        else
                        {
                            Execute.EXE("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\Edge\"", true, true);
                            Execute.EXE("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\Edge\" /E /g " + AdminGroupName + ":f", true, true);
                            Execute.EXE("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                            Execute.EXE("taskkill.exe", "/f /im msedge.exe", true, true);
                            Directory.Delete("C:\\Program Files (x86)\\Microsoft\\Edge", true);
                        }

                        DispatchedLogBoxAdd("Success\n\n", Brushes.Gray, null);

                        return true;
                    }
                    catch
                    { }
                }

                return false;
            }
        }

        public void Curser()
        {
            try
            {
                if (!Directory.Exists("assets\\Cursers"))
                {
                    DispatchedLogBoxAdd("Subdirectory \"assets\\Cursers\" not found, skipping\n\n", Brushes.Red, null);

                    ThreadIsAlive.Curser = false;
                    return;
                }

                //profile validator
                Boolean CheckInput(String input)
                {
                    switch (input)
                    {
                        case "0":
                            if (VerboseHashCheck("assets\\Cursers\\0.zip", Const.CurDef)[0])
                            {
                                return true;
                            }
                            break;

                        case "1":
                            if (VerboseHashCheck("assets\\Cursers\\1.zip", Const.CurDefHybrid)[0])
                            {
                                return true;
                            }
                            break;

                        case "2":
                            if (VerboseHashCheck("assets\\Cursers\\2.zip", Const.CurDefMono)[0])
                            {
                                return true;
                            }
                            break;

                        case "3":
                            if (VerboseHashCheck("assets\\Cursers\\3.zip", Const.CurBlack)[0])
                            {
                                return true;
                            }
                            break;

                        case "4":
                            if (VerboseHashCheck("assets\\Cursers\\4.zip", Const.CurBlackHybrid)[0])
                            {
                                return true;
                            }
                            break;

                        case "5":
                            if (VerboseHashCheck("assets\\Cursers\\5.zip", Const.CurBlackMono)[0])
                            {
                                return true;
                            }
                            break;
                    }

                    return false;
                }

                Boolean valide = false;
                String profile = "";

                do
                {
                    profile = Microsoft.VisualBasic.Interaction.InputBox("\n\n0     =      White\n1     =      White w/ black/white hourglass\n2     =      White Monochrome\n3     =      Black\n4     =      Black w/ black/white hourglass\n5     =      Black Monochrome",
                        "Select Profile",
                        "",
                        0,
                        0);

                    if (profile == "")
                    {
                        DispatchedLogBoxAdd("Nothing changed, canceling\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.Curser = false;
                        return;
                    }

                    valide = CheckInput(profile);

                    if (!valide)
                    {
                        DispatchedLogBoxAdd("Invalide input\n", Brushes.Yellow, null);

                        var result0 = System.Windows.Forms.MessageBox.Show(
                            "Possible inputs:\n0, 1, 2, 3, 4, 5",
                            "Invalide Input",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                while (!valide);

                //install
                DispatchedLogBoxAdd("Setting custom curser\n", Brushes.DarkGreen, null);

                if (!(VerboseHashCheck("temp\\" + "7z.dll", Const.zip7dll)[0]) || !(VerboseHashCheck("temp\\" + "7z.exe", Const.zip7exe)[0]))
                {
                    //[iqudhiwuo)T/&/(RR]
                    //InternExtract("temp\\", "7z.dll", "embeded_resources._7zip.7z.dll");
                    //InternExtract("temp\\", "7z.exe", "embeded_resources._7zip.7z.exe");
                }

                Execute.EXE("temp\\7z.exe", "x \"assets\\Cursers\\" + profile + ".zip\" -o\"temp\\temp\" -y", true, true);

                Execute.EXE("C:\\WINDOWS\\System32\\rundll32.exe", "setupapi,InstallHinfSection DefaultInstall 132 .\\temp\\temp\\install.inf", true, true);

                //set
                DispatchedLogBoxAdd("Using profile " + profile + "\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors", "Scheme Source", 1, RegistryValueKind.DWord);
                Execute.EXE("reg.exe", "import .\\temp\\temp\\" + profile + ".reg", true, true);

                PowerShell("Remove-Item -Path .\\temp\\temp -Recurse -Force");

            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            DispatchedLogBoxAdd("Done, new Cursor will display after reboot\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.Curser = false;
        }

        public void SafeBoot()
        {
            DispatchedLogBoxAdd("Restarting to \"Safe Mode\"\n", Brushes.DarkGreen, null);

            var result = System.Windows.Forms.MessageBox.Show(
                "Restarting to \"Safe Mode\".\nComputer will force restart in 10 seconds, proceed?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Execute.EXE("shutdown.exe", "/r /o /t 10 /f", true, false);

                DispatchedLogBoxAdd("in\n10\n", Brushes.Gray, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("9\n", Brushes.Gray, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("8\n", Brushes.Gray, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("7\n", Brushes.Gray, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("6\n", Brushes.Gray, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("5\n", Brushes.Gray, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("4\n", Brushes.Gray, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("3\n", Brushes.Yellow, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("2\n", Brushes.Yellow, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("1\n", Brushes.Yellow, null);
                Task.Delay(1000).Wait();
                DispatchedLogBoxAdd("Restarting\n", Brushes.Yellow, null);

                Task.Delay(1000).Wait();

                DispatchedLogBoxAdd("\nUnknown error, user cancled shutdown?\n\n", Brushes.Magenta, null);

            }
            else
            {
                DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.BootStuff = false;
        }

        public void UEFIBoot()
        {
            if (ThisMachine.IsUEFI)
            {
                DispatchedLogBoxAdd("Restarting to UEFI Firmware\n", Brushes.DarkGreen, null);

                var result = System.Windows.Forms.MessageBox.Show(
                    "Restarting to UEFI Firmware.\nComputer will force restart in 10 seconds, proceed?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Execute.EXE("shutdown.exe", "/r /fw /t 10 /f", true, false);

                    DispatchedLogBoxAdd("in\n10\n", Brushes.Gray, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("9\n", Brushes.Gray, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("8\n", Brushes.Gray, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("7\n", Brushes.Gray, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("6\n", Brushes.Gray, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("5\n", Brushes.Gray, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("4\n", Brushes.Gray, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("3\n", Brushes.Yellow, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("2\n", Brushes.Yellow, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("1\n", Brushes.Yellow, null);
                    Task.Delay(1000).Wait();
                    DispatchedLogBoxAdd("Restarting\n", Brushes.Yellow, null);

                    Task.Delay(1000).Wait();

                    DispatchedLogBoxAdd("\nUnknown error, user cancled shutdown?\n\n", Brushes.Magenta, null);
                }
                else
                {
                    DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                }
            }
            else
            {
                DispatchedLogBoxAdd("Restarting to firmware from OS not supported on legacy BIOS systems\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.BootStuff = false;
        }

        public void ActivateNumLock()
        {
            DispatchedLogBoxAdd("Activating NumLock on Startup\n", Brushes.DarkGreen, null);

            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 2, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.NumLock = false;

        }

        public void DeactivateNumLock()
        {
            DispatchedLogBoxAdd("Deactivating NumLock on Startup\n", Brushes.DarkGreen, null);

            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 0, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.NumLock = false;

        }

        public void DeactivateBootSound()
        {
            DispatchedLogBoxAdd("Deactivating Windows Boot Sound\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\BootAnimation", "DisableStartupSound", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\EditionOverrides", "UserSetting_DisableStartupSound", 1, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.BootSound = false;

        }

        public void ActivateBootSound()
        {
            DispatchedLogBoxAdd("Activating Windows Boot Sound\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\BootAnimation", "DisableStartupSound", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\EditionOverrides", "UserSetting_DisableStartupSound", 0, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.BootSound = false;
        }

        public void ResetUpdate()
        {
            try
            {
                DispatchedLogBoxAdd("Resetting Windows Update\n", Brushes.DarkGreen, null);

                var result = System.Windows.Forms.MessageBox.Show(
                    "Reset Windows Update?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 3, RegistryValueKind.DWord);

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0, RegistryValueKind.DWord);

                    DispatchedLogBoxAdd("Resetting Windows Update services\n", Brushes.Gray, null);
                    Service.SetStartupType("BITS", ServiceStartMode.Automatic);
                    Service.SetStartupType("wuauserv", ServiceStartMode.Automatic);

                    DispatchedLogBoxAdd("Enabling driver offering through Windows Update\n", Brushes.Gray, null);
                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\Device Metadata", new String[] { "PreventDeviceMetadataFromNetwork" });

                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DriverSearching", new String[] { "DontPromptForWindowsUpdate", "DontSearchWindowsUpdate", "DriverUpdateWizardWuSearchEnabled" });

                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate", new String[] { "ExcludeWUDriversInQualityUpdate" });

                    DispatchedLogBoxAdd("Enabling Windows Update automatic restart\n", Brushes.Gray, null);
                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", new String[] { "NoAutoRebootWithLoggedOnUsers", "AUPowerManagement" });


                    DispatchedLogBoxAdd("Enabled driver offering through Windows Update\n", Brushes.Gray, null);
                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\WindowsUpdate\\UX\\Settings", new String[] { "BranchReadinessLevel", "DeferFeatureUpdatesPeriodInDays", "DeferQualityUpdatesPeriodInDays" });

                    DispatchedLogBoxAdd("Stopping Windows Update Services\n", Brushes.Gray, null);
                    Execute.EXE("net.exe", "stop \"BITS\" /y", true, true);
                    Execute.EXE("net.exe", "stop \"wuauserv\" /y", true, true);
                    Execute.EXE("net.exe", "stop \"appidsvc\" /y", true, true);
                    Execute.EXE("net.exe", "stop \"cryptsvc\" /y", true, true);

                    DispatchedLogBoxAdd("Removing QMGR Data file\n", Brushes.Gray, null);
                    try
                    {
                        Directory.Delete(@"C:\ProgramData\Application Data\Microsoft\Network\Downloader", true);
                    }
                    catch (System.IO.DirectoryNotFoundException) { }

                    DispatchedLogBoxAdd("Flushing DNS\n", Brushes.Gray, null);
                    Execute.EXE("ipconfig.exe", "/flushdns", true, true);

                    DispatchedLogBoxAdd("Removing old Windows Update log\n", Brushes.Gray, null);
                    File.Delete("C:\\Windows\\WindowsUpdate.log");

                    DispatchedLogBoxAdd("Resetting the Windows Update Services to default settings\n", Brushes.Gray, null);
                    Execute.EXE("sc.exe", "sdset bits D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", true, true);
                    Execute.EXE("sc.exe", "wuauserv D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", true, true);

                    DispatchedLogBoxAdd("Reregistering *some* DLLs (BITfiles + Windows Update)\n", Brushes.Gray, null);
                    Execute.EXE("regsvr32.exe", "/s atl.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s urlmon.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s mshtml.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s shdocvw.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s browseui.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s jscript.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s vbscript.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s scrrun.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s msxml.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s msxml3.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s msxml6.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s actxprxy.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s softpub.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wintrust.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s dssenh.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s rsaenh.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s gpkcsp.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s sccbase.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s slbcsp.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s cryptdlg.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s oleaut32.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s ole32.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s shell32.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s initpki.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wuapi.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wuaueng.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wuaueng1.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wucltui.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wups.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wups2.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wuweb.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s qmgr.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s qmgrprxy.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wucltux.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s muweb.dll", true, true);
                    Execute.EXE("regsvr32.exe", "/s wuwebv.dll", true, true);

                    DispatchedLogBoxAdd("Removing WSUS (Windows Server) client settings\n", Brushes.Gray, null);
                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate", new String[] { "AccountDomainSid", "PingID", "SusClientId" });

                    DispatchedLogBoxAdd("Resetting the WinSock\n", Brushes.Gray, null);
                    Execute.EXE("netsh.exe", "winsock reset", true, true);
                    Execute.EXE("netsh.exe", "winhttp reset proxy", true, true);

                    DispatchedLogBoxAdd("Delete all BITS jobs\n", Brushes.Gray, null);
                    PowerShell("Get-BitsTransfer | Remove-BitsTransfer");

                    DispatchedLogBoxAdd("Attempting to install the Windows Update Agent\n", Brushes.Gray, null);
                    if (Environment.Is64BitOperatingSystem)
                    {
                        Execute.EXE("wusa.exe", "Windows8-RT-KB2937636-x64 /quiet", true, true);
                    }
                    else
                    {
                        Execute.EXE("wusa.exe", "Windows8-RT-KB2937636-x86 /quiet", true, true);
                    }

                    DispatchedLogBoxAdd("Reseting Windows Update policies\n", Brushes.Gray, null);
                    RegistryIO.DeleteSubKeyTree("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", new String[] { "WindowsUpdate" });
                    RegistryIO.DeleteSubKeyTree("HKEY_CURRENT_USER\\SOFTWARE\\Policies\\Microsoft\\Windows", new String[] { "WindowsUpdate" });

                    RegistryIO.DeleteSubKeyTree("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", new String[] { "WindowsUpdate" });
                    RegistryIO.DeleteSubKeyTree("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", new String[] { "WindowsUpdate" });

                    DispatchedLogBoxAdd("Starting Windows Update Services\n", Brushes.Gray, null);
                    Execute.EXE("net.exe", "start \"BITS\" /y", true, true);
                    Execute.EXE("net.exe", "start \"wuauserv\" /y", true, true);
                    Execute.EXE("net.exe", "start \"appidsvc\" /y", true, true);
                    Execute.EXE("net.exe", "start \"cryptsvc\" /y", true, true);

                    DispatchedLogBoxAdd("Forcing discovery\n", Brushes.Gray, null);
                    Execute.EXE("wuauclt.exe", "/resetauthorization /detectnow", true, true);

                    DispatchedLogBoxAdd("Done ", Brushes.DarkCyan, null);
                    DispatchedLogBoxAdd("restart your computer to complete\n\n", Brushes.Yellow, null);
                }
                else
                {
                    DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.WindowsUpdate = false;
        }

        public void NoWUDrivers()
        {
            DispatchedLogBoxAdd("Deactivating Windows Update\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.NoWUDrivers = false;
        }

        public void DeactivateWindowsUpdate()
        {
            DispatchedLogBoxAdd("Deactivating Windows Update\n", Brushes.DarkGreen, null);

            var result = System.Windows.Forms.MessageBox.Show(
                "Deactivate Windows Update?\n(Not recommended)",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0, RegistryValueKind.DWord);

                Service.SetStartupType("BITS", ServiceStartMode.Disabled);
                Service.SetStartupType("wuauserv", ServiceStartMode.Disabled);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.WindowsUpdate = false;
        }

        public void SecurityUpdatesOnly()
        {
            DispatchedLogBoxAdd("Setting Windows Update to \"Security Updates only\"\n", Brushes.DarkGreen, null);

            var result = System.Windows.Forms.MessageBox.Show(
                "Delays feature updates 1 year.\nDelays security updates 2 days, proceed?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                DispatchedLogBoxAdd("Disabling driver offering through Windows Update\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Device Metadata", "PreventDeviceMetadataFromNetwork", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DontPromptForWindowsUpdate", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DontSearchWindowsUpdate", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DriverUpdateWizardWuSearchEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Setting update policy\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "BranchReadinessLevel", 16, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "DeferFeatureUpdatesPeriodInDays", 365, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "DeferQualityUpdatesPeriodInDays", 2, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.WindowsUpdate = false;
        }

        public void FastWUPDATE()
        {
            DispatchedLogBoxAdd("Setting Windows Update to fast release\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "BranchReadinessLevel", 8, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.WindowsUpdate = false;
        }

        public void EncryptSMB()
        {
            try
            {
                DispatchedLogBoxAdd("SMB settings\n", Brushes.DarkGreen, null);

                Boolean B = false;

                //[iqudhiwuo)T/&/(RR]

                //if (SMBhardenMessage.OnlySMB3)
                //{
                //    DispatchedLogBoxAdd("Setting minimum SMB version to 3.1.1\n", Brushes.Gray, null);
                //    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters", "MinSMB2Dialect", 0x000000311, RegistryValueKind.DWord);

                //    DispatchedLogBoxAdd("Deactivating SMB 1\n", Brushes.Gray, null);
                //    PowerShell("Set-SmbServerConfiguration -EnableSMB1Protocol $false -Confirm:$false");

                //    B = true;
                //}

                //if (SMBhardenMessage.Encrypt)
                //{
                //    DispatchedLogBoxAdd("Enable Smb encryption\n", Brushes.Gray, null);
                //    PowerShell("Set-SmbServerConfiguration -EncryptData $true -Confirm:$false");
                //    PowerShell("Set-SmbServerConfiguration -DisableSmbEncryptionOnSecureConnection $false -Confirm:$false");

                //    if (IsWin11Server22Complient)
                //    {
                //        DispatchedLogBoxAdd("Setting Encryption Ciphers to AES 256 GCM and AES_128_GCM\n", Brushes.Gray, null);
                //        PowerShell("Set-SmbServerConfiguration -EncryptionCiphers \"AES_256_GCM, AES_128_GCM\" -Confirm:$false");
                //    }

                //    B = true;
                //}

                //if (SMBhardenMessage.RejectUnencrypted)
                //{
                //    DispatchedLogBoxAdd("Rejecting unencrypted SMB access\n", Brushes.Gray, null);
                //    PowerShell("Set-SmbServerConfiguration -RejectUnencryptedAccess $true -Confirm:$false");
                //    PowerShell("Set-SmbServerConfiguration -DisableSmbEncryptionOnSecureConnection $false -Confirm:$false");

                //    B = true;
                //}

                //if (SMBhardenMessage.RequireSign)
                //{
                //    DispatchedLogBoxAdd("Require traffic signature\n", Brushes.Gray, null);
                //    PowerShell("Set-SmbServerConfiguration -EnableSecuritySignature $true -Confirm:$false");
                //    PowerShell("Set-SmbServerConfiguration -RequireSecuritySignature $true -Confirm:$false");

                //    B = true;
                //}

                //if (SMBhardenMessage.Autoshares)
                //{
                //    DispatchedLogBoxAdd("Deactivating autoshares\n", Brushes.Gray, null);
                //    PowerShell("Set-SmbServerConfiguration -AutoShareWorkstation $false -Confirm:$false");
                //    PowerShell("Set-SmbServerConfiguration -AutoShareServer $false -Confirm:$false");

                //    B = true;
                //}

                //if (SMBhardenMessage.OnlyAES256GCM)
                //{
                //    DispatchedLogBoxAdd("Set Encryption to AES 256 GCM\n", Brushes.Gray, null);
                //    PowerShell("Set-SmbServerConfiguration -EncryptionCiphers \"AES_256_GCM\" -Confirm:$false");

                //    B = true;
                //}

                //if (B)
                //{
                //    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                //}
                //else
                //{
                //    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);

                //}
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.SMB = false;
        }

        public void DeactivateEncryptSMB()
        {
            try
            {
                DispatchedLogBoxAdd("Resetting SMB server config\n", Brushes.DarkGreen, null);
                RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\LanmanWorkstation\\Parameters", new String[] { "MinSMB2Dialect" });

                PowerShell("Set-SmbServerConfiguration -EnableSMB2Protocol $true -Confirm:$false");
                PowerShell("Set-SmbServerConfiguration -EncryptionCiphers \"AES_256_GCM, AES_128_GCM, AES_256_CCM, AES_128_CCM\" -Confirm:$false");
                PowerShell("Set-SmbServerConfiguration -EncryptData $true -Confirm:$false");
                PowerShell("Set-SmbServerConfiguration -RejectUnencryptedAccess $false -Confirm:$false");
                PowerShell("Set-SmbServerConfiguration -RequireSecuritySignature $false -Confirm:$false");
                PowerShell("Set-SmbServerConfiguration -AutoShareWorkstation $true -Confirm:$false");
                PowerShell("Set-SmbServerConfiguration -AutoShareServer $true -Confirm:$false");
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.SMB = false;
        }

        public void VerboseUAC()
        {
            DispatchedLogBoxAdd("UAC always request admin credentials\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 1, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.UAC = false;
        }

        public void DefaultUAC()
        {
            DispatchedLogBoxAdd("Default UAC prompt behavior\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 5, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.UAC = false;
        }

        public void PagefileClear()
        {
            DispatchedLogBoxAdd("Clearing pagefile On shutdown\n", Brushes.DarkGreen, null);

            var result = System.Windows.Forms.MessageBox.Show(
                    "Clearing the pagefile at shutdown may\nresult in prolonged shutdown time, continue?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "ClearPageFileAtShutdown", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.Pagefile = false;
        }

        public void PagefileDefault()
        {
            DispatchedLogBoxAdd("Default pagefile behavior\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "ClearPageFileAtShutdown", 0, RegistryValueKind.DWord);
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.Pagefile = false;
        }

        public void RequireCtrl()
        {
            DispatchedLogBoxAdd("Require \"Ctrl+Alt +Del\" to Log in\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 0, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.RequireCtrl = false;
        }

        public void DontRequireCtrl()
        {
            if (ThisMachine.OSType is not OSType.Server)
            {
                DispatchedLogBoxAdd("Don't require \"Ctrl+Alt +Del\" to Log in\n", Brushes.DarkGreen, null);

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Not supported on Windows Server, use GPO\n\n", Brushes.Yellow, null);
            }

            ThreadIsAlive.RequireCtrl = false;
        }

        public void InstallGPO()
        {
            try
            {
                MessageBoxManager.Cancel = "Never";
                MessageBoxManager.Register();

                var result = System.Windows.Forms.MessageBox.Show(
                "Install Group Policy Editor?",
                "Install Feature",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

                MessageBoxManager.Unregister();

                if (result == System.Windows.Forms.DialogResult.No)
                {
                    DispatchedLogBoxAdd("gpedit.msc not installed\n\n", Brushes.Yellow, null);
                    return;
                }

                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    DispatchedLogBoxAdd("gpedit.msc not installed\n\n", Brushes.Yellow, null);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "GPO Status", 2, RegistryValueKind.DWord);
                    return;
                }

                DispatchedLogBoxAdd("Installing / activating Group Policy Editor\n", Brushes.DarkGreen, null);
                String[] packages1 = Directory.GetFiles(@"C:\Windows\servicing\Packages", "Microsoft-Windows-GroupPolicy-ClientExtensions-Package~3*.mum");
                // array to list
                List<String> itemsList = packages1.ToList<String>();
                // or merge an other array to the list
                itemsList.AddRange(Directory.GetFiles(@"C:\Windows\servicing\Packages", "Microsoft-Windows-GroupPolicy-ClientTools-Package~3*.mum"));
                // list to array
                String[] allPackages = itemsList.ToArray();

                if (allPackages.Length == 0)
                {
                    DispatchedLogBoxAdd("\nNo packages found, skipping\n\n", Brushes.Yellow, null);
                    return;
                }

                DispatchedLogBoxAdd("This might take a while\n", Brushes.DarkGray, null);

                for (Int32 i = 0; i < allPackages.Length; i++)
                {
                    DispatchedLogBoxAdd("Installing package " + (i + 1) + "/" + allPackages.Length, Brushes.Gray, null);
                    Execute.EXE("dism.exe", "/online /norestart /add-package:\"" + allPackages[i] + "\"", true, true);
                    if (allPackages.Length > (i + 1))
                    {
                        DispatchedLogBoxAdd(allPackages[i]);
                    }
                }

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "GPO Status", 1, RegistryValueKind.DWord);
                DispatchedLogBoxAdd("\ngpedit.msc now available\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }
        }

        public void Zip7()
        {
            try
            {
                DispatchedLogBoxAdd("Installing 7-Zip\n", Brushes.DarkGreen, null);

                if (VerboseHashCheck("assets\\" + Const.zip7installName, Const.zip7installHash)[0])
                {
                    Execute.EXE("assets\\" + Const.zip7installName, "/S", true, true);
                    //[iqudhiwuo)T/&/(RR]
                    //InternExtract("C:\\Program Files\\7-Zip\\Codecs\\", "WinCryptHashers.64.dll", "embeded_resources._7zip.WinCryptHashers.64.dll");
                    //InternExtract("C:\\Program Files\\7-Zip\\Codecs\\", "WinCryptHashers.ini", "embeded_resources._7zip.WinCryptHashers.ini");

                    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                }
                else
                {
                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.Install = false;
        }

        public void VCRedist()
        {
            try
            {
                DispatchedLogBoxAdd("Installing Visual Studio Runtime\n", Brushes.DarkGreen, null);

                Int16 i = 0;

                if (VerboseHashCheck("assets\\runtimes\\VC_redist.x64.exe", Const.vc64Hash)[0])
                {
                    DispatchedLogBoxAdd("x64\n", Brushes.Gray, null);
                    Execute.EXE("assets\\runtimes\\VC_redist.x64.exe", "/install /quiet /norestart", true, true);
                    i++;
                }
                else if (VerboseHashCheck("assets\\runtimes\\VC_redist.x86.exe", Const.vc86Hash)[0])
                {
                    Console.WriteLine();
                }

                if (VerboseHashCheck("assets\\runtimes\\VC_redist.x86.exe", Const.vc86Hash)[0])
                {
                    DispatchedLogBoxAdd("x86\n", Brushes.Gray, null);
                    Execute.EXE("assets\\runtimes\\VC_redist.x86.exe", "/install /quiet /norestart", true, true);
                    i++;
                }

                if (i == 0)
                {
                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                }
                else if (i == 2)
                {
                    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                }
                else
                {
                    DispatchedLogBoxAdd("Finished with errors\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.Install = false;
        }

        public void Java()
        {
            try
            {
                DispatchedLogBoxAdd("Installing Java 17 Runtime / JDK\n", Brushes.DarkGreen, null);

                if (VerboseHashCheck("assets\\runtimes\\" + Const.javaName, Const.javaHash)[0])
                {
                    Execute.EXE("assets\\runtimes\\" + Const.javaName, "/s SPONSORS=0", true, true);

                    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
                }
                else
                {
                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.Install = false;
        }

        public void Codecs()
        {
            try
            {
                if (!Directory.Exists("assets\\Codecs"))
                {
                    DispatchedLogBoxAdd("Subdirectory \"assets\\Codecs\" not found, skipping\n\n", Brushes.Red, null);

                    ThreadIsAlive.Install = false;
                    return;
                }

                DispatchedLogBoxAdd("Installing media assets\\Codecs\n", Brushes.DarkGreen, null);

                var result = System.Windows.Forms.MessageBox.Show(
                    "Install all .Appx and .AppxBundle packages from\n\".\\assets\\Codecs\" directory?",
                    "Unverified Install",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    String[] packages = Directory.GetFiles("assets\\Codecs", "*.Appx");
                    List<String> itemsList = packages.ToList<String>();
                    itemsList.AddRange(Directory.GetFiles("assets\\Codecs", "*.AppxBundle"));
                    String[] allPackages = itemsList.ToArray();

                    if (allPackages.Length == 0)
                    {
                        DispatchedLogBoxAdd("No packages found, skipping\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.Install = false;
                        return;
                    }

                    for (Int32 i = 0; i < allPackages.Length; i++)
                    {
                        DispatchedLogBoxAdd("Installing package " + (i + 1) + "/" + allPackages.Length, Brushes.Gray, null);

                        PowerShell("Add-AppPackage -path " + allPackages[i]);

                        if (allPackages.Length > (i + 1))
                        {
                            DispatchedLogBoxAdd("sssssssssssssssssss");
                        }
                    }
                    DispatchedLogBoxAdd("\nDone\n\n", Brushes.DarkCyan, null);
                }
                else
                {
                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.Install = false;
        }

        public void ActivateLockScreen()
        {
            DispatchedLogBoxAdd("Activating lockscreen\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", 0, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

            ThreadIsAlive.LockScreen = false;
        }

        public void DeactivateLockScreen()
        {
            DispatchedLogBoxAdd("Deactivating lockscreen\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", 1, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

            ThreadIsAlive.LockScreen = false;
        }

        public void ActivateLockScreenNotifications()
        {
            DispatchedLogBoxAdd("Activating lockscreen notifications\n", Brushes.DarkGreen, null);

            RegistryIO.DeleteValues("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings", new String[] { "NOC_GLOBAL_SETTING_ALLOW_TOASTS_ABOVE_LOCK" });

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PushNotifications", "LockScreenToastEnabled", 1, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

            ThreadIsAlive.LockScreenNotifications = false;
        }

        public void DeactivateLockScreenNotifications()
        {
            DispatchedLogBoxAdd("Deactivating lockscreen notifications\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PushNotifications", "LockScreenToastEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings", "NOC_GLOBAL_SETTING_ALLOW_TOASTS_ABOVE_LOCK", 1, RegistryValueKind.DWord);

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);

            ThreadIsAlive.LockScreenNotifications = false;
        }

        public void ListUsers()
        {
            try
            {
                String[] usernames = GetSystemUserList();

                foreach (String username in usernames)
                {
                    DispatchedLogBoxAdd(username + "\n", Brushes.Gray, null);
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.AutoLogin = false;
        }

        public void ActivateAutologin(object arg)
        {
            try
            {
                String[] UserInput = (String[])arg;

                String[] usernames = GetSystemUserList();

                DispatchedLogBoxAdd("Setting up Auto-Login\n", Brushes.DarkGreen, null);

                Boolean UserIsValide = false;

                for (Int32 i = 0; i < usernames.Length; i++)
                {
                    if (UserInput[0] == usernames[i])
                    {
                        UserIsValide = true;
                        break;
                    }
                }

                if (UserIsValide && UserInput[1] != "" && UserInput[1].Length <= 256)
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                    "Make system accessable without password?",
                    "Warnig",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.AutoLogin = false;
                        return;
                    }

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DefaultUserName", UserInput[0], RegistryValueKind.String);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DefaultPassword", UserInput[1], RegistryValueKind.String);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "AutoAdminLogon", 1, RegistryValueKind.DWord);

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 1, RegistryValueKind.DWord);

                    DispatchedLogBoxAdd("Activated auto-login for ", Brushes.DarkCyan, null);
                    DispatchedLogBoxAdd(UserInput[0] + "\n\n", Brushes.Red, null);
                }
                else
                {
                    DispatchedLogBoxAdd("User or password invalide\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.AutoLogin = false;
        }

        public void HideUser(object arg)
        {
            try
            {
                String BUser = arg.ToString();

                String[] usernames = GetSystemUserList();

                DispatchedLogBoxAdd("Hiding user\n", Brushes.DarkGreen, null);

                Boolean UserIsValide = false;

                for (Int32 i = 0; i < usernames.Length; i++)
                {
                    if (BUser == usernames[i])
                    {
                        UserIsValide = true;
                        break;
                    }
                }

                if (UserIsValide)
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                    "Hide user " + BUser + "?",
                    "Warnig",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.AutoLogin = false;
                        return;
                    }

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList", BUser, 0, RegistryValueKind.DWord);

                    DispatchedLogBoxAdd("User ", Brushes.DarkCyan, null);
                    DispatchedLogBoxAdd(BUser, Brushes.Red, null);
                    DispatchedLogBoxAdd(" hidden after reboot\n\n", Brushes.DarkCyan, null);

                }
                else
                {
                    DispatchedLogBoxAdd("Invalide user\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.AutoLogin = false;
        }

        public void ShowUser(object arg)
        {
            try
            {
                String BUser = arg.ToString();

                String[] usernames = GetSystemUserList();

                DispatchedLogBoxAdd("Showing user\n", Brushes.DarkGreen, null);

                Boolean UserIsValide = false;

                for (Int32 i = 0; i < usernames.Length; i++)
                {
                    if (BUser == usernames[i])
                    {
                        UserIsValide = true;
                        break;
                    }
                }

                if (UserIsValide)
                {
                    RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\\SpecialAccounts\\UserList", new String[] { BUser });

                    DispatchedLogBoxAdd("User ", Brushes.DarkCyan, null);
                    DispatchedLogBoxAdd(BUser, Brushes.Red, null);
                    DispatchedLogBoxAdd(" visible after reboot\n\n", Brushes.DarkCyan, null);
                }
                else
                {
                    DispatchedLogBoxAdd("Invalide user\n\n", Brushes.Yellow, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.AutoLogin = false;
        }

        public void DeactivateAutologin()
        {
            DispatchedLogBoxAdd("Deactivating autologin\n", Brushes.DarkGreen, null);
            RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", new String[] { "DefaultUserName", "DefaultPassword", "AutoAdminLogon" });

            ThreadIsAlive.AutoLogin = false;
        }

        public void Harden()
        {
            try
            {
                DispatchedLogBoxAdd("Windows Client Hardening\n", Brushes.DarkGreen, null);

                DispatchedLogBoxAdd("Deactivating Local Security Questions\n", Brushes.Gray, null);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "NoLocalPasswordResetQuestions", 1, RegistryValueKind.DWord);

                try
                {
                    DispatchedLogBoxAdd("Stopping and disabling Intel ME service\n", Brushes.Gray, null);
                    Service.SetStartupType("WMIRegistrationService", ServiceStartMode.Disabled);
                }
                catch { }

                DispatchedLogBoxAdd("Require authentification after sleep\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Power\PowerSettings\0e796bdb-100d-47d6-a2d5-f7d2daa51f51", "ACSettingIndex", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Power\PowerSettings\0e796bdb-100d-47d6-a2d5-f7d2daa51f51", "DCSettingIndex", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Deactivating auto hotspot connect\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\config", "AutoConnectAllowedOEM", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Disabling Microsoft Geolocation service\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny", RegistryValueKind.String);

                DispatchedLogBoxAdd("Enable UAC\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 1, RegistryValueKind.DWord);

                if (TestPSCommand("Add-MpPreference"))
                {
                    DispatchedLogBoxAdd("Preventing credential theft\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids 9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2 -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Activating Windows Defender network protection\n", Brushes.Gray, null);
                    PowerShell("Set-MpPreference -EnableNetworkProtection Enabled");

                    DispatchedLogBoxAdd("Activating Windows Defender Sandbox\n", Brushes.Gray, null);
                    Execute.EXE("cmd.exe", "/c \"setx /M MP_FORCE_USE_SANDBOX 1\"", true, false);

                    DispatchedLogBoxAdd("Making VeraCrypt a trusted process\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -ExclusionProcess C:\\Program Files\\VeraCrypt\\VeraCrypt.exe");

                    DispatchedLogBoxAdd("Activating PUA\n", Brushes.Gray, null);
                    PowerShell("Set-MpPreference -PUAProtection enable");

                    DispatchedLogBoxAdd("Preventing Office child process creation\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids D4F940AB-401B-4EFC-AADC-AD5F3C50688A -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Block Office process injection\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids 75668C1F-73B5-4CF0-BB93-3ECF5CB7CC84 -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Block Win32-API calls from Makros\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids 92E97FA1-2EDF-4476-BDD6-9DD0B4DDDC7B -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Block Office from creating executables\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids 3B576869-A4EC-4529-8536-B80A7769E899 -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Block potentially obfuscated scripts\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids 5BEB7EFE-FD9A-4556-801D-275E5FFC04CC -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Block executable content from E-Mail client and Webmail\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids BE9BA2D9-53EA-4CDC-84E5-9B1EEEE46550 -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Block execution of downloaded content via Javascript or VBSscripts\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids D3E037E1-3EB8-44C8-A917-57927947596D -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Block Adobe Reader childprocess creation\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids 7674ba52-37eb-4a4f-a9a1-f0f9a1619a2c -AttackSurfaceReductionRules_Actions Enabled");

                    DispatchedLogBoxAdd("Prevent WMI misuse\n", Brushes.Gray, null);
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids e6db77e5-3df2-4cf1-b95a-636979351e5b -AttackSurfaceReductionRules_Actions Enabled");
                    PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids d1e49aac-8f56-4280-b9ba-993a6d77406c -AttackSurfaceReductionRules_Actions Enabled");
                }
                else
                {
                    DispatchedLogBoxAdd("Windows Defender not installed, skipping ASR-Rules\n", Brushes.Yellow, null);
                }

                DispatchedLogBoxAdd("Activating Windows Defender Passive mode 2\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows Defender", "PassiveMode", 2, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Scan system boot drivers\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\CurrentControlSet\Policies\EarlyLaunch", "DriverLoadPolicy", 3, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Deactivate useractivity upload\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableSettingSync", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Deactivating GameDVR\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Protecting common objects\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager", "ProtectionMode", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Deactivate http printing\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Printers", "DisableWebPnPDownload", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Printers", "DisableHTTPPrint2ing", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Securing Netlogon traffic\n", Brushes.Gray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Netlogon\Parameters", "SealSecureChannel", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Netlogon\Parameters", "SignSecureChannel", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Activating Windows Defender Exploit Guard\n", Brushes.Gray, null); //
                Execute.EXE("powershell.exe", "\"Set-Processmitigation -System -Enable DEP,EmulateAtlThunks,HighEntropy,SEHOP,SEHOPTelemetry,TerminateOnError\"", true, true);

                DispatchedLogBoxAdd("Deactivate WDigest for login\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\WDigest", "UseLogonCredential", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Mimikatz protection (check access requests)\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\LSASS.exe", "AuditLevel", 8, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Activating Remote Credential Guard-Mode (credentials delegation)\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation", "AllowProtectedCreds", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Changing default file opener to notepad.exe (ransomware protection)\n", Brushes.Magenta, null);
                DispatchedLogBoxAdd("hta\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype htafile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("wsh\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype wshfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("wsf\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype wsffile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("bat\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype batfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("js\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype jsfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("jse\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype jsefile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("vbe\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype vbefile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("vbs\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype vbsfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
                DispatchedLogBoxAdd("cmd\n", Brushes.DarkGray, null);
                Execute.EXE("cmd.exe", "/c ftype cmdfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);

                DispatchedLogBoxAdd("Firewall settings\n", Brushes.Magenta, null);
                Execute.EXE("netsh.exe", @"Advfirewall set allprofiles state on", true, true);
                DispatchedLogBoxAdd("Activating logging", Brushes.DarkGray, null);
                DispatchedLogBoxAdd("   (%systemroot%\\system32\\LogFiles\\Firewall\\pfirewall.log)\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", @"advfirewall set currentprofile logging filename %systemroot%\system32\LogFiles\Firewall\pfirewall.log", true, true);
                DispatchedLogBoxAdd("Seting max length to 4096\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", @"netsh advfirewall set currentprofile logging maxfilesize 4096", true, true);
                Execute.EXE("netsh.exe", @"advfirewall set currentprofile logging droppedconnections enable", true, true);
                Execute.EXE("netsh.exe", @"Advfirewall set allprofiles state on", true, true);
                Execute.EXE("netsh.exe", @"advfirewall set publicprofile firewallpolicy blockinboundalways,allowoutbound", true, true);

                DispatchedLogBoxAdd("Blocking Notepad.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block Notepad.exe netconns\" program=\"%systemroot%\\system32\\notepad.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
                DispatchedLogBoxAdd("Blocking regsvr32.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block regsvr32.exe netconns\" program=\"%systemroot%\\system32\\regsvr32.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
                DispatchedLogBoxAdd("Blocking calc.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block calc.exe netconns\" program=\"%systemroot%\\system32\\calc.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
                DispatchedLogBoxAdd("Blocking mshta.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block mshta.exe netconns\" program=\"%systemroot%\\system32\\mshta.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
                DispatchedLogBoxAdd("Blocking wscript.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block wscript.exe netconns\" program=\"%systemroot%\\system32\\wscript.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
                DispatchedLogBoxAdd("Blocking cscript.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block cscript.exe netconns\" program=\"%systemroot%\\system32\\cscript.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
                DispatchedLogBoxAdd("Blocking runscripthelper.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block runscripthelper.exe netconns\" program=\"%systemroot%\\system32\\runscripthelper.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
                DispatchedLogBoxAdd("Blocking hh.exe netconns\n", Brushes.DarkGray, null);
                Execute.EXE("netsh.exe", "advfirewall firewall add rule name=\"Block hh.exe netconns\" program=\"%systemroot%\\system32\\hh.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);

                DispatchedLogBoxAdd("IE and Edge hardening\n", Brushes.Magenta, null);
                DispatchedLogBoxAdd("Activating Smartscreen for Edge\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Policies\Microsoft\MicrosoftEdge\PhishingFilter", "EnabledV9", 1, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("IE software install notifications\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Installer", "SafeForScripting", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Deactivating Edge build-in passwordmanager\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Policies\Microsoft\MicrosoftEdge\Main", "FormSuggest Passwords", "no", RegistryValueKind.String);

                DispatchedLogBoxAdd("Preventing anonymous Sam-Account enumeration\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RestrictAnonymousSAM", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RestrictAnonymous", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "EveryoneIncludesAnonymous", 0, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Biometric security\n", Brushes.Magenta, null);
                DispatchedLogBoxAdd("Anti-spoof for facial recognition\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures", "EnhancedAntiSpoofing", 1, RegistryValueKind.DWord);
                DispatchedLogBoxAdd("Deactivating camera on locked screen", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", 1, RegistryValueKind.DWord);
                DispatchedLogBoxAdd("Deactivating app voice commands in locked state\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoiceAboveLock", 2, RegistryValueKind.DWord);
                DispatchedLogBoxAdd("Deactivating windows voice commands in locked state\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoice", 2, RegistryValueKind.DWord);

                DispatchedLogBoxAdd("Activating advanced Windows Event-Logging\n", Brushes.Magenta, null);
                DispatchedLogBoxAdd("Setting various log files to 1024000kb\n", Brushes.DarkGray, null);
                Execute.EXE("wevtutil.exe", "sl Security /ms:1024000 /f", true, true);
                Execute.EXE("wevtutil.exe", "sl Application /ms:1024000 /f", true, true);
                Execute.EXE("wevtutil.exe", "sl System /ms:1024000 /f", true, true);
                Execute.EXE("wevtutil.exe", "sl \"Windows Powershell\" /ms:1024000 /f", true, true);
                Execute.EXE("wevtutil.exe", "sl \"Microsoft-Windows-PowerShell/Operational\" /ms:1024000 /f", true, true);
                DispatchedLogBoxAdd("Activating Power-Shell logging\n", Brushes.DarkGray, null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\PowerShell\ModuleLogging", "EnableModuleLogging", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging", "EnableScriptBlockLogging", 1, RegistryValueKind.DWord);


                DispatchedLogBoxAdd("Done", Brushes.DarkCyan, null);
                DispatchedLogBoxAdd(" please reboot the computer\n\n", Brushes.Yellow, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.Harden = false;
        }

        //public void ActivateApplicationGuard()
        //{
        //    DispatchedLogBoxAdd("Install Windows-Defender-ApplicationGuard\n", Brushes.DarkGreen, null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Install Windows-Defender-ApplicationGuard?",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
        //        ThreadIsAlive.ApplicationGuard = false;
        //        return;
        //    }
        //
        //    PowerShell.Create().AddCommand("Enable-WindowsOptionalFeature")
        //           .AddParameter("-online")
        //           .AddParameter("FeatureName", "Windows-Defender-ApplicationGuard")
        //           .AddParameter("-norestart")
        //           .Invoke();
        //
        //    DispatchedLogBoxAdd("Done", Brushes.DarkCyan, null);
        //    DispatchedLogBoxAdd(" please reboot the computer\n\n", Brushes.Red, null);
        //}

        //public void DeactivateApplicationGuard()
        //{
        //    DispatchedLogBoxAdd("Deinstall Windows-Defender-ApplicationGuard\n", Brushes.DarkGreen, null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Deinstall Windows-Defender-ApplicationGuard?",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
        //        ThreadIsAlive.ApplicationGuard = false;
        //        return;
        //    }
        //    PowerShell.Create().AddCommand("Disable-WindowsOptionalFeature")
        //           .AddParameter("-online")
        //           .AddParameter("FeatureName", "Windows-Defender-ApplicationGuard")
        //           .AddParameter("-norestart")
        //           .Invoke();
        //
        //    DispatchedLogBoxAdd("Done", Brushes.DarkCyan, null);
        //    DispatchedLogBoxAdd(" please reboot the computer\n\n", Brushes.Red, null);
        //}

        //public void DeactivateVBS()
        //{
        //    DispatchedLogBoxAdd("Deactivating Virtualization Based Security\n", Brushes.DarkGreen, null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Deactivate Virtualization Based Security",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
        //        ThreadIsAlive.VBS = false;
        //        return;
        //    }
        //
        //    try
        //    {
        //        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", true))
        //        {
        //            key.DeleteValue("EnableVirtualizationBasedSecurity", false);
        //            key.DeleteValue("RequirePlatformSecurityFeatures", false);
        //            key.DeleteValue("LsaCfgFlags", false);
        //        }
        //    }
        //    catch (System.ArgumentException) { }
        //
        //    DispatchedLogBoxAdd("Done", Brushes.DarkCyan, null);
        //    DispatchedLogBoxAdd(" please reboot the computer\n\n", Brushes.Red, null);
        //}
        //
        //public void ActivateVBS()
        //{
        //    DispatchedLogBoxAdd("Activating Virtualization Based Security\n", Brushes.DarkGreen, null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Activate Virtualization Based Security",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
        //        ThreadIsAlive.VBS = false;
        //        return;
        //    }
        //
        //    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", "EnableVirtualizationBasedSecurity", 1, RegistryValueKind.DWord);
        //    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", "RequirePlatformSecurityFeatures", 3, RegistryValueKind.DWord);
        //    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", "LsaCfgFlags", 1, RegistryValueKind.DWord);
        //
        //    DispatchedLogBoxAdd("Done", Brushes.DarkCyan, null);
        //    DispatchedLogBoxAdd(" please reboot the computer\n\n", Brushes.Red, null);
        //}

        public void AllowUSBCode()
        {
            DispatchedLogBoxAdd("Allowing untrustworthy / unsigned code execution from USB\n", Brushes.DarkGreen, null);

            var result0 = System.Windows.Forms.MessageBox.Show(
                "Allowing untrustworthy / unsigned\ncode execution from USB?",
                "",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result0 == System.Windows.Forms.DialogResult.No)
            {
                DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                ThreadIsAlive.USBExecution = false;
                return;
            }

            PowerShell("Remove-MpPreference -AttackSurfaceReductionRules_Ids b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4");

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.USBExecution = false;
        }

        public void BlockUSBCode()
        {
            DispatchedLogBoxAdd("Blocking untrustworthy / unsigned code execution from USB (ASRR: b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4)\n", Brushes.DarkGreen, null);

            var result0 = System.Windows.Forms.MessageBox.Show(
                "Block untrustworthy / unsigned\ncode execution from USB?",
                "",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result0 == System.Windows.Forms.DialogResult.No)
            {
                DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                ThreadIsAlive.USBExecution = false;
                return;
            }

            PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Ids b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4");
            PowerShell("Add-MpPreference -AttackSurfaceReductionRules_Actions Enabled");

            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            ThreadIsAlive.USBExecution = false;
        }

        //public void DeactivateCFG()
        //{
        //    DispatchedLogBoxAdd("Deactivating Windows Defender Exploit Guard 'Control Flow Guard' extension\n", Brushes.DarkGreen, null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Deactivating Windows Defender Exploit Guard\n'Control Flow Guard' extension?",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
        //        ThreadIsAlive.CFG = false;
        //        return;
        //    }
        //
        //    Execute.EXE("powershell.exe", "\"Set-Processmitigation -System -Disable CFG\"", true, true);
        //
        //    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        //    ThreadIsAlive.CFG = false;
        //}
        //
        //public void ActivateCFG()
        //{
        //    DispatchedLogBoxAdd("Activating Windows Defender Exploit Guard 'Control Flow Guard' extension\n", Brushes.DarkGreen, null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Activating Windows Defender Exploit Guard\n'Control Flow Guard' extension?\nWill cause issues with apps like Discord.",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Warning);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
        //        ThreadIsAlive.CFG = false;
        //        return;
        //    }
        //
        //    Execute.EXE("powershell.exe", "\"Set-Processmitigation -System -Enable CFG\"", true, true);
        //
        //    DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        //    ThreadIsAlive.CFG = false;
        //}

        public void UserAutostart()
        {
            Execute.EXE("explorer.exe", "\"C:\\Users\\" + UserPath + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\"", false, false);
        }

        public void GlobalAutostart()
        {
            Execute.EXE("explorer.exe", "\"C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\"", false, false);
        }

        public void StandartBootMenuePolicy()
        {
            DispatchedLogBoxAdd("Resetting recovery menu to its defaults\n", Brushes.DarkGreen, null);
            Execute.EXE("bcdedit.exe", "/set {current} bootmenupolicy Standard", false, true);
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void LegacyBootMenuePolicy()
        {
            DispatchedLogBoxAdd("Enabeling legacy recovery menu (press F8 during boot to enter)\n", Brushes.DarkGreen, null);
            Execute.EXE("bcdedit.exe", "/set {current} bootmenupolicy Legacy", false, true);
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void SetMaxFailedLoginAttemptsBoot(object UserInput)
        {
            Int32 WValue = 2;

            if ((String)UserInput == "off")
            {
                WValue = 0;
            }
            else
            {
                try
                {
                    WValue = Int32.Parse((String)UserInput);
                }
                catch (Exception)
                {
                }

                if (WValue == 0 || (WValue >= 4 && WValue <= 99)) { }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                    "List of valide numbers:\n\n'0' / 'off'\n'4-99'",
                    "Invalide input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                    ThreadIsAlive.MaxFailedLoginAttempts = false;
                    return;
                }
            }

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "MaxDevicePasswordFailedAttempts", WValue, RegistryValueKind.DWord);

            if (WValue != 0)
            {
                DispatchedLogBoxAdd("Activated automatic reboot\nafter ", Brushes.DarkCyan, null);
                DispatchedLogBoxAdd(WValue.ToString(), Brushes.Red, null);
                DispatchedLogBoxAdd(" failed login attempts\n\n", Brushes.DarkCyan, null);
            }
            else
            {
                DispatchedLogBoxAdd("Deactivated automatic reboot\nafter n failed login attempts\n\n", Brushes.DarkCyan, null);
            }

            ThreadIsAlive.MaxFailedLoginAttempts = false;
        }

        public void SetScreenTimeout(object UserInput)
        {
            Int32 WValue = -1;

            if ((String)UserInput == "off")
            {
                WValue = 0;
            }
            else
            {
                try
                {
                    WValue = Int32.Parse((String)UserInput);
                }
                catch (Exception)
                {
                }

                if (WValue >= 0 && WValue <= 599940) { }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                    "List of valide numbers:\n\n'0' / 'off'\n'0-599940'",
                    "Invalide input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                    ThreadIsAlive.ScreenTimeOut = false;
                    return;
                }
            }

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "InactivityTimeoutSecs", WValue, RegistryValueKind.DWord);

            if (WValue != 0)
            {
                DispatchedLogBoxAdd("Activated user inactivity screen lock\nafter ", Brushes.DarkCyan, null);
                DispatchedLogBoxAdd(WValue.ToString(), Brushes.Red, null);
                DispatchedLogBoxAdd(" seconds ", Brushes.DarkCyan, null);
                DispatchedLogBoxAdd("please restart the computer\n\n", Brushes.Yellow, null);
            }
            else
            {
                DispatchedLogBoxAdd("Deactivated automatic screenlock\n\n", Brushes.DarkCyan, null);
            }

            ThreadIsAlive.ScreenTimeOut = false;
        }

        public void SystemLock(object arg)
        {
            try
            {
                if (ThisMachine.OSType is not OSType.Server)
                {
                    DispatchedLogBoxAdd("Not supported on Windows Server, use GPO\n\n", Brushes.Yellow, null);

                    ThreadIsAlive.SystemLock = false;
                    return;
                }

                String[] UserInput = (String[])arg;

                DispatchedLogBoxAdd("Setting account lockout policy\n", Brushes.DarkGreen, null);

                Int32 attempts = -1;
                Int32 duration = -1;

                try
                {
                    attempts = Int32.Parse(UserInput[0]);
                    if (attempts == 0)
                    {
                        goto skip;
                    }
                }
                catch (Exception)
                {
                    if (UserInput[0].ToString().ToLower() == "off")
                    {
                        goto skip;
                    }
                }

                try
                {
                    duration = Int32.Parse(UserInput[1]);
                }
                catch (Exception)
                {
                    if (UserInput[1].ToString().ToLower() == "manual")
                    {
                        duration = 0;
                    }
                }

                if (attempts >= 0 && attempts <= 999 && duration >= 0 && duration <= 999) { }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                    "List of valide numbers for\n\nMax. attempts: '(0/off)-999'\nLock duration: '(0/manual)-99999' [time in minutes]",
                    "Invalide input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                    ThreadIsAlive.SystemLock = false;
                    DispatchedLogBoxAdd("Invalide input\n\n", Brushes.Yellow, null);
                    return;
                }

                if (duration == 0)
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                    "Make accounts only unlockable by administrators?",
                    "Warnig",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.SystemLock = false;
                        return;
                    }
                }

                if (attempts != 0)
                {
                    Execute.EXE("net.exe", "accounts /lockoutthreshold:" + attempts, true, true);
                    if (duration == 0)
                    {
                        Execute.EXE("net.exe", "accounts /lockoutwindow:1", true, true);
                    }
                    else
                    {
                        Execute.EXE("net.exe", "accounts /lockoutwindow:" + duration, true, true);
                    }
                    Execute.EXE("net.exe", "accounts /lockoutduration:" + duration, true, true);

                    DispatchedLogBoxAdd("Done ", Brushes.DarkCyan, null);
                    DispatchedLogBoxAdd("(" + attempts + ", " + duration + ")\n\n", Brushes.DarkGray, null);
                    ThreadIsAlive.SystemLock = false;
                    return;
                }

            skip:
                Execute.EXE("net.exe", "accounts /lockoutthreshold:0", false, true);
                DispatchedLogBoxAdd("Deactivated automatic account lock", Brushes.DarkCyan, null);
                DispatchedLogBoxAdd(UserInput[0] + "\n\n", Brushes.Red, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

        }

        public void CheckHealth()
        {
            DispatchedLogBoxAdd("Checking systemlogs for errors\n", Brushes.DarkGreen, null);
            Execute.EXE("cmd.exe", "/k DISM /Online /Cleanup-Image /CheckHealth", false, true);
            ThreadIsAlive.SystemCheck = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void ScanHealth()
        {
            DispatchedLogBoxAdd("Checking component-store for errors\n", Brushes.DarkGreen, null);
            Execute.EXE("cmd.exe", "/k DISM /Online /Cleanup-Image /ScanHealth", false, true);
            ThreadIsAlive.SystemCheck = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void RestoreHealth()
        {
            DispatchedLogBoxAdd("Checking WinSxS for errors + automatic repair\n", Brushes.DarkGreen, null);
            Execute.EXE("cmd.exe", "/k DISM /Online /Cleanup-Image /RestoreHealth", false, true);
            ThreadIsAlive.SystemCheck = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void SFCScan()
        {
            DispatchedLogBoxAdd("Checking system for errors + automatic repair\n", Brushes.DarkGreen, null);
            Execute.EXE("cmd.exe", "/k sfc /scannow", false, true);
            ThreadIsAlive.SystemCheck = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void YesLoginBlur()
        {
            DispatchedLogBoxAdd("Changing logins screen attribute | activating blur\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\Microsoft\Windows\System", "DisableAcrylicBackgroundOnLogon", 0, RegistryValueKind.DWord);

            ThreadIsAlive.LoginBlur = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void NoLoginBlur()
        {
            DispatchedLogBoxAdd("Changing logins screen attribute | deactivating blur\n", Brushes.DarkGreen, null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\Microsoft\Windows\System", "DisableAcrylicBackgroundOnLogon", 1, RegistryValueKind.DWord);

            ThreadIsAlive.LoginBlur = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void ExportWLanConfig(object box)
        {
            try
            {
                String path = box.ToString();

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                try
                {
                    if (Service.GetServiceStatus("WlanSvc") != "running")
                    {
                        DispatchedLogBoxAdd("WlanSvc offline, starting service..\n", Brushes.DarkGray, null);
                        Execute.EXE("net.exe", "start WlanSvc", true, true);
                    }

                    DispatchedLogBoxAdd("Wifi profile export\n", Brushes.DarkGreen, null);
                }
                catch (System.InvalidOperationException)
                {
                    DispatchedLogBoxAdd("WlanSvc missing\n\n", Brushes.Yellow, null);
                    ThreadIsAlive.IOWLanConfig = false;
                    return;
                }

                //https://github.com/emoacht/ManagedNativeWifi

                var names = NativeWifi.EnumerateProfileNames();
                String[] profiles = names.Select(x => x.ToString()).ToArray();

                if (profiles.Length == 0)
                {

                    DispatchedLogBoxAdd(path + "\n", Brushes.DarkGray, null);
                    DispatchedLogBoxAdd("No wifi profiles found\n\n", Brushes.Yellow, null);
                    ThreadIsAlive.IOWLanConfig = false;
                    return;
                }

                if (AllProfilesWLan)
                {
                    DispatchedLogBoxAdd(path + "\n", Brushes.DarkGray, null);

                    if (Service.GetServiceStatus("WlanSvc") != "running")
                    {
                        DispatchedLogBoxAdd("WlanSvc offline, starting service..\n", Brushes.DarkGray, null);
                        Execute.EXE("net.exe", "start WlanSvc", true, true);
                    }

                    Execute.EXE("netsh.exe", "wlan export profile key=clear folder=" + path, true, true);

                    if (profiles.Length == 1)
                    {
                        DispatchedLogBoxAdd("Exported 1 profile\n", Brushes.Gray, null);
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Exported " + profiles.Length + " profiles\n", Brushes.Gray, null);
                    }
                }
                else
                {
                    DispatchedLogBoxAdd("Exporting single profile\n\n", Brushes.DarkGreen, null);
                    DispatchedLogBoxAdd("==Profiles==\n", Brushes.DarkGray, null);

                    foreach (String namee in profiles)
                    {
                        DispatchedLogBoxAdd(namee + "\n", Brushes.Gray, null);
                    }

                    Console.WriteLine();

                    String input = Microsoft.VisualBasic.Interaction.InputBox("Export Profile",
                           "Enter network name",
                           "",
                           0,
                           0);

                    if (profiles.Contains(input))
                    {
                        if (Service.GetServiceStatus("WlanSvc") != "running")
                        {
                            DispatchedLogBoxAdd("WlanSvc offline, starting service..\n", Brushes.DarkGray, null);
                            Execute.EXE("net.exe", "start WlanSvc", true, true);
                        }

                        DispatchedLogBoxAdd("Exporting wifi Profile \"" + input + "\" to\n", Brushes.Gray, null);
                        DispatchedLogBoxAdd(path + "\n", Brushes.DarkGray, null);
                        Execute.EXE("netsh.exe", "wlan export profile \"" + input + "\" key=clear folder=" + path, true, true);
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Invalide input\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.IOWLanConfig = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.IOWLanConfig = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void ImportWLanConfig(object box)
        {
            try
            {
                String path = box.ToString();

                if (AllProfilesWLan)
                {
                    String[] files;

                    try
                    {
                        if (Service.GetServiceStatus("WlanSvc") != "running")
                        {
                            DispatchedLogBoxAdd("WlanSvc offline, starting service..\n", Brushes.DarkGray, null);
                            Execute.EXE("net.exe", "start WlanSvc", true, true);
                        }

                        DispatchedLogBoxAdd("Wifi profile export\n", Brushes.DarkGreen, null);
                    }
                    catch (System.InvalidOperationException)
                    {
                        DispatchedLogBoxAdd("WlanSvc missing\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.IOWLanConfig = false;
                        return;
                    }

                    try
                    {
                        files = Directory.GetFiles(path, "*.xml");
                    }
                    catch (Exception)
                    {
                        DispatchedLogBoxAdd("Invalide path\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.IOWLanConfig = false;
                        return;
                    }

                    if (files.Length == 0)
                    {
                        DispatchedLogBoxAdd("No WLan profile files found in " + path + "\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.IOWLanConfig = false;
                        return;
                    }

                    DispatchedLogBoxAdd("Importing all WLan Profiles from\n", Brushes.DarkGreen, null);
                    DispatchedLogBoxAdd(path + "\n", Brushes.DarkGray, null);

                    foreach (String file in files)
                    {
                        Execute.EXE("netsh.exe", "wlan add profile filename=\"" + file + "\"", true, true);
                    }

                    if (files.Length == 1)
                    {
                        DispatchedLogBoxAdd("Imported 1 profile\n", Brushes.Gray, null);
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Imported " + files.Length + " profiles\n", Brushes.Gray, null);
                    }
                }
                else
                {
                    DispatchedLogBoxAdd("Importing single profile\n", Brushes.DarkGreen, null);

                    if (getfileagrs == null)
                    {
                        DispatchedLogBoxAdd("\nWaiting for backgroundtask to finish\n\n", Brushes.Yellow, null);

                        while (getfileagrs == null)
                        {
                            Task.Delay(256).Wait();
                        }
                    }

                    getfileagrs = new String[] { path, "Profile (*.xml)|*.xml|All files (*.*)|*.*", "Select Wifi Profile File" };

                    ThreadStart childref = new(GetFile);
                    Thread childThread = new(childref);
                    childThread.SetApartmentState(ApartmentState.STA);
                    childThread.Start();

                    while (getfileagrs[0] == path)
                    {
                        Task.Delay(256).Wait();
                    }

                    String file = getfileagrs[0];
                    getfileagrs = new String[] { null };

                    if (file == "cancled")
                    {
                        ThreadIsAlive.IOWLanConfig = false;
                        DispatchedLogBoxAdd("Cancled\n\n", Brushes.Yellow, null);
                        return;
                    }

                    if (Service.GetServiceStatus("WlanSvc") != "running")
                    {
                        DispatchedLogBoxAdd("WlanSvc offline, starting service..\n", Brushes.DarkGray, null);
                        Execute.EXE("net.exe", "start WlanSvc", true, true);
                    }

                    DispatchedLogBoxAdd("Importing WLan Profile \"" + file + "\"\n", Brushes.Gray, null);
                    Execute.EXE("netsh.exe", "wlan add profile filename=\"" + file + "\"", true, true);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.IOWLanConfig = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void LSASS_ON()
        {
            try
            {
                Boolean Policy = false;
                Boolean UEFI = false;

                MessageBoxManager.Yes = "Policy";
                MessageBoxManager.No = "Toggle Setting";
                MessageBoxManager.Register();

                DialogResult MSGOUT0 = System.Windows.Forms.MessageBox.Show(
                                "You are about to activate a LSASS hardening feature, do you wish to apply this as a policy (Registry GPO) or as a setting, the latter allows you to later deactivate this feature via the Windows GUI.",
                                "Protected Process Light",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Information);

                MessageBoxManager.Unregister();

                switch (MSGOUT0)
                {
                    case System.Windows.Forms.DialogResult.Cancel:
                        DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.LSASS = false;
                        return;
                    case System.Windows.Forms.DialogResult.Yes:
                        Policy = true;
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        Policy = false;
                        break;
                }

                if (ThisMachine.IsUEFI)
                {
                    if (ThisMachine.OSMajorVersion >= 22621)
                    {
                        DialogResult MSGOUT1 = System.Windows.Forms.MessageBox.Show(
                                "Do you wish to use UEFI Variables for policy enforcement?\n\nNote that in order to use UEFI Variables, Secure Boot must be enabled, otherwise it will only be active if the Registry value is present upon boot.\n\nInstructions to remove the EFI variable can be found on Microsoft.com under #how-to-remove-the-lsa-protection-uefi-variable.",
                                "Protected Process Light",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);

                        switch (MSGOUT1)
                        {
                            case System.Windows.Forms.DialogResult.Cancel:
                                DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                                ThreadIsAlive.LSASS = false;
                                return;
                            case System.Windows.Forms.DialogResult.Yes:
                                UEFI = true;
                                break;
                            case System.Windows.Forms.DialogResult.No:
                                UEFI = false;
                                break;
                        }
                    }
                    else
                    {
                        UEFI = true;
                    }
                }
                else
                {
                    UEFI = false;
                }

                MessageBoxManager.Yes = "Yes";
                MessageBoxManager.No = "Cancle";
                MessageBoxManager.Register();

                DialogResult MSGOUT2 = System.Windows.Forms.MessageBox.Show(
                               "Additional note on Third - party authentication modules:\n- Modules must be digitally signed with a Microsoft signature\n- Modules must comply with the Microsoft Security Development Lifecycle (SDL)\nMore information can be found on Microsoft.com under 'Protected process requirements for plug-ins or drivers'",
                                "Protected Process Light",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

                MessageBoxManager.Unregister();

                if (MSGOUT2 == System.Windows.Forms.DialogResult.No)
                {
                    DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                    ThreadIsAlive.LSASS = false;
                    return;
                }

                DispatchedLogBoxAdd("LSASS hardening\n", Brushes.DarkGreen, null);

                if (Policy)
                {
                    if (UEFI)
                    {
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "RunAsPPL", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "LSASSasEFIvar", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "RunAsPPL", 2, RegistryValueKind.DWord);
                    }
                }
                else
                {
                    if (UEFI)
                    {
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RunAsPPL", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "LSASSasEFIvar", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RunAsPPL", 2, RegistryValueKind.DWord);
                    }
                }

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RunAsPPLBoot", 2, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.LSASS = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void LSASS_OFF()
        {
            static void RemoveRegValues()
            {
                RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa", new String[] { "RunAsPPL", "RunAsPPLBoot" });
                RegistryIO.DeleteValues("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\System", new String[] { "RunAsPPL" });
            }

            try
            {
                if (RegistryIO.TestRegValuePresense("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "LSASSasEFIvar"))
                {
                    DialogResult MSGOUT0 = System.Windows.Forms.MessageBox.Show(
                    "LSASS has previously been configured with EFI Variables, in order to deactivate the LSASS protection follow Microsoft's guide on how to remove UEFI variables.\n\nContinue to remove Registry values?",
                    "Protected Process Light",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                    if (MSGOUT0 == System.Windows.Forms.DialogResult.No)
                    {
                        DispatchedLogBoxAdd("Nothing changed, skipping\n\n", Brushes.Yellow, null);
                        ThreadIsAlive.LSASS = false;
                        return;
                    }
                    else
                    {
                        DispatchedLogBoxAdd("Removing LSASS protection*\n", Brushes.DarkGreen, null);

                        DispatchedLogBoxAdd("To remove UEFI Variables: --> ", Brushes.DarkGray, null);
                        DispatchedLogBoxAdd("https://learn.microsoft.com/en-us/windows-server/security/credentials-protection-and-management/configuring-additional-lsa-protection#how-to-remove-the-lsa-protection-uefi-variable\n", Brushes.Cyan, null);

                        RemoveRegValues();

                        DispatchedLogBoxAdd("Changes will apply after a reboot\n", Brushes.Gray, null);
                    }
                }
                else
                {
                    DispatchedLogBoxAdd("Removing LSASS protection*\n", Brushes.DarkGreen, null);

                    RemoveRegValues();

                    DispatchedLogBoxAdd("Changes will apply after a reboot\n", Brushes.Gray, null);

                    DispatchedLogBoxAdd("You might need to remove UEFI variables or deactivate Secure Boot to fully deactivate the LSASS protection.\n", Brushes.Magenta, null);

                    DispatchedLogBoxAdd("To remove UEFI Variables: --> ", Brushes.DarkGray, null);

                    DispatchedLogBoxAdd("https://learn.microsoft.com/en-us/windows-server/security/credentials-protection-and-management/configuring-additional-lsa-protection#how-to-remove-the-lsa-protection-uefi-variable\n", Brushes.Cyan, null);
                }
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.LSASS = false;
            DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
        }

        public void YesTCPTune()
        {
            try
            {
                DispatchedLogBoxAdd("Activating TCP Tuning\n", Brushes.DarkGreen, null);

                Execute.EXE("netsh.exe", "interface tcp set global autotuninglevel=normal", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.TCPTune = false;
        }

        public void NoTCPTune()
        {
            try
            {
                DispatchedLogBoxAdd("Deactivating TCP Tuning\n", Brushes.DarkGreen, null);

                Execute.EXE("netsh.exe", "interface tcp set global autotuning=disabled", true, true);

                DispatchedLogBoxAdd("Done\n\n", Brushes.DarkCyan, null);
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n\n", Brushes.Red, null);
            }

            ThreadIsAlive.TCPTune = false;
        }

        //#######################################################################################################################

        public void Debug()
        {




        }

        //#######################################################################################################################

        public String[] GetSystemUserList()
        {
            return WindownsAccountInteract.GetSystemUserList();
        }

        public void GetFile()
        {
            using System.Windows.Forms.OpenFileDialog openFileDialog = new();
            openFileDialog.InitialDirectory = getfileagrs[0];
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = getfileagrs[1];
            openFileDialog.FilterIndex = 0;
            openFileDialog.Title = getfileagrs[2];

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                getfileagrs[0] = openFileDialog.FileName;
                return;
            }
            else
            {
                getfileagrs[0] = "cancled";
                return;
            }
        }
    }























}