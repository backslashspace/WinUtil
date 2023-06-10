using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using static WinUtil_Main.MainWindow;

namespace WinUtil_Main
{
    public partial class SMBhardenMessage
    {
        public SMBhardenMessage()
        {
            InitializeComponent();

            if (!MainWindow.IsWin11Server22Complient)
            {
                Window.Height = 210;

                ComLable.Visibility = System.Windows.Visibility.Visible;

                AES256GCM.IsEnabled = false;
                AES256GCM.Content = "Only allow AES-256-GCM\n";
                AES256GCM.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#707070");
            }
        }

        //#############################################################

        public static Boolean OnlySMB3 { get; set; }
        public static Boolean Encrypt { get; set; }
        public static Boolean RejectUnencrypted { get; set; }
        public static Boolean RequireSign { get; set; }
        public static Boolean Autoshares { get; set; }
        public static Boolean OnlyAES256GCM { get; set; }

        //#############################################################

        private void OnlySMB3True(Object sender, RoutedEventArgs e)
        {
            OnlySMB3 = true;
        }

        private void OnlySMB3False(Object sender, RoutedEventArgs e)
        {
            OnlySMB3 = false;
        }

        //
        
        private void EncryptTrue(Object sender, RoutedEventArgs e)
        {
            Encrypt = true;
        }

        private void EncryptFalse(Object sender, RoutedEventArgs e)
        {
            Encrypt = false;
        }

        //

        private void RejectUnencryptedTrue(Object sender, RoutedEventArgs e)
        {
            RejectUnencrypted = true;
        }

        private void RejectUnencryptedFalse(Object sender, RoutedEventArgs e)
        {
            RejectUnencrypted = false;
        }

        //

        private void RequireSignTrue(Object sender, RoutedEventArgs e)
        {
            RequireSign = true;
        }

        private void RequireSignFalse(Object sender, RoutedEventArgs e)
        {
            RequireSign = false;
        }

        //

        private void AutosharesTrue(Object sender, RoutedEventArgs e)
        {
            Autoshares = true;
        }

        private void AutosharesFalse(Object sender, RoutedEventArgs e)
        {
            Autoshares = false;
        }

        //

        private void OnlyAES256GCMTrue(Object sender, RoutedEventArgs e)
        {
            OnlyAES256GCM = true;
        }

        private void OnlyAES256GCMFalse(Object sender, RoutedEventArgs e)
        {
            OnlyAES256GCM = false;
        }

        //#############################################################

        private void Harden(Object sender, RoutedEventArgs e)
        {
            if (Autoshares && IsServer)
            {   
                DialogResult MSGOUT0 = System.Windows.Forms.MessageBox.Show(
                    "You are about to deactivate Autoshares (remove C$, ADMIN$, D$, etc..).\nNote that these must exist in order to edit shares in the Server Manager, otherwise it will throw an error.",
                    "SMB on Windows Server",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);

                if (MSGOUT0 == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
            }

            new Thread(Functions.EncryptSMB).Start();

            this.Close();
        }

        private void Cancle(Object sender, RoutedEventArgs e)
        {
            ThreadIsAlive.SMB = false;

            this.Close();
        }
    }
}
