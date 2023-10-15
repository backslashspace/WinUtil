using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace WinUtil
{ 
    public partial class MainWindow
    {










        private void OverviewButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            CurrentArea = "Overview";

            if (OldArea is "Overview")
            {
                return;
            }

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void AppearanceButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            CurrentArea = "Appearance";

            if (OldArea is "Appearance")
            {
                return;
            }

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void BehaviorButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            CurrentArea = "Behavior";

            if (OldArea is "Behavior")
            {
                return;
            }

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void PrivacyButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            CurrentArea = "Privacy";

            if (OldArea is "Privacy")
            {
                return;
            }

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void SecurityButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            CurrentArea = "Security";

            if (OldArea is "Security")
            {
                return;
            }

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void ProgramsButton(object sender, RoutedEventArgs e)
        {
            String OldArea = CurrentArea;

            CurrentArea = "Programs";

            if (OldArea is "Programs")
            {
                return;
            }

            ManageNavigationButtons(CurrentArea, OldArea);
        }



















        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        //Page Controll
        



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









        private void ManageNavigationButtons(String NewTab, String OldTab)
        {
            //activate new
            switch (NewTab)
            {
                case "Overview":
                    Overview.IsChecked = true;
                    OverviewGrid.Visibility = Visibility.Visible;
                    break;

                case "Appearance":
                    Appearance.IsChecked = true;
                    AppearanceGrid.Visibility = Visibility.Visible;
                    break;

                case "Behavior":
                    Behavior.IsChecked = true;
                    BehaviorGrid.Visibility = Visibility.Visible;
                    break;

                case "Privacy":
                    Privacy.IsChecked = true;
                    PrivacyGrid.Visibility = Visibility.Visible;
                    break;

                case "Security":
                    Security.IsChecked = true;
                    SecurityGrid.Visibility = Visibility.Visible;
                    break;

                case "Programs":
                    Programs.IsChecked = true;
                    ProgramsGrid.Visibility = Visibility.Visible;
                    break;
            }

            //deactivate old
            switch (OldTab)
            {
                case "Overview":
                    Overview.IsChecked = false;
                    OverviewGrid.Visibility = Visibility.Collapsed;
                    break;

                case "Appearance":
                    Appearance.IsChecked = false;
                    AppearanceGrid.Visibility = Visibility.Collapsed;
                    break;

                case "Behavior":
                    Behavior.IsChecked = false;
                    BehaviorGrid.Visibility = Visibility.Collapsed;
                    break;

                case "Privacy":
                    Privacy.IsChecked = false;
                    PrivacyGrid.Visibility = Visibility.Collapsed;
                    break;

                case "Security":
                    Security.IsChecked = false;
                    SecurityGrid.Visibility = Visibility.Collapsed;
                    break;

                case "Programs":
                    Programs.IsChecked = false;
                    ProgramsGrid.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}
