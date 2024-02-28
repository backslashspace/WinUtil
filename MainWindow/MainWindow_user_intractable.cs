using System.Windows;

namespace WinUtil
{
    public partial class MainWindow
    {
        #region Navigation Buttons
        internal static Navigation_Areas CurrentArea = Navigation_Areas.Overview;

        internal enum Navigation_Areas
        {
            Overview = 0,
            Appearance = 1,
            Behavior = 2,
            Privacy = 3,
            Security = 4,
            Programs = 5,
        }

        private void OverviewButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas OldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Overview;

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void AppearanceButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas OldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Appearance;

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void BehaviorButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas OldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Behavior;

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void PrivacyButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas OldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Privacy;

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void SecurityButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas OldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Security;

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        private void ProgramsButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas OldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Programs;

            ManageNavigationButtons(CurrentArea, OldArea);
        }

        //

        private void ManageNavigationButtons(Navigation_Areas NewTab, Navigation_Areas OldTab)
        {
            switch (NewTab)
            {
                case Navigation_Areas.Overview:
                    Overview.IsChecked = true;
                    OverviewGrid.Visibility = Visibility.Visible;
                    break;

                case Navigation_Areas.Appearance:
                    Appearance.IsChecked = true;
                    AppearanceGrid.Visibility = Visibility.Visible;
                    break;

                case Navigation_Areas.Behavior:
                    Behavior.IsChecked = true;
                    BehaviorGrid.Visibility = Visibility.Visible;
                    break;

                case Navigation_Areas.Privacy:
                    Privacy.IsChecked = true;
                    PrivacyGrid.Visibility = Visibility.Visible;
                    break;

                case Navigation_Areas.Security:
                    Security.IsChecked = true;
                    SecurityGrid.Visibility = Visibility.Visible;
                    break;

                case Navigation_Areas.Programs:
                    Programs.IsChecked = true;
                    ProgramsGrid.Visibility = Visibility.Visible;
                    break;
            }

            if (NewTab == OldTab)
            {
                return;
            }

            switch (OldTab)
            {
                case Navigation_Areas.Overview:
                    Overview.IsChecked = false;
                    OverviewGrid.Visibility = Visibility.Collapsed;
                    break;

                case Navigation_Areas.Appearance:
                    Appearance.IsChecked = false;
                    AppearanceGrid.Visibility = Visibility.Collapsed;
                    break;

                case Navigation_Areas.Behavior:
                    Behavior.IsChecked = false;
                    BehaviorGrid.Visibility = Visibility.Collapsed;
                    break;

                case Navigation_Areas.Privacy:
                    Privacy.IsChecked = false;
                    PrivacyGrid.Visibility = Visibility.Collapsed;
                    break;

                case Navigation_Areas.Security:
                    Security.IsChecked = false;
                    SecurityGrid.Visibility = Visibility.Collapsed;
                    break;

                case Navigation_Areas.Programs:
                    Programs.IsChecked = false;
                    ProgramsGrid.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        #endregion
    }
}