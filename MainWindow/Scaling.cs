using System;
using System.Windows;

namespace WinUtil
{
    public partial class MainWindow
    {
        private void OnRescale(object sender, SizeChangedEventArgs e)
        {
            Rescale();
        }

        //

        private Boolean[] Navigation_Button_Size_State = { true, true };
        private Boolean[] Navigation_Button_Area_Size = { true, true };

        private void Rescale()
        {
            Application.Object.Log_RichTextBox.Document.PageWidth = Application.Object.Log_RichTextBox.ActualWidth;

            Double windowHeight;
            Double windowWidth;

            if (WindowState == WindowState.Maximized)
            {
                windowHeight = ActualHeight - 16;
                windowWidth = ActualWidth - 16;
            }
            else
            {
                windowHeight = ActualHeight;
                windowWidth = ActualWidth;
            }

            //

            #region Content area size
            if (windowWidth > 1400)
            {
                if (Navigation_Button_Area_Size[1])
                {
                    Navigation_Column.Width = new GridLength(241, GridUnitType.Pixel);

                    Navigation_Button_Area_Size[0] = true;
                    Navigation_Button_Area_Size[1] = false;
                }
            }
            else if (windowWidth < 951)
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
                Navigation_Column.Width = new GridLength(166 + ((windowWidth - 950) / 6), GridUnitType.Pixel);

                Navigation_Button_Area_Size[0] = true;
                Navigation_Button_Area_Size[1] = true;
            }
            #endregion

            #region nav buttons
            if (windowWidth < 1200 || windowHeight < 708)
            {
                if (Navigation_Button_Size_State[0])
                {
                    Byte small_NavigationIconSize = 18;

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
                    Navigation_Button_Overview_Icon.Height = small_NavigationIconSize;

                    Appearance.Height = 36;
                    Appearance.Margin = new Thickness(9, 58, 0, 0);
                    Navigation_Button_Appearance_Icon.Height = small_NavigationIconSize;

                    Behavior.Height = 36;
                    Behavior.Margin = new Thickness(9, 98, 0, 0);
                    Navigation_Button_Behavior_Icon.Height = small_NavigationIconSize;

                    Privacy.Height = 36;
                    Privacy.Margin = new Thickness(9, 138, 0, 0);
                    Navigation_Button_Privacy_Icon.Height = small_NavigationIconSize;

                    Security.Height = 36;
                    Security.Margin = new Thickness(9, 178, 0, 0);
                    Navigation_Button_Security_Icon.Height = small_NavigationIconSize;

                    Programs.Height = 36;
                    Programs.Margin = new Thickness(9, 218, 0, 0);
                    Navigation_Button_Programs_Icon.Height = small_NavigationIconSize;
                }
            }
            else
            {
                if (Navigation_Button_Size_State[1])
                {
                    Byte big_NavigationIconSize = 22;

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
                    Navigation_Button_Overview_Icon.Height = big_NavigationIconSize;

                    Appearance.Height = 44;
                    Appearance.Margin = new Thickness(9, 60, 0, 0);
                    Navigation_Button_Appearance_Icon.Height = big_NavigationIconSize;

                    Behavior.Height = 44;
                    Behavior.Margin = new Thickness(9, 108, 0, 0);
                    Navigation_Button_Behavior_Icon.Height = big_NavigationIconSize;

                    Privacy.Height = 44;
                    Privacy.Margin = new Thickness(9, 156, 0, 0);
                    Navigation_Button_Privacy_Icon.Height = big_NavigationIconSize;

                    Security.Height = 44;
                    Security.Margin = new Thickness(9, 204, 0, 0);
                    Navigation_Button_Security_Icon.Height = big_NavigationIconSize;

                    Programs.Height = 44;
                    Programs.Margin = new Thickness(9, 252, 0, 0);
                    Navigation_Button_Programs_Icon.Height = big_NavigationIconSize;
                }
            }
            #endregion
        }
    }
}