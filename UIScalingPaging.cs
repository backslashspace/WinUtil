using System.Threading.Tasks;
using System.Windows;
using System;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace WinUtil_Main
{
    public partial class MainWindow
    {
        private static Boolean RescaleInProgress = false;
        private static String CurrentArea = "Overview";
        private static SByte PageNumber = 0;
        private static SByte CurrentPageAmmount = 0;

        private static Boolean[] Navbutton = { true, false};

        private void SysUptimeClock()
        {
            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);

            Int32 CurrentMin = uptime.Minutes;

            Exec(uptime);

            while (true)
            {
                uptime = TimeSpan.FromMilliseconds(Environment.TickCount);

                if (uptime.Minutes == CurrentMin)
                {
                    Task.Delay(384).Wait();

                    continue;
                }

                CurrentMin = uptime.Minutes;

                Exec(uptime);

                Task.Delay(59500).Wait();
            }

            static String TS(TimeSpan time)
            {
                if (time.Days != 0)
                {
                    return $"Uptime: {time.Days}d.{time.Hours}h:{time.Minutes}mm";
                }
                else
                {
                    return $"Uptime: {time.Hours}h:{time.Minutes}m";
                }
            }

            void Exec(TimeSpan time)
            {
                Dispatcher.Invoke(new Action(() => UptimeDisplay.Text = TS(time)));
            }
        }

        private void Rescale(Boolean RenderContenBoardOnly = false)
        {
            if (RescaleInProgress && !RenderContenBoardOnly)
            {
                return;
            }
            
            if (!RenderContenBoardOnly)
            {
                RescaleInProgress = true;
            }

            Double WindowHeight;
            Double WindowWidth;

            //Manage window and get scale
            if (WindowState == WindowState.Maximized)
            {
                WindowHeight = ActualHeight - 16;
                WindowWidth = ActualWidth - 16;

                //log window
                LogScroller.Margin = new Thickness(-1, -1, 0, 0);
            }
            else
            {
                WindowHeight = ActualHeight;
                WindowWidth = ActualWidth;

                //log window
                LogScroller.Margin = new Thickness(-1, -1, 0, 4);
            }

            //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

            //calc
            Double Width80 = (Double)Convert.ToInt32(WindowWidth / 100 * 80);
            Double NavWidth = WindowWidth - Width80 - 9;
            Double Width31 = WindowHeight / 100 * 31;
            Double ContentGridHeight = WindowHeight - Width31 - 38;

            if (RenderContenBoardOnly) goto fastrender;

            //WorkIndicator location corrector
            if (WindowState == WindowState.Maximized)
            {
                WorkIndicator.Margin = new Thickness(0, 0, Width80 + 4, 4);
            }
            else
            {
                WorkIndicator.Margin = new Thickness(0, 0, Width80 + 4, 8);
            }

            LogBox.Height = Width31;
            LogBox.Width = Width80;
            LogTextBox.Document.PageWidth = Width80 - 15;

            //nav buttons
            {
                Overview.Width = NavWidth;
                Appearance.Width = NavWidth;
                Behavior.Width = NavWidth;
                Privacy.Width = NavWidth;
                Security.Width = NavWidth;
                Programs.Width = NavWidth;

                if (ActualHeight < 708)
                {
                    if (!Navbutton[0])
                    {
                        Navbutton[0] = true;
                        Navbutton[1] = false;

                        Overview.FontSize = 16;
                        Appearance.FontSize = 14;
                        Behavior.FontSize = 14;
                        Privacy.FontSize = 14;
                        Security.FontSize = 14;
                        Programs.FontSize = 14;

                        Overview.Height = 60;
                        Overview.Margin = new Thickness(5, -4, 0, 0);

                        Appearance.Height = 50;
                        Appearance.Margin = new Thickness(5, 59, 0, 0);

                        Behavior.Height = 50;
                        Behavior.Margin = new Thickness(5, 107, 0, 0);

                        Privacy.Height = 50;
                        Privacy.Margin = new Thickness(5, 155, 0, 0);

                        Security.Height = 50;
                        Security.Margin = new Thickness(5, 203, 0, 0);

                        Programs.Height = 50;
                        Programs.Margin = new Thickness(5, 251, 0, 0);
                    }
                }
                else
                {
                    if (!Navbutton[1])
                    {
                        Navbutton[0] = false;
                        Navbutton[1] = true;

                        Overview.Height = 80;
                        Overview.Margin = new Thickness(5, -4, 0, 0);

                        Appearance.Height = 70;
                        Appearance.Margin = new Thickness(5, 79, 0, 0);

                        Behavior.Height = 70;
                        Behavior.Margin = new Thickness(5, 147, 0, 0);

                        Privacy.Height = 70;
                        Privacy.Margin = new Thickness(5, 215, 0, 0);

                        Security.Height = 70;
                        Security.Margin = new Thickness(5, 283, 0, 0);

                        Programs.Height = 70;
                        Programs.Margin = new Thickness(5, 351, 0, 0);

                        Overview.FontSize = 20;
                        Appearance.FontSize = 18;
                        Behavior.FontSize = 18;
                        Privacy.FontSize = 18;
                        Security.FontSize = 18;
                        Programs.FontSize = 18;
                    }
                }
            }

        fastrender:
            //scale content board
            {
                {
                    switch (CurrentArea)
                    {
                        case "Overview":
                            OverviewGridF();
                            break;

                        case "Appearance":
                            AppearanceGridF();
                            break;

                        case "Behavior":
                            BehaviorGridF();
                            break;

                        case "Privacy":
                            PrivacyGridF();
                            break;

                        case "Security":
                            SecurityGridF();
                            break;

                        case "Programs":
                            ProgramsGridF();
                            break;
                    }

                    //

                    void OverviewGridF()
                    {
                        OverviewGridBorder.Height = ContentGridHeight;
                        OverviewGridBorder.Width = Width80;

                        //set wininfo area
                        Double WAWidth = (Double)Convert.ToInt32(Width80 / 100 * 25);
                        Double WAHeight = ContentGridHeight - 95;

                        //WindowsArea.Width = WAWidth;
                        //WindowsArea.Height = WAHeight;

                        if (ContentGridHeight > 450)
                        {
                            //SystemLable.FontSize = 36;
                        }
                        else
                        {
                            //SystemLable.FontSize = 24;
                        }

                        //Double VersionHight = WindowsArea.ActualHeight / 100 * 25;
                        //WinVersion.Margin = new Thickness(5, VersionHight, 0, 0);
                        //BaU.Margin = new Thickness(5, VersionHight + 14, 0, 0);

                        //Double SysTypeHight = WindowsArea.ActualHeight / 100 * 50;
                        //SysType.Margin = new Thickness(5, SysTypeHight, 0, 0);
                        //SecBoot.Margin = new Thickness(5, SysTypeHight + 14, 0, 0);

                        //Double LicenseLableHight = WindowsArea.ActualHeight / 100 * 75;
                        //LicenseLable.Margin = new Thickness(5, LicenseLableHight, 0, 0);
                        //LicenseStatus.Margin = new Thickness(5, LicenseLableHight + 14, 0, 0);
                    }

                    void AppearanceGridF()
                    {
                        AppearanceBorder.Height = ContentGridHeight;
                        AppearanceBorder.Width = Width80;

                        switch (PageNumber)
                        {
                            case 0:
                                PG0();
                                break;

                            case 1:
                                PG1();
                                break;

                            case 2:
                                PG2();
                                break;
                        }

                        //page render
                        void PG0()
                        {

                        }

                        void PG1()
                        {

                        }

                        void PG2()
                        {

                        }
                    }

                    void BehaviorGridF()
                    {
                        BehaviorBorder.Height = ContentGridHeight;
                        BehaviorBorder.Width = Width80;
                    }

                    void PrivacyGridF()
                    {
                        PrivacyBorder.Height = ContentGridHeight;
                        PrivacyBorder.Width = Width80;
                    }

                    void SecurityGridF()
                    {
                        SecurityBorder.Height = ContentGridHeight;
                        SecurityBorder.Width = Width80;
                    }

                    void ProgramsGridF()
                    {
                        ProgramsBorder.Height = ContentGridHeight;
                        ProgramsBorder.Width = Width80;
                    }
                }
            }

            if (RenderContenBoardOnly) return;

            RescaleInProgress = false;
        }

        //

        public void ActivateWorker()
        {
            if (ActivityWorkerKiu < 1)
            {
                WorkIndicator.Visibility = Visibility.Visible;

                ++ActivityWorkerKiu;

                ActivityWorker();
            }
            else
            {
                ++ActivityWorkerKiu;
            }
        }

        public void DeactivateWorker()
        {
            if (ActivityWorkerKiu > 0)
            {
                --ActivityWorkerKiu;
            }
        }

        private static Int16 ActivityWorkerKiu = 0;
        private static Int16 WorkerRotation = 0;
        private async void ActivityWorker()
        {
            await Task.Run(() =>
            {
                while (ActivityWorkerKiu > 0)
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
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private const SByte PGNOverview = 0;
        private const SByte PGNAppearance = 2;
        private const SByte PGNBehavior = 0;
        private const SByte PGNPrivacy = 0;
        private const SByte PGNSecurity = 0;
        private const SByte PGNPrograms = 0;

        private void ChangePageVisibility(SByte ActivatePageNumber, SByte DeactivatePageNumber, String Area = null)
        {
            Area ??= CurrentArea;

            switch (Area)
            {
                case "Overview":
                    OverviewPG();
                    break;

                case "Appearance":
                    AppearancePG();
                    break;

                case "Behavior":
                    BehaviorPG();
                    break;

                case "Privacy":
                    PrivacyPG();
                    break;

                case "Security":
                    SecurityPG();
                    break;

                case "Programs":
                    ProgramsPG();
                    break;
            }

            void OverviewPG()
            {
                Dispatcher.Invoke(new Action(() => {

                    //show new page
                    switch (ActivatePageNumber)
                    {
                        case 0:
                            OverviewGridBorder.Visibility = Visibility.Visible;
                            break;

                        case 1:
                            throw new NotImplementedException("ChangePageVisibility > OverviewPG");
                            //break;

                        case 2:
                            throw new NotImplementedException("ChangePageVisibility > OverviewPG");
                            //break;

                        case -1:
                            break;

                        default:
                            throw new NotImplementedException("ChangePageVisibility > OverviewPG");
                    }

                    switch (DeactivatePageNumber)
                    {
                        case 0:
                            OverviewGridBorder.Visibility = Visibility.Collapsed;
                            break;

                        case 1:
                            throw new NotImplementedException("ChangePageVisibility > OverviewPG");
                            //break;

                        case 2:
                            throw new NotImplementedException("ChangePageVisibility > OverviewPG");
                            //break;

                        case -1:
                            break;

                        default:
                            throw new NotImplementedException("ChangePageVisibility > OverviewPG");
                    }
                }));
            }

            void AppearancePG()
            {
                Dispatcher.Invoke(new Action(() => {

                    //show new page
                    switch (ActivatePageNumber)
                    {
                        case 0:
                            AppearanceGrid0.Visibility = Visibility.Visible;
                            break;

                        case 1:
                            AppearanceGrid1.Visibility = Visibility.Visible;
                            break;

                        case 2:
                            AppearanceGrid2.Visibility = Visibility.Visible;
                            break;

                        case -1:
                            break;

                        default:
                            throw new NotImplementedException("ChangePageVisibility > AppearancePG");
                    }

                    switch (DeactivatePageNumber)
                    {
                        case 0:
                            AppearanceGrid0.Visibility = Visibility.Collapsed;
                            break;

                        case 1:
                            AppearanceGrid1.Visibility = Visibility.Collapsed;
                            break;

                        case 2:
                            AppearanceGrid2.Visibility = Visibility.Collapsed;
                            break;

                        case -1:
                            break;

                        default:
                            throw new NotImplementedException("ChangePageVisibility > AppearancePG");
                    }
                }));
            }

            void BehaviorPG()
            {
                //throw new NotImplementedException("ChangePageVisibility > BehaviorPG");
            }

            void PrivacyPG()
            {
                //throw new NotImplementedException("ChangePageVisibility > PrivacyPG");
            }

            void SecurityPG()
            {
                //throw new NotImplementedException("ChangePageVisibility > SecurityPG");
            }

            void ProgramsPG()
            {
                //throw new NotImplementedException("ChangePageVisibility > ProgramsPG");
            }
        }

        private void ManageNavigationButtons(String NewTab, String OldTab)
        {
            //show new page
            ChangePageVisibility(0, -1, NewTab);

            Dispatcher.BeginInvoke(new Action(() => Rescale(true)));

            //activate new
            switch (NewTab)
            {
                case "Overview":
                    Overview.IsChecked = true;
                    OverviewGridBorder.Visibility = Visibility.Visible;
                    break;

                case "Appearance":
                    Appearance.IsChecked = true;
                    AppearanceBorder.Visibility = Visibility.Visible;
                    break;

                case "Behavior":
                    Behavior.IsChecked = true;
                    BehaviorBorder.Visibility = Visibility.Visible;
                    break;

                case "Privacy":
                    Privacy.IsChecked = true;
                    PrivacyBorder.Visibility = Visibility.Visible;
                    break;

                case "Security":
                    Security.IsChecked = true;
                    SecurityBorder.Visibility = Visibility.Visible;
                    break;

                case "Programs":
                    Programs.IsChecked = true;
                    ProgramsBorder.Visibility = Visibility.Visible;
                    break;
            }

            //deactivate old
            switch (OldTab)
            {
                case "Overview":
                    Overview.IsChecked = false;
                    OverviewGridBorder.Visibility = Visibility.Collapsed;
                    break;

                case "Appearance":
                    Appearance.IsChecked = false;
                    AppearanceBorder.Visibility = Visibility.Collapsed;
                    break;

                case "Behavior":
                    Behavior.IsChecked = false;
                    BehaviorBorder.Visibility = Visibility.Collapsed;
                    break;

                case "Privacy":
                    Privacy.IsChecked = false;
                    PrivacyBorder.Visibility = Visibility.Collapsed;
                    break;

                case "Security":
                    Security.IsChecked = false;
                    SecurityBorder.Visibility = Visibility.Collapsed;
                    break;

                case "Programs":
                    Programs.IsChecked = false;
                    ProgramsBorder.Visibility = Visibility.Collapsed;
                    break;
            }

            //hide old page
            ChangePageVisibility(-1, PageNumber, OldTab);

            PageNumber = 0;
        }


    }
}
