using System;
using System.Diagnostics;

namespace Deregisterer
{
    internal static partial class Tools
    {
        internal enum SCAction
        {
            Create = 0,
            Delete = 1,
        }

        internal static Int32 SC(SCAction action)
        {
            Process process = new();
            process.StartInfo.FileName = "C:\\Windows\\System32\\sc.exe";
            process.StartInfo.Verb = "runas";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;

            if (action == SCAction.Create)
            {
                process.StartInfo.Arguments = $"create \"{Installer.EXECUTABLE_NAME_WITHOUT_EXTENSION}\" type=own binpath= \"\\\"{Installer.INSTALLATION_DIRECTORY_PATH}{Installer.EXECUTABLE_NAME}\\\" /maintenance\" displayname=\"{Installer.EXECUTABLE_NAME_WITHOUT_EXTENSION}\" start=auto";
            }
            else
            {
                process.StartInfo.Arguments = $"delete \"{Installer.EXECUTABLE_NAME_WITHOUT_EXTENSION}\"";
            }

            process.Start();
            process.WaitForExit();

            return process.ExitCode;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

        internal static void Process(String path, String args)
        {
            Process process = new();

            process.StartInfo.FileName = path;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.Arguments = args;

            process.Start();
            process.WaitForExit();
        }
    }
}