using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//
using BSS.System.Registry;

namespace WinUtil.Grid_Tabs
{
    public partial class AppearanceGrid : UserControl
    {
        public AppearanceGrid()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Init_Theme_Switch();

            Init_Context_Button();

            Init_Terminal_Integrator_Button();

            Visibility = Visibility.Collapsed;
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private void Init_Theme_Switch()
        {
            UInt32 v0 = xRegistry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", RegistryValueKind.DWord);
            UInt32 v1 = xRegistry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", RegistryValueKind.DWord);

            if ((v0 == 1) || (v1 == 1))
            {
                OSTheme_ToggleButton.Checked -= Set_System_Theme_Toggle;
                OSTheme_ToggleButton.IsChecked = true;
                OSTheme_ToggleButton.Checked += Set_System_Theme_Toggle;
            }
        }

        private void Init_Context_Button()
        {
            if (xRegistry.TestRegValuePresense(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", ""))
            {
                ContextMenu_ToggleButton.Checked -= ContextMenu_ToggleButton_Handler;
                ContextMenu_ToggleButton.IsChecked = true;
                ContextMenu_ToggleButton.Checked += ContextMenu_ToggleButton_Handler;
            }
        }
        internal void External_Set_OS_Aware_Context_Button_State()
        {
            if (Machine.UIVersion == Machine.WindowsUIVersion.Windows10)
            {
                Context_Button.IsEnabled = false;
                Context_Button_Icon.Opacity = 0.41;
                ContextMenu_ToggleButton.Opacity = 0.41;
                Context_Button_Head.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#747474"));
                Context_Button_Body.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#747474"));
            }
        }

        private void Init_Terminal_Integrator_Button()
        {
            if (xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHere", "MUIVerb")

                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "HasLUAShield")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\Background\shell\OpenWTHereAsAdmin", "MUIVerb")

                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHere", "MUIVerb")

                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "Extended")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "HasLUAShield")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "Icon")
                && xRegistry.TestRegValuePresense(@"HKEY_CLASSES_ROOT\Directory\shell\OpenWTHereAsAdmin", "MUIVerb"))
            {
                Terminal_Integration_ToggleButton.Checked -= Terminal_Integration_ToggleButton_Handler;
                Terminal_Integration_ToggleButton.IsChecked = true;
                Terminal_Integration_ToggleButton.Checked += Terminal_Integration_ToggleButton_Handler;
            }
        }
    }
}