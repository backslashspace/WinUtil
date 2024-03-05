using Microsoft.Win32;
using System;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
//
using BSS.Launcher;
using BSS.System.Registry;
using BSS.System.Service;

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
                        UInt32? TBSB = xRegistry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", RegistryValueKind.DWord, false);

                        xProcess.Run(Resource_Assets.su10exe, $"{Resource_Assets.su10settings} /nosrp /quiet", waitForExit: true, hiddenExecute: true);

                        //restore
                        if (TBSB != -1 && TBSB != null)
                        {
                            xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", TBSB, RegistryValueKind.DWord);
                        }
                    }
                    else
                    {
                        Telemetry_FState = false;
                        return;
                    }

                    LogBox.Add("Disabling Tailored Experiences\n");
                    xRegistry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableTailoredExperiencesWithDiagnosticData", 1, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, RegistryValueKind.DWord);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Windows Error Reporting\\QueueReporting\" /Disable", waitForExit: true, hiddenExecute: true);

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
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting", "Value", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots", "Value", 0, RegistryValueKind.DWord);

                    LogBox.Add("Disabling Microsoft Geolocation service\n");
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny", RegistryValueKind.String);

                    LogBox.Add("Disabling system telemetry\n");
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /Disable", waitForExit: true, hiddenExecute: true);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\ProgramDataUpdater\" /Disable", waitForExit: true, hiddenExecute: true);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Autochk\\Proxy\" /Disable", waitForExit: true, hiddenExecute: true);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Disable", waitForExit: true, hiddenExecute: true);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /Disable", waitForExit: true, hiddenExecute: true);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\DiskDiagnostic\\Microsoft-Windows-DiskDiagnosticDataCollector\" /Disable", waitForExit: true, hiddenExecute: true);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClient\" /Disable", waitForExit: true, hiddenExecute: true);
                    xProcess.Run("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Feedback\\Siuf\\DmClientOnScenarioDownload\" /Disable", waitForExit: true, hiddenExecute: true);

                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);

                    LogBox.Add("Removing advertisement ID");
                    xRegistry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord);
                    xRegistry.DeleteValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Id", true);
                    xRegistry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord);

                    LogBox.Add("Deactivating website access to language list");
                    xRegistry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, RegistryValueKind.DWord);
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