using BSS.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class MiscellaneousConfigWindow
    {
        private const String NGEN_SOURCE = "NGEN";

        private static async Task NGEN() => await Task.Run(() =>
        {
            try
            {
                String[] runtimes = Directory.GetDirectories("C:\\Windows\\Microsoft.NET\\Framework");
                for (Int32 i = 0; i < runtimes.Length; ++i)
                {
                    if (Regex.Match(runtimes[i], "v\\d+.\\d+").Success)
                    {
                        if (File.Exists(runtimes[i] + "\\ngen.exe"))
                        {
                            Log.FastLog($"Starting {runtimes[i]}\\ngen.exe executeQueuedItems", LogSeverity.Info, NGEN_SOURCE);
                            Util.Execute.Process(new(runtimes[i] + "\\ngen.exe", "executeQueuedItems", true, true, true));
                        }
                    }
                }

                runtimes = Directory.GetDirectories("C:\\Windows\\Microsoft.NET\\Framework64");
                for (Int32 i = 0; i < runtimes.Length; ++i)
                {
                    if (Regex.Match(runtimes[i], "v\\d+.\\d+").Success)
                    {
                        if (File.Exists(runtimes[i] + "\\ngen.exe"))
                        {
                            Log.FastLog($"Starting {runtimes[i]}\\ngen.exe executeQueuedItems", LogSeverity.Info, NGEN_SOURCE);
                            Util.Execute.Process(new(runtimes[i] + "\\ngen.exe", "executeQueuedItems", true, true, true));
                        }
                    }
                }

                Log.FastLog("Done", LogSeverity.Info, NGEN_SOURCE);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to execute ngen.exe: " + exception.Message, LogSeverity.Error, NGEN_SOURCE);
            }
        });
    }
}