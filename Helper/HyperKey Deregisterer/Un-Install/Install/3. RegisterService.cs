using System;
using System.Diagnostics;
using System.Threading;

namespace Deregisterer
{
    internal static partial class Installer
    {
        internal static void RegisterService()
        {
            try
            {
                Int32 exitCode = Tools.SC(Tools.SCAction.Create);

                if (Tools.SC(Tools.SCAction.Create) == 1073)
                {
                    Tools.SC(Tools.SCAction.Delete);

                    KillMMC();

                    Thread.Sleep(1024);

                    exitCode = Tools.SC(Tools.SCAction.Create);
                }

                if (exitCode != 0) Environment.Exit(-1);
            }
            catch
            {
                Environment.Exit(-1);
            }
        }

        private static void KillMMC()
        {
            Process process = new();
            process.StartInfo.FileName = "C:\\Windows\\System32\\taskkill.exe";
            process.StartInfo.Verb = "runas";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = "taskkill /f /im mmc.exe";

            process.Start();
            process.WaitForExit();
        }
    }
}