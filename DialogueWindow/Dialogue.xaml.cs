using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WinUtil
{
    /// <remarks>Create a message box with up to 3 buttons and a custom icon</remarks>
    public partial class Dialogue : Window
    {
        /// <summary>Index of the pressed button from RTL</summary>
        internal Byte? Result;

        #region Main
        /// <summary>Builds a dynamic customizable message box</summary>
        ///
        /// <param name="Title">Window title</param>
        /// <param name="Body">Text body</param>
        /// <param name="Icon">Body icon</param>
        /// <param name="Button_0_Text">Button-0 text</param>
        ///
        /// <returns><see langword="byte"/>? <paramref name="Result"/> = (<see langword="null"/> until window was closed | index of pressed button from RTL)</returns>
        internal Dialogue(String Title, String Body, Icons Icon, String Button_0_Text)
        {
            BuildWindow(Title, Body, Icon, 187);

            Buttons = new System.Windows.Controls.Button[1];

            PushButton(0, Button_0_Text, 14, Button_Style.Dark);
        }

        /// <summary>Builds a dynamic customizable message box</summary>
        ///
        /// <param name="Title">Window title</param>
        /// <param name="Body">Text body</param>
        /// <param name="Icon">Body icon</param>
        /// <param name="Button_0_Text">Button-0 text</param>
        /// <param name="Default_Button">Index of default button from RTL</param>
        ///
        /// <returns><see langword="byte"/>? <paramref name="Result"/> = (<see langword="null"/> until window was closed | index of pressed button from RTL)</returns>
        internal Dialogue(String Title, String Body, Icons Icon, String Button_0_Text, Byte Default_Button)
        {
            if (Default_Button != 0) { throw new ArgumentException("Byte Default_Button: out of range\nOverload max = 0"); }

            BuildWindow(Title, Body, Icon, 187);

            Buttons = new System.Windows.Controls.Button[1];

            PushButton(0, Button_0_Text, 14, Button_Style.Blue);
        }

        /// <summary>Builds a dynamic customizable message box</summary>
        ///
        /// <param name="Title">Window title</param>
        /// <param name="Body">Text body</param>
        /// <param name="Icon">Body icon</param>
        /// <param name="Button_0_Text">Button-0 text</param>
        /// <param name="Button_1_Text">Button-1 text</param>
        ///
        /// <returns><see langword="byte"/>? <paramref name="Result"/> = (<see langword="null"/> until window was closed | index of pressed button from RTL)</returns>
        internal Dialogue(String Title, String Body, Icons Icon, String Button_0_Text, String Button_1_Text)
        {

            BuildWindow(Title, Body, Icon, 211);

            Buttons = new System.Windows.Controls.Button[2];

            PushButton(0, Button_0_Text, 16, Button_Style.Dark);

            PushButton(1, Button_1_Text, 103, Button_Style.Dark);
        }

        /// <summary>Builds a dynamic customizable message box</summary>
        ///
        /// <param name="Title">Window title</param>
        /// <param name="Body">Text body</param>
        /// <param name="Icon">Body icon</param>
        /// <param name="Button_0_Text">Button-0 text</param>
        /// <param name="Button_1_Text">Button-1 text</param>
        /// <param name="Default_Button">Index of default button from RTL</param>
        ///
        /// <returns><see langword="byte"/>? <paramref name="Result"/> = (<see langword="null"/> until window was closed | index of pressed button from RTL)</returns>
        internal Dialogue(String Title, String Body, Icons Icon, String Button_0_Text, String Button_1_Text, Byte Default_Button)
        {
            BuildWindow(Title, Body, Icon, 211);

            Buttons = new System.Windows.Controls.Button[2];

            switch (Default_Button)
            {
                case 0:
                    PushButton(0, Button_0_Text, 16, Button_Style.Blue);
                    PushButton(1, Button_1_Text, 103, Button_Style.Dark);
                    break;

                case 1:
                    PushButton(0, Button_0_Text, 16, Button_Style.Dark);
                    PushButton(1, Button_1_Text, 103, Button_Style.Blue);
                    break;

                default:
                    throw new ArgumentException("Byte Default_Button: out of range\nOverload max = 1 (0, 1)");
            }
        }

        /// <summary>Builds a dynamic customizable message box</summary>
        ///
        /// <param name="Title">Window title</param>
        /// <param name="Body">Text body</param>
        /// <param name="Icon">Body icon</param>
        /// <param name="Button_0_Text">Button-0 text</param>
        /// <param name="Button_1_Text">Button-1 text</param>
        /// <param name="Button_2_Text">Button-2 text</param>
        ///
        /// <returns><see langword="byte"/>? <paramref name="Result"/> = (<see langword="null"/> until window was closed | index of pressed button from RTL)</returns>
        internal Dialogue(String Title, String Body, Icons Icon, String Button_0_Text, String Button_1_Text, String Button_2_Text)
        {
            BuildWindow(Title, Body, Icon, 298);

            Buttons = new System.Windows.Controls.Button[3];

            PushButton(0, Button_0_Text, 16, Button_Style.Dark);
            PushButton(1, Button_1_Text, 103, Button_Style.Dark);
            PushButton(2, Button_2_Text, 190, Button_Style.Dark);
        }

        /// <summary>Builds a dynamic customizable message box</summary>
        ///
        /// <param name="Title">Window title</param>
        /// <param name="Body">Text body</param>
        /// <param name="Icon">Body icon</param>
        /// <param name="Button_0_Text">Button-0 text</param>
        /// <param name="Button_1_Text">Button-1 text</param>
        /// <param name="Button_2_Text">Button-2 text</param>
        /// <param name="Default_Button">Index of default button from RTL</param>
        ///
        /// <returns><see langword="byte"/>? <paramref name="Result"/> = (<see langword="null"/> until window was closed | index of pressed button from RTL)</returns>
        internal Dialogue(String Title, String Body, Icons Icon, String Button_0_Text, String Button_1_Text, String Button_2_Text, Byte Default_Button)
        {
            BuildWindow(Title, Body, Icon, 298);

            Buttons = new System.Windows.Controls.Button[3];

            switch (Default_Button)
            {
                case 0:
                    PushButton(0, Button_0_Text, 16, Button_Style.Blue);
                    PushButton(1, Button_1_Text, 103, Button_Style.Dark);
                    PushButton(2, Button_2_Text, 190, Button_Style.Dark);
                    break;

                case 1:
                    PushButton(0, Button_0_Text, 16, Button_Style.Dark);
                    PushButton(1, Button_1_Text, 103, Button_Style.Blue);
                    PushButton(2, Button_2_Text, 190, Button_Style.Dark);
                    break;

                case 2:
                    PushButton(0, Button_0_Text, 16, Button_Style.Dark);
                    PushButton(1, Button_1_Text, 103, Button_Style.Dark);
                    PushButton(2, Button_2_Text, 190, Button_Style.Blue);
                    break;

                default:
                    throw new ArgumentException("Byte Default_Button: out of range\nOverload max = 2 (0, 1, 2)");
            }
        }
        #endregion

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal enum Icons
        {
            Gear = 0,
            Gear_Tick = 1,
            Shield_Exclamation_Mark = 2,
            Triangle_Exclamation_Mark = 3,
            Circle_Error = 4,
            Shield_Error = 5,
            Tick = 6,
            Admin_Shield = 7,
            Shield_Tick = 8,
            Shield_Question = 9,
            Circle_Question = 10,
            Globe = 11,
            Lock = 12,
            Performance = 13,
        }

        internal enum Button_Style
        {
            Dark = 0,
            Blue = 1,
        }

        private System.Windows.Controls.Button[] Buttons;

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        void PushButton(Byte Index, String Content, Double X, Button_Style Style)
        {
            Buttons[Index] = new()
            {
                Content = Content,
                Margin = new Thickness(0, 0, X, 13),
                Height = 23,
                Width = 80,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            if (Style == Button_Style.Blue)
            {
                Buttons[Index].Style = (Style)FindResource("Focused_Button");
            }

            Button_Grid.Children.Add(Buttons[Index]);

            switch (Index)
            {
                case 0:
                    Buttons[Index].Click += new RoutedEventHandler(Button_0);
                    break;

                case 1:
                    Buttons[Index].Click += new RoutedEventHandler(Button_1);
                    break;

                case 2:
                    Buttons[Index].Click += new RoutedEventHandler(Button_2);
                    break;
            }

            if (Style == Button_Style.Blue)
            {
                Buttons[Index].Focus();
                Keyboard.Focus(Buttons[Index]);
            }
        }

        private void BuildWindow(String Title, String Body, Icons Icon, Int16 MinWidth)
        {
            InitializeComponent();
            Width = MinWidth;

            Window_Title.Text = Title;
            Text_Body.Text = "";

            Int16 Possition = 0;
            Byte Max_Line_Length;
            Double Window_Height;
            Double Window_Width;

            Char[] Body_Chars = Body.ToCharArray();

            //

            if (Body_Chars.Length < 350)
            {
                Max_Line_Length = 100;
            }
            else
            {
                Max_Line_Length = 255;
            }

            for (Int16 I = 0; I < Body_Chars.Length; ++I)
            {
                if (Body_Chars[I] == '\n')
                {
                    Possition = 0;
                }
                else if (Possition == Max_Line_Length)
                {
                    Possition = 0;

                    Text_Body.Text += '\n';
                }

                Text_Body.Text += Body_Chars[I];

                ++Possition;
            }

            //

            Text_Body.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Text_Body.Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            Size TextBlockDim = Text_Body.RenderSize;

            Window_Width = TextBlockDim.Width + 62 + 33;

            //

            if (TextBlockDim.Height > Text_Body.FontSize && TextBlockDim.Height < Text_Body.FontSize * 2)
            {
                //when one line 
                Window_Height = 26 + TextBlockDim.Height + 96;
            }
            else
            {
                //when two line 
                Text_Body.Margin = new Thickness(64, 24, 30, 0);

                Window_Height = 30 + TextBlockDim.Height + 96;
            }

            //

            if (Window_Height > Height)
            {
                Height = Window_Height;
            }

            if (Window_Width > Width)
            {
                Width = Window_Width;
            }

            //

            switch (Icon)
            {
                case Icons.Admin_Shield:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_78.ico", UriKind.Relative));
                    break;

                case Icons.Circle_Error:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_98.ico", UriKind.Relative));
                    break;

                case Icons.Circle_Question:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_99.ico", UriKind.Relative));
                    break;

                case Icons.Gear:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\shell32_16826.ico", UriKind.Relative));
                    break;

                case Icons.Gear_Tick:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_114.ico", UriKind.Relative));
                    break;

                case Icons.Globe:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\shell32_14.ico", UriKind.Relative));
                    break;

                case Icons.Lock:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\shell32_48", UriKind.Relative));
                    break;

                case Icons.Performance:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_150.ico", UriKind.Relative));
                    break;

                case Icons.Shield_Error:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_105.ico", UriKind.Relative));
                    break;

                case Icons.Shield_Exclamation_Mark:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_107.ico", UriKind.Relative));
                    break;

                case Icons.Shield_Question:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_104.ico", UriKind.Relative));
                    break;

                case Icons.Shield_Tick:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_106.ico", UriKind.Relative));
                    break;

                case Icons.Tick:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\shell32_1_16802.ico", UriKind.Relative));
                    break;

                case Icons.Triangle_Exclamation_Mark:
                    Dialogue_Icon.Source = new BitmapImage(new Uri(@"Icons\imageres_84.ico", UriKind.Relative));
                    break;
            }
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        #region Button_Handler
        private void Button_0(object sender, RoutedEventArgs e)
        {
            Result = 0;

            Close();
        }

        private void Button_1(object sender, RoutedEventArgs e)
        {
            Result = 1;

            Close();
        }

        private void Button_2(object sender, RoutedEventArgs e)
        {
            Result = 2;

            Close();
        }
        #endregion
    }
}