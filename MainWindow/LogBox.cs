using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace WinUtil
{
    internal class LogBox
    {
        internal static void Add(String text = null, SolidColorBrush foreground = null, SolidColorBrush background = null, Boolean stayInLine = false, Boolean scrollToEnd = true, FontWeight fontWeight = default)
        {
            Application.Dispatcher.Invoke(new Action(() => MainWindow.LogBoxAdd(text, foreground, background, stayInLine, scrollToEnd, fontWeight)));
        }

        internal static void Remove(UInt32 amount = 1)
        {
            Application.Dispatcher.Invoke(new Action(() => MainWindow.LogBoxRemoveLine(amount)));
        }
    }

    public partial class MainWindow
    {
        internal static void LogBoxAdd(String text = null, SolidColorBrush foreground = null, SolidColorBrush background = null, Boolean stayInLine = false, Boolean scrollToEnd = true, FontWeight fontWeight = default)
        {
            foreground ??= Brushes.LightGray;

            TextRange TxR;

            if (stayInLine)
            {
                TxR = new(Application.Object.Log_RichTextBox.Document.ContentEnd, Application.Object.Log_RichTextBox.Document.ContentEnd)
                {
                    Text = text
                };
            }
            else
            {
                TxR = new(Application.Object.Log_RichTextBox.Document.ContentEnd, Application.Object.Log_RichTextBox.Document.ContentEnd)
                {
                    Text = "\n" + text
                };
            }

            TxR.ApplyPropertyValue(TextElement.ForegroundProperty, foreground);

            if (fontWeight != default)
            {
                TxR.ApplyPropertyValue(TextElement.FontWeightProperty, fontWeight);
            }

            if (background != null)
            {
                TxR.ApplyPropertyValue(TextElement.BackgroundProperty, background);
            }

            if (scrollToEnd == true)
            {
                Application.Object.Log_ScrollViewer.ScrollToEnd();
            }
        }

        internal static void LogBoxRemoveLine(UInt32 amount = 1)
        {
            for (UInt32 I = 0; I < amount; I++)
            {
                Application.Object.Log_RichTextBox.Document.Blocks.Remove(Application.Object.Log_RichTextBox.Document.Blocks.LastBlock);
            }
        }
    }
}