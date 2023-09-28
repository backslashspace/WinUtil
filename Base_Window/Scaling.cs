using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Data;

namespace WinUtil
{
    public partial class MainWindow
    {
        private static Boolean[] Navigation_Button_Size_State = { true, false };

        internal void Rescale()
        {
            Double Window_Height;
            Double Window_Width;

            #region Window_State_Aware

            if (WindowState == WindowState.Maximized)
            {
                Window_Height = ActualHeight - 16;
                Window_Width = ActualWidth - 16;

                LogScroller.Margin = new Thickness(-1, -1, 0, 0);
            }
            else
            {
                Window_Height = ActualHeight;
                Window_Width = ActualWidth;

                LogScroller.Margin = new Thickness(-1, -1, 0, 4);
            }

            #endregion

            //calc layout
            Double NavWidth = 250;

            if (Window_Width < 1295)
            {
                Double Width80 = (Double)Convert.ToInt32(Window_Width / 100 * 80);
                Double Height31 = Window_Height / 100 * 31;

                NavWidth = Window_Width - Width80 - 9;

                ContentArea.Height = Window_Height - Height31 - 38;
                ContentArea.Width = Width80;

                LogBox.Height = Height31;
                LogBox.Width = Width80;
                LogTextBox.Document.PageWidth = Width80 - 15; 
            }
            else
            {
                Double RWidth = Window_Width - NavWidth - 9;
                ContentArea.Width = RWidth;
                LogBox.Width = RWidth;

                Double Height31 = Window_Height / 100 * 31;
                ContentArea.Height = Window_Height - Height31 - 38;
                LogBox.Height = Height31;
            }

            #region nav buttons

            Overview.Width = NavWidth;
            Appearance.Width = NavWidth;
            Behavior.Width = NavWidth;
            Privacy.Width = NavWidth;
            Security.Width = NavWidth;
            Programs.Width = NavWidth;

            if (ActualWidth < 858 || ActualHeight < 708)
            {
                if (!Navigation_Button_Size_State[0])
                {
                    Byte N_IconSize = 18;

                    Navigation_Button_Size_State[0] = true;
                    Navigation_Button_Size_State[1] = false;

                    Overview.FontSize = 16;
                    Appearance.FontSize = 14;
                    Behavior.FontSize = 14;
                    Privacy.FontSize = 14;
                    Security.FontSize = 14;
                    Programs.FontSize = 14;

                    Overview.Height = 60;
                    Overview.Margin = new Thickness(5, -4, 0, 0);
                    Navigation_Button_Overview_Icon.Height = N_IconSize;
                    Navigation_Button_Overview_Icon.Width = N_IconSize;

                    Appearance.Height = 44;
                    Appearance.Margin = new Thickness(5, 56, 0, 0);
                    Navigation_Button_Appearance_Icon.Height = N_IconSize;
                    Navigation_Button_Appearance_Icon.Width = N_IconSize;

                    Behavior.Height = 44;
                    Behavior.Margin = new Thickness(5, 96, 0, 0);
                    Navigation_Button_Behavior_Icon.Height = N_IconSize;
                    Navigation_Button_Behavior_Icon.Width = N_IconSize;

                    Privacy.Height = 44;
                    Privacy.Margin = new Thickness(5, 136, 0, 0);
                    Navigation_Button_Privacy_Icon.Height = N_IconSize;
                    Navigation_Button_Privacy_Icon.Width = N_IconSize;

                    Security.Height = 44;
                    Security.Margin = new Thickness(5, 176, 0, 0);
                    Navigation_Button_Security_Icon.Height = N_IconSize;
                    Navigation_Button_Security_Icon.Width = N_IconSize;

                    Programs.Height = 44;
                    Programs.Margin = new Thickness(5, 216, 0, 0);
                    Navigation_Button_Programs_Icon.Height = N_IconSize;
                    Navigation_Button_Programs_Icon.Width = N_IconSize;
                }
            }
            else
            {
                if (!Navigation_Button_Size_State[1])
                {
                    Byte L_IconSize = 22;

                    Navigation_Button_Size_State[0] = false;
                    Navigation_Button_Size_State[1] = true;

                    Overview.FontSize = 18;
                    Appearance.FontSize = 16;
                    Behavior.FontSize = 16;
                    Privacy.FontSize = 16;
                    Security.FontSize = 16;
                    Programs.FontSize = 16;

                    Overview.Height = 70;
                    Overview.Margin = new Thickness(5, -4, 0, 0);
                    Navigation_Button_Overview_Icon.Height = L_IconSize;
                    Navigation_Button_Overview_Icon.Width = L_IconSize;

                    Appearance.Height = 54;
                    Appearance.Margin = new Thickness(5, 66, 0, 0);
                    Navigation_Button_Appearance_Icon.Height = L_IconSize;
                    Navigation_Button_Appearance_Icon.Width = L_IconSize;

                    Behavior.Height = 54;
                    Behavior.Margin = new Thickness(5, 116, 0, 0);
                    Navigation_Button_Behavior_Icon.Height = L_IconSize;
                    Navigation_Button_Behavior_Icon.Width = L_IconSize;

                    Privacy.Height = 54;
                    Privacy.Margin = new Thickness(5, 166, 0, 0);
                    Navigation_Button_Privacy_Icon.Height = L_IconSize;
                    Navigation_Button_Privacy_Icon.Width = L_IconSize;

                    Security.Height = 54;
                    Security.Margin = new Thickness(5, 216, 0, 0);
                    Navigation_Button_Security_Icon.Height = L_IconSize;
                    Navigation_Button_Security_Icon.Width = L_IconSize;

                    Programs.Height = 54;
                    Programs.Margin = new Thickness(5, 266, 0, 0);
                    Navigation_Button_Programs_Icon.Height = L_IconSize;
                    Navigation_Button_Programs_Icon.Width = L_IconSize;
                }
            }
            #endregion
        }
    }
}