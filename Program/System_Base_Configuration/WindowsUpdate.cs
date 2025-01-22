using BSS.Logging;
using Microsoft.Win32;
using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class BaseConfigWindow
    {
        private const String WINUPDATE_SOURCE = "WinUpdate";

        private async static Task WindowsUpdate()
        {
            OptionSelector.Option[] options =
            [
                new(true, false, "Disable Peer to Peer updates",                                null!),
                new(true, false, "Disable automatic download of OEM Software",                  null!),
                new(true, false, "Disable automatic driver updates via Windows Update",         null!),
                new(true, false, "Disable automatic uwp app updates via Windows Update",        null!),
                new(true, false, "Disable dynamic update rollouts (experimental features)",     null!),
                new(false, false, "Disable automatic Windows Updates",                          null!),
                new(false, false, "Disable updates for Office, etc via Windows Updates",        null!),
                new(false, false, "Only install stable releases",                               null!),
                new(false, true, "",                                                            null!),
                new(false, false, "Unset all options",                                          null!),
                new(false, false, "Full Service Reset (may fix issues)",                        null!),
            ];

            OptionSelector optionSelector = new("Windows Update", options, new(true, 0, "win_update.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return;

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            await Task.Run(() =>
            {
                if (optionSelector.Result.UserSelection[10])
                {
                    try
                    {
                        Log.FastLog("Resetting Windows Update - this might take a while", LogSeverity.Info, WINUPDATE_SOURCE);

                        ResetWindowsUpdate();

                        Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, WINUPDATE_SOURCE);

                        return;
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Unsetting Windows Update options failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                        return;
                    }
                }

                if (optionSelector.Result.UserSelection[9])
                {
                    try
                    {
                        Log.FastLog("Unsetting Windows Update options", LogSeverity.Info, WINUPDATE_SOURCE);

                        UnsetWindowsUpdate();
              
                        Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, WINUPDATE_SOURCE);

                        return;
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Unsetting Windows Update options failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                        return;
                    }
                }

                //

                if (optionSelector.Result.UserSelection[0])
                {
                    try
                    {
                        Log.FastLog("Disabling Peer to Peer updates", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Disabling Peer to Peer updates failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                if (optionSelector.Result.UserSelection[1])
                {
                    try
                    {
                        Log.FastLog("Disabling automatic download of OEM Software", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata", "PreventDeviceMetadataFromNetwork", 1, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Disabling automatic download of OEM Software failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                if (optionSelector.Result.UserSelection[2])
                {
                    try
                    {
                        Log.FastLog("Disabling automatic driver updates via Windows Update", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "DriverUpdateWizardWuSearchEnabled", 0, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Disabling automatic driver updates via Windows Update failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                if (optionSelector.Result.UserSelection[3])
                {
                    try
                    {
                        Log.FastLog("Disabling automatic app updates via Windows Update", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsStore\WindowsUpdate", "AutoDownload", 2, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Disabling automatic app updates via Windows Update failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                if (optionSelector.Result.UserSelection[4])
                {
                    try
                    {
                        Log.FastLog("Disabling dynamic update rollouts (experimental features)", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", 0, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Disabling dynamic update rollouts (experimental features) failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                if (optionSelector.Result.UserSelection[5])
                {
                    try
                    {
                        Log.FastLog("Disabling automatic Windows Updates", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\wuauserv", "Start", 4, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Disabling automatic Windows Updates failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                if (optionSelector.Result.UserSelection[6])
                {
                    try
                    {
                        Log.FastLog("Disabling updates for Office, etc via Windows Updates", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Services\7971f918-a847-4430-9279-4a52d1efe18d", "RegisteredWithAU", 0, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Disabling updates for Office, etc via Windows Updates failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                if (optionSelector.Result.UserSelection[7])
                {
                    try
                    {
                        Log.FastLog("Only receive stable releases via Windows Updates", LogSeverity.Info, WINUPDATE_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "BranchReadinessLevel", 16, RegistryValueKind.DWord);
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Only receive stable releases via Windows Updates failed with: " + exception.Message, LogSeverity.Error, WINUPDATE_SOURCE);
                    }
                }

                Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, WINUPDATE_SOURCE);
            });
        }

        private static void UnsetWindowsUpdate()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\DeliveryOptimization", true);
            key?.DeleteValue("DODownloadMode", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\DeliveryOptimization\\Config", true);
            key?.DeleteValue("DODownloadMode", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Device Metadata", true);
            key?.DeleteValue("PreventDeviceMetadataFromNetwork", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\DriverSearching", true);
            key?.DeleteValue("SearchOrderConfig", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate", true);
            key?.DeleteValue("ExcludeWUDriversInQualityUpdate", false);
            key?.DeleteValue("DriverUpdateWizardWuSearchEnabled", false);
            key?.DeleteValue("BranchReadinessLevel", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsStore\\WindowsUpdate", true);
            key?.DeleteValue("AutoDownload", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\PolicyManager\\current\\device\\System", true);
            key?.DeleteValue("AllowExperimentation", false);

            key = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Services\\wuauserv", true);
            key?.DeleteValue("Start", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", true);
            key?.DeleteValue("NoAutoUpdate", false);

            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Services\\7971f918-a847-4430-9279-4a52d1efe18d", true);
            key?.DeleteValue("RegisteredWithAU", false);
        }

        private static void ResetWindowsUpdate()
        {
            UnsetWindowsUpdate();

            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "stop \"BITS\" /y", true, true,true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "stop \"wuauserv\" /y", true, true,true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "stop \"appidsvc\" /y", true, true,true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "stop \"cryptsvc\" /y", true, true,true));

            if (Directory.Exists(@"C:\ProgramData\Application Data\Microsoft\Network\Downloader"))
            {
                Directory.Delete(@"C:\ProgramData\Application Data\Microsoft\Network\Downloader", true);
            }

            Util.Execute.Process(new(@"C:\Windows\System32\sc.exe", "sdset bits D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\sc.exe", "wuauserv D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;PU)", true, true, true));

            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s atl.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s urlmon.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s mshtml.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s shdocvw.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s browseui.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s jscript.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s vbscript.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s scrrun.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s msxml.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s msxml3.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s msxml6.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s actxprxy.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s softpub.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wintrust.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s dssenh.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s rsaenh.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s gpkcsp.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s sccbase.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s slbcsp.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s cryptdlg.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s oleaut32.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s ole32.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s shell32.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s initpki.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wuapi.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wuaueng.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wuaueng1.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wucltui.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wups.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wups2.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wuweb.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s qmgr.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s qmgrprxy.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wucltux.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s muweb.dll", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\regsvr32.exe", "/s wuwebv.dll", true, true, true));

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate", true);
            key?.DeleteValue("AccountDomainSid", false);
            key?.DeleteValue("PingID", false);
            key?.DeleteValue("SusClientId", false);

            Util.Execute.Process(new(@"C:\Windows\System32\netsh.exe", "winsock reset", true, true,true));
            Util.Execute.Process(new(@"C:\Windows\System32\netsh.exe", "winhttp reset proxy", true, true,true));

            PowerShell.Create().AddScript("Get-BitsTransfer | Remove-BitsTransfer").Invoke();

            if (Environment.Is64BitOperatingSystem)
            {
                Util.Execute.Process(new(@"C:\Windows\System32\wusa.exe", "Windows8-RT-KB2937636-x64 /quiet", true, true, true));
            }
            else
            {
                Util.Execute.Process(new(@"C:\Windows\System32\wusa.exe", "Windows8-RT-KB2937636-x86 /quiet", true, true, true));
            }

            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "start \"BITS\" /y", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "start \"wuauserv\" /y", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "start \"appidsvc\" /y", true, true, true));
            Util.Execute.Process(new(@"C:\Windows\System32\net.exe", "start \"cryptsvc\" /y", true, true, true));

            Util.Execute.Process(new(@"C:\Windows\System32\wuauclt.exe", "/resetauthorization /detectnow", true, true, true));

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\BITS", "Start", 2, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\wuauserv", "Start", 3, RegistryValueKind.DWord);
        }
    }
}