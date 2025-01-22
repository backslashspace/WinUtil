using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class BaseConfigWindow
    {
        private const String STABILITY_RELIABILITY_SOURCE = "Stability";

        private async static Task StabilityReliability()
        {

            OptionSelector.Option[] options =
            [
                new(true, false, "*Disable fast startup",                        "It is recommended to turn off the Windows 'Hybrid Shutdown' feature,\nthis will provide better compatibility and stability in the long term,\nas Hybrid Shutdown does not fully shut down the system in normal\noperation when pressing 'Shut Down PC'."),
                new(true, false, "*Explorer process separation",                 "This can improve system stability and prevents the desktop from\ncrashing when an Explorer window becomes unresponsive. (Each explorer window gets its own process)"),
                new(true, false, "*Enable NTFS long paths",                      "This allows windows to use paths that are longer than 260 characters."),
                new(true, false, "*Deactivate local security questions",         "This improves security, answers of security questions are stored as plain text in the registry."),
                new(true, false, "*Properly handle multiple network adapters",   "This allows the user to have multiple *usable* network connections at the same time.\n\nBy default, Windows tries to keep the number of network connections to a minimum, this could lead to automatic disconnects under some conditions.\n\nAn example would be being connected to a network via Wi-Fi and Ethernet at the same time, by default, Windows would always prefer the Ethernet adapter for all traffic and disconnect from the Wi-Fi network after some time, even if some resources like Internet are only accessible from the Wi-Fi network.\n(The same applies to WWAN cards)\n\n[i] IgnoreNonRoutableEthernet=1, fMinimizeConnections=0"),
                new(true, false, "*Disable Windows Update auto reboot",          "Disable Windows Update auto reboot while users are logged in."),
                new(true, false, "Create small memory dump file on crash",       null!),
                new(true, false, "*Remove dynamic user home directory links",    "By default, the documents, video and image folders are linked to each other,\nthis can lead to some unexpected behavior,\nlike copying the contents of all folders when only copying the documents folder."),
                new(true, false, "*Use old boot policy",                         "Allows to enter the recovery menu during boot (F8)"),
                new(true, false, "*Enable BSoD messages",                        "The old bluescreen of death | more crash information"),
                new(true, false, "*Show 'End Task' Button",                      "Allows you to end a task when right clicking an app in the task bar"),
            ];

            OptionSelector optionSelector = new("Stability & Reliability", options, new(true, 0, "stability.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return;

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            await Task.Run(() =>
            {
                try
                {
                    if (optionSelector.Result.UserSelection[0])
                    {
                        Log.FastLog("[MACHINE] Deactivating fast startup", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating fast startup failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[1])
                    {
                        Log.FastLog("[USER] Activating Explorer process separation", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SeparateProcess", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Activating Explorer process separation failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[2])
                    {
                        Log.FastLog("[MACHINE] Enabling NTFS long paths", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Enabling NTFS long paths failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[3])
                    {
                        Log.FastLog("[MACHINE] Deactivating local security questions", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows", "NoLocalPasswordResetQuestions", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating local security questions failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[4])
                    {
                        Log.FastLog("[MACHINE] Changing network adapter behavior", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WcmSvc\Local", "fMinimizeConnections", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Wcmsvc", "IgnoreNonRoutableEthernet", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Changing network adapter behavior failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[5])
                    {
                        Log.FastLog("[MACHINE] Disabling Windows Update auto reboot", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Disabling Windows Update auto reboot failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[6])
                    {
                        Log.FastLog("[MACHINE] Set create small memory dump file on crash", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\CrashControl", "CrashDumpEnabled", 3, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Set create small memory dump file on crash failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[7])
                    {
                        List<String> directoryList = new();

                        foreach (String directory in Directory.EnumerateFileSystemEntries(RunContextInfo.Windows.UserHomePath))
                        {
                            if ((((Int32)new DirectoryInfo(directory).Attributes) & 0x400) == 0x400) directoryList.Add(directory);
                        }
                        foreach (String directory in Directory.EnumerateFileSystemEntries(RunContextInfo.Windows.UserHomePath + "\\Documents"))
                        {
                            if ((((Int32)new DirectoryInfo(directory).Attributes) & 0x400) == 0x400) directoryList.Add(directory);
                        }

                        if (directoryList.Count == 0)
                        {
                            Log.FastLog("Skipping remove dynamic user home directory links - no junctions found", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        }
                        else
                        {
                            Log.FastLog($"[USER] Remove dynamic user home directory links - found {directoryList.Count} junctions", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);

                            for (Int32 i = 0; i < directoryList.Count; ++i)
                            {
                                Directory.Delete(directoryList[i], true);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Remove dynamic user home directory links failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    Util.Execute.StartInfo startInfo;

                    if (optionSelector.Result.UserSelection[8])
                    {
                        Log.FastLog("[MACHINE] Setting Boot Policy to 'Legacy'", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        startInfo = new(@"C:\Windows\System32\bcdedit.exe", "/set {current} bootmenupolicy Legacy", true, true, true);
                        Util.Execute.Process(startInfo);
                    }
                    else
                    {
                        Log.FastLog("[MACHINE] Setting Boot Policy to 'Standard'", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        startInfo = new(@"C:\Windows\System32\bcdedit.exe", "/set {current} bootmenupolicy Standard", true, true, true);
                        Util.Execute.Process(startInfo);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Setting Boot Policy failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[9])
                    {
                        Log.FastLog("[MACHINE] Enabling BSoD messages", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Enabling BSoD messages failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[10])
                    {
                        Log.FastLog("[USER] Enabling 'End Task' Button", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Enabling 'End Task' Button failed with: " + exception.Message, LogSeverity.Error, STABILITY_RELIABILITY_SOURCE);
                }
            });

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            Util.RestartExplorerForUser();

            Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, STABILITY_RELIABILITY_SOURCE);
        }
    }
}