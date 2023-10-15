using System;
using System.Windows;

namespace WinUtil
{
    public partial class MainWindow
    {
        private static Boolean[] Navigation_Button_Size_State = { true, true };
        private static Boolean[] Navigation_Button_Area_Size = { true, true };

        internal void Rescale()
        {
            LogTextBox.Document.PageWidth = LogTextBox.ActualWidth;

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
    }
}