using System;
using System.Windows.Media;
using System.Windows;

namespace SiSyPHuS_Win10_2022_Host_INIT
{
    public partial class MainWindow
    {
        #region Close_Button
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
        #endregion

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static class WindowButtonColors
        {
            public static String Close_Button_Color_Idle = "#202020";
            public static String Close_Button_Color_Mouse_Is_Over = "#c42b1c";
            public static String Close_Button_Color_Down = "#b22a1b";
            public static String Close_Button_Stroke_Color_Enabled = "#ffffff";
            public static String Close_Button_Stroke_Color_Disabled = "#777777";
        }
    }
}