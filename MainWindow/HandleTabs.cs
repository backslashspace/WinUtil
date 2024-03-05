using System.Windows;

namespace WinUtil
{
    public partial class MainWindow
    {
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
            Navigation_Areas oldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Overview;

            HandleNavigationButtons(CurrentArea, oldArea);
        }

        private void AppearanceButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas oldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Appearance;

            HandleNavigationButtons(CurrentArea, oldArea);
        }

        private void BehaviorButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas oldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Behavior;

            HandleNavigationButtons(CurrentArea, oldArea);
        }

        private void PrivacyButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas oldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Privacy;

            HandleNavigationButtons(CurrentArea, oldArea);
        }

        private void SecurityButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas oldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Security;

            HandleNavigationButtons(CurrentArea, oldArea);
        }

        private void ProgramsButton(object sender, RoutedEventArgs e)
        {
            Navigation_Areas oldArea = CurrentArea;

            CurrentArea = Navigation_Areas.Programs;

            HandleNavigationButtons(CurrentArea, oldArea);
        }

        //

        private void HandleNavigationButtons(Navigation_Areas newArea, Navigation_Areas oldArea)
        {
            if (newArea == oldArea)
            {
                return;
            }

            switch (newArea)
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

            switch (oldArea)
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
    }
}