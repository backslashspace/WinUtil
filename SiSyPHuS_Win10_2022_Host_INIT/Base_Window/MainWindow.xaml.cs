using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SiSyPHuS_Win10_2022_Host_INIT
{
    public partial class MainWindow : Window
    {
        internal static String[] Data_To_Be_Writen = null;
        internal static String[] Current_File_Content;

        public MainWindow(App.ApplicationMode AppMode)
        {
            //this will only be used on crashes (i hope)
            Environment.ExitCode = -2;

            switch (AppMode)
            {
                case App.ApplicationMode.Manual:
                    Init_GUI();
                    return;

                case App.ApplicationMode.Auto_NoGUI:
                    Init_NoGui();
                    return;
            }
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static Boolean Wainting = true;
        private static Boolean Exited = false;

        private void Init_GUI()
        {
            InitializeComponent();

            for (Byte B = 0; B < Hostfile.TelemetryData.Length; ++B)
            {
                if (Hostfile.TelemetryData[B] == "")
                {
                    continue;
                }

                if (B > 14)
                {
                    List_2.Text += $"{Hostfile.TelemetryData[B]}\n";
                }
                else
                {
                    List.Text += $"{Hostfile.TelemetryData[B]}\n";
                }
            }

            //load hostfile content and update UI
            GUI_Load();

            Visual_Loader();
        }       

        private async void GUI_Load()
        {
            await Task.Run(() => Hostfile.Get_New_Data());

            Wainting = false;

            while (!Exited) await Task.Delay(100);

            if (Data_To_Be_Writen.Length == 0)
            {
                Continue.IsEnabled = false;
                Continue.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));

                AddNum.Text = $"Domains to be added {Data_To_Be_Writen.Length}/{Hostfile.TelemetryData.Length} | Nothing to do";
                AddNum.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#77aa77"));

                List.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));
                List_2.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));

                return;
            }

            AddNum.Text = $"Domains to be added {Data_To_Be_Writen.Length}/{Hostfile.TelemetryData.Length} | {Hostfile.TelemetryData.Length - Data_To_Be_Writen.Length} duplicates in file";

            Continue.IsEnabled = true;
            Continue.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
        }

        private async void Visual_Loader()
        {
            while (Wainting)
            {
                AddNum.Text = "Loading Hostfile information";
                await Task.Delay(300);
                AddNum.Text += ".";
                await Task.Delay(300);
                AddNum.Text += ".";
                await Task.Delay(300);
                AddNum.Text += ".";
                await Task.Delay(300);
            }

            Exited = true;
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private void Init_NoGui()
        {
            Hostfile.Get_New_Data();

            if (Data_To_Be_Writen.Length == 0)
            {
                Environment.Exit(-1);
            }

            Hostfile.Write_File();

            Environment.Exit(Data_To_Be_Writen.Length);
        }
    }
}