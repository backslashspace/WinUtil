using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace SiSyPHuS_Win10_2022_Host_INIT
{
    public partial class MainWindow
    {
        private struct WindowButtonColors
        {
            public static String Close_Button_Color_Idle = "#202020";
            public static String Close_Button_Color_Mouse_Is_Over = "#c42b1c";
            public static String Close_Button_Color_Down = "#b22a1b";
            public static String Close_Button_Stroke_Color_Enabled = "#ffffff";
            public static String Close_Button_Stroke_Color_Disabled = "#777777";
        }

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

        //##########################################################################

        private static Boolean Finished = false;

        private async void Add_Button(object sender, RoutedEventArgs e)
        {
            Continue.IsEnabled = false;
            Continue.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#77aa77"));

            Exited = false;

#pragma warning disable CS4014 
            Task.Run(() => Pr());
#pragma warning restore CS4014

            await Task.Run(() => Hostfile.Write_File());

            Finished = true;

            while (!Exited) await Task.Delay(100);

            Headline.Text = "Appended domains to hostfile";
            Headline.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ddffdd"));

            List.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));
            List_2.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));

            AddNum.Text = "Done";

            Environment.ExitCode = 0;
        }

        //

        private void Pr()
        {
            void Set(String In)
            {
                Dispatcher.Invoke(new Action(() => AddNum.Text = In));
            }

            while (!Finished)
            {
                Set("Writing to Hostfile");
                Task.Delay(300).Wait();
                Set("Writing to Hostfile.");
                Task.Delay(300).Wait();
                Set("Writing to Hostfile..");
                Task.Delay(300).Wait();
                Set("Writing to Hostfile...");
                Task.Delay(300).Wait();
            }

            Exited = true;
        }
    }
}