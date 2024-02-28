using System;
using System.Windows;
using System.Windows.Media;

namespace WinUtil.Grid_Tabs
{
    public partial class WS_Update : Window
    {
        public WS_Update()
        {
            InitializeComponent();
        }

        internal enum Button
        {
            NoDrivers = 0,
            Security_Only = 1,
            No_Updates = 2,
            Reset = 3,
            Cancel = 4
        }

        internal Button Choice = Button.Reset;

        private void NoDriver(object sender, RoutedEventArgs e)
        {
            Choice = Button.NoDrivers;

            this.Close();
        }

        private void SecurityOnly(object sender, RoutedEventArgs e)
        {
            Dialogue dialogue = new(
                "WU: Confirm",
                "Delay feature updates 1 year &\ndelay security updates 2 days, proceed?",
                Dialogue.Icons.Shield_Exclamation_Mark,
                "Continue",
                "Cancel");

            dialogue.ShowDialog();

            if (dialogue.Result == 0)
            {
                Choice = Button.Security_Only;
            }
            else
            {
                Choice = Button.Cancel;
            }

            this.Close();
        }

        private void NoUpdates(object sender, RoutedEventArgs e)
        {
            Dialogue dialogue = new(
                "WU: Confirm",
                "Are you sure that you want to disable all windows updates?",
                Dialogue.Icons.Shield_Exclamation_Mark,
                "Continue",
                "Cancel");

            dialogue.ShowDialog();

            if (dialogue.Result == 0)
            {
                Choice = Button.No_Updates;
            }
            else
            {
                Choice = Button.Cancel;
            }

            this.Close();
        }

        private void ResetUpdateService(object sender, RoutedEventArgs e)
        {
            Choice = Button.Reset;

            this.Close();
        }

        #region Frame_Control

        #region Close_Button
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Choice = Button.Cancel;

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
        #endregion
    }
}