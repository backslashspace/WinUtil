using System;
using System.Windows;

namespace SiSyPHuS_Win10_2022_Host_INIT
{
    public partial class App : Application
    {
        private void Application_Startup(Object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                Init_Window(ApplicationMode.Manual, " - Direct Start");

                return;
            }

            switch (e.Args[0])
            {
                //no-gui
                case "e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9":
                    Init_Window(ApplicationMode.Auto_NoGUI, null);
                    return;

                default:
                    Init_Window(ApplicationMode.Manual, " - Direct Start | Invalid Args[0]");
                    return;
            }
        }

        private void Init_Window(ApplicationMode AppMode, String Title)
        {
            MainWindow wnd = new(AppMode);

            wnd.Window_Title.Text += Title;

            wnd.Show();
        }

        //

        public enum ApplicationMode
        {
            Manual = 0,
            Auto_NoGUI = 1,
        }
    }
}