using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class SystemSecurity
    {
        private const String PAGEFILE_SOURCE = "PageFile";

        private static Task PageFile()
        {
            OptionSelector.Option[] options =
            [
                new(false, false, "*Clear page file at shutdown",     "Depending on the amount of memory present, this can greatly extend the shutdown time and increase disk wear."),
                new(false, false, "Deactivate page file",             null!),
                new(false, false, "Deactivate crash dump",            null!),
            ];

            OptionSelector optionSelector = new("Attack Surface Reduction", options, new(true, 0, "asr.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return Task.CompletedTask;

            //

            if (optionSelector.Result.UserSelection[0])
            {
                Log.FastLog("Activating page file overwrite at shutdown", LogSeverity.Info, PAGEFILE_SOURCE);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\Session Manager\\Memory Management", "ClearPageFileAtShutdown", 1, RegistryValueKind.DWord);
            }

            if (optionSelector.Result.UserSelection[1])
            {
                Log.FastLog("Deactivating page file", LogSeverity.Info, PAGEFILE_SOURCE);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\Session Manager\\Memory Management", "PagingFiles", new String[] { "\0\0", "\0\0" }, RegistryValueKind.MultiString);
            }

            if (optionSelector.Result.UserSelection[2])
            {
                Log.FastLog("Deactivating crash dumps", LogSeverity.Info, PAGEFILE_SOURCE);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\System\\ControlSet001\\Control\\CrashControl", "CrashDumpEnabled", 0, RegistryValueKind.DWord);
            }

            Log.FastLog("Done, a reboot is required to apply the changes", LogSeverity.Info, PAGEFILE_SOURCE);

            return Task.CompletedTask;
        }
    }
}