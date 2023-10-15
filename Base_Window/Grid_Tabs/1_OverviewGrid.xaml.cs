using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using static WinUtil.MainWindow;

namespace WinUtil.Base_Window
{
    public partial class OverviewGrid : UserControl
    {
        public OverviewGrid()
        {
            InitializeComponent();

            Task.Run(() => SysUptimeClock());
        }


        #region LogBoxAdd
        internal delegate void Overview_Grid_Log_Event(String Text, SolidColorBrush Foreground, SolidColorBrush Background, Boolean StayInLine, Boolean ScrollToEnd, FontWeight FontWeight);
        internal event Overview_Grid_Log_Event Commit_Log;

        private void LogBoxAdd(String Text = null, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Commit_Log?.Invoke(Text, Foreground, Background, StayInLine, ScrollToEnd, FontWeight);
        }
        #endregion

        #region LogBoxRemoveLine
        internal delegate void Overview_Grid_Log_Remove_Event(UInt32 Lines);
        internal event Overview_Grid_Log_Remove_Event Remove_Log_Line;

        private void LogBoxRemoveLine(UInt32 Lines = 1)
        {
            Remove_Log_Line?.Invoke(Lines);
        }
        #endregion













        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LogBoxAdd("sdddddddddddddddddddddddddddddddd");
        }









        internal void SysUptimeClock()
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













        private void Area_Size_Changed(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (CurrentArea != "Overview") return;

            Double WAWidth = (Double)Convert.ToInt32(ActualWidth / 100 * 25);
            Double WAHeight = ActualHeight - 95;

            WindowsArea.Width = WAWidth;
            WindowsArea.Height = WAHeight;

            if (ActualHeight > 450)
            {
                //SystemLable.FontSize = 36;
            }
            else
            {
                //SystemLable.FontSize = 24;
            }

            //Double VersionHight = WindowsArea.ActualHeight / 100 * 25;
            //WinVersion.Margin = new Thickness(5, VersionHight, 0, 0);
            //BaU.Margin = new Thickness(5, VersionHight + 14, 0, 0);

            //Double SysTypeHight = WindowsArea.ActualHeight / 100 * 50;
            //SysType.Margin = new Thickness(5, SysTypeHight, 0, 0);
            //SecBoot.Margin = new Thickness(5, SysTypeHight + 14, 0, 0);

            //Double LicenseLableHight = WindowsArea.ActualHeight / 100 * 75;
            //LicenseLable.Margin = new Thickness(5, LicenseLableHight, 0, 0);
            //LicenseStatus.Margin = new Thickness(5, LicenseLableHight + 14, 0, 0);
        }
    }
}