using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Threading.Tasks;

namespace WinUtil
{ 
    public partial class MainWindow
    {
        #region UI Log
        internal static void LogBoxAdd(String Text = null, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Foreground ??= Brushes.LightGray;

            TextRange TxR;

            if (StayInLine)
            {
                TxR = new(Log_Static.Document.ContentEnd, Log_Static.Document.ContentEnd)
                {
                    Text = Text
                };
            }
            else
            {
                TxR = new(Log_Static.Document.ContentEnd, Log_Static.Document.ContentEnd)
                {
                    Text = "\n" + Text
                };
            }

            TxR.ApplyPropertyValue(TextElement.ForegroundProperty, Foreground);

            if (FontWeight != default)
            {
                TxR.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeight);
            }

            if (Background != null)
            {
                TxR.ApplyPropertyValue(TextElement.BackgroundProperty, Background);
            }

            if (ScrollToEnd == true)
            {
                LogScrollViewer_Static.ScrollToEnd();
            }
        }

        internal static void DispatchedLogBoxAdd(String Text = null, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Dispatcher_Static.Invoke(new Action(() => LogBoxAdd(Text, Foreground, Background, StayInLine, ScrollToEnd, FontWeight)));
        }

        //

        private static void LogBoxRemoveLine(UInt32 Lines = 1)
        {
            for (UInt32 I = 0; I < Lines; I++)
            {
                Log_Static.Document.Blocks.Remove(Log_Static.Document.Blocks.LastBlock);
            }
        }

        internal static void DispatchedLogBoxRemoveLine(UInt32 Lines = 1)
        {
            Dispatcher_Static.Invoke(new Action(() => LogBoxRemoveLine(Lines)));
        }
        #endregion

        //#######################################################################################################

        #region Work Indicator
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
        #endregion
    }
}