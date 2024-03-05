using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//
using BSS.System.Registry;

namespace WinUtil.Grid_Tabs
{
    public partial class OverviewGrid : UserControl
    {
        internal static String LicenseMessage = null;

        public OverviewGrid()
        {
            InitializeComponent();

            Thread uptimeClock = new(() => SysUptimeClock());
            uptimeClock.Name = "OverviewGrid - SysUptimeClock";
            uptimeClock.Start();

            Loaded += OnLoaded;
        }

        //

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UISetter();
        }

        private void UISetter()
        {
            //todo
            String os = null;
            String edition = null;

            String displayVersion = null;
            String versionNumber = $"[{Machine.OSMajorVersion}.{Machine.OSMinorVersion}]";

            String UEFIIsOn = $"UEFI enabled: {Machine.IsUEFI}";
            String secureBootIsOn = $"SecureBoot enabled: {Machine.SecureBootEnabled}";

            //OSPType & OSPEdition
            String productName = xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "productName", RegistryValueKind.String);

            String[] temp = productName.Split(' ');

            os = temp[0] + " ";

            if (Machine.Role == Machine.HostRole.Server)
            {
                os += temp[1] + temp[2];
            }
            else
            {
                //win 11
                if (Machine.OSMajorVersion >= 22000)
                {
                    os += "11";
                }
                else
                {
                    os += temp[1];
                }
            }

            os += "®️";

            for (Int16 i = 2; i < temp.Length; ++i)
            {
                edition += temp[i];
            }

            //BaU

            displayVersion = $"Version: {xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "displayVersion", RegistryValueKind.String)}";

            //set
            OSPType.Text = os;
            OSPEdition.Text = edition;

            WinVersion.Text = displayVersion;
            BaU.Text = versionNumber;

            SysType.Text = UEFIIsOn;
            SecBoot.Text = secureBootIsOn;
        }

        
        




























       













        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new Exception("message");
        }









        private void SysUptimeClock()
        {
            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);

            Int32 CurrentMin = uptime.Minutes;

            Exec(uptime);

            while (true)
            {
                uptime = TimeSpan.FromMilliseconds(Environment.TickCount);

                if (uptime.Minutes == CurrentMin)
                {
                    Task.Delay(384).Wait();

                    continue;
                }

                CurrentMin = uptime.Minutes;

                Exec(uptime);

                Task.Delay(59500).Wait();
            }

            static String TS(TimeSpan time)
            {
                if (time.Days != 0)
                {
                    return $"Uptime: {time.Days}d.{time.Hours}h:{time.Minutes}mm";
                }
                else
                {
                    return $"Uptime: {time.Hours}h:{time.Minutes}m";
                }
            }

            void Exec(TimeSpan time)
            {
                Dispatcher.Invoke(new Action(() => UptimeDisplay.Text = TS(time)));
            }
        }













        
    }
}