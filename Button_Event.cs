using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using NetTools;
using SysGet;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;

namespace WinUtil_Main
{
    public partial class MainWindow
    {
        //Navigation Controll
        private void OverviewButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            if (CurrentArea is "Overview")
            {
                return;
            }

            CurrentPageAmmount = PGNOverview;

            CurrentArea = "Overview";

            Dispatcher.Invoke(new Action(() => ManageNavigationButtons(CurrentArea, OldArea)));
        }

        private void AppearanceButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            if (CurrentArea is "Appearance")
            {
                return;
            }

            CurrentPageAmmount = PGNAppearance;

            CurrentArea = "Appearance";

            Dispatcher.Invoke(new Action(() => ManageNavigationButtons(CurrentArea, OldArea)));
        }

        private void BehaviorButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            if (CurrentArea is "Behavior")
            {
                return;
            }

            CurrentPageAmmount = PGNBehavior;

            CurrentArea = "Behavior";

            Dispatcher.Invoke(new Action(() => ManageNavigationButtons(CurrentArea, OldArea)));
        }

        private void PrivacyButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            if (CurrentArea is "Privacy")
            {
                return;
            }

            CurrentPageAmmount = PGNPrivacy;

            CurrentArea = "Privacy";

            Dispatcher.Invoke(new Action(() => ManageNavigationButtons(CurrentArea, OldArea)));
        }

        private void SecurityButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            if (CurrentArea is "Security")
            {
                return;
            }

            CurrentPageAmmount = PGNPrivacy;

            CurrentArea = "Security";

            Dispatcher.Invoke(new Action(() => ManageNavigationButtons(CurrentArea, OldArea)));
        }

        private void ProgramsButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            if (CurrentArea is "Programs")
            {
                return;
            }

            CurrentPageAmmount = PGNPrograms;

            CurrentArea = "Programs";

            Dispatcher.Invoke(new Action(() => ManageNavigationButtons(CurrentArea, OldArea)));
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        //Page Controll
        private void PageMinus(object sender, RoutedEventArgs e)
        {
            if (PageNumber == 0) 
            {
                return;
            }

            SByte OldPageNumber = PageNumber;

            PageNumber--;

            ChangePageVisibility(PageNumber, OldPageNumber);

            Rescale();
        }

        private void PagePlus(object sender, RoutedEventArgs e)
        {
            if (CurrentPageAmmount == PageNumber)
            {
                return;
            }

            SByte OldPageNumber = PageNumber;

            PageNumber++;

            ChangePageVisibility(PageNumber, OldPageNumber);

            Rescale();
        }

        private String GetButtonTag(object sender)
        {
            try
            {
                return ((Button)sender).Tag.ToString();
            }
            catch
            {
                return null;
            }
        }

        //#######################################################################################################

        //work buttons

















    }
}
