using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using System.Windows;

namespace Stimulator
{
    public sealed partial class MainWindow
    {
        private void OnLoaded(Object sender, RoutedEventArgs e)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            BuildNumberBox.Text += fileVersionInfo.FileVersion;

            Log.FastLog("Starting initialization -> collection environment information", LogSeverity.Info, "MainWindow");

            //

            if (!UInt32.TryParse((String)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", null),
                out RunContextInfo.Windows.MajorVersion))
            {
                Log.FastLog("Unable to read Windows CurrentBuildNumber", LogSeverity.Error, "Init");
                Close();
            }

            Object ubr = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", null);
            if (!(ubr is not null and Int32))
            {
                Log.FastLog("Unable to read Windows UBR", LogSeverity.Error, "Init");
                Close();
            }
            RunContextInfo.Windows.MinorVersion = unchecked((UInt32)(Int32)ubr!);

            Log.FastLog($"Running Windows {RunContextInfo.Windows.MajorVersion}.{RunContextInfo.Windows.MinorVersion}", LogSeverity.Info, "Init");

            //

            Object windowsType = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallationType", null);
            if (!(windowsType is not null and String))
            {
                Log.FastLog("Unable to read Windows InstallationType", LogSeverity.Error, "Init");
                Close();
            }
            RunContextInfo.Windows.IsServer = ((String)windowsType!).ToLower() == "server";

            Log.FastLog($"Detected Windows {(RunContextInfo.Windows.IsServer ? "Server" : "Client")}", LogSeverity.Info, "Init");

            //

            Object editionID = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID", null);
            if (editionID is not null and String)
            {
                RunContextInfo.Windows.IsHomeEdition = (String)editionID == "Core";
            }
            
            //

            Log.FastLog($"Hostname:     {RunContextInfo.Windows.HostName}", LogSeverity.Info, "Init");
            Log.FastLog($"NetBios name: {RunContextInfo.Windows.NetBiosHostname}", LogSeverity.Info, "Init");

            //

            String domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (domain != "")
            {
                RunContextInfo.Windows.IsDomainJoined = true;
                RunContextInfo.Windows.Domain = domain;

                Log.FastLog($"Domain: {RunContextInfo.Windows.Domain}", LogSeverity.Info, "Init");
            }
            else
            {
                Log.FastLog("Domain: none", LogSeverity.Info, "Init");
            }

            //

            RunContextInfo.Windows.Username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            RunContextInfo.Windows.UserHomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            Log.FastLog($"Running as user: '{RunContextInfo.Windows.Username}'", LogSeverity.Info, "Init");
            Log.FastLog($"User home path: '{RunContextInfo.Windows.UserHomePath}'", LogSeverity.Info, "Init");

            //

            if (PowerShell.Create().AddScript("$env:firmware_type").Invoke()[0].BaseObject.ToString().ToLower() == "uefi")
            {
                RunContextInfo.Windows.IsUEFI = true;
                Log.FastLog($"System running in UEFI mode", LogSeverity.Info, "Init");
            }
            else
            {
                RunContextInfo.Windows.IsUEFI = false;
                Log.FastLog($"System running in LEGACY mode", LogSeverity.Info, "Init");
            }

            //

            if (RunContextInfo.Windows.IsUEFI)
            {
                try
                {
                    String secureBootQueryResult = PowerShell.Create().AddScript("Confirm-SecureBootUEFI").Invoke()[0].BaseObject.ToString().ToLower();

                    if (secureBootQueryResult == "true")
                    {
                        RunContextInfo.Windows.SecureBootEnabled = true;
                        Log.FastLog("SecureBoot: enabled", LogSeverity.Info, "Init");
                    }
                    else
                    {
                        RunContextInfo.Windows.SecureBootEnabled = false;
                        Log.FastLog("SecureBoot: not enabled", LogSeverity.Info, "Init");
                    }
                }
                catch
                {
                    Log.FastLog($"Failed to query SecureBoot status via PowerShell using: 'Confirm-SecureBootUEFI' – Legacy System? - (Assuming Secure Boot is OFF)", LogSeverity.Warning, "Init");
                    RunContextInfo.Windows.SecureBootEnabled = false;
                }
            }

            //

            SelectQuery groupNameQuery = new(@"SELECT Name FROM Win32_Group WHERE SID LIKE 'S-1-5-32-544'");
            ManagementObjectSearcher searcher = new(groupNameQuery);
            ManagementObject wmiObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault();

            RunContextInfo.Windows.AdministratorGroupName = $"{wmiObject["Name"]}";

            Log.FastLog($"System Administrators group name: '{RunContextInfo.Windows.AdministratorGroupName}'", LogSeverity.Info, "Init");

            //

            Log.FastLog("Done", LogSeverity.Info, "Init");
        }
    }
}