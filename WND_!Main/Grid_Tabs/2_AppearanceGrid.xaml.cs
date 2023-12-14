using EXT.System.Registry;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace WinUtil.Grid_Tabs
{
    public partial class AppearanceGrid : UserControl
    {
        private void InitThemeButton()
        {
            Int32 V0 = xRegistry.Get.Value("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", RegistryValueKind.DWord);
            Int32 V1 = xRegistry.Get.Value("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", RegistryValueKind.DWord);

            if ((V0 == 1) || (V1 == 1))
            {
                Bright_Mode_Switch.Checked -= Set_System_Light_Mode;
                Bright_Mode_Switch.IsChecked = true;
                Bright_Mode_Switch.Checked += Set_System_Light_Mode;
            }
        }

        public AppearanceGrid()
        {
            InitializeComponent();

            MainWindow.Sys_Theme_Switch_Setter += InitThemeButton;
        }

        //#######################################################################################################

        private void Set_System_Dark_Mode(object sender, RoutedEventArgs e)
        {
            MainWindow.LogBoxAdd("Setting system to dark mode\n", Brushes.LightBlue);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, RegistryValueKind.DWord);
            MainWindow.LogBoxAdd("Done\n", Brushes.MediumSeaGreen);
        }
        private void Set_System_Light_Mode(object sender, RoutedEventArgs e)
        {
            MainWindow.LogBoxAdd("Setting system to light mode\n", Brushes.LightBlue);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1, RegistryValueKind.DWord);
            MainWindow.LogBoxAdd("Done\n", Brushes.MediumSeaGreen);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.Write(Bright_Mode_Switch.IsEnabled);
        }
    }
}