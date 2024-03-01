using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace WinUtil
{
    internal class LogBox
    {
        internal static void Add(String Text = null, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Application.Dispatcher.Invoke(new Action(() => MainWindow.LogBoxAdd(Text, Foreground, Background, StayInLine, ScrollToEnd, FontWeight)));
        }

        internal static void DispatchedLogBoxRemoveLine(UInt32 Lines = 1)
        {
            Application.Dispatcher.Invoke(new Action(() => MainWindow.LogBoxRemoveLine(Lines)));
        }
    }

    public partial class MainWindow
    {
        internal static void LogBoxAdd(String Text = null, SolidColorBrush Foreground = null, SolidColorBrush Background = null, Boolean StayInLine = false, Boolean ScrollToEnd = true, FontWeight FontWeight = default)
        {
            Foreground ??= Brushes.LightGray;

            TextRange TxR;

            if (StayInLine)
            {
                TxR = new(Application.Object.Log_RichTextBox.Document.ContentEnd, Application.Object.Log_RichTextBox.Document.ContentEnd)
                {
                    Text = Text
                };
            }
            else
            {
                TxR = new(Application.Object.Log_RichTextBox.Document.ContentEnd, Application.Object.Log_RichTextBox.Document.ContentEnd)
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
                Application.Object.Log_ScrollViewer.ScrollToEnd();
            }
        }

        internal static void LogBoxRemoveLine(UInt32 Lines = 1)
        {
            for (UInt32 I = 0; I < Lines; I++)
            {
                Application.Object.Log_RichTextBox.Document.Blocks.Remove(Application.Object.Log_RichTextBox.Document.Blocks.LastBlock);
            }
        }
    }
}