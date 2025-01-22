using BSS.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

#pragma warning disable IDE0079
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace Stimulator
{
    internal static class Util
    {
        internal static Boolean IsWindows10UI() => (RunContextInfo.Windows.IsServer && RunContextInfo.Windows.MajorVersion< 26100) || RunContextInfo.Windows.MajorVersion< 22000;

        internal static Boolean UnlockRegistryKey(String keyPath, String user)
        {
            if (!Execute.Process(new(RunContextInfo.ExecutablePath + "\\assets\\SetACL.exe", $"-on \"{keyPath}\" -ot reg -actn setowner -ownr \"n:{user}\" -rec Yes", true, true, true)).Success) return false;
            if (!Execute.Process(new(RunContextInfo.ExecutablePath + "\\assets\\SetACL.exe", $"-on \"{keyPath}\" -ot reg -actn ace -ace \"n:{user};p:full\" -rec Yes", true, true, true)).Success) return false;

            return true;
        }

        internal static Boolean RestartExplorerForUser()
        {
            if (!KillExplorer(false)) return false;

            Execute.StartInfo startInfo = new("c:\\windows\\explorer.exe");

            return Execute.Process(startInfo).Success;
        }

        internal static Boolean KillExplorer(Boolean wholeMachine = false)
        {
            Execute.StartInfo startInfo;

            if (wholeMachine) startInfo = new("c:\\windows\\system32\\taskkill.exe", $"/f /fi \"USERNAME eq {RunContextInfo.Windows.Username}\" /im explorer.exe", true, true, true);
            else startInfo = new("c:\\windows\\system32\\taskkill.exe", $"/f /im explorer.exe", true, true, true);

            return Execute.Process(startInfo).Success;
        }

        internal static class WindowsLicenseStatus
        {
            /// <summary>Retrieves the Windows activation status using the WMI</summary>
            public static Boolean GetStatus(out String licenseMessage)
            {
                Byte status = QueryStatus();

                licenseMessage = LicenseStatusToString(status);

                return status == 1;
            }

            /// <summary>Maps the WMI license status to its representative string</summary>
            public static String LicenseStatusToString(Byte status) => (status) switch
            {
                0 => "Unlicensed: 0",
                1 => "Licensed: 1",
                2 => "OOBGrace: 2",
                3 => "OOTGrace: 3",
                4 => "NonGenuineGrace: 4",
                5 => "Notification: 5",
                6 => "ExtendedGrace: 6",
                _ => "unknown_status: error",
            };

            /// <summary>Retrieves the Windows activation status as byte using WMI</summary>
            public static Byte QueryStatus()
            {
                SelectQuery licenseQuery = new(@"SELECT LicenseStatus
                                             FROM SoftwareLicensingProduct
                                             WHERE PartialProductKey is not null AND Name LIKE 'Windows%'");

                ManagementObjectSearcher searcher = new(licenseQuery);

                ManagementBaseObject wmiObject = searcher.Get().OfType<ManagementBaseObject>().FirstOrDefault();

                return Byte.Parse($"{wmiObject["LicenseStatus"]}");
            }
        }

        internal static class Execute
        {
            internal readonly ref struct StartInfo
            {
                internal StartInfo(String path) => Path = path;

                internal StartInfo(String path, String args)
                {
                    Path = path;
                    Args = args;
                }

                internal StartInfo(String path, String args, Boolean runAs)
                {
                    Path = path;
                    Args = args;
                    RunAs = runAs;
                }

                internal StartInfo(String path, String args, Boolean runAs, Boolean waitForExit)
                {
                    Path = path;
                    Args = args;
                    RunAs = runAs;
                    WaitForExit = waitForExit;
                }

                internal StartInfo(String path, String args, Boolean runAs, Boolean waitForExit, Boolean hiddenExecute)
                {
                    Path = path;
                    Args = args;
                    RunAs = runAs;
                    WaitForExit = waitForExit;
                    HiddenExecute = hiddenExecute;
                }

                internal StartInfo(String path, String args, Boolean runAs, Boolean waitForExit, Boolean hiddenExecute, Boolean redirectOutput)
                {
                    Path = path;
                    Args = args;
                    RunAs = runAs;
                    WaitForExit = waitForExit;
                    HiddenExecute = hiddenExecute;
                    RedirectOutput = redirectOutput;
                }

                internal StartInfo(String path, String args, Boolean runAs, Boolean waitForExit, Boolean hiddenExecute, Boolean redirectOutput, String workingDirectory)
                {
                    Path = path;
                    Args = args;
                    RunAs = runAs;
                    WaitForExit = waitForExit;
                    HiddenExecute = hiddenExecute;
                    RedirectOutput = redirectOutput;
                    WorkingDirectory = workingDirectory;
                }

                internal readonly String Path;
                internal readonly String Args;
                internal readonly String WorkingDirectory;
                internal readonly Boolean RunAs;
                internal readonly Boolean RedirectOutput;
                internal readonly Boolean HiddenExecute;
                internal readonly Boolean WaitForExit;
            }

            internal ref struct Result
            {
                public Result() => Success = false;

                internal Result(Boolean success) => Success = success;

                internal Result(Int32 exitCode, String stdOut, String stdErr)
                {
                    ExitCode = exitCode;
                    StdOut = stdOut;
                    StdErr = stdErr;

                    Success = true;
                }

                internal Boolean Success = false;

                internal Int32 ExitCode;
                internal String StdOut;
                internal String StdErr;
            }

            internal static Result Process(StartInfo startInfo)
            {
                if (!File.Exists(startInfo.Path))
                {
                    Log.FastLog($"'{startInfo.Path}' not found", LogSeverity.Error, "Util.Execute.Process()");
                    return new(false);
                }

                using Process process = new();

                process.StartInfo.FileName = startInfo.Path;

                if (startInfo.RunAs)
                {
                    process.StartInfo.Verb = "runas";
                }

                if (startInfo.HiddenExecute)
                {
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.CreateNoWindow = true;
                }

                if (startInfo.RedirectOutput)
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                }
                else
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.StartInfo.RedirectStandardError = false;
                }

                if (startInfo.WorkingDirectory != null)
                {
                    process.StartInfo.WorkingDirectory = startInfo.WorkingDirectory;
                }

                if (startInfo.Args != null)
                {
                    process.StartInfo.Arguments = startInfo.Args;
                }

                try
                {
                    process.Start();
                }
                catch
                {
                    Log.FastLog($"Failed to start '{startInfo.Path}' no access or not an executable?", LogSeverity.Error, "Util.Execute.Process()");
                    return new();
                }

                if (startInfo.RedirectOutput)
                {
                    String stdOUT = process.StandardOutput.ReadToEnd();
                    String errOUT = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    return new(process.ExitCode, stdOUT, errOUT);
                }
                else if (startInfo.WaitForExit)
                {
                    process.WaitForExit();

                    return new(process.ExitCode, null, null);
                }

                return new(true);
            }
        }




    }
}