using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using System.Threading;
//libs
using static PowershellHelper.PowershellHelper;
using RegistryTools;
using WinUser;

namespace WinUtil_Main
{
    public partial class MainWindow : Window
    {
        //Window UI buttons

        //minimize
        private void Button_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MinimizeButtonMouseIsOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimizeButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2d2d2d"));
        }

        private void MinimizeButtonMouseIsNotOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimizeButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(WindowControllButton));
        }

        private void MinimizeButtonMouseClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimizeButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a2a"));
        }

        //change window state

        private void Button_ToggleWindowState(object sender, RoutedEventArgs e)
        {
            Toggle_WindowState();
            SetIconFromGetState_MinMaxIcon();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            

            SetIconFromGetState_MinMaxIcon();

            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness(8, 8, 8, 8);
            }
            else
            {
                BorderThickness = new Thickness(0, 0, 0, 0);
            }

            
        }

        private void Toggle_WindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness(0, 0, 0, 0);

                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;

                BorderThickness = new Thickness(8, 8, 8, 8);
            }
        }

        private void SetIconFromGetState_MinMaxIcon()
        {
            if (WindowState == WindowState.Maximized)
            {
                Minimize_Button.Margin = new Thickness(0, -31, 93, 0);
                Minimize_Button.Height = 31;

                Close_Button.Margin = new Thickness(0, -31, 0, 0);
                Close_Button.Height = 31;
                Close_Button.Width = 47;

                //

                ButtonStateIsNormal.Visibility = Visibility.Hidden;
                ButtonStateIsMaximized.Visibility = Visibility.Visible;

                IconStateIsNormal.Visibility = Visibility.Hidden;
                IconStateIsMaximized.Visibility = Visibility.Visible;
            }
            else
            {
                Minimize_Button.Margin = new Thickness(0, -29, 93, 0);
                Minimize_Button.Height = 29;

                Close_Button.Margin = new Thickness(0, -29, 2, 0);
                Close_Button.Height = 29;
                Close_Button.Width = 45;

                //

                ButtonStateIsNormal.Visibility = Visibility.Visible;
                ButtonStateIsMaximized.Visibility = Visibility.Hidden;

                IconStateIsNormal.Visibility = Visibility.Visible;
                IconStateIsMaximized.Visibility = Visibility.Hidden;
            }
        }

        private void ChangeWindowStateColorMouseIsOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeWindowStateColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2d2d2d"));
        }

        private void ChangeWindowStateColorMouseIsNotOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeWindowStateColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(WindowControllButton));
        }

        private void ChangeWindowStateColoMouseClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeWindowStateColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a2a"));
        }

        //close button

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseButtonColorMouseIsOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c42b1c"));
        }

        private void CloseButtonColorMouseIsNotOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(WindowControllButton));
        }

        private void CloseButtonColorMouseClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b22a1b"));
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private void OnRescale(object sender, SizeChangedEventArgs e)
        {
            Rescale();
        }

        //colors
        private const String WindowControllButton = "#202020";

        private const String FontColor = "#f0f0f0";

        private const String ContentPanel = "#282828";

        private const String LogPanel = "#131317";

        //#######################################################################################################

        //SDG_Functions

        private void LogBoxRemoveLine(UInt32 Lines = 1)
        {
            for (UInt32 I = 0; I < Lines; I++)
            {
                LogTextBox.Document.Blocks.Remove(LogTextBox.Document.Blocks.LastBlock);
            }
        }
        public void DispatchedLogBoxRemoveLine(UInt32 Lines = 1)
        {
            Dispatcher.Invoke(new Action(() => { LogBoxRemoveLine(Lines); }));
        }
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
        public void DispatchedLogBoxAdd(String Text, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Dispatcher.Invoke(new Action(() => { LogBoxAdd(Text, Foreground, Background, StayInLine, ScrollToEnd, FontWeight); }));
        }

        //#######################################################################################################

        public MainWindow()
        {
            InitializeComponent();

            //apply color
            this.Resources["BackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(WindowControllButton));
            this.Resources["ContentPanel"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentPanel));
            this.Resources["LogPanel"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(LogPanel));

            Init();
        }

        private async void Init()
        {
            ActivateWorker();

            //verify external modules
            await Task.Run(() =>
            {
                DispatchedLogBoxAdd("Verifying program files\n", Brushes.LightGray, StayInLine: true);

                Boolean ErrorAction = false;

                //general

                if (!InternalHashCalculator("ManagedNativeWifi.dll").Equals(ExtResources.ManagedNativeWifi, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid ManagedNativeWifi.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("System.Runtime.CompilerServices.Unsafe.dll").Equals(ExtResources.SystemRuntimeCompilerServicesUnsafe, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid System.Runtime.CompilerServices.Unsafe.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("System.Buffers.dll").Equals(ExtResources.SystemBuffersdll, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid System.Buffers.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("System.Diagnostics.DiagnosticSource.dll").Equals(ExtResources.SystemDiagnosticsDiagnosticSourcedll, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid System.Diagnostics.DiagnosticSource.dll", Brushes.OrangeRed);
                }
                
                if (!InternalHashCalculator("System.Memory.dll").Equals(ExtResources.SystemMemorydll, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid System.Memory.dll", Brushes.OrangeRed);
                }
                
                if (!InternalHashCalculator("System.Numerics.Vectors.dll").Equals(ExtResources.SystemNumericsVectorsdll, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid System.Numerics.Vectors.dll", Brushes.OrangeRed);
                }

                //custom libs

                if (!InternalHashCalculator("CustomWinMessageBox.dll").Equals(ExtResources.CustomWinMessageBox, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid CustomWinMessageBox.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("HashTools.dll").Equals(ExtResources.HashTools, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid HashTools.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("PowershellHelper.dll").Equals(ExtResources.PowershellHelper, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid PowershellHelper.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("ProgramLauncher.dll").Equals(ExtResources.ProgramLauncher, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid ProgramLauncher.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("RegistryTools.dll").Equals(ExtResources.RegistryTools, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid RegistryTools.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("ServiceTools.dll").Equals(ExtResources.ServiceTools, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid ServiceTools.dll", Brushes.OrangeRed);
                }

                if (!InternalHashCalculator("WinUser.dll").Equals(ExtResources.WinUser, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorAction = true;
                    DispatchedLogBoxAdd("[Critical] invalid WinUser.dll", Brushes.OrangeRed);
                }

                //check

                if (ErrorAction)
                {
                    DeactivateWorker();

                    DispatchedLogBoxAdd("\nTerminating application", Brushes.Red);

                    Task.Delay(7000).Wait();

                    Environment.Exit(1);
                }

                DispatchedLogBoxRemoveLine(2);
            });

            await Task.Run(() => Load());

            DeactivateWorker();
        }

        private void Load()
        {
            //test for proper start

            Boolean NormalStart = true;

            try
            {
                if (!Environment.GetCommandLineArgs()[1].Equals("e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9"))
                {
                    NormalStart = false;
                    DispatchedLogBoxAdd("[Warn] Invalid launch hash: \"" + Environment.GetCommandLineArgs()[1] + "\"\n\n", Brushes.Orange, FontWeight: FontWeights.Bold, StayInLine: true);
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                NormalStart = false;
                DispatchedLogBoxAdd("[Info] Direct start\n\n", Brushes.LightBlue, FontWeight: FontWeights.Bold, StayInLine: true);
            }

            //load info
            try
            {
                DispatchedLogBoxAdd("Getting hostname", Brushes.LightGray);

                ThisMachine.NetBiosHostname = Environment.MachineName;

                ThisMachine.Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

                DispatchedLogBoxRemoveLine();
                DispatchedLogBoxAdd("Getting domain state", Brushes.LightGray);

                //get user infos
                if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName != "")
                {
                    ThisMachine.IsInDomain = true;
                }

                DispatchedLogBoxRemoveLine();
                DispatchedLogBoxAdd("Getting user info", Brushes.LightGray);

                User = WindownsAccountInteract.GetUACUser();

                UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Split('\\')[2];

                DispatchedLogBoxRemoveLine();
                DispatchedLogBoxAdd("Getting Firmware type", Brushes.LightGray);

                //check systemtype
                if (PowerShell("$env:firmware_type", OutPut: true)[0].ToUpper() == "UEFI")
                {
                    ThisMachine.IsUEFI = true;

                    DispatchedLogBoxRemoveLine();
                    DispatchedLogBoxAdd("Getting SecureBoot status", Brushes.LightGray);

                    if (PowerShell("Confirm-SecureBootUEFI", OutPut: true)[0].ToUpper() == "TRUE")
                    {
                        ThisMachine.SecureBootEnabled = true;
                    }
                }

                Task INSTGPO = null;
                if (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID", RegistryValueKind.String, false) == "Core" && (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 2 || RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 1))
                {
                    DispatchedLogBoxRemoveLine();
                    DispatchedLogBoxAdd("Detected Home edition", Brushes.LightGray);

                    INSTGPO = Task.Run(() => new Button_Worker().InstallGPO());
                }

                DispatchedLogBoxRemoveLine();
                DispatchedLogBoxAdd("Getting local Administrator group name", Brushes.LightGray);

                //gets local admingroup name
                AdminGroupName = WindownsAccountInteract.GetAdminGroupName();

                //gets exepath
                ExePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                DispatchedLogBoxRemoveLine();
                DispatchedLogBoxAdd("Getting OS version information", Brushes.LightGray);
                //get OS details
                //get version
                try
                {
                    Int32 CBN = Int32.Parse(RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", RegistryValueKind.String, false));
                    Int32 UBR = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", RegistryValueKind.DWord, false);

                    ThisMachine.OSMajorVersion = CBN;
                    ThisMachine.OSMinorVersion = UBR;
                }
                catch { }

                DispatchedLogBoxRemoveLine();
                DispatchedLogBoxAdd("Getting OS edition", Brushes.LightGray);
                //get type/edition
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
                    default:
                        break;
                }

                //

                UISetter();

                //

                DispatchedLogBoxRemoveLine();
                DispatchedLogBoxAdd("Getting WUT license information", Brushes.LightGray);

                Task WL = null;

                //check if system is activated[set reg value]
                if (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Windows Activation Status", RegistryValueKind.DWord, true) != 1)
                {
                    DispatchedLogBoxAdd("missing, starting license status retriever", Brushes.LightGray);

                    //Retrieves Windows License status from the Windows Management Engine and saves it in the Registry
                    WL = Task.Run(() =>
                    {
                        ActivateWorker();

                        Thread.Sleep(10000);

                        if (PowerShell("$test = Get-CimInstance SoftwareLicensingProduct -Filter \"Name like 'Windows%'\" | where { $_.PartialProductKey } | select LicenseStatus; $test = $test -replace \"@{LicenseStatus=\"; $test = $test -replace \"}\"; reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil\" /v \"Windows Activation Status\" /t reg_dword /d $test /f; Write-Output $test", OutPut: true)[1] == "1")
                        {
                            ThisMachine.WindowsLicenseStatus = 1;

                            //set active status in gui






                        }
                        else
                        {
                            //set ss status in gui
                        }

                        DeactivateWorker();
                    });
                }
                else
                {
                    DispatchedLogBoxAdd("found, status '1'", Brushes.LightGray);

                    ThisMachine.WindowsLicenseStatus = 1;
                }

                //

                DispatchedLogBoxRemoveLine(2);

                if (NormalStart)
                {
                    DispatchedLogBoxAdd("Successfully loaded system information\n", Brushes.LightBlue, StayInLine: true);
                }
                else
                {
                    DispatchedLogBoxAdd("Successfully loaded system information\n", Brushes.LightBlue);
                }

                WL!?.Wait();
                WL!?.Dispose();
                INSTGPO!?.Wait();
                INSTGPO!?.Dispose();

                //     🥄
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd(ex.Message + "\n", Brushes.Red);
            }
        }

        private void UISetter()
        {
            String ProductName = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", RegistryValueKind.String);

            String OS = null;
            String Edition = null;

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









            Dispatcher.Invoke(new Action(() =>
            {
                OSPType.Text = OS;
                OSPEdition.Text = Edition;

            }));
            




        }

        //#######################################################################################################
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

            LogBoxAdd(WindowsArea.ActualHeight.ToString());



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