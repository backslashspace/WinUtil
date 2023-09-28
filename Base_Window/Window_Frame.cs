using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Forms;

namespace WinUtil
{
    public partial class MainWindow
    {
        private static class WindowButtonColors
        {
            public static String Minimize_Button_Color_Idle = "#202020";
            public static String Minimize_Button_Color_Mouse_Is_Over = "#2d2d2d";
            public static String Minimize_Button_Color_Down = "#2a2a2a";
            public static String Minimize_Button_Stroke_Color_Enabled = "#ffffff";
            public static String Minimize_Button_Stroke_Color_Disabled = "#777777";

            public static String State_Button_Color_Idle = "#202020";
            public static String State_Button_Color_Mouse_Is_Over = "#2d2d2d";
            public static String State_Button_Color_Down = "#2a2a2a";
            public static String State_Button_Stroke_Color_Enabled = "#ffffff";
            public static String State_Button_Stroke_Color_Disabled = "#777777";

            public static String Close_Button_Color_Idle = "#202020";
            public static String Close_Button_Color_Mouse_Is_Over = "#c42b1c";
            public static String Close_Button_Color_Down = "#b22a1b";
            public static String Close_Button_Stroke_Color_Enabled = "#ffffff";
            public static String Close_Button_Stroke_Color_Disabled = "#777777";
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        #region Window Head Button Logic

        #region Minimize_Window

        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Minimize_Button_Mouse_Is_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Minimize_Button_Color(WindowButtonColors.Minimize_Button_Color_Mouse_Is_Over);
        }

        private void Minimize_Button_Mouse_Is_Not_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Minimize_Button_Color(WindowButtonColors.Minimize_Button_Color_Idle);
        }

        private void Minimize_Button_Down(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Minimize_Button_Color(WindowButtonColors.Minimize_Button_Color_Down);
        }

        //

        private void Update_Minimize_Button_Color(String NewHexColor)
        {
            Minimize_Button_Button_Background.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(NewHexColor));
        }

        #endregion

        #region Window_Toggle_State

        //trigger via button
        private void Button_Toggle_WindowState(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }

            Refresh_Window_Base();
        }

        //trigger via Window frame
        private void Frame_Changed_State(object sender, EventArgs e)
        {
            Refresh_Window_Base();
        }

        private void Refresh_Window_Base()
        {
            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness(8, 8, 8, 8);

                Max_Window_State_Button_Icon_Path.Visibility = Visibility.Visible;
                Normal_Window_State_Button_Icon_Path.Visibility = Visibility.Collapsed;

                Minimize_Button.Margin = new Thickness(0, -31, 93, 0);
                Minimize_Button.Height = 31;

                Close_Button.Margin = new Thickness(0, -31, 0, 0);
                Close_Button.Height = 31;
                Close_Button.Width = 47;
            }
            else
            {
                BorderThickness = new Thickness(0, 0, 0, 0);

                Normal_Window_State_Button_Icon_Path.Visibility = Visibility.Visible;
                Max_Window_State_Button_Icon_Path.Visibility = Visibility.Collapsed;

                Minimize_Button.Margin = new Thickness(0, -29, 93, 0);
                Minimize_Button.Height = 29;

                Close_Button.Margin = new Thickness(0, -29, 2, 0);
                Close_Button.Height = 29;
                Close_Button.Width = 45;
            }
        }

        //

        private void Window_State_Button_Toggle_Mouse_Is_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Window_State_Button_Color(WindowButtonColors.State_Button_Color_Mouse_Is_Over);
        }

        private void Window_State_Button_Toggle_Mouse_Is_Not_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Window_State_Button_Color(WindowButtonColors.State_Button_Color_Idle);
        }

        private void Window_State_Button_Toggle_Down(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Window_State_Button_Color(WindowButtonColors.State_Button_Color_Down);
        }

        //

        private void Update_Window_State_Button_Color(String NewHexColor)
        {
            Window_State_Button_Background.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(NewHexColor));
        }

        #endregion

        #region Close_Button

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            ConfirmClose();
        }

        private void Close_Button_Mouse_Is_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Close_Button_Color(WindowButtonColors.Close_Button_Color_Mouse_Is_Over);
        }

        private void Close_Button_Mouse_Is_Not_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Close_Button_Color(WindowButtonColors.Close_Button_Color_Idle);
        }

        private void Close_Button_Down(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Close_Button_Color(WindowButtonColors.Close_Button_Color_Down);
        }

        //

        private void Update_Close_Button_Color(String NewHexColor)
        {
            Close_Button_Background.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(NewHexColor));
        }

        //

        private void ConfirmClose()
        {
            if (ActivityWorkerKiu > 0)
            {
                DialogResult R = System.Windows.Forms.MessageBox.Show(
                    "sdply profid dfo user?",
                    "Cose",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (R != System.Windows.Forms.DialogResult.No)
                {
                    return;
                }
            }

            Environment.Exit(0);
        }

        #endregion

        #endregion
    }
}