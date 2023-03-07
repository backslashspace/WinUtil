using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using System.Management.Automation;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Management;
using System.ServiceProcess;
using System.Windows.Forms;
using ManagedNativeWifi;

namespace WinUtil
{
    public class Functions : MainWindow
    {
        public static string[] getfileagrs = { null };

        public static void ActivateWindows()
        {
            bool redo = false;

            if (ActivationClicks == 0)
            {
                ActivationClicks++;
            }
            else
            {
                var result0 = System.Windows.Forms.MessageBox.Show(
                "Re-Check Windows license status?",
                "WMI",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

                if (result0 == System.Windows.Forms.DialogResult.No)
                {
                    Write("Nothing changed, skipping\n\n", "darkyellow", null);
                    ThreadIsAlive.ActivateWindows = false;
                    return;
                }

                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\WinUtil", true))
                    {
                        key.DeleteValue("Windows Activation Status", false);
                    }
                }
                catch (System.ArgumentException) { }

                Start("cmd.exe", "/c powershell -command \"$test = Get-CimInstance SoftwareLicensingProduct -Filter \\\"Name like 'Windows%'\\\" | where { $_.PartialProductKey } | select LicenseStatus; $test = $test -replace \\\"@{LicenseStatus=\\\"; $test = $test -replace \\\"}\\\"; reg add \\\"HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil\\\" /v \\\"Windows Activation Status\\\" /t reg_dword /d $test /f\"", true, false);

                Write("Re-Check Windows license status\n\n", "darkyellow", null);

                redo = true;
            }

            if (!TestRegValuePresense("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Windows Activation Status") && !redo)
            {
                Write("Waiting for backgroundtask to finish\n\n", "darkyellow", null);
            }
            int i = 0;
            while (!TestRegValuePresense("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Windows Activation Status"))
            {
                Thread.Sleep(500);

                if (i >= 180)
                {
                    Write("Couldn't get license information, restart application.\n\n", "darkyellow", null);
                    ThreadIsAlive.ActivateWindows = false;
                    return;
                }
                i++;
            }

            long status = SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "Windows Activation Status", RegistryValueKind.DWord, 1);

            if (status != 1)
            {
                if (IsServer)
                {
                    Write("Activating Server (KMS38)..\n", "darkgreen", null);

                    InternExtract("temp\\", "KMS38_Activation.cmd", "winactivator.KMS38_Activation.cmd");

                    Start("cmd.exe", "/c \"temp\\KMS38_Activation.cmd\"", true, true);
                    File.Delete("temp\\KMS38_Activation.cmd");

                    Write(status + " --> 1\n", "darkgreen", null);
                }
                else
                {
                    Write("Activating Client (HWID)..\n", "darkgreen", null);

                    InternExtract("temp\\", "HWID_Activation.cmd", "winactivator.HWID_Activation.cmd");
                    Start("cmd.exe", "/c \"temp\\HWID_Activation.cmd\"", true, true);
                    File.Delete("temp\\HWID_Activation.cmd");

                    Write(status + " --> 1\n", "darkgreen", null);
                }

                Write("Activated Windows\n\n", "darkcyan", null);
            }
            else
            {
                Write("Windows is already licensed\n\n", "darkcyan", null);
            }

            ThreadIsAlive.ActivateWindows = false;
        }

