using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace SiSyPHuS_Win10_2022_Host_INIT
{
    public partial class MainWindow
    {
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