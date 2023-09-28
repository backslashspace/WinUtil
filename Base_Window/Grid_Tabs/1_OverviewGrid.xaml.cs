using System;
using System.Threading;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using static WinUtil.MainWindow;

namespace WinUtil.Base_Window
{
    public partial class OverviewGrid : UserControl
    {
        public OverviewGrid()
        {
            InitializeComponent();

            
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

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