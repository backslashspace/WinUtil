using Microsoft.Win32;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
//
using BSS.System.Windows;
using BSS.System.Registry;

namespace WinUtil
{
    internal class SystemInfo
    {
        internal static void Load()
        {
            try
            {
                //check launch hash
                try
                {
                    if (!Environment.GetCommandLineArgs()[1].Equals("e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9"))
                    {
                        LogBox.Add($"[Warn] Invalid launch hash: \"{Environment.GetCommandLineArgs()[1]}\"\n", Brushes.Orange, fontWeight: FontWeights.Bold);
                    }
                }
                catch (System.IndexOutOfRangeException)
                {
                    Application.Dispatcher.Invoke(() => Application.Object.Window_Title.Text += " - Direct start");
                }



















                //get os version
                try
                {
                    Machine.OSMajorVersion = UInt32.Parse(xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", RegistryValueKind.String, false));
                    Machine.OSMinorVersion = xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", RegistryValueKind.DWord, false);

                    LogBox.Add("Obtained OS version", Brushes.DarkGray);
                }
                catch (Exception ex)
                {
                    LogBox.Add($"Error obtaining Version info: {ex.Message}\n", Brushes.Red);
                }

                //get os edition
                switch (xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallationType", RegistryValueKind.String).ToLower())
                {
                    case "server":
                        Machine.Role = Machine.HostRole.Server;
                        if (Machine.OSMajorVersion >= 20348)
                        {
                            Machine.WindowPlatformFeatureCompliance = Machine.OSPlatformFeatureCompliance.Windows11_Server2022;
                        }
                        break;
                    case "client":
                        if (Machine.OSMajorVersion >= 22000)
                        {
                            Machine.UIVersion = Machine.WindowsUIVersion.Windows11;
                            Machine.WindowPlatformFeatureCompliance = Machine.OSPlatformFeatureCompliance.Windows11_Server2022;
                        }
                        break;
                }

                LogBox.Add("Obtained OS edition", Brushes.DarkGray);

                Application.Dispatcher.BeginInvoke(new Action(() => Application.Object.AppearanceGrid.External_Set_OS_Aware_Context_Button_State()));

                //

                Machine.NetBiosHostname = Environment.MachineName;
                Machine.Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                LogBox.Add($"Hostname = {Machine.Hostname}\nNetBios Hostname = {Machine.NetBiosHostname}", Brushes.DarkGray);

                if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName != "")
                {
                    Machine.IsInDomain = true;
                }
                LogBox.Add($"Is Domain joined = {Machine.IsInDomain}", Brushes.DarkGray);

                Machine.User = xLocalUsers.GetUACUser();
                Machine.UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Split('\\')[2];
                LogBox.Add($"User = '{Machine.User}'", Brushes.DarkGray);
                LogBox.Add($"User Home = '{Machine.UserPath}'", Brushes.DarkGray);

                if (xFirmware.GetFWType() == xFirmware.FirmwareType.UEFI)
                {
                    Machine.IsUEFI = true;
                    LogBox.Add($"OS Firmware type is UEFI = {Machine.IsUEFI}", Brushes.DarkGray);

                    if (xFirmware.GetSecureBootStatus())
                    {
                        Machine.SecureBootEnabled = true;
                        LogBox.Add($"Secure Boot is enabled = {Machine.SecureBootEnabled}", Brushes.DarkGray);
                    }
                }

                Task INSTGPO = null;
                if (xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID", RegistryValueKind.String, false) == "Core"
                    && (xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 2
                    || xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 1))
                {
                    LogBox.Add("Detected Home edition", Brushes.DarkGray);

                    //[!] worker display interg
                    //INSTGPO = Task.Run(() => InstallGPO());
                }

                Machine.AdminGroupName = xLocalGroups.GetAdminGroupName();
                LogBox.Add($"Local Administrator group name = {Machine.AdminGroupName}", Brushes.DarkGray);

                //gets exe path
                Machine.ExePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                LogBox.Add("Obtaining Windows license information.. ", Brushes.DarkGray);

                if (xWinLicense.GetStatus(out String LicenseMessage))
                {
                    Machine.IsActivated = true;
                }
                else
                {
                    Machine.IsActivated = false;
                }

                LogBox.Add(LicenseMessage, Brushes.DarkGray, stayInLine: true);
                Application.Dispatcher.BeginInvoke(new Action(() => Application.Object.OverviewGrid.LicenseStatus.Text = LicenseMessage));

                //

                LogBox.Add("\nSuccessfully loaded system information\n", Brushes.LightBlue);

                INSTGPO!?.Wait();
                INSTGPO!?.Dispose();
            }
            catch (Exception ex)
            {
                LogBox.Add($"{ex.Message}\n", Brushes.Red);
            }
        }
    }
}