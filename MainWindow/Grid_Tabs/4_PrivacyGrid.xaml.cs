using Microsoft.Win32;
using System;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//
using EXT.Launcher.Process;
using EXT.System.Registry;
using EXT.System.Service;
using System.Threading.Tasks;

namespace WinUtil.Grid_Tabs
{
    public partial class PrivacyGrid : UserControl
    {
        public PrivacyGrid()
        {
            InitializeComponent();
        }

        private static Boolean Telemetry_FState = false;
        private void Telemetry(object sender, RoutedEventArgs e)
        {
            if (Telemetry_FState)
            {
                return;
            }

            Telemetry_FState = true;

            MainWindow.ActivateWorker();

            Task.Run(() =>
            {
                try
                {
                    LogBox.Add("Deactivating telemetry", Brushes.LightBlue);

                    LogBox.Add("Running O&O ShutUp10");
                    if (Global.VerboseHashCheck(Resource_Assets.su10exe, Resource_Assets.su10exeHash)[0] && Global.VerboseHashCheck(Resource_Assets.su10settings, Resource_Assets.su10settingsHash)[0])
                    {
                        Int32? TBSB = xRegistry.Get.Value(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", RegistryValueKind.DWord, false);

                        xProcess.Run(Resource_Assets.su10exe, $"{Resource_Assets.su10settings} /nosrp /quiet", WaitForExit: true, HiddenExecute: true);

                        //restore
                        if (TBSB != -1 && TBSB != null)
                        {
                            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", TBSB, RegistryValueKind.DWord);
                        }
                    }
                    else
                    {
                        Telemetry_FState = false;
                        return;
                    }

                    LogBox.Add("Disabling Tailored Experiences\n");
                    Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableTailoredExperiencesWithDiagnosticData", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, RegistryValueKind.DWord);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Windows Error Reporting\\QueueReporting\" /Disable", true, false);

                    LogBox.Add("Stopping and disabling Diagnostics Tracking Service\n");
                    xService.SetStartupType("DiagTrack", ServiceStartMode.Disabled);

                    try
                    {
                        LogBox.Add("Stopping and disabling Intel ME service\n");
                        xService.SetStartupType("WMIRegistrationService", ServiceStartMode.Disabled);
                    }
                    catch { }

                    LogBox.Add("Stopping and disabling Superfetch service\n");
                    xService.SetStartupType("SysMain", ServiceStartMode.Disabled);

                    LogBox.Add("Disabling Wi-Fi Sense\n");
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting", "Value", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots", "Value", 0, RegistryValueKind.DWord);

                    LogBox.Add("Disabling Microsoft Geolocation service\n");
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny", RegistryValueKind.String);

                    LogBox.Add("Disabling system telemetry\n");
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /Disable", true, false);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\ProgramDataUpdater\" /Disable", true, false);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Autochk\\Proxy\" /Disable", true, false);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Disable", true, false);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /Disable", true, false);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\DiskDiagnostic\\Microsoft-Windows-DiskDiagnosticDataCollector\" /Disable", true, false);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClient\" /Disable", true, false);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClientOnScenarioDownload\" /Disable", true, false);

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);

                    LogBox.Add("Removing advertisement ID");
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord);
                    xRegistry.Delete.DeleteValues(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", new String[] { "Id" });
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord);

                    LogBox.Add("Deactivating website access to language list");
                    Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    LogBox.Add(ex.Message + "\n", Brushes.Red);
                }

                MainWindow.DeactivateWorker();

                Telemetry_FState = false;
            });
        }










    }
}