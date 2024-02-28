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
                WorkIndicator_Static.Visibility = Visibility.Visible;

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
            MainWindowIcon_Static.Visibility = Visibility.Collapsed;

            await Task.Run(() =>
            {
                while (Activity_Worker_Instances > 0)
                {
                    Dispatcher_Static.Invoke(new Action(() =>
                    {
                        WorkIndicator_Static.RenderTransform = new RotateTransform(WorkerRotation += 5);
                    }));

                    Task.Delay(1).Wait();

                    if (WorkerRotation == 360)
                    {
                        WorkerRotation = 0;
                    }
                }
            });

            WorkIndicator_Static.Visibility = Visibility.Collapsed;

            MainWindowIcon_Static.Visibility = Visibility.Visible;
        }
    }
}