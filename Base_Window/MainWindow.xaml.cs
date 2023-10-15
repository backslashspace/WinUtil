using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using System.Security.Cryptography;
//libs
using static PowershellHelper.PowershellHelper;
using RegistryTools;
using WinUser;
using System.IO;
using System.Windows.Threading;

namespace WinUtil
{
    public partial class MainWindow : Window
    {
        #region Window Builder
        public MainWindow()
        {
            InitializeComponent();

            #region Register usercontrols
            OverviewGrid.Commit_Log += External_LogBoxAdd;
            OverviewGrid.Remove_Log_Line += External_LogBoxRemoveLine;
            #endregion

            Init();
        }

        private async void Init()
        {
            #region HashMake
            static String InitialValidator(String FilePath)
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
            #endregion

            ActivateWorker();

            LogBoxAdd("Verifying program files\n", Brushes.Gray, StayInLine: true);

            Boolean ErrorAction = false;

            if (!InitialValidator("ManagedNativeWifi.dll").Equals(ExtResources.ManagedNativeWifi, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid ManagedNativeWifi.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("CustomWinMessageBox.dll").Equals(ExtResources.CustomWinMessageBox, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid CustomWinMessageBox.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("HashTools.dll").Equals(ExtResources.HashTools, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid HashTools.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("PowershellHelper.dll").Equals(ExtResources.PowershellHelper, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid PowershellHelper.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("ProgramLauncher.dll").Equals(ExtResources.ProgramLauncher, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid ProgramLauncher.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("RegistryTools.dll").Equals(ExtResources.RegistryTools, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid RegistryTools.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("ServiceTools.dll").Equals(ExtResources.ServiceTools, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid ServiceTools.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("WinUser.dll").Equals(ExtResources.WinUser, StringComparison.OrdinalIgnoreCase))
            {
                ErrorAction = true;
                LogBoxAdd("[Critical] invalid WinUser.dll", Brushes.OrangeRed);
            }

            //

            if (ErrorAction)
            {
                DeactivateWorker();

                LogBoxAdd("\nTerminating application", Brushes.Red);

                await Task.Delay(7000);

                Environment.Exit(1);
            }

            await Task.Run(() => Load());

            DeactivateWorker();
        }

        private void Load()
        {
            //test for proper start

            try
            {
                if (!Environment.GetCommandLineArgs()[1].Equals("e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9"))
                {
                    DispatchedLogBoxAdd("[Warn] Invalid launch hash: \"" + Environment.GetCommandLineArgs()[1] + "\"\n", Brushes.Orange, FontWeight: FontWeights.Bold);
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                Dispatcher.Invoke(new Action(() => Window_Title.Text += " - Direct start"));
            }

            //load info
            try
            {
                DispatchedLogBoxAdd("Getting hostname", Brushes.Gray);
                ThisMachine.NetBiosHostname = Environment.MachineName;
                ThisMachine.Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

                DispatchedLogBoxAdd("Getting domain state", Brushes.Gray);
                if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName != "")
                {
                    ThisMachine.IsInDomain = true;
                }

                DispatchedLogBoxAdd("Getting user info", Brushes.Gray);
                User = WindownsAccountInteract.GetUACUser();
                UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Split('\\')[2];

                DispatchedLogBoxAdd("Getting Firmware type", Brushes.Gray);
                if (PowerShell("$env:firmware_type", OutPut: true)[0].ToUpper() == "UEFI")
                {
                    ThisMachine.IsUEFI = true;

                    DispatchedLogBoxAdd("Getting SecureBoot status", Brushes.Gray);

                    if (PowerShell("Confirm-SecureBootUEFI", OutPut: true)[0].ToUpper() == "TRUE")
                    {
                        ThisMachine.SecureBootEnabled = true;
                    }
                }

                Task INSTGPO = null;
                if (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID", RegistryValueKind.String, false) == "Core" && (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 2 || RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 1))
                {
                    DispatchedLogBoxAdd("Detected Home edition", Brushes.Gray);

                    //[!] worker display interg
                    INSTGPO = Task.Run(() => InstallGPO());
                }

                DispatchedLogBoxAdd("Getting local Administrator group name", Brushes.Gray);
                AdminGroupName = WindownsAccountInteract.GetAdminGroupName();

                //gets exe path
                ExePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                DispatchedLogBoxAdd("Getting OS version information", Brushes.Gray);
                try
                {
                    Int32 CBN = Int32.Parse(RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", RegistryValueKind.String, false));
                    Int32 UBR = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", RegistryValueKind.DWord, false);

                    ThisMachine.OSMajorVersion = CBN;
                    ThisMachine.OSMinorVersion = UBR;
                }
                catch (Exception ex)
                {
                    DispatchedLogBoxAdd($"Error obtaining Version info: {ex.Message}\n", Brushes.Gray);
                }

                DispatchedLogBoxAdd("Getting OS edition", Brushes.Gray);
                String IType = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallationType", RegistryValueKind.String).ToLower();

                switch (IType)
                {
                    case "server":
                        ThisMachine.OSType = OSType.Server;
                        if (ThisMachine.OSMajorVersion >= 20348)
                        {
                            ThisMachine.WindowPlatformFeatureCompliance = WindowPlatformFeatureCompliance.Windows11_Server2022;
                        }
                        break;
                    case "client":
                        if (ThisMachine.OSMajorVersion >= 22000)
                        {
                            ThisMachine.UIVersion = WindowsUIVersion.Windows11;
                            ThisMachine.WindowPlatformFeatureCompliance = WindowPlatformFeatureCompliance.Windows11_Server2022;
                        }
                        break;
                }

                //check if system is activated[set reg value]
                DispatchedLogBoxAdd("Getting Windows license information ", Brushes.Gray);

                Task WL = null;
                Int32 LS;

                try
                {
                    LS = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "Windows Activation Status", RegistryValueKind.DWord, true);

                    if (LS == 1)
                    {
                        DispatchedLogBoxAdd("| found, status '1'\n", Brushes.Gray, StayInLine: true);

                        ThisMachine.WindowsLicenseStatus = 1;
                    }
                    else
                    {
                        DispatchedLogBoxAdd("| refreshing license status\n", Brushes.Gray, StayInLine: true);

                        ThisMachine.WindowsLicenseStatus = -1;

                        GetLicense();
                    }
                }
                catch
                {
                    DispatchedLogBoxAdd("| missing, starting license status retriever\n", Brushes.Gray, StayInLine: true);

                    ThisMachine.WindowsLicenseStatus = -1;

                    GetLicense();
                }

                void GetLicense()
                {
                    WL = Task.Run(() =>
                    {
                        try
                        {
                            //Retrieves Windows License status from the Windows Management Engine and saves it in the Registry
                            Int32 License = Int32.Parse(PowerShell("$test = Get-CimInstance SoftwareLicensingProduct -Filter \"Name like 'Windows%'\" | where { $_.PartialProductKey } | select LicenseStatus; $test = $test -replace \"@{LicenseStatus=\"; $test = $test -replace \"}\"; reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil\" /v \"Windows Activation Status\" /t reg_dword /d $test /f; Write-Output $test", OutPut: true)[1]);

                            if (License == 1)
                            {
                                ThisMachine.WindowsLicenseStatus = 1;

                                DispatchedLogBoxAdd("[Info] License status = 1 (activated)\n", Brushes.Gray);

                                Dispatcher.Invoke(new Action(() => OverviewGrid.LicenseStatus.Text = "Licensed: 1"));
                            }
                            else
                            {
                                DispatchedLogBoxAdd($"[Info] License status = {License}\n", Brushes.Gray);

                                Dispatcher.Invoke(new Action(() => OverviewGrid.LicenseStatus.Text = StatusToString(License)));
                            }
                        }
                        catch
                        {
                            DispatchedLogBoxAdd("[Warn] Error retrieving license information\n", Brushes.Orange, FontWeight: FontWeights.Bold);

                            ThisMachine.WindowsLicenseStatus = -2;

                            Dispatcher.Invoke(new Action(() => OverviewGrid.LicenseStatus.Text = StatusToString(-2)));
                        }
                    });
                }

                //

                UISetter();

                //

                DispatchedLogBoxAdd("Successfully loaded system information\n", Brushes.LightBlue);

                INSTGPO!?.Wait();
                INSTGPO!?.Dispose();
                WL!?.Wait();
                WL!?.Dispose();
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd($"{ex.Message}\n", Brushes.Red);
            }
        }

        private void UISetter()
        {
            String OS = null;
            String Edition = null;

            String DisplayVersion = null;
            String VersionNumber = $"[{ThisMachine.OSMajorVersion}.{ThisMachine.OSMinorVersion}]";

            String UEFIIsOn = $"UEFI enabled: {ThisMachine.IsUEFI}";
            String SecureBootIsOn = $"SecureBoot enabled: {ThisMachine.SecureBootEnabled}";

            String LS = null;

            //OSPType & OSPEdition

            String ProductName = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", RegistryValueKind.String);

            String[] temp = ProductName.Split(' ');

            OS = temp[0] + " ";

            if (ThisMachine.OSType == OSType.Server)
            {
                OS += temp[1] + temp[2];
            }
            else
            {
                //win 11
                if (ThisMachine.OSMajorVersion >= 22000)
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

            DisplayVersion = $"Version: {RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "DisplayVersion", RegistryValueKind.String)}";

            //LicenseStatus

            LS = StatusToString(ThisMachine.WindowsLicenseStatus);

            //set

            Dispatcher.Invoke(new Action(() =>
            {
                OverviewGrid.OSPType.Text = OS;
                OverviewGrid.OSPEdition.Text = Edition;

                OverviewGrid.WinVersion.Text = DisplayVersion;
                OverviewGrid.BaU.Text = VersionNumber;

                OverviewGrid.SysType.Text = UEFIIsOn;
                OverviewGrid.SecBoot.Text = SecureBootIsOn;

                OverviewGrid.LicenseStatus.Text = LS;
            }));
        }

        static String StatusToString(Int32 Status)
        {
            return (Status) switch
            {
                -2 => "Error obtaining status",
                -1 => "Retrieving status",
                0 => "Unlicensed: 0",
                1 => "Licensed: 1",
                2 => "OOBGrace: 2",
                3 => "OOTGrace: 3",
                4 => "NonGenuineGrace: 4",
                5 => "Notification: 5",
                6 => "ExtendedGrace: 6",
                _ => "AHHH: error",
            };
        }
        #endregion

        //#######################################################################################################

        internal static String CurrentArea = "Overview";

        private void OnRescale(object sender, SizeChangedEventArgs e)
        {
            Rescale();
        }

        //#######################################################################################################

        #region LogBoxAdd
        private void LogBoxAdd(String Text = null, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Foreground ??= Brushes.LightGray;

            TextRange TxR;

            if (StayInLine)
            {
                TxR = new(LogTextBox.Document.ContentEnd, LogTextBox.Document.ContentEnd)
                {
                    Text = Text
                };
            }
            else
            {
                TxR = new(LogTextBox.Document.ContentEnd, LogTextBox.Document.ContentEnd)
                {
                    Text = "\n" + Text
                };
            }

            TxR.ApplyPropertyValue(TextElement.ForegroundProperty, Foreground);

            if (FontWeight != default)
            {
                TxR.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeight);
            }

            if (Background != null)
            {
                TxR.ApplyPropertyValue(TextElement.BackgroundProperty, Background);
            }

            if (ScrollToEnd == true)
            {
                LogScroller.ScrollToEnd();
            }
        }

        internal void DispatchedLogBoxAdd(String Text = null, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Dispatcher.Invoke(new Action(() => LogBoxAdd(Text, Foreground, Background, StayInLine, ScrollToEnd, FontWeight)));
        }

        private void External_LogBoxAdd(String Text, SolidColorBrush Foreground, SolidColorBrush Background, Boolean StayInLine, Boolean ScrollToEnd, FontWeight FontWeight)
        {
            LogBoxAdd(Text, Foreground, Background, StayInLine, ScrollToEnd, FontWeight);
        }
        #endregion

        #region LogBoxRemoveLine
        private void LogBoxRemoveLine(UInt32 Lines = 1)
        {
            for (UInt32 I = 0; I < Lines; I++)
            {
                LogTextBox.Document.Blocks.Remove(LogTextBox.Document.Blocks.LastBlock);
            }
        }

        internal void DispatchedLogBoxRemoveLine(UInt32 Lines = 1)
        {
            Dispatcher.Invoke(new Action(() => LogBoxRemoveLine(Lines)));
        }

        private void External_LogBoxRemoveLine(UInt32 Lines)
        {
            LogBoxRemoveLine(Lines);
        }
        #endregion

        //#######################################################################################################

        private static Boolean TTT = false;

        private void Debug(object sender, RoutedEventArgs e)
        {
            if (!TTT)
            {
                ActivateWorker();

                TTT = true;
            }
            else
            {
                DeactivateWorker();

                TTT = false;
            }

            LogBoxAdd($"{ActualWidth}");
            LogBoxAdd($"{Navigation_Column.Width}");

            //OSPType.Text = "Windows Server®️";
            //OSPEdition.Text = "Pro for Workstations";
            //WinVersion.Text = "Version: 22H2";
            //BaU.Text = "[22621.1928]";
            //SysType.Text = "UEFI enabled: true";
            //SecBoot.Text = "SecureBoot enabled: true";
            //LicenseStatus.Text = "Activated [1]";








        }
    }
}