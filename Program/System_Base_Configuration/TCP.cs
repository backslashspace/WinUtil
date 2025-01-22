using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class BaseConfigWindow
    {
        private const String TCP_SOURCE = "TCP";

        private static Task TCP()
        {
            OptionSelector.Option[] options =
            [
                new(true, false, "Enable RSS (Receive Side Scaling)",                           null!),
                new(true, false, "Enable TCP window scaling (default)",                         null!),
                new(true, false, "Enable large TCP windows and timestamps (RFC 1323)",          null!),
                new(true, false, "Enable TCP selective acknowledgements (RFC 2018)",            null!),
                new(true, false, "Set TCP window to 16776960 = default = x8",                   null!),
                new(false, true, "",                                                            null!),
                new(false, false, "Unset TCP parameters",                                       null!),
            ];

            OptionSelector optionSelector = new(TCP_SOURCE, options, new(true, 0, "tcp.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return Task.CompletedTask;

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            if (optionSelector.Result.UserSelection[6])
            {
                try
                {
                    Log.FastLog("Unsetting TCP config", LogSeverity.Info, TCP_SOURCE);

                    RegistryKey parameters = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", true);
                    parameters.DeleteValue("Tcp1323Opts", false);
                    parameters.DeleteValue("SackOpts", false);
                    parameters.DeleteValue("TcpWindowSize", false);

                    Log.FastLog("Done", LogSeverity.Info, TCP_SOURCE);

                    return Task.CompletedTask;
                }
                catch (Exception exception)
                {
                    Log.FastLog("Unsetting TCP config failed with: " + exception.Message, LogSeverity.Error, TCP_SOURCE);

                    return Task.CompletedTask;
                }
            }

            if (optionSelector.Result.UserSelection[0])
            {
                try
                {
                    Log.FastLog("Enabling RSS (Receive Side Scaling)", LogSeverity.Info, TCP_SOURCE);

                    if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "int tcp set global rss=enabled", true, true, true)).Success)
                    {
                        Log.FastLog("Enabling RSS (Receive Side Scaling) failed, netsh.exe not found", LogSeverity.Error, TCP_SOURCE);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Enabling RSS (Receive Side Scaling) failed with: " + exception.Message, LogSeverity.Error, TCP_SOURCE);
                }
            }

            if (optionSelector.Result.UserSelection[1])
            {
                try
                {
                    Log.FastLog("Enabling TCP window scaling (default)", LogSeverity.Info, TCP_SOURCE);

                    if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "int tcp set global autotuninglevel=normal", true, true, true)).Success)
                    {
                        Log.FastLog("Enabling TCP window scaling (default) failed, netsh.exe not found", LogSeverity.Error, TCP_SOURCE);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Enabling TCP window scaling (default) failed with: " + exception.Message, LogSeverity.Error, TCP_SOURCE);
                }
            }

            if (optionSelector.Result.UserSelection[2])
            {
                try
                {
                    Log.FastLog("Enabling large TCP windows and timestamps (RFC 1323)", LogSeverity.Info, TCP_SOURCE);

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "Tcp1323Opts", 3, RegistryValueKind.DWord);
                }
                catch (Exception exception)
                {
                    Log.FastLog("Enabling large TCP windows and timestamps (RFC 1323) failed with: " + exception.Message, LogSeverity.Error, TCP_SOURCE);
                }
            }

            if (optionSelector.Result.UserSelection[3])
            {
                try
                {
                    Log.FastLog("Enabling TCP selective acknowledgements (RFC 2018)", LogSeverity.Info, TCP_SOURCE);

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "SackOpts ", 1, RegistryValueKind.DWord);
                }
                catch (Exception exception)
                {
                    Log.FastLog("Enabling TCP selective acknowledgements (RFC 2018) failed with: " + exception.Message, LogSeverity.Error, TCP_SOURCE);
                }
            }

            if (optionSelector.Result.UserSelection[4])
            {
                try
                {
                    Log.FastLog("Setting TCP window to 16776960", LogSeverity.Info, TCP_SOURCE);

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpWindowSize", 16776960, RegistryValueKind.DWord);
                }
                catch (Exception exception)
                {
                    Log.FastLog("Setting TCP window to 16776960 failed with: " + exception.Message, LogSeverity.Error, TCP_SOURCE);
                }
            }

            Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, TCP_SOURCE);

            return Task.CompletedTask;
        }
    }
}