using Microsoft.Win32;
using ProgramLauncher;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinUtil
{
    public partial class MainWindow
    {
        private String GetButtonTag(object sender)
        {
            try
            {
                return ((Button)sender).Tag.ToString();
            }
            catch
            {
                return null;
            }
        }

        //#######################################################################################################

        private static Boolean TTT = false;

        private void Debug(object sender, RoutedEventArgs e)
        {

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

            return;








            Dialogue d = new
                ("WinUtil: Debug",
                "Execute offensive debug?",
                Dialogue.Icons.Shield_Exclamation_Mark,
                "Cancle",
                "Act",
                0);

            d.ShowDialog();

            if (d.Result != 1)
            {
                DispatchedLogBoxAdd("[i] Cancled debug", Brushes.DarkGoldenrod);

                return;
            }

            //











            if (Directory.Exists("C:\\Program Files\\Notepad++"))
            {
                Dialogue Found = new
                    ("Notepad++",
                    "Notepad++ installation was found at\nC:\\Program Files\\Notepad++\n\nContinue to install / overwrite config?",
                    Dialogue.Icons.Circle_Question,
                    "Continue",
                    "Cancle",
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

                DispatchedLogBoxAdd("Installing Notepad++ with custom settings", Brushes.DarkCyan);
            }
            else
            {
                DispatchedLogBoxAdd("Installing Notepad++", Brushes.DarkCyan);
            }

            //

            Execute.EXE("assets\\Program\\Notepad++\\npp.8.5.8.Installer.x64.exe", "/S", WaitForExit: true);
            DispatchedLogBoxAdd("Installed Notepad++", Brushes.DarkGray);

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

                        DispatchedLogBoxAdd("Copied 'Cascadia Mono' to C:\\Windows\\Fonts\\", Brushes.DarkGray);
                    }

                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", "Cascadia Mono Regular (TrueType)", "CascadiaMono.ttf", RegistryValueKind.String);

                    DispatchedLogBoxAdd("Registered font", Brushes.DarkGray);
                }

                Execute.EXE("assets\\7z.exe", $"x \"assets\\Program\\Notepad++\\NPP(NoLogVS-Config).zip\" -o\"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\Notepad++\" -y", HiddenExecute: true, WaitForExit: true);

                if (Directory.Exists("C:\\Program Files\\Notepad++"))
                {
                    Directory.Delete("C:\\Program Files\\Notepad++", true);
                }

                Execute.EXE("assets\\7z.exe", $"x \"assets\\Program\\Notepad++\\exe.zip\" -o\"C:\\Program Files\\Notepad++\" -y", HiddenExecute: true, WaitForExit: true);
                DispatchedLogBoxAdd("Copied program files", Brushes.DarkGray);

                Registry.SetValue(@"HKEY_CLASSES_ROOT\Applications\notepad++.exe\DefaultIcon", "", "C:\\Windows\\System32\\imageres.dll,97", RegistryValueKind.String);
                DispatchedLogBoxAdd("Set default file icon for notepad++.exe", Brushes.DarkGray);
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