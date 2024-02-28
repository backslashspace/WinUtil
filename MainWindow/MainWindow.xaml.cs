using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.Security.Cryptography;
//libs
using EXT.System.User;
using EXT.System.Firmware;
using EXT.System.License;
using EXT.System.Registry;
using EXT.System.Group;
using EXT.Launcher.Process;
using WinUtil.Grid_Tabs;

namespace WinUtil
{
    public partial class MainWindow : Window
    {
        internal static Dispatcher Dispatcher_Static;

        internal static Viewbox WorkIndicator_Static;
        internal static Image MainWindowIcon_Static;

        internal static ScrollViewer LogScrollViewer_Static;
        internal static RichTextBox Log_Static;

        //#######################################################################################################

        public MainWindow()
        {
            InitializeComponent();

            EnableDebug();

            Dispatcher_Static = Dispatcher;

            WorkIndicator_Static = WorkIndicator;
            MainWindowIcon_Static = MainWindowIcon;

            LogScrollViewer_Static = Log_ScrollViewer;
            Log_Static = Log_RichTextBox;

            Boolean hadErrors = Verify();

            if (hadErrors)
            {
                DeactivateWorker();

                LogBoxAdd("\nTerminating application", Brushes.Red);

                Task.Run(() => LoadErrorTimedExit());

                return;
            }
            else
            {
                LogBoxAdd("Verified program files\n", Brushes.LightGreen, StayInLine: true);
            }

            Loaded += InitiateApplication;
        }

        private static void LoadErrorTimedExit()
        {
            Task.Delay(7000).Wait();

            Environment.Exit(1);
        }

