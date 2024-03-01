using System;
using System.Windows.Media;
using System.Windows;
using System.Threading.Tasks;

namespace WinUtil
{ 
    public partial class MainWindow
    {
        private static Int16 Activity_Worker_Instances = 0;

        internal static void ActivateWorker()
        {
            if (Activity_Worker_Instances < 1)
            {
                Application.Object.WorkIndicator.Visibility = Visibility.Visible;

                ++Activity_Worker_Instances;

                ActivityWorker();
            }
            else
            {
                ++Activity_Worker_Instances;
            }
        }

        internal static void DeactivateWorker()
        {
            if (Activity_Worker_Instances > 0)
            {
                --Activity_Worker_Instances;
            }
        }

        //# # # # # # # # # # # # # # # # # #

        private static UInt16 WorkerRotation = 0;

        private static async void ActivityWorker()
        {
            Application.Object.MainWindowIcon.Visibility = Visibility.Collapsed;

            await Task.Run(() =>
            {
                while (Activity_Worker_Instances > 0)
                {
                    Application.Dispatcher.Invoke(new Action(() =>
                    {
                        Application.Object.WorkIndicator.RenderTransform = new RotateTransform(WorkerRotation += 5);
                    }));

                    Task.Delay(1).Wait();

                    if (WorkerRotation == 360)
                    {
                        WorkerRotation = 0;
                    }
                }
            });

            Application.Object.WorkIndicator.Visibility = Visibility.Collapsed;

            Application.Object.MainWindowIcon.Visibility = Visibility.Visible;
        }
    }
}