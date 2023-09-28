using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WinUtil.MainWindow;

namespace WinUtil.Base_Window
{
    public partial class AppearanceGrid : UserControl
    {
        public AppearanceGrid()
        {
            InitializeComponent();
        }

        private static SByte CurrentPageAmmount = 3;

        private void PageMinus(object sender, RoutedEventArgs e)
        {
            if (PageNumber == 0)
            {
                return;
            }

            SByte OldPageNumber = PageNumber;

            PageNumber--;

            ChangePageVisibility(PageNumber, OldPageNumber);

           //Dispatcher.BeginInvoke(new Action(() => MainWindow.Rescale()), System.Windows.Threading.DispatcherPriority.DataBind, null);

            
        }

        private void PagePlus(object sender, RoutedEventArgs e)
        {
            if (PageNumber == CurrentPageAmmount - 1)
            {
                return;
            }

            SByte OldPageNumber = PageNumber;

            PageNumber++;

            ChangePageVisibility(PageNumber, OldPageNumber);

            //Rescale();
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static SByte PageNumber = 0;

        protected void ChangePageVisibility(SByte ActivatePageNumber, SByte DeactivatePageNumber)
        {






            //show new page
            switch (ActivatePageNumber)
            {
                case 0:
                    AppearanceGrid0.Visibility = Visibility.Visible;
                    break;

                case 1:
                    AppearanceGrid1.Visibility = Visibility.Visible;
                    break;

                case 2:
                    AppearanceGrid2.Visibility = Visibility.Visible;
                    break;


                default:
                    throw new NotImplementedException("ChangePageVisibility > AppearancePG");
            }

            switch (DeactivatePageNumber)
            {
                case 0:
                    AppearanceGrid0.Visibility = Visibility.Collapsed;
                    break;

                case 1:
                    AppearanceGrid1.Visibility = Visibility.Collapsed;
                    break;

                case 2:
                    AppearanceGrid2.Visibility = Visibility.Collapsed;
                    break;

                default:
                    throw new NotImplementedException("ChangePageVisibility > AppearancePG");
            }

        }


        private void Area_Size_Changed(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (CurrentArea != "Appearance") return;

            //Double WAWidth = (Double)Convert.ToInt32(ActualWidth / 100 * 25);
            //Double WAHeight = ActualHeight - 95;

            //AppearanceBorder.Width = WAWidth;
            //AppearanceBorder.Height = WAHeight;

            //if (ActualHeight > 450)
            //{
            //    //SystemLable.FontSize = 36;
            //}
            //else
            //{
            //    //SystemLable.FontSize = 24;
            //}

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