        private Boolean Verify()
        {
            ActivateWorker();

            Boolean hadErrors = false;
            String errorString = "";

            if (!Validator("ManagedNativeWifi.dll").Equals(EXT_DLL.ManagedNativeWifi, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid ManagedNativeWifi.dll");
            }

            if (!Validator("WinX64_HashTools.dll").Equals(EXT_DLL.WinX64_HashTools, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid WinX64_HashTools.dll");
            }

            if (!Validator("WinX64_Launcher.dll").Equals(EXT_DLL.WinX64_Launcher, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid WinX64_Launcher.dll");
            }

            if (!Validator("WinX64_System.dll").Equals(EXT_DLL.WinX64_System, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid WinX64_System.dll");
            }

            if (!Validator("System.CodeDom.dll").Equals(EXT_DLL.CodeDom, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid System.CodeDom.dll");
            }

            if (hadErrors)
            {
                LogBoxAdd(errorString, Brushes.OrangeRed, StayInLine: true);
            }

            return hadErrors;

            //# # # # # # # # # #

            static String Validator(String FilePath)
            {
                if (!File.Exists(FilePath))
                {
                    return "";
                }

                using SHA256 SHA256 = SHA256.Create();

                String Hash;

                try
                {
                    using FileStream Stream = File.OpenRead(FilePath);

                    Hash = BitConverter.ToString(SHA256.ComputeHash(Stream)).Replace("-", String.Empty);

                    Stream.Close();
                    Stream.Dispose();
                }
                catch
                {
                    Hash = null;
                }

                SHA256.Dispose();

                return Hash;
            }

            static void BuildErrorString(ref String errorString, String line)
            {
                errorString += errorString == "" ? line : "\n" + line;
            }
        }

        private async void InitiateApplication(object sender, EventArgs e)
        {
            await Task.Run(() => Load());

            DeactivateWorker();
        }

        //async
        private void Load()
        {
            //get os version
            try
            {
                Machine.OSMajorVersion = Int32.Parse(xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", RegistryValueKind.String, false));
                Machine.OSMinorVersion = xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", RegistryValueKind.DWord, false);

                LogBox.Add("Obtained OS version", Brushes.DarkGray);
            }
            catch (Exception ex)
            {
                LogBox.Add($"Error obtaining Version info: {ex.Message}\n", Brushes.Red);
            }

            //get os edition
            switch (xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallationType", RegistryValueKind.String).ToLower())
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

            Dispatcher.Invoke(new Action(() => AppearanceGrid.SetContextButtonVisibility()));

            //check launch hash
            try
            {
                if (!Environment.GetCommandLineArgs()[1].Equals("e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9"))
                {
                    LogBox.Add($"[Warn] Invalid launch hash: \"{Environment.GetCommandLineArgs()[1]}\"\n", Brushes.Orange, FontWeight: FontWeights.Bold);
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                Dispatcher_Static.Invoke(new Action(() => Window_Title.Text += " - Direct start"));
            }

            //load info
            try
            {
                Machine.NetBiosHostname = Environment.MachineName;
                Machine.Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                LogBox.Add($"Hostname = {Machine.Hostname}\nNetBios Hostname = {Machine.NetBiosHostname}", Brushes.DarkGray);

                if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName != "")
                {
                    Machine.IsInDomain = true;
                }
                LogBox.Add($"Is Domain joined = {Machine.IsInDomain}", Brushes.DarkGray);

                Machine.User = xWindowsUser.GetUACUser();
                Machine.UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Split('\\')[2];
                LogBox.Add($"User = '{Machine.User}'", Brushes.DarkGray);
                LogBox.Add($"User Home = '{Machine.UserPath}'", Brushes.DarkGray);

                if (xFirmware.GetFWType() == xFirmware.FirmwareType.UEFI)
                {
                    Machine.IsUEFI = true;
                    LogBox.Add($"OS Firmware type is UEFI = {Machine.IsUEFI}", Brushes.DarkGray);

                    if (xFirmware.SecureBoot_IsEnabled())
                    {
                        Machine.SecureBootEnabled = true;
                        LogBox.Add($"Secure Boot is enabled = {Machine.SecureBootEnabled}", Brushes.DarkGray);
                    }
                }
                
                Task INSTGPO = null;
                if (xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID", RegistryValueKind.String, false) == "Core" && (xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 2 || xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 1))
                {
                    LogBox.Add("Detected Home edition", Brushes.DarkGray);

                    //[!] worker display interg
                    //INSTGPO = Task.Run(() => InstallGPO());
                }

                Machine.AdminGroupName = xWindowsGroup.GetAdminGroupName();
                LogBox.Add($"Local Administrator group name = {Machine.AdminGroupName}", Brushes.DarkGray);

                //gets exe path
                Machine.ExePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                LogBox.Add("Obtaining Windows license information.. ", Brushes.DarkGray);

                if (xWinLicense.GetStatus(out String LicenseMessage))
                {
                    Machine.IsActivated = true;
                }
                else
                {
                    Machine.IsActivated = false;
                }

                LogBox.Add(LicenseMessage, Brushes.DarkGray, StayInLine: true);
                
                //

                UISetter(LicenseMessage);

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

        //async
        private void UISetter(String LicenseMessage)
        {
            String OS = null;
            String Edition = null;

            String DisplayVersion = null;
            String VersionNumber = $"[{Machine.OSMajorVersion}.{Machine.OSMinorVersion}]";

            String UEFIIsOn = $"UEFI enabled: {Machine.IsUEFI}";
            String SecureBootIsOn = $"SecureBoot enabled: {Machine.SecureBootEnabled}";

            //OSPType & OSPEdition
            String ProductName = xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", RegistryValueKind.String);

            String[] temp = ProductName.Split(' ');

            OS = temp[0] + " ";

            if (Machine.Role == Machine.HostRole.Server)
            {
                OS += temp[1] + temp[2];
            }
            else
            {
                //win 11
                if (Machine.OSMajorVersion >= 22000)
                {
                    OS += "11";
                }
                else
                {
                    OS += temp[1];
                }
            }

            OS += "®️";

            for (Int16 i = 2; i < temp.Length; ++i)
            {
                Edition += temp[i];
            }

            //BaU

            DisplayVersion = $"Version: {xRegistry.Get.Value("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "DisplayVersion", RegistryValueKind.String)}";

            //set
            Dispatcher.Invoke(new Action(() =>
            {
                OverviewGrid.SetInfoBox(ref OS, ref Edition, ref DisplayVersion, ref VersionNumber, ref UEFIIsOn, ref SecureBootIsOn, ref LicenseMessage);
            }));
        }

        //#######################################################################################################

        private static Boolean RSE_FState = false;
        private void Restart_Explorer(object sender, RoutedEventArgs e)
        {
            if (RSE_FState)
            {
                return;
            }

            RSE_FState = true;

            Task.Run(() =>
            {
                xProcess.Run("C:\\Windows\\System32\\taskkill.exe", "/IM explorer.exe /F", WaitForExit: true, HiddenExecute: true);
                Task.Delay(1000).Wait();
                xProcess.Run("C:\\Windows\\explorer.exe", null, WaitForExit: true, HiddenExecute: true);

                RSE_FState = false;
            });
        }
    }
}