        public static void General()
        {
            Write("Running general settings\n", "darkgreen", null);

            Write("Activating Windows Defender Sandbox\n", "gray", null);
            Start("cmd.exe", "/c \"setx /M MP_FORCE_USE_SANDBOX 1\"", true, false);

            Write("Setting VeraCrypt as Trusted Process\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-ExclusionProcess", "C:\\Program Files\\VeraCrypt\\VeraCrypt.exe")
                   .Invoke();

            Write("Disabling lockscreen tips and tricks\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "ContentDeliveryAllowed", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\Subscriptions\338387", "SubscriptionContext", "sc-mode=0", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Lock Screen", "SlideshowEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 1, RegistryValueKind.DWord);
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true))
            {
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    if (subKeyName.Contains("$lockscreenpinnedtiles$windows.data.curatedtilecollection.tilecollection"))
                    {
                        byte[] bytes = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x56, 0x51, 0x4C, 0x29, 0xDC, 0x12, 0xD9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0x0A, 0x0A, 0x00, 0xCA, 0x32, 0x00, 0xCC, 0x83, 0x12, 0x03, 0x26, 0x7B, 0x00, 0x32, 0x00, 0x43, 0x00, 0x46, 0x00, 0x43, 0x00, 0x44, 0x00, 0x46, 0x00, 0x46, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x45, 0x00, 0x37, 0x00, 0x31, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x38, 0x00, 0x45, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x39, 0x00, 0x45, 0x00, 0x32, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x33, 0x00, 0x38, 0x00, 0x44, 0x00, 0x31, 0x00, 0x35, 0x00, 0x36, 0x00, 0x36, 0x00, 0x46, 0x00, 0x30, 0x00, 0x37, 0x00, 0x31, 0x00, 0x7D, 0x00, 0x26, 0x7B, 0x00, 0x44, 0x00, 0x31, 0x00, 0x38, 0x00, 0x43, 0x00, 0x45, 0x00, 0x34, 0x00, 0x43, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x43, 0x00, 0x38, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x43, 0x00, 0x31, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x41, 0x00, 0x41, 0x00, 0x36, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x32, 0x00, 0x33, 0x00, 0x33, 0x00, 0x46, 0x00, 0x46, 0x00, 0x30, 0x00, 0x44, 0x00, 0x32, 0x00, 0x41, 0x00, 0x35, 0x00, 0x43, 0x00, 0x39, 0x00, 0x7D, 0x00, 0x26, 0x7B, 0x00, 0x45, 0x00, 0x44, 0x00, 0x38, 0x00, 0x35, 0x00, 0x42, 0x00, 0x36, 0x00, 0x43, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x33, 0x00, 0x39, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x31, 0x00, 0x38, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x36, 0x00, 0x41, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x46, 0x00, 0x39, 0x00, 0x39, 0x00, 0x36, 0x00, 0x44, 0x00, 0x45, 0x00, 0x38, 0x00, 0x31, 0x00, 0x37, 0x00, 0x42, 0x00, 0x39, 0x00, 0x37, 0x00, 0x7D, 0x00, 0x00, 0xCD, 0x0A, 0x12, 0x0A, 0x03, 0x26, 0x7B, 0x00, 0x32, 0x00, 0x43, 0x00, 0x46, 0x00, 0x43, 0x00, 0x44, 0x00, 0x46, 0x00, 0x46, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x45, 0x00, 0x37, 0x00, 0x31, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x38, 0x00, 0x45, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x39, 0x00, 0x45, 0x00, 0x32, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x33, 0x00, 0x38, 0x00, 0x44, 0x00, 0x31, 0x00, 0x35, 0x00, 0x36, 0x00, 0x36, 0x00, 0x46, 0x00, 0x30, 0x00, 0x37, 0x00, 0x31, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xF3, 0xBF, 0xF3, 0xE7, 0x02, 0x24, 0x9C, 0xCE, 0x03, 0x44, 0xE4, 0x91, 0x01, 0x66, 0x9E, 0xD7, 0x8C, 0xEC, 0xD8, 0xC2, 0x99, 0xF8, 0x71, 0x00, 0xD2, 0x0A, 0x52, 0x50, 0x00, 0x7E, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x6D, 0x00, 0x75, 0x00, 0x6E, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x61, 0x00, 0x70, 0x00, 0x70, 0x00, 0x73, 0x00, 0x5F, 0x00, 0x38, 0x00, 0x77, 0x00, 0x65, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x62, 0x00, 0x33, 0x00, 0x64, 0x00, 0x38, 0x00, 0x62, 0x00, 0x62, 0x00, 0x77, 0x00, 0x65, 0x00, 0x21, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x6C, 0x00, 0x69, 0x00, 0x76, 0x00, 0x65, 0x00, 0x2E, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x61, 0x00, 0x72, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x32, 0x00, 0x00, 0x26, 0x7B, 0x00, 0x44, 0x00, 0x31, 0x00, 0x38, 0x00, 0x43, 0x00, 0x45, 0x00, 0x34, 0x00, 0x43, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x43, 0x00, 0x38, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x43, 0x00, 0x31, 0x00, 0x41, 0x00, 0x2D, 0x00, 0x41, 0x00, 0x41, 0x00, 0x36, 0x00, 0x45, 0x00, 0x2D, 0x00, 0x32, 0x00, 0x33, 0x00, 0x33, 0x00, 0x46, 0x00, 0x46, 0x00, 0x30, 0x00, 0x44, 0x00, 0x32, 0x00, 0x41, 0x00, 0x35, 0x00, 0x43, 0x00, 0x39, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xCA, 0xC9, 0xB3, 0x8C, 0x0D, 0x24, 0x8E, 0x19, 0x44, 0x9A, 0x98, 0x01, 0x66, 0xAA, 0xDD, 0x8D, 0xF9, 0x83, 0xDE, 0xF4, 0xD2, 0xC9, 0x01, 0x00, 0xD2, 0x0A, 0x4E, 0x50, 0x00, 0x7E, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x6D, 0x00, 0x75, 0x00, 0x6E, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x61, 0x00, 0x70, 0x00, 0x70, 0x00, 0x73, 0x00, 0x5F, 0x00, 0x38, 0x00, 0x77, 0x00, 0x65, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x62, 0x00, 0x33, 0x00, 0x64, 0x00, 0x38, 0x00, 0x62, 0x00, 0x62, 0x00, 0x77, 0x00, 0x65, 0x00, 0x21, 0x00, 0x6D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x77, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x64, 0x00, 0x6F, 0x00, 0x77, 0x00, 0x73, 0x00, 0x6C, 0x00, 0x69, 0x00, 0x76, 0x00, 0x65, 0x00, 0x2E, 0x00, 0x6D, 0x00, 0x61, 0x00, 0x69, 0x00, 0x6C, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x31, 0x00, 0x00, 0x26, 0x7B, 0x00, 0x45, 0x00, 0x44, 0x00, 0x38, 0x00, 0x35, 0x00, 0x42, 0x00, 0x36, 0x00, 0x43, 0x00, 0x42, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x33, 0x00, 0x39, 0x00, 0x34, 0x00, 0x2D, 0x00, 0x34, 0x00, 0x31, 0x00, 0x38, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x38, 0x00, 0x36, 0x00, 0x41, 0x00, 0x43, 0x00, 0x2D, 0x00, 0x46, 0x00, 0x39, 0x00, 0x39, 0x00, 0x36, 0x00, 0x44, 0x00, 0x45, 0x00, 0x38, 0x00, 0x31, 0x00, 0x37, 0x00, 0x42, 0x00, 0x39, 0x00, 0x37, 0x00, 0x7D, 0x00, 0x0A, 0x05, 0xCB, 0xED, 0x96, 0xEC, 0x0E, 0x24, 0x94, 0xA7, 0x01, 0x44, 0x83, 0x83, 0x01, 0x66, 0x86, 0xD9, 0xE6, 0xB7, 0xE9, 0xBB, 0xE0, 0xBD, 0x97, 0x01, 0x00, 0xD2, 0x0A, 0x26, 0x50, 0x00, 0x7E, 0x00, 0x4D, 0x00, 0x69, 0x00, 0x63, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x66, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x53, 0x00, 0x6B, 0x00, 0x79, 0x00, 0x70, 0x00, 0x65, 0x00, 0x41, 0x00, 0x70, 0x00, 0x70, 0x00, 0x5F, 0x00, 0x6B, 0x00, 0x7A, 0x00, 0x66, 0x00, 0x38, 0x00, 0x71, 0x00, 0x78, 0x00, 0x66, 0x00, 0x33, 0x00, 0x38, 0x00, 0x7A, 0x00, 0x67, 0x00, 0x35, 0x00, 0x63, 0x00, 0x21, 0x00, 0x41, 0x00, 0x70, 0x00, 0x70, 0x00, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCD, 0xC8, 0x12, 0x12, 0x01, 0x0E, 0x50, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x64, 0x00, 0x54, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x53, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x01, 0x33, 0x00, 0x00, 0x00 };


                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", bytes, RegistryValueKind.Binary);

                        break;
                    }
                }
            }

            Write("Showing encrypted or compressed NTFS files in color\n", "gray", null);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowEncryptCompressedColor", 1, RegistryValueKind.DWord);

            Write("Deactivating Local Security Questions\n", "gray", null);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "NoLocalPasswordResetQuestions", 1, RegistryValueKind.DWord);

            Write("Enabling NumLock on startup\n", "gray", null);
            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 2, RegistryValueKind.DWord);

            Write("Disabling auto reboot (while users are logged in)\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);

            Write("Enabling NTFSLongpaths\n", "gray", null);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled", 1, RegistryValueKind.DWord);

            Write("Setting Desktop-background-quality to 100%\n", "gray", null);
            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);

            Write("Disabling SafeSearchMode\n", "gray", null);
            Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\SearchSettings", "SafeSearchMode", 0, RegistryValueKind.DWord);

            Write("Disabling HibernationBoot\n", "gray", null);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord);

            if (!IsWin11OrNewer)
            {
                Write("Opening Explorer Ribbon\n", "gray", null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOn", 0, RegistryValueKind.DWord);
            }

            Write("Enabling clipboard history\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Clipboard", "EnableClipboardHistory", 1, RegistryValueKind.DWord);

            Write("Disabling Cortana\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);

            Write("Disabling thirdParty suggestions\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\CloudContent", "DisableThirdPartySuggestions", 1, RegistryValueKind.DWord);

            Write("Showing filename extensions\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord);

            Write("Setting default explorer page to \"This PC\"\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord);

            Write("Enabling Explorer Process separation\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SeparateProcess", 1, RegistryValueKind.DWord);

            Write("Hiding Taskbar Icons\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord);

            if (IsWin11OrNewer)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 1, RegistryValueKind.DWord);
            }
            else
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 2, RegistryValueKind.DWord);
            }

            //Write("Setting NTP Server to pool.ntp.org\n", "gray", null);
            //Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DateTime\Servers", "", "0", RegistryValueKind.String);
            //Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DateTime\Servers", "0", "pool.ntp.org", RegistryValueKind.String);

            Write("Deactivating Explorer Compact Mode\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", 1, RegistryValueKind.DWord);

            Write("Removing DynLinks\n", "gray", null);
            Start("cmd.exe", "start cmd /c powershell.exe -command \"$sessionuser = Get-WMIObject -class Win32_ComputerSystem | Select-Object username; $sessionuser = $sessionuser -replace \\\"$env:COMPUTERNAME\\\"; $sessionuser = $sessionuser -replace '@{username=\\\\'; $sessionuser = $sessionuser -replace \\\"}\\\"; Get-ChildItem -Path \\\"C:\\Users\\$sessionuser\\\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \\\"ReparsePoint\\\" } | remove-item -force -ErrorAction SilentlyContinue; Get-ChildItem -Path \\\"C:\\Users\\$sessionuser\\documents\\\" -Force | Where-Object { $_.LinkType -ne $null -or $_.Attributes -match \\\"ReparsePoint\\\" } | remove-item -force -ErrorAction SilentlyContinue\"", true, true);

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DontPrettyPath", 1, RegistryValueKind.DWord);

            if (int.Parse(SafeReadRegString("hklm", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild", 1)) <= 22557)
            {
                Write("Showing task manager details\n", "gray", null);
                Start("cmd.exe", "/c \"Taskmgr.exe\n", true, false);
                do
                {
                    Thread.Sleep(128);
                }
                while ((byte[])Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\TaskManager", "Preferences", null) == null);
                Start("taskkill.exe", "/f /im Taskmgr.exe", true, true);
                byte[] temp = (byte[])Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\TaskManager", "Preferences", null);
                temp[28] = 0;
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\TaskManager", "Preferences", temp, RegistryValueKind.Binary);
            }
            else
            {
                Write("Task Manager patch not run in builds 22557+ due to bug\n", "gray", null);
            }

            Write("Showing file operations details\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager", "EnthusiastMode", 1, RegistryValueKind.DWord);

            Write("Setting various services to manual..\n", "gray", null);
            string[] services = {
                "diagnosticshub.standardcollector.service",
                "DiagTrack",
                "DPS",
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
                "HpTouchpointAnalyticsService",
                "HvHost",
                "vmickvpexchange",
                "vmicguestinterface",
                "vmicshutdown",
                "vmicheartbeat",
                "vmicvmsession",
                "vmicrdv",
                "vmictimesync"};
            for (int i = 0; i < services.Length; i++)
            {
                Write(services[i] + "\n", "darkgray", null);
                var svc = new ServiceController(services[i]);
                SetServiceStartType.ChangeStartMode(svc, ServiceStartMode.Manual);
            }

            //will break msstore
            //var deactivateMSSTORE = new ServiceController("StorSvc");
            //SetServiceStartType.ChangeStartMode(deactivateMSSTORE, ServiceStartMode.Manual);

            Write("*Various performance tweaks*\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 10, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 10, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control", "WaitToKillServiceTimeout", 2000, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WaitToKillAppTimeout", 5000, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WaitToKillServiceTimeout", 2000, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\Ndu", "Start", 4, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", "IRPStackSize", 20, RegistryValueKind.DWord);

            Write("Removing AutoLogger file and restricting directory\n", "gray", null);
            File.Delete(@"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl");
            Start(@"icacls.exe", @"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\Autologger /deny SYSTEM:(OI)(CI)F", true, false);

            Start("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
            Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.General = false;
        }

        public static void DeacTelemetry()
        {
            Write("Deactivating telemetry\n", "darkgreen", null);

            Write("Running ShutUp10\n", "gray", null);
            if (!(CompareHash256("temp\\" + Const.su10exe, Const.su10exeHash, false)[0] == 1) || !(CompareHash256("temp\\" + Const.su10settings, Const.su10settingsHash, false)[0] == 1))
            {
                InternExtract("temp\\", Const.su10exe, "shutup10." + Const.su10exe);
                InternExtract("temp\\", Const.su10settings, "shutup10." + Const.su10settings);
            }
            Start("temp\\" + Const.su10exe, "temp\\" + Const.su10settings + " /nosrp /quiet", true, true);

            if (IsWin11OrNewer)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 1, RegistryValueKind.DWord);
            }
            else
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 2, RegistryValueKind.DWord);
            }

            Write("Deactivating GameDVR\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);

            Write("Disabling Storage Sense\n", "gray", null);
            Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", false);

            Write("Disabling system telemetry\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /Disable", true, false);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\ProgramDataUpdater\" /Disable", true, false);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Autochk\\Proxy\" /Disable", true, false);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Disable", true, false);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /Disable", true, false);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\DiskDiagnostic\\Microsoft-Windows-DiskDiagnosticDataCollector\" /Disable", true, false);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord);

            Write("Disabling Application suggestions\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "OemPreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEverEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353698Enabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);

            Write("Disabling Feedback\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord);
            Start("reg.exe", "add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v \"DoNotShowFeedbackNotifications\" /t reg_dword /d 1 /f", true, false);

            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClient\" /Disable", true, false);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClientOnScenarioDownload\" /Disable", true, false);

            Write("Disabling Tailored Experiences\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableTailoredExperiencesWithDiagnosticData", 1, RegistryValueKind.DWord);

            Write("Disabling Advertising ID\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord);

            Write("Disabling Tailored Experiences\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, RegistryValueKind.DWord);
            Start("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Windows Error Reporting\\QueueReporting\" /Disable", true, false);

            Write("Stopping and disabling Diagnostics Tracking Service\n", "gray", null);
            var svc = new ServiceController("DiagTrack");
            SetServiceStartType.ChangeStartMode(svc, ServiceStartMode.Disabled);

            Write("Stopping and disabling Superfetch service\n", "gray", null);
            svc = new ServiceController("SysMain");
            SetServiceStartType.ChangeStartMode(svc, ServiceStartMode.Disabled);

            Write("Disabling News and Interests\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow", 1, RegistryValueKind.DWord);

            Write("Disabling Wi-Fi Sense\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting", "Value", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots", "Value", 0, RegistryValueKind.DWord);

            if (SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "Blocked Telemetry IPs", RegistryValueKind.DWord, 1) == 0 || SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "Blocked Telemetry IPs", RegistryValueKind.DWord, 1) == -1)
            {
                Write("Excluding 'hosts' file from Windows Defender\n", "darkgray", null);
                PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-ExclusionPath", "C:\\Windows\\System32\\drivers\\etc\\hosts")
                   .Invoke();

                Thread.Sleep(3200);

                Write("Adding blacklisted Domains..\n", "darkgray", null);

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
                     + "\n127.0.0.1 www.gstatic.com"
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
                     + "\n127.0.0.1 m.hotmail.com"
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
                }
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "Blocked Telemetry IPs", 1, RegistryValueKind.DWord);

            }
            else
            {
                Write("Already added blacklisted Domains. Skipping.\n", "darkyellow", null);
            }

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.N0Telemetry = false;
        }

        public static void DarkMode()
        {
            Write("Setting system to darkmode\n", "darkgreen", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, RegistryValueKind.DWord);
            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.SystemTheme = false;
        }

        public static void Backgroundappsno()
        {
            Write("Disabling Background Apps\n", "darkgreen", null);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsRunInBackground", 2, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.Backgroundapps = false;
        }

        public static void Backgroundappsyes()
        {
            Write("Enabling Background Apps\n", "darkgreen", null);

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", true))
                {
                    key.DeleteValue("LetAppsRunInBackground", false);
                }
            }
            catch (System.ArgumentException) { }

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.Backgroundapps = false;
        }

        public static void LightMode()
        {
            Write("Setting system to lightmode\n", "darkgreen", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1, RegistryValueKind.DWord);
            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.SystemTheme = false;
        }

        public static void Restart_Explorer()
        {
            Write("Restarting Explorer\n", "darkgreen", null);

            Start("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, false);

            Thread.Sleep(1000);

            Process.Start("explorer.exe");

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.Restart_Explorer = false;
        }

        public static void BetterWT()
        {
            if (!IsWin11OrNewer)
            {
                bool isinstalled = false;

                string[] Folders = Directory.EnumerateDirectories("C:\\Program Files\\WindowsApps").ToArray();

                for (int i = 0; i < Folders.Length; i++)
                {
                    if (Folders[i].Contains("WindowsTerminal"))
                    {
                        isinstalled = true;
                    }
                }

                if (!isinstalled)
                {
                    Write("Installing Windows Terminal\n", "darkgreen", null);

                    if (CompareHash256("assets\\" + Const.VCLibsName, Const.VCLibs, true)[0] == 1 && CompareHash256("assets\\Windows Terminal\\" + Const.WTName, Const.WT, true)[0] == 1)
                    {
                        PowerShell.Create().AddCommand("Add-AppxPackage")
                            .AddParameter("-path", "assets\\" + Const.VCLibsName)
                            .Invoke();

                        PowerShell.Create().AddCommand("Add-AppxPackage")
                            .AddParameter("-path", "assets\\Windows Terminal\\" + Const.WTName)
                            .Invoke();

                        Write("Done, connect to internet to activate app\n\n", "darkcyan", null);
                    }
                    else
                    {
                        ThreadIsAlive.WT = false;

                        Write("\nError\n\n", "darkyellow", null);
                    }
                }
            }

            Write("Integrating Windows Terminal into extended Context menus\n", "darkgreen", null);

            if (!(CompareHash256("temp\\SetACL.exe", Const.ACLHash, false)[0] == 1))
            {
                InternExtract("temp\\", "SetACL.exe", "SetACL.exe");
            }

            string temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:" + AdminGroupName + "\" -rec Yes";
            Start("temp\\SetACL.exe", temp, true, true);

            temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Powershell\" -ot reg -actn ace -ace \"n:" + AdminGroupName + ";p:full\" -rec Yes";
            Start("temp\\SetACL.exe", temp, true, true);

            temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\shell\\Powershell\" -ot reg -actn setowner -ownr \"n:" + AdminGroupName + "\" -rec Yes";
            Start("temp\\SetACL.exe", temp, true, true);

            temp = "-on \"HKEY_CLASSES_ROOT\\Directory\\shell\\Powershell\" -ot reg -actn ace -ace \"n:" + AdminGroupName + ";p:full\" -rec Yes";
            Start("temp\\SetACL.exe", temp, true, true);

            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\Background\shell\Powershell", false);
            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\Powershell", false);


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

            Start("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
            Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

            ThreadIsAlive.WT = false;

            Write("Done, use with Shift + Right-Click\n\n", "darkcyan", null);
        }

        public static void NormalWT()
        {
            Write("Resetting Windows Terminal integration\n", "darkgreen", null);

            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\Background\shell\Powershell", false);
            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\Powershell", false);

            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\Background\shell\OpenWTHere", false);
            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\Background\shell\OpenWTHereAsAdmin", false);
            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\OpenWTHere", false);
            Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\OpenWTHereAsAdmin", false);

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", true))
                {
                    key.DeleteValue("{9F156763-7844-4DC4-B2B1-901F640F5155}", false);
                }
            }
            catch (System.ArgumentException) { }

            Start("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
            Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.WT = false;
        }

        public static void ClearTaskBar()
        {
            Write("Clearing Taskbar\n", "darkgreen", null);

            Start("cmd.exe", "/c \"DEL /F /S /Q /A \"%AppData%\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar\\*\"\"", true, false);
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband", false);

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0, RegistryValueKind.DWord);

            Start("cmd.exe", "/c \"taskkill /f /im explorer.exe\"", true, true);
            Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.ClearTaskBar = false;
        }

        public static void Nano()
        {
            if (File.Exists("C:\\Program Files\\Nano\\nano.exe") && File.Exists("C:\\Program Files\\Nano\\libncursesw6.dll"))
            {
                Write("Nano already installed\n\n", "darkcyan", null);

                ThreadIsAlive.Nano = false;
                return;
            }
            else
            {
                if (CompareHash256("assets\\Nano.zip", Const.Nano, true)[0] == 1)
                {
                    if (!(CompareHash256("temp\\" + "7z.exe", Const.zip7dll, false)[0] == 1) || !(CompareHash256("temp\\" + "7z.dll", Const.zip7exe, false)[0] == 1))
                    {
                        InternExtract("temp\\", "7z.dll", "_7zip.7z.dll");
                        InternExtract("temp\\", "7z.exe", "_7zip.7z.exe");
                    }

                    Write("Installing Nano..\n", "darkgreen", null);

                    Start("temp\\7z.exe", "x \"assets\\Nano.zip\" -o\"C:\\Program Files\\Nano\" -y", true, true);

                    Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) + ";C:\\Program Files\\Nano;", EnvironmentVariableTarget.Machine);

                    Write("Done\n\n", "darkcyan", null);

                    ThreadIsAlive.Nano = false;
                    return;
                }
                else
                {
                    Write("Nothing changed, skipping\n\n", "darkyellow", null);
                }

                ThreadIsAlive.Nano = false;
                return;
            }
        }

        public static void NotepadPlusPlus()
        {
            if (File.Exists("C:\\Program Files\\Notepad++\\notepad++.exe"))
            {
                Write("Notepad++ already installed\n\n", "darkcyan", null);

                ThreadIsAlive.WGetAction = false;
                return;
            }
            else
            {
                if (WinGetIsInstalled(true) == 1)
                {
                    if (InternetIsAvailable())
                    {
                        Write("Installing Notepad++ (via WinGet)..\n", "darkgreen", null);

                        Start("winget.exe", "install notepad++ --accept-package-agreements --accept-source-agreements --scope machine", true, true);

                        if (File.Exists("C:\\Program Files\\Notepad++\\notepad++.exe"))
                        {
                            var result0 = System.Windows.Forms.MessageBox.Show(
                            "Associate the following file types with Notepad++?\n*.json *.config *.conf *.cfg *.txt",
                            "Notepad--",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                            if (result0 == System.Windows.Forms.DialogResult.Yes)
                            {
                                Write(".config\n", "darkgray", null);
                                Start("cmd.exe", "/c assoc .config=configfile", false, true);
                                Start("cmd.exe", "/c ftype configfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);

                                Write(".txt\n", "darkgray", null);
                                Start("cmd.exe", "/c assoc .txt=txtfile", false, true);
                                Start("cmd.exe", "/c ftype txtfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);
                                Start("cmd.exe", "/c assoc .txt=txtfile", false, true);

                                Write(".conf\n", "darkgray", null);
                                Start("cmd.exe", "/c assoc .conf=conffile", false, true);
                                Start("cmd.exe", "/c ftype conffile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);

                                Write(".cfg\n", "darkgray", null);
                                Start("cmd.exe", "/c assoc .cfg=cfgfile", false, true);
                                Start("cmd.exe", "/c ftype cfgfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);

                                Write(".json\n", "darkgray", null);
                                Start("cmd.exe", "/c assoc .json=jsonfile", false, true);
                                Start("cmd.exe", "/c ftype jsonfile=\"C:\\Program Files\\Notepad++\\notepad++.exe\" %1", true, true);
                            }

                            Write("Done\n\n", "darkcyan", null);

                            ThreadIsAlive.WGetAction = false;
                            return;
                        }
                        else
                        {
                            if (!InternetIsAvailable())
                            {
                                Write("Finished with errors, lost connection?\n\n", "red", null);

                                ThreadIsAlive.WGetAction = false;
                                return;
                            }
                            else
                            {
                                Write("Error\n\n", "red", null);

                                ThreadIsAlive.WGetAction = false;
                                return;
                            }
                        }
                    }
                    else
                    {
                        Write("No internet connection detected\n\n", "red", null);

                        ThreadIsAlive.WGetAction = false;
                        return;
                    }
                }
            }

            ThreadIsAlive.WGetAction = false;
        }
    
        public static void RemWebView()
        {

            if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\EdgeWebView"))
            {
                Write("Removing msedgewebview2\n", "darkgreen", null);

                try
                {
                    Start("taskkill.exe", "/f /im msedgewebview2.exe", true, true);
                    Start(Directory.GetDirectories(@"C:\Program Files (x86)\Microsoft\EdgeWebView\Application")[0] + @"\Installer\setup.exe", "--uninstall --msedgewebview --system-level --verbose-logging", true, true);
                }
                catch (Exception ex) when (ex is System.IO.DirectoryNotFoundException || ex is System.ComponentModel.Win32Exception)
                {
                    Write("\nProgram setup file missing, skipping deinstallation\n\n", "darkyellow", null);
                    Write("\nRemoving program files\n\n", "gray", null);
                    Start("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\EdgeWebView\"", true, true);
                    Start("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\EdgeWebView\" /E /g " + AdminGroupName + ":f", true, true);
                    Start("taskkill.exe", "/f /im msedgewebview2.exe", true, true);
                    Thread.Sleep(1001);
                    Directory.Delete("C:\\Program Files (x86)\\Microsoft\\EdgeWebView", true);
                }

                Write("Making msedgewebview2 User-unistallable\n", "gray", null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft EdgeWebView", "NoRemove", 0, RegistryValueKind.DWord);
                
                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("No EdgeWebView installation found, skipping\n\n", "darkyellow", null);
            }

            ThreadIsAlive.WebView = false;
        }

        public static void Debloat()
        {
            Write("Removing Bloatware\n", "darkgreen", null);

            string[] Bloatware = new string[] {
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

            Write("Removing UWP bloat..\n", "gray", null);

            for (int i = 0; i < Bloatware.Length; i++)
            {
                Write("Removing " + Bloatware[i] + " | ", "darkgray", null);
                try
                {
                    PowerShell.Create().AddCommand("Get-AppxPackage")
                       .AddParameter("-Allusers")
                       .AddParameter("-Name", Bloatware[i])
                       .AddCommand("Remove-AppxPackage")
                       .AddParameter("-Allusers")
                       .Invoke();

                    Write("×\n", "green", null);
                }
                catch (Exception)
                {
                    Write("×\n", "red", null);
                }
            }

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.Debloat = false;
        }

        public static void RemoveXbox()
        {
            Write("Removing XBox bloatware\n", "darkgreen", null);

            string[] Bloatware = new string[] {
                    "Microsoft.XboxApp"
                    , "Microsoft.XboxIdentityProvider"
                    , "Microsoft.Xbox.TCUI"
                    , "Microsoft.XboxGamingOverlay"
                    , "Microsoft.GamingApp"
                    , "Microsoft.XboxGameOverlay"
                    , "Microsoft.XboxSpeechToTextOverlay"};

            for (int i = 0; i < Bloatware.Length; i++)
            {
                Write("Removing " + Bloatware[i] + " | ", "darkgray", null);
                try
                {
                    PowerShell.Create().AddCommand("Get-AppxPackage")
                       .AddParameter("-Allusers")
                       .AddParameter("-Name", Bloatware[i])
                       .AddCommand("Remove-AppxPackage")
                       .AddParameter("-Allusers")
                       .Invoke();

                    Write("×\n", "green", null);
                }
                catch (Exception)
                {
                    Write("×\n", "red", null);
                }
            }

            

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.RemoveXbox = false;
        }

        public static void RMOneDrive()
        {
            if (File.Exists(@"C:\Users\" + SessionUser + @"\AppData\Local\Microsoft\OneDrive\OneDrive.exe") || File.Exists("C:\\Windows\\SysWOW64\\OneDriveSetup.exe") || File.Exists("C:\\Windows\\System32\\OneDriveSetup.exe"))
            {

                Write("Removing + Disabling OneDrive\n", "darkgreen", null);
                
                Start("taskkill.exe", "/f /im OneDriveSetup.exe", true, true);
                Start("taskkill.exe", "/f /im OneDrive.exe", true, true);
                
                if (File.Exists("C:\\Windows\\System32\\OneDriveSetup.exe"))
                {
                    Start("C:\\Windows\\System32\\OneDriveSetup.exe", "/uninstall", true, true);
                }

                if (File.Exists(@"C:\Windows\SysWOW64\OneDriveSetup.exe"))
                {
                    Start(@"C:\Windows\SysWOW64\OneDriveSetup.exe", "/uninstall", true, true);
                }

                Start("taskkill.exe", "/f /im explorer.exe", true, true);

                Start("takeown.exe", "/f \"C:\\Users\\" + SessionUser + "\\OneDrive\"", true, true);
                Start("cacls.exe", "\"C:\\Users\\" + SessionUser + "\\OneDrive\" /E /g " + AdminGroupName + ":f", true, true);
                Start("powershell.exe", "\"Remove-Item \"C:\\Users\\" + SessionUser + "\\OneDrive\" -Recurse -Force\"", true, true);

                Directory.Delete(@"C:\Users\" + SessionUser + @"\AppData\Local\Microsoft\OneDrive", true);
                Directory.Delete(@"C:\ProgramData\Microsoft OneDrive", true);

                Start("powershell.exe", "\"Remove-Item \"C:\\OneDriveTemp\" -Recurse -Force\"", true, true);

                Registry.ClassesRoot.DeleteSubKeyTree(@"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", false);
                Registry.ClassesRoot.DeleteSubKeyTree(@"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", false);

                Start("takeown.exe", "/f \"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\"", true, true);
                Start("cacls.exe", "\"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\" /E /g " + AdminGroupName + ":f", true, true);
                Start("takeown.exe", "/f \"C:\\Windows\\System32\\OneDriveSetup.exe\"", true, true);
                Start("cacls.exe", "\"C:\\Windows\\System32\\OneDriveSetup.exe\" /E /g " + AdminGroupName + ":f", true, true);
                Start("powershell.exe", "\"Remove-Item \"C:\\Windows\\SysWOW64\\OneDriveSetup.exe\" -Recurse -Force\"", true, true);
                Start("powershell.exe", "\"Remove-Item \"C:\\Windows\\System32\\OneDriveSetup.exe\" -Recurse -Force\"", true, true);

                foreach (string s in Directory.GetDirectories("C:\\Windows\\WinSxS"))
                {
                    if (s.Contains("microsoft-windows-onedrive-setup"))
                    {
                        Start("takeown.exe", "/a /f \"" + s + "\\OneDriveSetup.exe", true, true);

                        Start("icacls.exe", "\"" + s + "\\OneDriveSetup.exe\" /GRANT " + AdminGroupName + ":f", true, true);

                        Start("powershell.exe", "\"Remove-Item \'" + s + "\\OneDriveSetup.exe\' -Force\"", true, true);

                        break;
                    }
                }

                Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("No OneDrive installation detected, skipping\n\n", "darkyellow", null);
            }

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", false);


            ThreadIsAlive.RMOneDrive = false;
        }

        public static void NoAppFilehistory()
        {
            Write("Dectivate File/app History\n", "darkgreen", null);

            if (IsWin11OrNewer)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true))
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        if (subKeyName.Contains("windows.data.unifiedtile.startglobalproperties"))
                        {
                            byte[] bytes = new byte[] { 0x02, 0x00, 0x00, 0x00, 0xE0, 0x45, 0xDF, 0x00, 0xFF, 0x0E, 0xD9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0xC2, 0x14, 0x01, 0xC2, 0x3C, 0x01, 0xC5, 0x5A, 0x02, 0x00 };

                            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", bytes, RegistryValueKind.Binary);
                            
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
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs", true))
                {
                    key.DeleteValue("MRUListEx", false);
                }
            }
            catch (System.ArgumentException) { }

            Start("taskkill.exe", "/f /im explorer.exe", true, true);
            Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.AppFilehistory = false;
        }

        public static void YesAppFilehistory()
        {
            Write("Activate File/app History\n", "darkgreen", null);

            if (IsWin11OrNewer)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount", true))
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        if (subKeyName.Contains("windows.data.unifiedtile.startglobalproperties"))
                        {
                            byte[] bytes = new byte[] { 0x02, 0x00, 0x00, 0x00, 0xF9, 0xDE, 0x68, 0xD8, 0x05, 0x0F, 0xD9, 0x01, 0x00, 0x00, 0x00, 0x00, 0x43, 0x42, 0x01, 0x00, 0xC2, 0x3C, 0x01, 0xC5, 0x5A, 0x02, 0x00 };

                            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\" + subKeyName + "\\Current", "Data", bytes, RegistryValueKind.Binary);

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

            Start("taskkill.exe", "/f /im explorer.exe", true, true);
            Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.AppFilehistory = false;
        }

        public static void LegacyCmen()
        {
            if (IsWin11OrNewer)
            {
                Write("Setting Legacy Context Menue\n", "darkgreen", null);

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);

                Start("taskkill.exe", "/f /im explorer.exe", true, true);
                Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Nothing changed, setting only available on Windows 11 or newer.\n\n", "darkyellow", null);
            }

            ThreadIsAlive.Cmen = false;
        }

        public static void DefaultCmen()
        {
            if (IsWin11OrNewer)
            {
                Write("Setting default context menue\n", "darkgreen", null);

                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", false);

                Start("taskkill.exe", "/f /im explorer.exe", true, true);
                Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Nothing changed, setting only available on Windows 11 or newer.\n\n", "darkyellow", null);
            }

            ThreadIsAlive.Cmen = false;
        }

        public static void LegacyRibbon()
        {
            if (IsWin11OrNewer)
            {
                Write("Setting Legacy Explorer Ribbon\n", "darkgreen", null);

                Start("taskkill.exe", "/f /im explorer.exe", true, true);

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", "{e2bf9676-5f8f-435c-97eb-11607a5bedf7}", "", RegistryValueKind.String);
                Thread.Sleep(400);

                Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

                Thread.Sleep(400);

                Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

                Thread.Sleep(400);

                Start("taskkill.exe", "/f /im explorer.exe", true, true);

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", "{e2bf9676-5f8f-435c-97eb-11607a5bedf7}", "", RegistryValueKind.String);
                Thread.Sleep(400);

                Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Nothing changed, setting only available on Windows 11 or newer.\n\n", "darkyellow", null);
            }

            ThreadIsAlive.Ribbon = false;
        }

        public static void DefaultRibbon()
        {
            if (IsWin11OrNewer)
            {
                Write("Setting default Explorer Ribbon\n", "darkgreen", null);

                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Ribbon", "MinimizedStateTabletModeOff", 1, RegistryValueKind.DWord);

                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked", true))
                    {
                        key.DeleteValue("{e2bf9676-5f8f-435c-97eb-11607a5bedf7}", false);
                    }
                }
                catch (System.ArgumentException) { }

                Start("taskkill.exe", "/f /im explorer.exe", true, true);
                Start("cmd.exe", "/c \"start explorer.exe\"", true, true);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Nothing changed, setting only available on Windows 11 or newer.\n\n", "darkyellow", null);
            }

            ThreadIsAlive.Ribbon = false;
        }

        public static void Firefox()
        {
            if (!Directory.Exists("assets\\Firefox"))
            {
                Write("Subdirectory \"assets\\Firefox\" not found, skipping\n\n", "red", null);

                ThreadIsAlive.Install = false;
                return;
            }

            //profile validator
            bool CheckInput(string input)
            {
                if (input == "00" || input == "0")
                {
                    if (CompareHash256("assets\\Firefox\\00.zip", Const.FSH00, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "10")
                {
                    if (CompareHash256("assets\\Firefox\\10.zip", Const.FSH10, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "11")
                {
                    if (CompareHash256("assets\\Firefox\\11.zip", Const.FSH11, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "20")
                {
                    if (CompareHash256("assets\\Firefox\\20.zip", Const.FSH20, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "21")
                {
                    if (CompareHash256("assets\\Firefox\\21.zip", Const.FSH21, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "30")
                {
                    if (CompareHash256("assets\\Firefox\\30.zip", Const.FSH30, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "31")
                {
                    if (CompareHash256("assets\\Firefox\\31.zip", Const.FSH31, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "40" || input == "41")
                {
                    if (CompareHash256("assets\\Firefox\\41.zip", Const.FSH41, true)[0] == 1)
                    {
                        return true;
                    }
                }
                return false;
            }

            bool Softinstall = false;
            bool valide = false;
            string profileid = "";
            bool onlineinstall = false;

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
                    Write("Applying Firefox profiles\n", "darkgreen", null);
                    Softinstall = true;
                }
                else if (result0 == System.Windows.Forms.DialogResult.No)
                {
                    Write("Re-Installing Firefox\n", "darkgreen", null);
                }
                else
                {
                    Write("Nothing changed, skipping\n\n", "darkyellow", null);
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
                    Write("Nothing changed, canceling\n\n", "darkyellow", null);
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
                    Write("Invalide input\n", "darkyellow", null);

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
                            Write("Installing Firefox (via WinGet)..\n", "darkgreen", null);

                            Start("winget.exe", "install Mozilla.Firefox --accept-package-agreements --accept-source-agreements --scope machine --custom \"INSTALL_MAINTENANCE_SERVICE=false /quiet\"", true, true);

                            if (!InternetIsAvailable())
                            {
                                Write("Finished with errors, lost connection?\n\n", "red", null);

                                ThreadIsAlive.Install = false;
                                return;
                            }
                        }
                        else
                        {
                            Write("No internet connection detected\n\n", "red", null);

                            ThreadIsAlive.Install = false;
                            return;
                        }
                    }
                }
                else
                {
                    Write("Installing Firefox..\n", "darkgreen", null);

                    if (CompareHash256("assets\\Firefox\\" + Const.FirefoxImageName, Const.FirefoxImageHash, true)[0] == 1)
                    {
                        Start("assets\\Firefox\\" + Const.FirefoxImageName, "/s /MaintenanceService={false}", true, true);
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
                            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
                            {
                                openFileDialog.InitialDirectory = "assets\\Firefox\\";
                                openFileDialog.RestoreDirectory = true;
                                openFileDialog.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
                                openFileDialog.FilterIndex = 0;
                                openFileDialog.Title = "Firefox Setup File";

                                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    Write("\n(" + openFileDialog.FileName + ") ", "darkgray", null);

                                    Write("Installing with custom file..\n", "darkyellow", null);

                                    Start(openFileDialog.FileName, "/s /MaintenanceService={false}", true, true);
                                }
                                else
                                {
                                    Write("Nothing changed, skipping\n\n", "darkyellow", null);
                                    ThreadIsAlive.Install = false;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Write("Nothing changed, skipping\n\n", "darkyellow", null);
                            ThreadIsAlive.Install = false;
                            return;
                        }
                    }
                }

                if (CheckInput(profileid) != true)
                {
                    if (Softinstall)
                    {
                        Write("Nothing changed, skipping\n\n", "darkyellow", null);
                    }
                    else
                    {
                        Write("Finished with errors, skipping profile\n\n", "darkyellow", null);
                    }

                    ThreadIsAlive.Install = false;
                    return;
                }
            }

            //apple e profile
            Write("Using profile " + profileid + " \n", "gray", null);

            if (!(CompareHash256("temp\\" + "7z.exe", Const.zip7dll, false)[0] == 1) || !(CompareHash256("temp\\" + "7z.dll", Const.zip7exe, false)[0] == 1))
            {
                InternExtract("temp\\", "7z.dll", "_7zip.7z.dll");
                InternExtract("temp\\", "7z.exe", "_7zip.7z.exe");
            }

            Start("temp\\7z.exe", "x \"assets\\Firefox\\" + profileid + ".zip\" -o\"C:\\Users\\" + SessionUser + "\\AppData\\Roaming\\Mozilla\" -y", true, true);

            ThreadIsAlive.Install = false;

            Write("Done\n\n", "darkcyan", null);
        }

        public static void RMEdge()
        {
            Write("Removing Edge\n", "darkgreen", null);

            try
            {
                if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\EdgeUpdate") || Directory.Exists(@"C:\Users\" + SessionUser + @"\AppData\Local\Microsoft\EdgeUpdate"))
                {
                    Write("Removing EdgeUpdate\n", "gray", null);
                    try
                    {
                        Start("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\EdgeUpdate\"", true, true);
                        Start("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\EdgeUpdate\" /E /g " + AdminGroupName + ":f", true, true);
                        Start("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                        Thread.Sleep(1001);
                        Directory.Delete("C:\\Program Files (x86)\\Microsoft\\EdgeUpdate", true);

                    }
                    catch (Exception) { }

                    try
                    {
                        PowerShell.Create().AddCommand("Unregister-ScheduledTask")
                           .AddParameter("-TaskName", "MicrosoftEdgeUpdateTaskMachineCore")
                           .AddParameter("-Confirm", false)
                           .Invoke();

                        PowerShell.Create().AddCommand("Unregister-ScheduledTask")
                               .AddParameter("-TaskName", "MicrosoftEdgeUpdateTaskMachineUA")
                               .AddParameter("-Confirm", false)
                               .Invoke();

                        Start("cacls.exe", "\"C:\\Users\\" + SessionUser + "\\AppData\\Local\\Microsoft\\EdgeUpdate\" /E /g " + AdminGroupName + ":f", true, true);
                        Start("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                        Thread.Sleep(500);
                        Directory.Delete("C:\\Users\\" + SessionUser + "\\AppData\\Local\\Microsoft\\EdgeUpdate", true);
                    }
                    catch (Exception) { }
                }
                else
                {
                    Write("No EdgeUpdate installation found, skipping\n", "darkyellow", null);
                }

                if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\Edge"))
                {
                    Write("Removing Microsoft Edge\n", "gray", null);

                    try
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge", true))
                        {
                            key.DeleteValue("NoRemove", false);
                        }
                    }
                    catch (System.ArgumentException) { }

                    try
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\ClientState\{56EB18F8-B008-4CBD-B6D2-8C97FE7E9062}", true))
                        {
                            key.DeleteValue("experiment_control_labels", false);
                        }
                    }
                    catch (System.ArgumentException) { }

                    try
                    {
                        Start("taskkill.exe", "/f /im msedge.exe", true, true);
                        Start(Directory.GetDirectories(@"C:\Program Files (x86)\Microsoft\Edge\Application")[0] + @"\Installer\setup.exe", " --uninstall --msedge --system-level --verbose-logging", false, false);
                    }
                    catch (Exception ex) when (ex is System.IO.DirectoryNotFoundException || ex is System.ComponentModel.Win32Exception)
                    {
                        Write("\nProgram setup file missing, skipping deinstallation\n\n", "darkyellow", null);
                        Write("\nRemoving program files\n\n", "gray", null);
                        Start("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\Edge\"", true, true);
                        Start("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\Edge\" /E /g " + AdminGroupName + ":f", true, true);
                        Start("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                        Start("taskkill.exe", "/f /im msedge.exe", true, true);
                        Directory.Delete("C:\\Program Files (x86)\\Microsoft\\Edge", true);
                    }

                    if (!(CompareHash256("temp\\SetACL.exe", Const.ACLHash, false)[0] == 1))
                    {
                        InternExtract("temp\\", "SetACL.exe", "SetACL.exe");
                    }

                    string temp = "-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\InboxApplications\" -ot reg -actn setowner -ownr \"n:" + AdminGroupName + "\" -rec Yes";
                    Start("temp\\SetACL.exe", temp, true, true);

                    temp = "-on \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\InboxApplications\" -ot reg -actn ace -ace \"n:" + AdminGroupName + ";p:full\" -rec Yes";
                    Start("temp\\SetACL.exe", temp, true, true);

                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications", true))
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            if (subKeyName.Contains("neutral__8wekyb3d8bbwe") && !subKeyName.Contains("Microsoft.MicrosoftEdgeDevToolsClient"))
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications\" + subKeyName, false);

                                break;
                            }
                        }
                    }
                }
                else
                {
                    Write("No Edge installation found, skipping\n", "darkyellow", null);
                }

                while (File.Exists(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe") && Process.GetProcessesByName("setup").Length != 0)
                {
                    Thread.Sleep(256);
                }

                Thread.Sleep(2560);

                Start("taskkill.exe", "/f /im setup.exe", true, true);

                if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\Edge"))
                {
                    try
                    {
                        if (File.Exists(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"))
                        {
                            Write("Manaually removing Edge\n", "darkyellow", null);
                            Write("Install Windows without internet to be able to remove Edge completely\n", "darkyellow", null);
                        }

                        Start("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\Edge\"", true, true);
                        Start("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\Edge\" /E /g " + AdminGroupName + ":f", true, true);
                        Start("taskkill.exe", "/f /im MicrosoftEdgeUpdate.exe", true, true);
                        Start("taskkill.exe", "/f /im msedge.exe", true, true);
                        Thread.Sleep(1001);
                        Directory.Delete("C:\\Program Files (x86)\\Microsoft\\Edge", true);
                    }
                    catch (Exception ex) when (ex is System.IO.DirectoryNotFoundException || ex is System.ComponentModel.Win32Exception) { }
                }

                Write("Making Microsoft Edge User-unistallable\n", "gray", null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge", "NoRemove", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update", "NoRemove", 0, RegistryValueKind.DWord);

                Write("Preventing Microsoft Edge from reinstalling via Windows Update\n", "gray", null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\EdgeUpdate", "DoNotUpdateToEdgeWithChromium", 1, RegistryValueKind.DWord);

                if (Directory.Exists(@"C:\Program Files (x86)\Microsoft\EdgeCore"))
                {
                    Write("Removing EdgeCore\n", "gray", null);

                    Start("takeown.exe", "/f \"C:\\Program Files(x86)\\Microsoft\\EdgeCore\"", true, true);
                    Start("cacls.exe", "\"C:\\Program Files(x86)\\Microsoft\\EdgeCore\" /E /g " + AdminGroupName + ":f", true, true);
                    Start("taskkill.exe", "/f /im msedge.exe", true, true);
                    Start("taskkill.exe", "/f /im msedgewebview2.exe", true, true);
                    Thread.Sleep(1001);
                    Directory.Delete("C:\\Program Files (x86)\\Microsoft\\EdgeCore", true);
                }
                else
                {
                    Write("No EdgeCore installation found, skipping\n", "darkyellow", null);
                }
            }
            catch (Exception ex)
            {
                Write("ERROR: " + ex.ToString(), "red", null);

                ThreadIsAlive.RMEdge = false;
                return;
            }

            ThreadIsAlive.RMEdge = false;

            Write("Done\n\n", "darkcyan", null);
        }

        public static void Curser()
        {
            if (!Directory.Exists("assets\\Cursers"))
            {
                Write("Subdirectory \"assets\\Cursers\" not found, skipping\n\n", "red", null);

                ThreadIsAlive.Curser = false;
                return;
            }

            //profile validator
            bool CheckInput(string input)
            {
                if (input == "0")
                {
                    if (CompareHash256("assets\\Cursers\\0.zip", Const.CurDef, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "1")
                {
                    if (CompareHash256("assets\\Cursers\\1.zip", Const.CurDefHybrid, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "2")
                {
                    if (CompareHash256("assets\\Cursers\\2.zip", Const.CurDefMono, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "3")
                {
                    if (CompareHash256("assets\\Cursers\\3.zip", Const.CurBlack, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "4")
                {
                    if (CompareHash256("assets\\Cursers\\4.zip", Const.CurBlackHybrid, true)[0] == 1)
                    {
                        return true;
                    }
                }
                else if (input == "5")
                {
                    if (CompareHash256("assets\\Cursers\\5.zip", Const.CurBlackMono, true)[0] == 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool valide = false;
            string profile = "";

            do
            {
                profile = Microsoft.VisualBasic.Interaction.InputBox("\n\n0     =      White\n1     =      White w/ black/white hourglass\n2     =      White Monochrome\n3     =      Black\n4     =      Black w/ black/white hourglass\n5     =      Black Monochrome",
                    "Select Profile",
                    "",
                    0,
                    0);

                if (profile == "")
                {
                    Write("Nothing changed, canceling\n\n", "darkyellow", null);
                    ThreadIsAlive.Curser = false;
                    return;
                }

                valide = CheckInput(profile);

                if (!valide)
                {
                    Write("Invalide input\n", "darkyellow", null);

                    var result0 = System.Windows.Forms.MessageBox.Show(
                        "Possible inputs:\n0, 1, 2, 3, 4, 5",
                        "Invalide Input",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            while (!valide);

            //install
            Write("Setting custom curser\n", "darkgreen", null);

            if (!(CompareHash256("temp\\" + "7z.exe", Const.zip7dll, false)[0] == 1) || !(CompareHash256("temp\\" + "7z.dll", Const.zip7exe, false)[0] == 1))
            {
                InternExtract("temp\\", "7z.dll", "_7zip.7z.dll");
                InternExtract("temp\\", "7z.exe", "_7zip.7z.exe");
            }

            Start("temp\\7z.exe", "x \"assets\\Cursers\\" + profile + ".zip\" -o\"temp\\temp\" -y", true, true);

            Start("C:\\WINDOWS\\System32\\rundll32.exe", "setupapi,InstallHinfSection DefaultInstall 132 .\\temp\\temp\\install.inf", true, true);

            //set
            Write("Using profile " + profile + "\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors", "Scheme Source", 1, RegistryValueKind.DWord);
            Start("reg.exe", "import .\\temp\\temp\\" + profile + ".reg", true, true);

            try
            {
                PowerShell.Create().AddCommand("Remove-Item")
                   .AddParameter("-Path", ".\\temp\\temp")
                   .AddParameter("-Recurse")
                   .AddParameter("-Force")
                   .Invoke();
            }
            catch (Exception) { }

            Write("Done, new Cursor will display after reboot\n\n", "darkcyan", null);
            ThreadIsAlive.Curser = false;
        }

        public static void SafeBoot()
        {
            Write("Restarting to \"Safe Mode\"\n", "darkgreen", null);

            var result = System.Windows.Forms.MessageBox.Show(
                "Restarting to \"Safe Mode\".\nComputer will force restart in 10 seconds, proceed?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Start("shutdown.exe", "/r /o /t 10 /f", true, false);

                Write("in\n10\n", "gray", null);
                Thread.Sleep(1000);
                Write("9\n", "gray", null);
                Thread.Sleep(1000);
                Write("8\n", "gray", null);
                Thread.Sleep(1000);
                Write("7\n", "gray", null);
                Thread.Sleep(1000);
                Write("6\n", "gray", null);
                Thread.Sleep(1000);
                Write("5\n", "gray", null);
                Thread.Sleep(1000);
                Write("4\n", "gray", null);
                Thread.Sleep(1000);
                Write("3\n", "darkyellow", null);
                Thread.Sleep(1000);
                Write("2\n", "darkyellow", null);
                Thread.Sleep(1000);
                Write("1\n", "darkyellow", null);
                Thread.Sleep(400);
                Write("Restarting\n", "darkyellow", null);

                Thread.Sleep(4000);

                Write("\nUnknown error, user cancled shutdown?\n\n", "magenta", null);

            }
            else
            {
                Write("Cancled\n\n", "darkyellow", null);
            }

            ThreadIsAlive.BootStuff = false;
        }

        public static void UEFIBoot()
        {
            if (IsUEFI)
            {

                Write("Restarting to UEFI Firmware\n", "darkgreen", null);

                var result = System.Windows.Forms.MessageBox.Show(
                    "Restarting to UEFI Firmware.\nComputer will force restart in 10 seconds, proceed?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Start("shutdown.exe", "/r /fw /t 10 /f", true, false);

                    Write("in\n10\n", "gray", null);
                    Thread.Sleep(1000);
                    Write("9\n", "gray", null);
                    Thread.Sleep(1000);
                    Write("8\n", "gray", null);
                    Thread.Sleep(1000);
                    Write("7\n", "gray", null);
                    Thread.Sleep(1000);
                    Write("6\n", "gray", null);
                    Thread.Sleep(1000);
                    Write("5\n", "gray", null);
                    Thread.Sleep(1000);
                    Write("4\n", "gray", null);
                    Thread.Sleep(1000);
                    Write("3\n", "darkyellow", null);
                    Thread.Sleep(1000);
                    Write("2\n", "darkyellow", null);
                    Thread.Sleep(1000);
                    Write("1\n", "darkyellow", null);
                    Thread.Sleep(400);
                    Write("Restarting\n", "darkyellow", null);

                    Thread.Sleep(4000);

                    Write("\nUnknown error, user cancled shutdown?\n\n", "magenta", null);

                }
                else
                {
                    Write("Cancled\n\n", "darkyellow", null);
                }
            }
            else
            {
                Write("Restarting to firmware from OS not supported on legacy BIOS systems\n\n", "darkyellow", null);
            }

            ThreadIsAlive.BootStuff = false;
        }

        public static void ActivateNumLock()
        {
            Write("Activating NumLock on Startup\n", "darkgreen", null);

            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 2, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.NumLock = false;

        }

        public static void DeactivateNumLock()
        {
            Write("Deactivating NumLock on Startup\n", "darkgreen", null);

            Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 0, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.NumLock = false;

        }

        public static void DeactivateBootSound()
        {
            Write("Deactivating Windows Boot Sound\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\BootAnimation", "DisableStartupSound", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\EditionOverrides", "UserSetting_DisableStartupSound", 1, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.BootSound = false;

        }

        public static void ActivateBootSound()
        {
            Write("Activating Windows Boot Sound\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\BootAnimation", "DisableStartupSound", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\EditionOverrides", "UserSetting_DisableStartupSound", 0, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.BootSound = false;

        }

        public static void ResetUpdate()
        {
            Write("Resetting Windows Update\n", "darkgreen", null);

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

                Write("Resetting Windows Update services\n", "gray", null);
                var svc = new ServiceController("BITS");
                SetServiceStartType.ChangeStartMode(svc, ServiceStartMode.Automatic);

                svc = new ServiceController("wuauserv");
                SetServiceStartType.ChangeStartMode(svc, ServiceStartMode.Automatic);

                Write("Enabling driver offering through Windows Update\n", "gray", null);
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Device Metadata", true))
                    {
                        key.DeleteValue("PreventDeviceMetadataFromNetwork", false);
                    }
                }
                catch (Exception) { }

                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DriverSearching", true))
                    {
                        key.DeleteValue("DontPromptForWindowsUpdate", false);
                        key.DeleteValue("DontSearchWindowsUpdate", false);
                        key.DeleteValue("DriverUpdateWizardWuSearchEnabled", false);
                    }
                }
                catch (Exception) { }

                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", true))
                    {
                        key.DeleteValue("ExcludeWUDriversInQualityUpdate", false);
                    }
                }
                catch (Exception) { }

                Write("Enabling Windows Update automatic restart\n", "gray", null);
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true))
                    {
                        key.DeleteValue("NoAutoRebootWithLoggedOnUsers", false);
                        key.DeleteValue("AUPowerManagement", false);
                    }
                }
                catch (Exception) { }

                Write("Enabled driver offering through Windows Update\n", "gray", null);
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", true))
                    {
                        key.DeleteValue("BranchReadinessLevel", false);
                        key.DeleteValue("DeferFeatureUpdatesPeriodInDays", false);
                        key.DeleteValue("DeferQualityUpdatesPeriodInDays", false);
                    }
                }
                catch (Exception) { }

                Write("Stopping Windows Update Services\n", "gray", null);
                Start("net.exe", "stop \"BITS\" /y", true, true);
                Start("net.exe", "stop \"wuauserv\" /y", true, true);
                Start("net.exe", "stop \"appidsvc\" /y", true, true);
                Start("net.exe", "stop \"cryptsvc\" /y", true, true);

                Write("Removing QMGR Data file\n", "gray", null);
                try
                {
                    Directory.Delete(@"C:\ProgramData\Application Data\Microsoft\Network\Downloader", true);
                }
                catch (System.IO.DirectoryNotFoundException) { }

                Write("Flushing DNS\n", "gray", null);
                Start("ipconfig.exe", "/flushdns", true, true);

                Write("Removing old Windows Update log\n", "gray", null);
                File.Delete("C:\\Windows\\WindowsUpdate.log");

                Write("Resetting the Windows Update Services to default settings\n", "gray", null);
                Start("sc.exe", "sdset bits D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", true, true);
                Start("sc.exe", "wuauserv D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", true, true);

                Write("Reregistering *some* DLLs (BITfiles + Windows Update)\n", "gray", null);
                Start("regsvr32.exe", "/s atl.dll", true, true);
                Start("regsvr32.exe", "/s urlmon.dll", true, true);
                Start("regsvr32.exe", "/s mshtml.dll", true, true);
                Start("regsvr32.exe", "/s shdocvw.dll", true, true);
                Start("regsvr32.exe", "/s browseui.dll", true, true);
                Start("regsvr32.exe", "/s jscript.dll", true, true);
                Start("regsvr32.exe", "/s vbscript.dll", true, true);
                Start("regsvr32.exe", "/s scrrun.dll", true, true);
                Start("regsvr32.exe", "/s msxml.dll", true, true);
                Start("regsvr32.exe", "/s msxml3.dll", true, true);
                Start("regsvr32.exe", "/s msxml6.dll", true, true);
                Start("regsvr32.exe", "/s actxprxy.dll", true, true);
                Start("regsvr32.exe", "/s softpub.dll", true, true);
                Start("regsvr32.exe", "/s wintrust.dll", true, true);
                Start("regsvr32.exe", "/s dssenh.dll", true, true);
                Start("regsvr32.exe", "/s rsaenh.dll", true, true);
                Start("regsvr32.exe", "/s gpkcsp.dll", true, true);
                Start("regsvr32.exe", "/s sccbase.dll", true, true);
                Start("regsvr32.exe", "/s slbcsp.dll", true, true);
                Start("regsvr32.exe", "/s cryptdlg.dll", true, true);
                Start("regsvr32.exe", "/s oleaut32.dll", true, true);
                Start("regsvr32.exe", "/s ole32.dll", true, true);
                Start("regsvr32.exe", "/s shell32.dll", true, true);
                Start("regsvr32.exe", "/s initpki.dll", true, true);
                Start("regsvr32.exe", "/s wuapi.dll", true, true);
                Start("regsvr32.exe", "/s wuaueng.dll", true, true);
                Start("regsvr32.exe", "/s wuaueng1.dll", true, true);
                Start("regsvr32.exe", "/s wucltui.dll", true, true);
                Start("regsvr32.exe", "/s wups.dll", true, true);
                Start("regsvr32.exe", "/s wups2.dll", true, true);
                Start("regsvr32.exe", "/s wuweb.dll", true, true);
                Start("regsvr32.exe", "/s qmgr.dll", true, true);
                Start("regsvr32.exe", "/s qmgrprxy.dll", true, true);
                Start("regsvr32.exe", "/s wucltux.dll", true, true);
                Start("regsvr32.exe", "/s muweb.dll", true, true);
                Start("regsvr32.exe", "/s wuwebv.dll", true, true);

                Write("Removing WSUS (Windows Server) client settings\n", "gray", null);
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate", true))
                    {
                        key.DeleteValue("AccountDomainSid", false);
                        key.DeleteValue("PingID", false);
                        key.DeleteValue("SusClientId", false);
                    }
                }
                catch (Exception) { }

                Write("Resetting the WinSock\n", "gray", null);
                Start("netsh.exe", "winsock reset", true, true);
                Start("netsh.exe", "winhttp reset proxy", true, true);

                Write("Delete all BITS jobs\n", "gray", null);
                PowerShell.Create().AddCommand("Get-BitsTransfer")
                       .AddCommand("Remove-BitsTransfer")
                       .Invoke();

                Write("Attempting to install the Windows Update Agent\n", "gray", null);
                if (Environment.Is64BitOperatingSystem)
                {
                    Start("wusa.exe", "Windows8-RT-KB2937636-x64 /quiet", true, true);
                }
                else
                {
                    Start("wusa.exe", "Windows8-RT-KB2937636-x86 /quiet", true, true);
                }

                Write("Reseting Windows Update policies\n", "gray", null);
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows", true))
                    {
                        key.DeleteSubKeyTree("WindowsUpdate", false);
                    }
                }
                catch (Exception) { }
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies", true))
                    {
                        key.DeleteSubKeyTree("WindowsUpdate", false);
                    }
                }
                catch (Exception) { }
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows", true))
                    {
                        key.DeleteSubKeyTree("WindowsUpdate", false);
                    }
                }
                catch (Exception) { }
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies", true))
                    {
                        key.DeleteSubKeyTree("WindowsUpdate", false);
                    }
                }
                catch (Exception) { }

                Write("Starting Windows Update Services\n", "gray", null);
                Start("net.exe", "start \"BITS\" /y", true, true);
                Start("net.exe", "start \"wuauserv\" /y", true, true);
                Start("net.exe", "start \"appidsvc\" /y", true, true);
                Start("net.exe", "start \"cryptsvc\" /y", true, true);

                Write("Forcing discovery\n", "gray", null);
                Start("wuauclt.exe", "/resetauthorization /detectnow", true, true);

                Write("Done ", "darkcyan", null);
                Write("restart your computer to complete\n\n", "darkyellow", null);
            }
            else
            {
                Write("Cancled\n\n", "darkyellow", null);
            }

            ThreadIsAlive.WindowsUpdate = false;
        }

        public static void NoWUDrivers()
        {
            Write("Deactivating Windows Update\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.NoWUDrivers = false;
        }

        public static void DeactivateWindowsUpdate()
        {
            Write("Deactivating Windows Update\n", "darkgreen", null);

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

                var svc = new ServiceController("BITS");
                SetServiceStartType.ChangeStartMode(svc, ServiceStartMode.Disabled);

                svc = new ServiceController("wuauserv");
                SetServiceStartType.ChangeStartMode(svc, ServiceStartMode.Disabled);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Cancled\n\n", "darkyellow", null);
            }

            ThreadIsAlive.WindowsUpdate = false;
        }

        public static void SecurityUpdatesOnly()
        {
            Write("Setting Windows Update to \"Security Updates only\"\n", "darkgreen", null);

            var result = System.Windows.Forms.MessageBox.Show(
                "Delays feature updates 1 year.\nDelays security updates 2 days, proceed?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Write("Disabling driver offering through Windows Update\n", "gray", null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Device Metadata", "PreventDeviceMetadataFromNetwork", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DontPromptForWindowsUpdate", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DontSearchWindowsUpdate", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DriverSearching", "DriverUpdateWizardWuSearchEnabled", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);

                Write("Setting update policy\n", "gray", null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "BranchReadinessLevel", 16, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "DeferFeatureUpdatesPeriodInDays", 365, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "DeferQualityUpdatesPeriodInDays", 2, RegistryValueKind.DWord);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Cancled\n\n", "darkyellow", null);
            }

            ThreadIsAlive.WindowsUpdate = false;
        }

        public static void FastWUPDATE()
        {
            Write("Setting Windows Update to fast release\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "BranchReadinessLevel", 8, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.WindowsUpdate = false;
        }

        public static void EncryptSMB()
        {
            Write("SMB settings\n", "darkgreen", null);

            string option = null;
            bool isvalide = false;

            do
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("example:\n\n0    =     only allow SMB 3.1.1\n1    =     0 + encrypt traffic\n2    =    1 + reject unencrypted traffic\n3    =     2 + deactivate autoshares \n4    =     3 + require traffic signing+AES-256-GCM",
                           "Select Profile",
                           "",
                           0,
                           0);

                if (input == "")
                {
                    Write("Canceling\n\n", "darkyellow", null);
                    ThreadIsAlive.Install = false;
                    return;
                }
                else if (input == "0")
                {
                    option = "0";
                    isvalide = false;
                }
                else if (input == "1")
                {
                    option = "1";
                    isvalide = false;
                }
                else if (input == "2")
                {
                    option = "2";
                    isvalide = false;
                }
                else if (input == "3")
                {
                    option = "3";
                    isvalide = false;
                }
                else if (input == "4")
                {
                    option = "4";
                    isvalide = false;
                }
                else
                {
                    Write("Invalide input\n\n", "darkyellow", null);

                    var result0 = System.Windows.Forms.MessageBox.Show(
                        "Possible inputs:\n0, 1, 2, 3, 4",
                        "Invalide Input",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                }
            }
            while (isvalide);

            if (option == "4" || option == "3" || option == "2" || option == "1" || option == "0")
            {
                Write("Setting minimum SMB version to 3.1.1\n", "gray", null);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters", "MinSMB2Dialect", 0x000000311, RegistryValueKind.DWord);

                Write("Setting Encryption Ciphers to AES 256 GCM and AES_128_GCM\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-EncryptionCiphers", "AES_256_GCM, AES_128_GCM")
                       .AddParameter("-Confirm:", false);

                Write("Deactivating SMB 1\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-EnableSMB1Protocol", false)
                       .AddParameter("-Confirm:", false)
                       .Invoke();

                Write("Enable Smb encryption on secure connection\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-DisableSmbEncryptionOnSecureConnection", false)
                       .AddParameter("-Confirm:", false)
                       .Invoke();
            }
            if (option == "4" || option == "3" || option == "2" || option == "1")
            {
                Write("Enable Smb encryption\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-EncryptData", true)
                       .AddParameter("-Confirm:", false)
                       .Invoke();

            }
            if (option == "4" || option == "3" || option == "2")
            {
                Write("Rejecting unencrypted Smb access\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-RejectUnencryptedAccess", true)
                       .AddParameter("-Confirm:", false)
                       .Invoke();
            }
            if (option == "4" || option == "3")
            {
                Write("Deactivating autoshares\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-AutoShareWorkstation", false)
                       .AddParameter("-Confirm:", false)
                       .Invoke();

                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-AutoShareServer", false)
                       .AddParameter("-Confirm:", false)
                       .Invoke();
            }
            if (option == "4")
            {
                Write("Set Encryption to AES 256 GCM\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-EncryptionCiphers", "AES_256_GCM")
                       .AddParameter("-Confirm:", false)
                       .Invoke();

                Write("Require traffic signature\n", "gray", null);
                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-EnableSecuritySignature", true)
                       .AddParameter("-Confirm:", false)
                       .Invoke();

                PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-RequireSecuritySignature", true)
                       .AddParameter("-Confirm:", false)
                       .Invoke();
            }
            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.SMB = false;
        }

        public static void DeactivateEncryptSMB()
        {
            Write("Resetting SMB server config\n", "darkgreen", null);

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters", true))
                {
                    key.DeleteValue("MinSMB2Dialect", false);
                }
            }
            catch (System.ArgumentException) { }

            PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                   .AddParameter("-EncryptionCiphers", "AES_256_GCM, AES_128_GCM, AES_256_CCM, AES_128_CCM")
                   .AddParameter("-Confirm:", false);

        
            PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                   .AddParameter("-EncryptData", false)
                   .AddParameter("-Confirm:", false)
                   .Invoke();

         
            PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                   .AddParameter("-RejectUnencryptedAccess", false)
                   .AddParameter("-Confirm:", false)
                   .Invoke();

            PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                       .AddParameter("-RequireSecuritySignature", false)
                       .AddParameter("-Confirm:", false)
                       .Invoke();

            PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                   .AddParameter("-AutoShareWorkstation", true)
                   .AddParameter("-Confirm:", false)
                   .Invoke();

            PowerShell.Create().AddCommand("Set-SmbServerConfiguration")
                   .AddParameter("-AutoShareServer", true)
                   .AddParameter("-Confirm:", false)
                   .Invoke();


            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.SMB = false;
        }

        public static void VerboseUAC()
        {
            Write("UAC always request admin credentials\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 1, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.UAC = false;
        }

        public static void DefaultUAC()
        {
            Write("Default UAC prompt behavior\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 5, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.UAC = false;
        }

        public static void PagefileClear()
        {
            Write("Clearing pagefile On shutdown\n", "darkgreen", null);

            var result = System.Windows.Forms.MessageBox.Show(
                    "Clearing the pagefile at shutdown may\nresult in prolonged shutdown time, continue?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "ClearPageFileAtShutdown", 1, RegistryValueKind.DWord);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Cancled\n\n", "darkyellow", null);
            }

            ThreadIsAlive.Pagefile = false;
        }

        public static void PagefileDefault()
        {
            Write("Default pagefile behavior\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "ClearPageFileAtShutdown", 0, RegistryValueKind.DWord);
            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.Pagefile = false;
        }

        public static void RequireCtrl()
        {
            Write("Require \"Ctrl+Alt +Del\" to Log in\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 0, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.RequireCtrl = false;
        }

        public static void DontRequireCtrl()
        {
            if (!IsServer)
            {
                Write("Don't require \"Ctrl+Alt +Del\" to Log in\n", "darkgreen", null);

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 1, RegistryValueKind.DWord);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Not supported on Windows Server, use GPO\n\n", "darkyellow", null);
            }

            ThreadIsAlive.RequireCtrl = false;
        }

        public static void InstallGPO()
        {
            //test if installable + test if disabled by user
            if (SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "GPO Status", RegistryValueKind.DWord, 2) != 2 && SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "GPO Status", RegistryValueKind.DWord, 1) != 1)
            {
                var result = System.Windows.Forms.MessageBox.Show(
                "Install Group Policy Editor?\n\nPress \"Cancle\" to never ask again",
                "Install Feature",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

                if (result == System.Windows.Forms.DialogResult.No)
                {
                    Write("gpedit.msc not installed\n\n", "darkyellow", null);
                    return;
                }

                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    Write("gpedit.msc not installed\n\n", "darkyellow", null);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "GPO Status", 2, RegistryValueKind.DWord);
                    return;
                }

                Write("Installing / activating Group Policy Editor\n", "darkgreen", null);
                string[] packages1 = Directory.GetFiles(@"C:\Windows\servicing\Packages", "Microsoft-Windows-GroupPolicy-ClientExtensions-Package~3*.mum");
                // array to list
                List<string> itemsList = packages1.ToList<string>();
                // or merge an other array to the list
                itemsList.AddRange(Directory.GetFiles(@"C:\Windows\servicing\Packages", "Microsoft-Windows-GroupPolicy-ClientTools-Package~3*.mum"));
                // list to array
                string[] allPackages = itemsList.ToArray();

                if (allPackages.Length == 0)
                {
                    Write("\nNo packages found, skipping\n\n", "darkyellow", null);
                    return;
                }

                Write("This might take a while\n", "darkgray", null);

                for (int i = 0; i < allPackages.Length; i++)
                {
                    Write("Installing package " + (i + 1) + "/" + allPackages.Length, "gray", null);
                    Start("dism.exe", "/online /norestart /add-package:\"" + allPackages[i] + "\"", true, true);
                    if (allPackages.Length > (i + 1))
                    {
                        Console.Write("\r");
                    }
                }

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\WinUtil", "GPO Status", 1, RegistryValueKind.DWord);
                Write("\ngpedit.msc now available\n\n", "darkcyan", null);
            }
        }

        public static void Zip7()
        {
            Write("Installing 7-Zip\n", "darkgreen", null);

            if (CompareHash256("assets\\" + Const.zip7installName, Const.zip7installHash, true)[0] == 1)
            {
                Start("assets\\" + Const.zip7installName, "/S", true, true);

                InternExtract("C:\\Program Files\\7-Zip\\Codecs\\", "WinCryptHashers.64.dll", "_7zip.WinCryptHashers.64.dll");
                InternExtract("C:\\Program Files\\7-Zip\\Codecs\\", "WinCryptHashers.ini", "_7zip.WinCryptHashers.ini");

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Nothing changed, skipping\n\n", "darkyellow", null);
            }
            ThreadIsAlive.Install = false;
        }

        public static void VCRedist()
        {
            Write("Installing Visual Studio Runtime\n", "darkgreen", null);

            short i = 0;

            if (CompareHash256("assets\\runtimes\\VC_redist.x64.exe", Const.vc64Hash, true)[0] == 1)
            {
                Write("x64\n", "gray", null);
                Start("assets\\runtimes\\VC_redist.x64.exe", "/install /quiet /norestart", true, true);
                i++;
            }
            else if (CompareHash256("assets\\runtimes\\VC_redist.x86.exe", Const.vc86Hash, false)[0] == 1)
            {
                Console.WriteLine();
            }

            if (CompareHash256("assets\\runtimes\\VC_redist.x86.exe", Const.vc86Hash, true)[0] == 1)
            {
                Write("x86\n", "gray", null);
                Start("assets\\runtimes\\VC_redist.x86.exe", "/install /quiet /norestart", true, true);
                i++;
            }

            if (i == 0)
            {
                Write("Nothing changed, skipping\n\n", "darkyellow", null);
            }
            else if (i == 2)
            {
                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Finished with errors\n\n", "darkyellow", null);
            }

            ThreadIsAlive.Install = false;
        }

        public static void Java()
        {
            Write("Installing Java 17 Runtime / JDK\n", "darkgreen", null);

            if (CompareHash256("assets\\runtimes\\" + Const.javaName, Const.javaHash, true)[0] == 1)
            {
                Start("assets\\runtimes\\" + Const.javaName, "/s SPONSORS=0", true, true);

                Write("Done\n\n", "darkcyan", null);
            }
            else
            {
                Write("Nothing changed, skipping\n\n", "darkyellow", null);
            }
            ThreadIsAlive.Install = false;
        }

        public static void Codecs()
        {
            if (!Directory.Exists("assets\\Codecs"))
            {
                Write("Subdirectory \"assets\\Codecs\" not found, skipping\n\n", "red", null);

                ThreadIsAlive.Install = false;
                return;
            }

            Write("Installing media assets\\Codecs\n", "darkgreen", null);

            var result = System.Windows.Forms.MessageBox.Show(
                "Install all .Appx and .AppxBundle packages from\n\".\\assets\\Codecs\" directory?",
                "Unverified Install",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                string[] packages = Directory.GetFiles("assets\\Codecs", "*.Appx");
                List<string> itemsList = packages.ToList<string>();
                itemsList.AddRange(Directory.GetFiles("assets\\Codecs", "*.AppxBundle"));
                string[] allPackages = itemsList.ToArray();

                if (allPackages.Length == 0)
                {
                    Write("No packages found, skipping\n\n", "darkyellow", null);
                    ThreadIsAlive.Install = false;
                    return;
                }

                for (int i = 0; i < allPackages.Length; i++)
                {
                    Write("Installing package " + (i + 1) + "/" + allPackages.Length, "gray", null);
                    PowerShell.Create().AddCommand("Add-AppPackage")
                        .AddParameter("-path", allPackages[i])
                        .Invoke();
                    if (allPackages.Length > (i + 1))
                    {
                        Console.Write("\r");
                    }
                }
                Write("\nDone\n\n", "darkcyan", null);
            }
            else
            {
                Write("Nothing changed, skipping\n\n", "darkyellow", null);
            }
            ThreadIsAlive.Install = false;
        }

        public static void ActivateLockScreen()
        {
            Write("Activating lockscreen\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", 0, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);

            ThreadIsAlive.LockScreen = false;
        }

        public static void DeactivateLockScreen()
        {
            Write("Deactivating lockscreen\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", 1, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);

            ThreadIsAlive.LockScreen = false;
        }

        public static void ActivateLockScreenNotifications()
        {
            Write("Activating lockscreen notifications\n", "darkgreen", null);

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings", true))
                {
                    key.DeleteValue("NOC_GLOBAL_SETTING_ALLOW_TOASTS_ABOVE_LOCK", false);
                }
            }
            catch (System.ArgumentException) { }

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PushNotifications", "LockScreenToastEnabled", 1, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);

            ThreadIsAlive.LockScreenNotifications = false;
        }

        public static void DeactivateLockScreenNotifications()
        {
            Write("Deactivating lockscreen notifications\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PushNotifications", "LockScreenToastEnabled", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings", "NOC_GLOBAL_SETTING_ALLOW_TOASTS_ABOVE_LOCK", 1, RegistryValueKind.DWord);

            Write("Done\n\n", "darkcyan", null);

            ThreadIsAlive.LockScreenNotifications = false;
        }

        public static void ListUsers()
        {
            string[] usernames = GetSystemUserList();

            foreach (string username in usernames)
            {
                Write(username + "\n", "gray", null);
            }

            Console.WriteLine();

            ThreadIsAlive.AutoLogin = false;
        }

        public static void ActivateAutologin(object arg)
        {
            string[] UserInput = (string[])arg;

            string[] usernames = GetSystemUserList();

            Write("Setting up Auto-Login\n", "darkgreen", null);

            bool UserIsValide = false;

            for (int i = 0; i < usernames.Length; i++)
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
                    Write("Cancled\n\n", "darkyellow", null);
                    ThreadIsAlive.AutoLogin = false;
                    return;
                }

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DefaultUserName", UserInput[0], RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DefaultPassword", UserInput[1], RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "AutoAdminLogon", 1, RegistryValueKind.DWord);

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "DisableCAD", 1, RegistryValueKind.DWord);

                Write("Activated auto-login for ", "darkcyan", null);
                Write(UserInput[0] + "\n\n", "red", null);
            }
            else
            {
                Write("User or password invalide\n\n", "darkyellow", null);
            }

            ThreadIsAlive.AutoLogin = false;
        }

        public static void HideUser(object arg)
        {
            string BUser = arg.ToString();

            string[] usernames = GetSystemUserList();

            Write("Hiding user\n", "darkgreen", null);

            bool UserIsValide = false;

            for (int i = 0; i < usernames.Length; i++)
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
                    Write("Cancled\n\n", "darkyellow", null);
                    ThreadIsAlive.AutoLogin = false;
                    return;
                }

                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList", BUser, 0, RegistryValueKind.DWord);

                Write("User ", "darkcyan", null);
                Write(BUser, "red", null);
                Write(" hidden after reboot\n\n", "darkcyan", null);

            }
            else
            {
                Write("Invalide user\n\n", "darkyellow", null);
            }

            ThreadIsAlive.AutoLogin = false;
        }

        public static void ShowUser(object arg)
        {
            string BUser = arg.ToString();

            string[] usernames = GetSystemUserList();

            Write("Showing user\n", "darkgreen", null);

            bool UserIsValide = false;

            for (int i = 0; i < usernames.Length; i++)
            {
                if (BUser == usernames[i])
                {
                    UserIsValide = true;
                    break;
                }
            }

            if (UserIsValide)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList", true))
                    {
                        key.DeleteValue(BUser, false);
                    }
                }
                catch (Exception) { }

                Write("User ", "darkcyan", null);
                Write(BUser, "red", null);
                Write(" visible after reboot\n\n", "darkcyan", null);

            }
            else
            {
                Write("Invalide user\n\n", "darkyellow", null);
            }

            ThreadIsAlive.AutoLogin = false;
        }

        public static void DeactivateAutologin()
        {
            Write("Deactivating autologin\n", "darkgreen", null);
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    key.DeleteValue("DefaultUserName", false);
                    key.DeleteValue("DefaultPassword", false);
                    key.DeleteValue("AutoAdminLogon", false);

                    Write("Done\n\n", "darkcyan", null);
                }
            }
            catch (System.ArgumentException) { }

            ThreadIsAlive.AutoLogin = false;
        }

        public static void Harden()
        {
            Write("Windows Client Hardening\n", "darkgreen", null);

            Write("Deactivating Local Security Questions\n", "gray", null);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "NoLocalPasswordResetQuestions", 1, RegistryValueKind.DWord);

            Write("Require authentification after sleep\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Power\PowerSettings\0e796bdb-100d-47d6-a2d5-f7d2daa51f51", "ACSettingIndex", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Power\PowerSettings\0e796bdb-100d-47d6-a2d5-f7d2daa51f51", "DCSettingIndex", 1, RegistryValueKind.DWord);

            Write("Deactivating auto hotspot connect\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\config", "AutoConnectAllowedOEM", 0, RegistryValueKind.DWord);

            Write("Enable UAC\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 1, RegistryValueKind.DWord);

            Write("Preventing credential theft\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Activating Windows Defender network protection\n", "gray", null);
            PowerShell.Create().AddCommand("Set-MpPreference")
                   .AddParameter("-EnableNetworkProtection", "Enabled")
                   .Invoke();

            Write("Activating Windows Defender Sandbox\n", "gray", null);
            Start("cmd.exe", "/c \"setx /M MP_FORCE_USE_SANDBOX 1\"", true, false);

            Write("Making VeraCrypt a trusted process\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-ExclusionProcess", "C:\\Program Files\\VeraCrypt\\VeraCrypt.exe")
                   .Invoke();

            Write("Activating PUA\n", "gray", null);
            PowerShell.Create().AddCommand("Set-MpPreference")
                   .AddParameter("-PUAProtection", "enable")
                   .Invoke();

            Write("Activating Windows Defender periodic scan\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows Defender", "PassiveMode", 2, RegistryValueKind.DWord);

            Write("Scan system boot drivers\n", "gray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\CurrentControlSet\Policies\EarlyLaunch", "DriverLoadPolicy", 3, RegistryValueKind.DWord);

            Write("Preventing Office child process creation\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "D4F940AB-401B-4EFC-AADC-AD5F3C50688A")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Block Office process injection\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "75668C1F-73B5-4CF0-BB93-3ECF5CB7CC84")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Block Win32-API calls from Makros\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "92E97FA1-2EDF-4476-BDD6-9DD0B4DDDC7B")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Block Office from creating executables\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "3B576869-A4EC-4529-8536-B80A7769E899")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Block potentially obfuscated scripts\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "5BEB7EFE-FD9A-4556-801D-275E5FFC04CC")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Block executable content from E-Mail client and Webmail\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "BE9BA2D9-53EA-4CDC-84E5-9B1EEEE46550")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Block execution of downloaded content via Javascript or VBSscripts\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "D3E037E1-3EB8-44C8-A917-57927947596D")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Block Adobe Reader childprocess creation\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "7674ba52-37eb-4a4f-a9a1-f0f9a1619a2c")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Prevent WMI misuse\n", "gray", null);
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "e6db77e5-3df2-4cf1-b95a-636979351e5b")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();
            PowerShell.Create().AddCommand("Add-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "d1e49aac-8f56-4280-b9ba-993a6d77406c")
                   .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                   .Invoke();

            Write("Deactivate useractivity upload\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\SettingSync", "DisableSettingSync", 0, RegistryValueKind.DWord);

            Write("Deactivating GameDVR\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);

            Write("Protecting common objects\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager", "ProtectionMode", 1, RegistryValueKind.DWord);

            Write("Deactivate http printing\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Printers", "DisableWebPnPDownload", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\Printers", "DisableHTTPPrinting", 1, RegistryValueKind.DWord);

            Write("Securing Netlogon traffic\n", "gray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Netlogon\Parameters", "SealSecureChannel", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Netlogon\Parameters", "SignSecureChannel", 1, RegistryValueKind.DWord);

            Write("Activating Windows Defender Exploit Guard\n", "gray", null); //
            Start("powershell.exe", "\"Set-Processmitigation -System -Enable DEP,EmulateAtlThunks,HighEntropy,SEHOP,SEHOPTelemetry,TerminateOnError\"", true, true);

            Write("LSASS hardening\n", "magenta", null);
            Write("Running LSA as protected process\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RunAsPPL", 1, RegistryValueKind.DWord);

            Write("Deactivate WDigest for login\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\WDigest", "UseLogonCredential", 0, RegistryValueKind.DWord);

            Write("Mimikatz protection (check access requests)\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\LSASS.exe", "AuditLevel", 8, RegistryValueKind.DWord);

            Write("Activating Remote Credential Guard-Mode (credentials delegation)\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation", "AllowProtectedCreds", 1, RegistryValueKind.DWord);

            Write("Changing default file opener to notepad.exe (ransomware protection)\n", "magenta", null);
            Write("hta\n", "darkgray", null);
            Start("cmd.exe", "/c ftype htafile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("wsh\n", "darkgray", null);
            Start("cmd.exe", "/c ftype wshfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("wsf\n", "darkgray", null);
            Start("cmd.exe", "/c ftype wsffile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("bat\n", "darkgray", null);
            Start("cmd.exe", "/c ftype batfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("js\n", "darkgray", null);
            Start("cmd.exe", "/c ftype jsfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("jse\n", "darkgray", null);
            Start("cmd.exe", "/c ftype jsefile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("vbe\n", "darkgray", null);
            Start("cmd.exe", "/c ftype vbefile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("vbs\n", "darkgray", null);
            Start("cmd.exe", "/c ftype vbsfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);
            Write("cmd\n", "darkgray", null);
            Start("cmd.exe", "/c ftype cmdfile=\"%SystemRoot%\\system32\\NOTEPAD.EXE\" \"%1\"", true, true);

            Write("Firewall settings\n", "magenta", null);
            Start("netsh.exe", @"Advfirewall set allprofiles state on", true, true);
            Write("Activating logging", "darkgray", null);
            Write("   (%systemroot%\\system32\\LogFiles\\Firewall\\pfirewall.log)\n", "darkgray", null);
            Start("netsh.exe", @"advfirewall set currentprofile logging filename %systemroot%\system32\LogFiles\Firewall\pfirewall.log", true, true);
            Write("Seting max length to 4096\n", "darkgray", null);
            Start("netsh.exe", @"netsh advfirewall set currentprofile logging maxfilesize 4096", true, true);
            Start("netsh.exe", @"advfirewall set currentprofile logging droppedconnections enable", true, true);
            Start("netsh.exe", @"Advfirewall set allprofiles state on", true, true);
            Start("netsh.exe", @"advfirewall set publicprofile firewallpolicy blockinboundalways,allowoutbound", true, true);

            Write("Blocking Notepad.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block Notepad.exe netconns\" program=\"%systemroot%\\system32\\notepad.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
            Write("Blocking regsvr32.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block regsvr32.exe netconns\" program=\"%systemroot%\\system32\\regsvr32.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
            Write("Blocking calc.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block calc.exe netconns\" program=\"%systemroot%\\system32\\calc.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
            Write("Blocking mshta.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block mshta.exe netconns\" program=\"%systemroot%\\system32\\mshta.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
            Write("Blocking wscript.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block wscript.exe netconns\" program=\"%systemroot%\\system32\\wscript.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
            Write("Blocking cscript.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block cscript.exe netconns\" program=\"%systemroot%\\system32\\cscript.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
            Write("Blocking runscripthelper.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block runscripthelper.exe netconns\" program=\"%systemroot%\\system32\\runscripthelper.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);
            Write("Blocking hh.exe netconns\n", "darkgray", null);
            Start("netsh.exe", "advfirewall firewall add rule name=\"Block hh.exe netconns\" program=\"%systemroot%\\system32\\hh.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true);

            Write("IE and Edge hardening\n", "magenta", null);
            Write("Activating Smartscreen for Edge\n", "darkgray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Policies\Microsoft\MicrosoftEdge\PhishingFilter", "EnabledV9", 1, RegistryValueKind.DWord);

            Write("IE software install notifications\n", "darkgray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\Installer", "SafeForScripting", 0, RegistryValueKind.DWord);

            Write("Deactivating Edge build-in passwordmanager\n", "darkgray", null);
            Registry.SetValue(@"HKEY_CURRENT_USER\Policies\Microsoft\MicrosoftEdge\Main", "FormSuggest Passwords", "no", RegistryValueKind.String);

            Write("Preventing anonymous Sam-Account enumeration\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RestrictAnonymousSAM", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RestrictAnonymous", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "EveryoneIncludesAnonymous", 0, RegistryValueKind.DWord);

            Write("Biometric security\n", "magenta", null);
            Write("Anti-spoof for facial recognition\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures", "EnhancedAntiSpoofing", 1, RegistryValueKind.DWord);
            Write("Deactivating camera on locked screen", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", 1, RegistryValueKind.DWord);
            Write("Deactivating app voice commands in locked state\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoiceAboveLock", 2, RegistryValueKind.DWord);
            Write("Deactivating windows voice commands in locked state\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoice", 2, RegistryValueKind.DWord);

            Write("Activating advanced Windows Event-Logging\n", "magenta", null);
            Write("Setting various log files to 1024000kb\n", "darkgray", null);
            Start("wevtutil.exe", "sl Security /ms:1024000 /f", true, true);
            Start("wevtutil.exe", "sl Application /ms:1024000 /f", true, true);
            Start("wevtutil.exe", "sl System /ms:1024000 /f", true, true);
            Start("wevtutil.exe", "sl \"Windows Powershell\" /ms:1024000 /f", true, true);
            Start("wevtutil.exe", "sl \"Microsoft-Windows-PowerShell/Operational\" /ms:1024000 /f", true, true);
            Write("Activating Power-Shell logging\n", "darkgray", null);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\PowerShell\ModuleLogging", "EnableModuleLogging", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging", "EnableScriptBlockLogging", 1, RegistryValueKind.DWord);


            Write("Done", "darkcyan", null);
            Write(" please reboot the computer\n\n", "darkyellow", null);
            ThreadIsAlive.Harden = false;
        }

        //public static void ActivateApplicationGuard()
        //{
        //    Write("Install Windows-Defender-ApplicationGuard\n", "darkgreen", null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Install Windows-Defender-ApplicationGuard?",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        Write("Cancled\n\n", "darkyellow", null);
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
        //    Write("Done", "darkcyan", null);
        //    Write(" please reboot the computer\n\n", "red", null);
        //}

        //public static void DeactivateApplicationGuard()
        //{
        //    Write("Deinstall Windows-Defender-ApplicationGuard\n", "darkgreen", null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Deinstall Windows-Defender-ApplicationGuard?",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        Write("Cancled\n\n", "darkyellow", null);
        //        ThreadIsAlive.ApplicationGuard = false;
        //        return;
        //    }
        //    PowerShell.Create().AddCommand("Disable-WindowsOptionalFeature")
        //           .AddParameter("-online")
        //           .AddParameter("FeatureName", "Windows-Defender-ApplicationGuard")
        //           .AddParameter("-norestart")
        //           .Invoke();
        //
        //    Write("Done", "darkcyan", null);
        //    Write(" please reboot the computer\n\n", "red", null);
        //}

        //public static void DeactivateVBS()
        //{
        //    Write("Deactivating Virtualization Based Security\n", "darkgreen", null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Deactivate Virtualization Based Security",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        Write("Cancled\n\n", "darkyellow", null);
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
        //    Write("Done", "darkcyan", null);
        //    Write(" please reboot the computer\n\n", "red", null);
        //}
        //
        //public static void ActivateVBS()
        //{
        //    Write("Activating Virtualization Based Security\n", "darkgreen", null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Activate Virtualization Based Security",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        Write("Cancled\n\n", "darkyellow", null);
        //        ThreadIsAlive.VBS = false;
        //        return;
        //    }
        //
        //    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", "EnableVirtualizationBasedSecurity", 1, RegistryValueKind.DWord);
        //    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", "RequirePlatformSecurityFeatures", 3, RegistryValueKind.DWord);
        //    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", "LsaCfgFlags", 1, RegistryValueKind.DWord);
        //
        //    Write("Done", "darkcyan", null);
        //    Write(" please reboot the computer\n\n", "red", null);
        //}

        public static void AllowUSBCode()
        {
            Write("Allowing untrustworthy / unsigned code execution from USB\n", "darkgreen", null);

            var result0 = System.Windows.Forms.MessageBox.Show(
                "Allowing untrustworthy / unsigned\ncode execution from USB?",
                "",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result0 == System.Windows.Forms.DialogResult.No)
            {
                Write("Cancled\n\n", "darkyellow", null);
                ThreadIsAlive.USBExecution = false;
                return;
            }

            PowerShell.Create().AddCommand("Remove-MpPreference")
                   .AddParameter("-AttackSurfaceReductionRules_Ids", "b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4")
                   .Invoke();

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.USBExecution = false;
        }

        public static void BlockUSBCode()
        {
            Write("Blocking untrustworthy / unsigned code execution from USB (ASRR: b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4)\n", "darkgreen", null);

            var result0 = System.Windows.Forms.MessageBox.Show(
                "Block untrustworthy / unsigned\ncode execution from USB?",
                "",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result0 == System.Windows.Forms.DialogResult.No)
            {
                Write("Cancled\n\n", "darkyellow", null);
                ThreadIsAlive.USBExecution = false;
                return;
            }

            PowerShell.Create().AddCommand("Add-MpPreference")
                  .AddParameter("-AttackSurfaceReductionRules_Ids", "b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4")
                  .AddParameter("-AttackSurfaceReductionRules_Actions", "Enabled")
                  .Invoke();

            Write("Done\n\n", "darkcyan", null);
            ThreadIsAlive.USBExecution = false;
        }

        //public static void DeactivateCFG()
        //{
        //    Write("Deactivating Windows Defender Exploit Guard 'Control Flow Guard' extension\n", "darkgreen", null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Deactivating Windows Defender Exploit Guard\n'Control Flow Guard' extension?",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        Write("Cancled\n\n", "darkyellow", null);
        //        ThreadIsAlive.CFG = false;
        //        return;
        //    }
        //
        //    Start("powershell.exe", "\"Set-Processmitigation -System -Disable CFG\"", true, true);
        //
        //    Write("Done\n\n", "darkcyan", null);
        //    ThreadIsAlive.CFG = false;
        //}
        //
        //public static void ActivateCFG()
        //{
        //    Write("Activating Windows Defender Exploit Guard 'Control Flow Guard' extension\n", "darkgreen", null);
        //
        //    var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Activating Windows Defender Exploit Guard\n'Control Flow Guard' extension?\nWill cause issues with apps like Discord.",
        //        "",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Warning);
        //
        //    if (result0 == System.Windows.Forms.DialogResult.No)
        //    {
        //        Write("Cancled\n\n", "darkyellow", null);
        //        ThreadIsAlive.CFG = false;
        //        return;
        //    }
        //
        //    Start("powershell.exe", "\"Set-Processmitigation -System -Enable CFG\"", true, true);
        //
        //    Write("Done\n\n", "darkcyan", null);
        //    ThreadIsAlive.CFG = false;
        //}

        public static void UserAutostart()
        {
            Start("explorer.exe", "\"C:\\Users\\" + SessionUser + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\"", false, false);
        }

        public static void GlobalAutostart()
        {
            Start("explorer.exe", "\"C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\"", false, false);
        }

        public static void StandartBootMenuePolicy()
        {
            Write("Resetting recovery menu to its defaults\n", "darkgreen", null);
            Start("bcdedit.exe", "/set {current} bootmenupolicy Standard", false, true);
            Write("Done\n\n", "darkcyan", null);
        }

        public static void LegacyBootMenuePolicy()
        {
            Write("Enabeling legacy recovery menu (press F8 during boot to enter)\n", "darkgreen", null);
            Start("bcdedit.exe", "/set {current} bootmenupolicy Legacy", false, true);
            Write("Done\n\n", "darkcyan", null);
        }

        public static void SetMaxFailedLoginAttemptsBoot(object UserInput)
        {
            int WValue = 2;

            if ((string)UserInput == "off")
            {
                WValue = 0;
            }
            else
            {
                try
                {
                    WValue = int.Parse((string)UserInput);
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
                Write("Activated automatic reboot\nafter ", "darkcyan", null);
                Write(WValue.ToString(), "red", null);
                Write(" failed login attempts\n\n", "darkcyan", null);
            }
            else
            {
                Write("Deactivated automatic reboot\nafter n failed login attempts\n\n", "darkcyan", null);
            }

            ThreadIsAlive.MaxFailedLoginAttempts = false;
        }

        public static void SetScreenTimeout(object UserInput)
        {
            int WValue = -1;

            if ((string)UserInput == "off")
            {
                WValue = 0;
            }
            else
            {
                try
                {
                    WValue = int.Parse((string)UserInput);
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
                Write("Activated user inactivity screen lock\nafter ", "darkcyan", null);
                Write(WValue.ToString(), "red", null);
                Write(" seconds ", "darkcyan", null);
                Write("please restart the computer\n\n", "yellow", null);
            }
            else
            {
                Write("Deactivated automatic screenlock\n\n", "darkcyan", null);
            }

            ThreadIsAlive.ScreenTimeOut = false;
        }

        public static void SystemLock(object arg)
        {

            if (IsServer)
            {
                Write("Not supported on Windows Server, use GPO\n\n", "darkyellow", null);

                ThreadIsAlive.SystemLock = false;
                return;
            }

            string[] UserInput = (string[])arg;

            Write("Setting account lockout policy\n", "darkgreen", null);

            int attempts = -1;
            int duration = -1;

            try
            {
                attempts = int.Parse(UserInput[0]);
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
                duration = int.Parse(UserInput[1]);
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
                Write("Invalide input\n\n", "darkyellow", null);
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
                    Write("Cancled\n\n", "darkyellow", null);
                    ThreadIsAlive.SystemLock = false;
                    return;
                }
            }

            if (attempts != 0)
            {
                Start("net.exe", "accounts /lockoutthreshold:" + attempts, true, true);
                if (duration == 0)
                {
                    Start("net.exe", "accounts /lockoutwindow:1", true, true);
                }
                else
                {
                    Start("net.exe", "accounts /lockoutwindow:" + duration, true, true);
                }
                Start("net.exe", "accounts /lockoutduration:" + duration, true, true);

                Write("Done ", "darkcyan", null);
                Write("(" + attempts + ", " + duration + ")\n\n", "darkgray", null);
                ThreadIsAlive.SystemLock = false;
                return;
            }

        skip:
            Start("net.exe", "accounts /lockoutthreshold:0", false, true);
            Write("Deactivated automatic account lock", "darkcyan", null);
            Write(UserInput[0] + "\n\n", "red", null);

        }

        public static void CheckHealth()
        {
            Write("Checking systemlogs for errors\n", "darkgreen", null);
            Start("cmd.exe", "/k DISM /Online /Cleanup-Image /CheckHealth", false, true);
            ThreadIsAlive.SystemCheck = false;
            Write("Done\n\n", "darkcyan", null);
        }

        public static void ScanHealth()
        {
            Write("Checking component-store for errors\n", "darkgreen", null);
            Start("cmd.exe", "/k DISM /Online /Cleanup-Image /ScanHealth", false, true);
            ThreadIsAlive.SystemCheck = false;
            Write("Done\n\n", "darkcyan", null);
        }

        public static void RestoreHealth()
        {
            Write("Checking WinSxS for errors + automatic repair\n", "darkgreen", null);
            Start("cmd.exe", "/k DISM /Online /Cleanup-Image /RestoreHealth", false, true);
            ThreadIsAlive.SystemCheck = false;
            Write("Done\n\n", "darkcyan", null);
        }

        public static void SFCScan()
        {
            Write("Checking system for errors + automatic repair\n", "darkgreen", null);
            Start("cmd.exe", "/k sfc /scannow", false, true);
            ThreadIsAlive.SystemCheck = false;
            Write("Done\n\n", "darkcyan", null);
        }

        public static void YesLoginBlur()
        {
            Write("Changing logins screen attribute | activating blur\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\Microsoft\Windows\System", "DisableAcrylicBackgroundOnLogon", 0, RegistryValueKind.DWord);

            ThreadIsAlive.LoginBlur = false;
            Write("Done\n\n", "darkcyan", null);
        }

        public static void NoLoginBlur()
        {
            Write("Changing logins screen attribute | deactivating blur\n", "darkgreen", null);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\Microsoft\Windows\System", "DisableAcrylicBackgroundOnLogon", 1, RegistryValueKind.DWord);

            ThreadIsAlive.LoginBlur = false;
            Write("Done\n\n", "darkcyan", null);
        }

        public static void ExportWLanConfig(object box)
        {
            string path = box.ToString();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                if (GetServiceStatus("WlanSvc") != "running")
                {
                    Write("WlanSvc offline, starting service..\n", "darkgray", null);
                    Start("net.exe", "start WlanSvc", true, true);
                }

                Write("Wifi profile export\n", "darkgreen", null);
            }
            catch (System.InvalidOperationException)
            {
                Write("WlanSvc missing\n\n", "darkyellow", null);
                ThreadIsAlive.IOWLanConfig = false;
                return;
            }

            //https://github.com/emoacht/ManagedNativeWifi

            var names = NativeWifi.EnumerateProfileNames();
            string[] profiles = names.Select(x => x.ToString()).ToArray();

            if (profiles.Length == 0)
            {
                
                Write(path + "\n", "darkgray", null);
                Write("No wifi profiles found\n\n", "darkyellow", null);
                ThreadIsAlive.IOWLanConfig = false;
                return;
            }

            if (AllProfilesWLan)
            {
                Write(path + "\n", "darkgray", null);

                if (GetServiceStatus("WlanSvc") != "running")
                {
                    Write("WlanSvc offline, starting service..\n", "darkgray", null);
                    Start("net.exe", "start WlanSvc", true, true);
                }

                Start("netsh.exe", "wlan export profile key=clear folder=" + path, true, true);

                if (profiles.Length == 1)
                {
                    Write("Exported 1 profile\n", "gray", null);
                }
                else
                {
                    Write("Exported " + profiles.Length + " profiles\n", "gray", null);
                }
            }
            else
            {
                Write("Exporting single profile\n\n", "darkgreen", null);
                Write("==Profiles==\n", "darkgray", null);

                foreach (string namee in profiles)
                {
                    Write(namee + "\n", "gray", null);
                }

                Console.WriteLine();

                string input = Microsoft.VisualBasic.Interaction.InputBox("Export Profile",
                       "Enter network name",
                       "",
                       0,
                       0);

                if (profiles.Contains(input))
                {
                    if (GetServiceStatus("WlanSvc") != "running")
                    {
                        Write("WlanSvc offline, starting service..\n", "darkgray", null);
                        Start("net.exe", "start WlanSvc", true, true);
                    }

                    Write("Exporting wifi Profile \"" + input + "\" to\n", "gray", null);
                    Write(path + "\n", "darkgray", null);
                    Start("netsh.exe", "wlan export profile \"" + input + "\" key=clear folder=" + path, true, true);
                }
                else
                {
                    Write("Invalide input\n\n", "darkyellow", null);
                    ThreadIsAlive.IOWLanConfig = false;
                    return;
                }
            }

            ThreadIsAlive.IOWLanConfig = false;
            Write("Done\n\n", "darkcyan", null);
        }

        public static void ImportWLanConfig(object box)
        {
            string path = box.ToString();

            if (AllProfilesWLan)
            {
                string[] files;

                try
                {
                    if (GetServiceStatus("WlanSvc") != "running")
                    {
                        Write("WlanSvc offline, starting service..\n", "darkgray", null);
                        Start("net.exe", "start WlanSvc", true, true);
                    }

                    Write("Wifi profile export\n", "darkgreen", null);
                }
                catch (System.InvalidOperationException)
                {
                    Write("WlanSvc missing\n\n", "darkyellow", null);
                    ThreadIsAlive.IOWLanConfig = false;
                    return;
                }

                try
                {
                    files = Directory.GetFiles(path, "*.xml");
                }
                catch (Exception)
                {
                    Write("Invalide path\n\n", "darkyellow", null);
                    ThreadIsAlive.IOWLanConfig = false;
                    return;
                }

                if (files.Length == 0)
                {
                    Write("No WLan profile files found in " + path + "\n\n", "darkyellow", null);
                    ThreadIsAlive.IOWLanConfig = false;
                    return;
                }

                Write("Importing all WLan Profiles from\n", "darkgreen", null);
                Write(path + "\n", "darkgray", null);

                foreach (string file in files)
                {
                    Start("netsh.exe", "wlan add profile filename=\"" + file + "\"", true, true);
                }

                if (files.Length == 1)
                {
                    Write("Imported 1 profile\n", "gray", null);
                }
                else
                {
                    Write("Imported " + files.Length + " profiles\n", "gray", null);
                }
            }
            else
            {
                Write("Importing single profile\n", "darkgreen", null);

                if (getfileagrs == null)
                {
                    Write("\nWaiting for backgroundtask to finish\n\n", "darkyellow", null);

                    while (getfileagrs == null)
                    {
                        Thread.Sleep(256);
                    }
                }

                getfileagrs = new string[] { path, "Profile (*.xml)|*.xml|All files (*.*)|*.*", "Select Wifi Profile File" };

                ThreadStart childref = new ThreadStart(GetFile);
                Thread childThread = new Thread(childref);
                childThread.SetApartmentState(ApartmentState.STA);
                childThread.Start();

                while (getfileagrs[0] == path)
                {
                    Thread.Sleep(256);
                }

                string file = getfileagrs[0];
                getfileagrs = new string[] {null};

                if (file == "cancled")
                {
                    ThreadIsAlive.IOWLanConfig = false;
                    Write("Cancled\n\n", "darkyellow", null);
                    return;
                }

                if (GetServiceStatus("WlanSvc") != "running")
                {
                    Write("WlanSvc offline, starting service..\n", "darkgray", null);
                    Start("net.exe", "start WlanSvc", true, true);
                }

                Write("Importing WLan Profile \"" + file + "\"\n", "gray", null);
                Start("netsh.exe", "wlan add profile filename=\"" + file + "\"", true, true);
            }

            ThreadIsAlive.IOWLanConfig = false;
            Write("Done\n\n", "darkcyan", null);
        }

        //#######################################################################################################################

        public static void Debug()
        {




        }

        //#######################################################################################################################

        public static string[] GetSystemUserList()
        {
            List<string> list = new List<string>();

            SelectQuery query = new SelectQuery("Win32_UserAccount");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            Regex regex = new Regex("(\",Name=\")");

            foreach (ManagementObject envVar in searcher.Get())
            {
                string[] temp = regex.Split(envVar.ToString());
                list.Add(temp[2].Remove(temp[2].Length - 1));
            }

            return list.ToArray();
        }

        public static void GetFile()
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
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
}