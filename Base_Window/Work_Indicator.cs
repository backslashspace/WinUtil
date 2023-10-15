using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;

namespace WinUtil
{
    public partial class MainWindow
    {
        private static Int16 Activity_Worker_Instances = 0;

        internal void ActivateWorker()
        {
            if (Activity_Worker_Instances < 1)
            {
                WorkIndicator.Visibility = Visibility.Visible;

                ++Activity_Worker_Instances;

                ActivityWorker();
            }
            else
            {
                ++Activity_Worker_Instances;
            }
        }

        internal void DeactivateWorker()
        {
            if (Activity_Worker_Instances > 0)
            {
                --Activity_Worker_Instances;
            }
        }

        //# # # # # # # # # # # # # # # # # #

        private static Int16 WorkerRotation = 0;

        private async void ActivityWorker()
        {
            MainWindowIcon.Visibility = Visibility.Collapsed;

            await Task.Run(() =>
            {
                while (Activity_Worker_Instances > 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        WorkIndicator.RenderTransform = new RotateTransform(WorkerRotation += 5);
                    }));

                    Task.Delay(1).Wait();

                    if (WorkerRotation == 360)
                    {
                        WorkerRotation = 0;
                    }
                }
            });

            WorkIndicator.Visibility = Visibility.Collapsed;

            MainWindowIcon.Visibility = Visibility.Visible;
        }
    }
}