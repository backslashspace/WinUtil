using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
//
using EXT.Launcher.Process;

namespace WinUtil
{
    internal static class Debug
    {
        [DllImport("Kernel32")]
        internal static extern void AllocConsole();

        //

        [Conditional("DEBUG")]
        internal static void Write(object Input)
        {
            Console.Write(Input);
        }

        [Conditional("DEBUG")]
        internal static void WriteLine(object Input)
        {
            Console.WriteLine(Input);
        }
    }

    public partial class MainWindow
    {
#if DEBUG
        private void SoftDebug()
        {
            Debug.AllocConsole();

            Debug_Button.IsEnabled = true;
            Debug_Button.Visibility = Visibility.Visible;
        }
#endif

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

        private static Boolean TTT = false;

        private void Button_Debug(object sender, RoutedEventArgs e)
        {
            //Debug.Write("test\n");

            //(_, Int32 ExitCode) = xProcess.Run(Resource_Assets.Host_INIT_PathName, "e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9", WaitForExit: true);
            Debug.WriteLine("debug");
            
            return;

            #region WARN
            Dialogue DBG = new
                ("WinUtil: Debug",
                "Execute offensive debug?",
                Dialogue.Icons.Shield_Exclamation_Mark,
                "Cancel",
                "Act",
                0);

            DBG.ShowDialog();

            if (DBG.Result != 1)
            {
                LogBox.Add("[i] Canceld debug", Brushes.Orange);

                return;
            }
            #endregion

            

            






















































            return;

            if (Directory.Exists("C:\\Program Files\\Notepad++"))
            {
                Dialogue Found = new
                    ("Notepad++",
                    "Notepad++ installation was found at\nC:\\Program Files\\Notepad++\n\nContinue to install / overwrite config?",
                    Dialogue.Icons.Circle_Question,
                    "Continue",
                    "Cancel",
                    0);

                Found.ShowDialog();

                if (Found.Result is not 0)
                {
                    return;
                }
            }

            Boolean CustomInstall = false;

            Dialogue InstallMethode = new
                ("Notepad++",
                "Use custom settings, this will apply the following settings:\n\n" +
                " - minimal install\n" +
                " - all languages included\n" +
                " - no logging / cache\n" +
                " - Visual Studio theme (font, background, foreground & text selection color)\n" +
                " - default icon for .txt files",
                Dialogue.Icons.Gear,
                "Custom",
                "Default");

            InstallMethode.ShowDialog();

            if (InstallMethode.Result is 0)
            {
                CustomInstall = true;

                LogBox.Add("Installing Notepad++ with custom settings", Brushes.DarkCyan);
            }
            else
            {
                LogBox.Add("Installing Notepad++", Brushes.DarkCyan);
            }

            //

            xProcess.Run("assets\\Program\\Notepad++\\npp.8.5.8.Installer.x64.exe", "/S", WaitForExit: true);
            LogBox.Add("Installed Notepad++", Brushes.DarkGray);

            if (CustomInstall)
            {
                if (Directory.Exists($"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\Notepad++"))
                {
                    Directory.Delete($"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\Notepad++", true);
                }

                if (Machine.WindowPlatformFeatureCompliance is Machine.OSPlatformFeatureCompliance.Windows10_or_Older)
                {
                    if (!File.Exists("C:\\Windows\\Fonts\\CascadiaMono.ttf"))
                    {
                        File.Copy("assets\\Program\\Notepad++\\CascadiaMono.ttf", "C:\\Windows\\Fonts\\CascadiaMono.ttf", false);

                        LogBox.Add("Copied 'Cascadia Mono' to C:\\Windows\\Fonts\\", Brushes.DarkGray);
                    }

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", "Cascadia Mono Regular (TrueType)", "CascadiaMono.ttf", RegistryValueKind.String);

                    LogBox.Add("Registered font", Brushes.DarkGray);
                }

                xProcess.Run("assets\\7z.exe", $"x \"assets\\Program\\Notepad++\\NPP(NoLogVS-Config).zip\" -o\"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\Notepad++\" -y", HiddenExecute: true, WaitForExit: true);

                if (Directory.Exists("C:\\Program Files\\Notepad++"))
                {
                    Directory.Delete("C:\\Program Files\\Notepad++", true);
                }

                xProcess.Run("assets\\7z.exe", $"x \"assets\\Program\\Notepad++\\exe.zip\" -o\"C:\\Program Files\\Notepad++\" -y", HiddenExecute: true, WaitForExit: true);
                LogBox.Add("Copied program files", Brushes.DarkGray);

                Registry.SetValue(@"HKEY_CLASSES_ROOT\Applications\notepad++.exe\DefaultIcon", "", "C:\\Windows\\System32\\imageres.dll,97", RegistryValueKind.String);
                LogBox.Add("Set default file icon for notepad++.exe", Brushes.DarkGray);
            }













            return;



            if (!TTT)
            {
                ActivateWorker();

                TTT = true;
            }
            else
            {
                DeactivateWorker();

                TTT = false;
            }

            LogBoxAdd($"{ActualWidth}");
            LogBoxAdd($"{Navigation_Column.Width}");

            //OSPType.Text = "Windows Server®️";
            //OSPEdition.Text = "Pro for Workstations";
            //WinVersion.Text = "Version: 22H2";
            //BaU.Text = "[22621.1928]";
            //SysType.Text = "UEFI enabled: true";
            //SecBoot.Text = "SecureBoot enabled: true";
            //LicenseStatus.Text = "Activated [1]";








        }
    }
}