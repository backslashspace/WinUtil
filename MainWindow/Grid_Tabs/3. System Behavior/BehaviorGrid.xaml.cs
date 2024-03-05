using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
//
using BSS.System.Registry;

namespace WinUtil.Grid_Tabs
{
    public partial class BehaviorGrid : UserControl
    {
        private void InitBackGroundAppsToggleButton()
        {
            UInt32? tmp = xRegistry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\AppPrivacy", "LetAppsRunInBackground", RegistryValueKind.DWord);

            if (tmp == 2)
            {
                BackGroundApps_ToggleButton.Checked -= BackGroundAppsToggle;
                BackGroundApps_ToggleButton.IsChecked = true;
                BackGroundApps_ToggleButton.Checked += BackGroundAppsToggle;
            }
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        public BehaviorGrid()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitBackGroundAppsToggleButton();

            Visibility = Visibility.Collapsed;
        }
    }
}