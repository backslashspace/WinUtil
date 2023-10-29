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
using Win32Tools;
using RegistryTools;
using WinUser;

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

        #region Window Builder
        public MainWindow()
        {
            InitializeComponent();

            Dispatcher_Static = Dispatcher;

            WorkIndicator_Static = WorkIndicator;
            MainWindowIcon_Static = MainWindowIcon;

            LogScrollViewer_Static = Log_ScrollViewer;
            Log_Static = Log_RichTextBox;

            Init_Window();
        }

        private async void Init_Window()
        {
            ActivateWorker();

            Boolean HashError = false;

            if (!InitialValidator("ManagedNativeWifi.dll").Equals(EXT_DLL.ManagedNativeWifi, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid ManagedNativeWifi.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("HashTools.dll").Equals(EXT_DLL.HashTools, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid HashTools.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("PowershellHelper.dll").Equals(EXT_DLL.PowershellHelper, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid PowershellHelper.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("ProgramLauncher.dll").Equals(EXT_DLL.ProgramLauncher, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid ProgramLauncher.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("RegistryTools.dll").Equals(EXT_DLL.RegistryTools, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid RegistryTools.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("ServiceTools.dll").Equals(EXT_DLL.ServiceTools, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid ServiceTools.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("WinUser.dll").Equals(EXT_DLL.WinUser, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid WinUser.dll", Brushes.OrangeRed);
            }

            if (!InitialValidator("Win32Tools.dll").Equals(EXT_DLL.Win32Tools, StringComparison.OrdinalIgnoreCase))
            {
                HashError = true;
                LogBoxAdd("[Critical] invalid Win32Tools.dll", Brushes.OrangeRed);
            }

            //

            if (HashError)
            {
                DeactivateWorker();

                LogBoxAdd("\nTerminating application", Brushes.Red);

                await Task.Delay(7000);

                Environment.Exit(1);
            }
            else
            {
                LogBoxAdd("Verified program files\n", Brushes.LightGreen, StayInLine: true);
            }

            await Task.Run(() => Load());

            DeactivateWorker();

            //# # # # # # # # # #

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
        }

        private void Load()
        {
            //test for proper start
            try
            {
                if (!Environment.GetCommandLineArgs()[1].Equals("e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9"))
                {
                    DispatchedLogBoxAdd($"[Warn] Invalid launch hash: \"{Environment.GetCommandLineArgs()[1]}\"\n", Brushes.Orange, FontWeight: FontWeights.Bold);
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
                DispatchedLogBoxAdd($"Hostname = {Machine.Hostname}\nNetBios Hostname = {Machine.NetBiosHostname}", Brushes.DarkGray);

                if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName != "")
                {
                    Machine.IsInDomain = true;
                }
                DispatchedLogBoxAdd($"Is Domain joined = {Machine.IsInDomain}", Brushes.DarkGray);

                Machine.User = WindownsAccountInteract.GetUACUser();
                Machine.UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Split('\\')[2];
                DispatchedLogBoxAdd($"User = '{Machine.User}'", Brushes.DarkGray);
                DispatchedLogBoxAdd($"User Home = '{Machine.UserPath}'", Brushes.DarkGray);

                if (Firmware.GetFWType() == Firmware.FirmwareType.UEFI)
                {
                    Machine.IsUEFI = true;
                    DispatchedLogBoxAdd($"Firmware type is UEFI = {Machine.IsUEFI}", Brushes.DarkGray);

                    if (Firmware.SecureBoot_IsEnabled())
                    {
                        Machine.SecureBootEnabled = true;
                        DispatchedLogBoxAdd($"Secure Boot is enabled = {Machine.SecureBootEnabled}", Brushes.DarkGray);
                    }
                }
                
                Task INSTGPO = null;
                if (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID", RegistryValueKind.String, false) == "Core" && (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 2 || RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 1))
                {
                    DispatchedLogBoxAdd("Detected Home edition", Brushes.DarkGray);

                    //[!] worker display interg
                    //INSTGPO = Task.Run(() => InstallGPO());
                }

                Machine.AdminGroupName = WindownsAccountInteract.GetAdminGroupName();
                DispatchedLogBoxAdd($"Local Administrator group name = {Machine.AdminGroupName}", Brushes.DarkGray);

                //gets exe path
                Machine.ExePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                try
                {
                    Machine.OSMajorVersion = Int32.Parse(RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", RegistryValueKind.String, false));
                    Machine.OSMinorVersion = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", RegistryValueKind.DWord, false);

                    DispatchedLogBoxAdd("Obtained OS version", Brushes.DarkGray);
                }
                catch (Exception ex)
                {
                    DispatchedLogBoxAdd($"Error obtaining Version info: {ex.Message}\n", Brushes.Red);
                }

                switch (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallationType", RegistryValueKind.String).ToLower())
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
                DispatchedLogBoxAdd("Obtained OS edition", Brushes.DarkGray);

                DispatchedLogBoxAdd("Obtaining Windows license information.. ", Brushes.DarkGray);

                if (WindowsLicense.GetStatus(out String LicenseMessage))
                {
                    Machine.IsActivated = true;
                }
                else
                {
                    Machine.IsActivated = false;
                }

                DispatchedLogBoxAdd(LicenseMessage, Brushes.DarkGray, StayInLine: true);
                
                //

                UISetter(LicenseMessage);

                //

                DispatchedLogBoxAdd("\nSuccessfully loaded system information\n", Brushes.LightBlue);

                INSTGPO!?.Wait();
                INSTGPO!?.Dispose();
            }
            catch (Exception ex)
            {
                DispatchedLogBoxAdd($"{ex.Message}\n", Brushes.Red);
            }
        }

        private void UISetter(String LicenseMessage)
        {
            String OS = null;
            String Edition = null;

            String DisplayVersion = null;
            String VersionNumber = $"[{Machine.OSMajorVersion}.{Machine.OSMinorVersion}]";

            String UEFIIsOn = $"UEFI enabled: {Machine.IsUEFI}";
            String SecureBootIsOn = $"SecureBoot enabled: {Machine.SecureBootEnabled}";

            //OSPType & OSPEdition

            String ProductName = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", RegistryValueKind.String);

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

            DisplayVersion = $"Version: {RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "DisplayVersion", RegistryValueKind.String)}";

            //set

            Dispatcher.Invoke(new Action(() =>
            {
                OverviewGrid.OSPType.Text = OS;
                OverviewGrid.OSPEdition.Text = Edition;

                OverviewGrid.WinVersion.Text = DisplayVersion;
                OverviewGrid.BaU.Text = VersionNumber;

                OverviewGrid.SysType.Text = UEFIIsOn;
                OverviewGrid.SecBoot.Text = SecureBootIsOn;

                OverviewGrid.LicenseStatus.Text = LicenseMessage;
            }));
        }

        #endregion

        //#######################################################################################################

        #region Window Scaling
        private void OnRescale(object sender, SizeChangedEventArgs e)
        {
            Rescale();
        }

        //

        private Boolean[] Navigation_Button_Size_State = { true, true };
        private Boolean[] Navigation_Button_Area_Size = { true, true };

        internal void Rescale()
        {
            Log_Static.Document.PageWidth = Log_Static.ActualWidth;

            Double Window_Height;
            Double Window_Width;

            #region Window_State_Aware
            if (WindowState == WindowState.Maximized)
            {
                Window_Height = ActualHeight - 16;
                Window_Width = ActualWidth - 16;
            }
            else
            {
                Window_Height = ActualHeight;
                Window_Width = ActualWidth;
            }
            #endregion

            //

            #region Content area size
            if (Window_Width > 1400)
            {
                if (Navigation_Button_Area_Size[1])
                {
                    Navigation_Column.Width = new GridLength(241, GridUnitType.Pixel);

                    Navigation_Button_Area_Size[0] = true;
                    Navigation_Button_Area_Size[1] = false;
                }
            }
            else if (Window_Width < 951)
            {
                if (Navigation_Button_Area_Size[0])
                {
                    Navigation_Column.Width = new GridLength(166, GridUnitType.Pixel);

                    Navigation_Button_Area_Size[0] = false;
                    Navigation_Button_Area_Size[1] = true;
                }
            }
            else
            {
                Navigation_Column.Width = new GridLength(166 + ((Window_Width - 950) / 6), GridUnitType.Pixel);

                Navigation_Button_Area_Size[0] = true;
                Navigation_Button_Area_Size[1] = true;
            }
            #endregion

            #region nav buttons
            if (Window_Width < 1200 || Window_Height < 708)
            {
                if (Navigation_Button_Size_State[0])
                {
                    Byte N_IconSize = 18;

                    Navigation_Button_Size_State[0] = false;
                    Navigation_Button_Size_State[1] = true;

                    Overview.FontSize = 15;
                    Appearance.FontSize = 15;
                    Behavior.FontSize = 15;
                    Privacy.FontSize = 15;
                    Security.FontSize = 15;
                    Programs.FontSize = 15;

                    Overview.Height = 52;
                    Overview.Margin = new Thickness(9, 0, 0, 0);
                    Navigation_Button_Overview_Icon.Height = N_IconSize;

                    Appearance.Height = 36;
                    Appearance.Margin = new Thickness(9, 58, 0, 0);
                    Navigation_Button_Appearance_Icon.Height = N_IconSize;

                    Behavior.Height = 36;
                    Behavior.Margin = new Thickness(9, 98, 0, 0);
                    Navigation_Button_Behavior_Icon.Height = N_IconSize;

                    Privacy.Height = 36;
                    Privacy.Margin = new Thickness(9, 138, 0, 0);
                    Navigation_Button_Privacy_Icon.Height = N_IconSize;

                    Security.Height = 36;
                    Security.Margin = new Thickness(9, 178, 0, 0);
                    Navigation_Button_Security_Icon.Height = N_IconSize;

                    Programs.Height = 36;
                    Programs.Margin = new Thickness(9, 218, 0, 0);
                    Navigation_Button_Programs_Icon.Height = N_IconSize;
                }
            }
            else
            {
                if (Navigation_Button_Size_State[1])
                {
                    Byte L_IconSize = 22;

                    Navigation_Button_Size_State[0] = true;
                    Navigation_Button_Size_State[1] = false;

                    Overview.FontSize = 17;
                    Appearance.FontSize = 17;
                    Behavior.FontSize = 17;
                    Privacy.FontSize = 17;
                    Security.FontSize = 17;
                    Programs.FontSize = 17;

                    Overview.Height = 54;
                    Overview.Margin = new Thickness(9, 0, 0, 0);
                    Navigation_Button_Overview_Icon.Height = L_IconSize;

                    Appearance.Height = 44;
                    Appearance.Margin = new Thickness(9, 60, 0, 0);
                    Navigation_Button_Appearance_Icon.Height = L_IconSize;

                    Behavior.Height = 44;
                    Behavior.Margin = new Thickness(9, 108, 0, 0);
                    Navigation_Button_Behavior_Icon.Height = L_IconSize;

                    Privacy.Height = 44;
                    Privacy.Margin = new Thickness(9, 156, 0, 0);
                    Navigation_Button_Privacy_Icon.Height = L_IconSize;

                    Security.Height = 44;
                    Security.Margin = new Thickness(9, 204, 0, 0);
                    Navigation_Button_Security_Icon.Height = L_IconSize;

                    Programs.Height = 44;
                    Programs.Margin = new Thickness(9, 252, 0, 0);
                    Navigation_Button_Programs_Icon.Height = L_IconSize;
                }
            }
            #endregion
        }
        #endregion
    }
}