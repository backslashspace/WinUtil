using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WinUtil
{
    /// <remarks>Create a window with checkboxes</remarks>
    public partial class Dynamic_Select : Window
    {
        /// <summary>Input order -> output order</summary>
        internal Boolean[] Result;

        /// <summary><see langword="null"/> until window was closed via 'close', 'cancel' or 'continue'</summary>
        internal Boolean? Was_Canceled = null;

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        /// <summary>Builds a dynamic customizable window with checkboxes and description.</summary>
        ///
        /// <param name="Title">Window title</param>
        /// <param name="IconPath">Relative path to window icon</param>
        /// <param name="Head">Description headline</param>
        /// <param name="Body">Description body</param>
        /// <param name="Fields">Description body</param>
        ///
        /// <remarks>
        ///     <see langword="object"/>[,] <paramref name="Fields"/> has the following format:<br/>
        ///     { <see langword="bool"/> (Checkbox IsEnabled), <see langword="bool"/> (default value), <see langword="string"/> (text), <see langword="string"/> (tooltip) }
        /// </remarks>
        ///
        /// <returns><see langword="bool"/>[] <paramref name="Result"/> = (input order = output order)<br/>
        /// <see langword="bool"/>? <paramref name="Was_Canceled"/> = (<see langword="null"/> until window was closed via 'close', 'cancel' or 'continue')</returns>
        ///
        /// <exception cref="ArgumentException"></exception>
        public Dynamic_Select(String Title, String IconPath, String Head, String Body, Object[,] Fields)
        {
            IntBoxes = Fields.GetLength(0);

            if (IntBoxes < 2) { throw new ArgumentException("Dynamic_Select: less than 2 elements, why\n"); }

            CheckBoxes = new CheckBox[IntBoxes];

            InitializeComponent();

            CheckBoxStyle = (Style)FindResource("CheckBox");

            //

            BuildTextBody(Body, IntBoxes);

            BuildSelection(Fields);

            //

            Window_Title.Text = Title;

            Text_Head.Text = Head;

            if (IconPath != null)
            {
                WND_Icon.Source = new BitmapImage(new Uri(IconPath, UriKind.RelativeOrAbsolute));
            }
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private Style CheckBoxStyle;
        private Int32 IntBoxes;
        private CheckBox[] CheckBoxes;

        private ColumnDefinition[] SelectionColumns;

        static readonly SolidColorBrush FontColor = new((Color)ColorConverter.ConvertFromString("#dddddd"));

        private UIMode Mode = UIMode.Small;

        private enum UIMode
        {
            Small = 0,
            Big = 1
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private void BuildTextBody(String Body, Int32 NumberOfBoxes)
        {
            if (Body == null)
            {
                if (NumberOfBoxes > 6)
                {
                    Mode = UIMode.Big;
                }

                return;
            }

            Text_Body.Text = "";

            Int16 Possition = 0;
            Byte Max_Line_Length;

            Char[] Body_Chars = Body.ToCharArray();

            #region Set UI-Mode
            if (Body_Chars.Length < 350 && NumberOfBoxes < 7)
            {
                Max_Line_Length = 65;
            }
            else
            {
                Max_Line_Length = 120;

                Mode = UIMode.Big;
            }
            #endregion

            #region Manual-Testwrap
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
            #endregion

            Text_Body.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Text_Body.Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            Head_Row.Height = new GridLength(Head_Row.Height.Value + Text_Body.RenderSize.Height, GridUnitType.Pixel);

            Height += Text_Body.RenderSize.Height;
        }

        private void BuildSelection(Object[,] Fields)
        {
            SByte Columns = 2;

            if (Mode == UIMode.Big)
            {
                Columns = 4;
            }

            SelectionColumns = new ColumnDefinition[Columns];

            //

            for (SByte i = 0; i < Columns; ++i)
            {
                SelectionColumns[i] = new();
                SelectionColumns[i].Width = new GridLength(1, GridUnitType.Star);
                BoxGrid.ColumnDefinitions.Add(SelectionColumns[i]);
            }

            Double[] MaxWidth = new Double[Columns];

            #region Pulish Boxes
            Double tmp;
            SByte CurrentColumn = 0;
            Double BoxY = 10;

            for (Int16 i = 0; i < IntBoxes; ++i)
            {
                tmp = PushBox(i, (Boolean)Fields[i, 0], (Boolean)Fields[i, 1], (String)Fields[i, 2], (String)Fields[i, 3], BoxY);

                if (MaxWidth[CurrentColumn] < tmp)
                {
                    MaxWidth[CurrentColumn] = tmp;
                }

                Grid.SetColumn(CheckBoxes[i], CurrentColumn);

                if (CurrentColumn < Columns - 1)
                {
                    ++CurrentColumn;
                }
                else
                {
                    CurrentColumn = 0;

                    BoxY += 30;
                }
            }
            #endregion

            #region Set Body_Row Width
            Double BodyWidth = 0;

            for (Byte b = 0; b < Columns; ++b)
            {
                BodyWidth += 30 + MaxWidth[b];

                SelectionColumns[b].Width = new GridLength(30 + MaxWidth[b], GridUnitType.Pixel);
            }

            if (Width + 10 < BodyWidth)
            {
                Width = BodyWidth + 10;
            }
            else
            {
                Double PlusWidth = (Width - BodyWidth) / Columns;

                for (Byte b = 0; b < Columns; ++b)
                {
                    SelectionColumns[b].Width = new GridLength(30 + MaxWidth[b] + PlusWidth, GridUnitType.Pixel);
                }
            }
            #endregion

            #region Set Body_Row Heigth
            Byte BoxHeigth = 25;
            Double BodyHeigth;
            UInt16 BoxRows;

            if (Mode == UIMode.Small)
            {
                BoxRows = (UInt16)Math.Ceiling((Double)IntBoxes / 2);
            }
            else
            {
                if (IntBoxes < 4)
                {
                    BoxRows = 1;
                }
                else
                {
                    BoxRows = (UInt16)Math.Ceiling((Double)IntBoxes / 4);
                }
            }

            if (BoxRows > 1)
            {
                BodyHeigth = BoxHeigth * BoxRows + 25;

                if (BoxRows > 2)
                {
                    BodyHeigth += (BoxRows - 2) * 5;
                }
            }
            else
            {
                BodyHeigth = 45;
            }

            Body_Row.Height = new GridLength(BodyHeigth, GridUnitType.Pixel);

            Height += BodyHeigth;
            #endregion

            //# # # # # # # # # # # # # # # # # # # # # # # # # #

            Double PushBox(Int32 BoxIndex, Boolean IsEnabled, Boolean IsChecked, String Content, String ToolTip, Double Y)
            {
                CheckBoxes[BoxIndex] = new()
                {
                    Style = CheckBoxStyle,
                    Content = Content,
                    ToolTip = ToolTip,
                    IsChecked = IsChecked,
                    IsEnabled = IsEnabled,
                    Margin = new Thickness(15, Y, 0, 0),
                    Height = 25,
                    FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Foreground = FontColor,
                };

                BoxGrid.Children.Add(CheckBoxes[BoxIndex]);

                CheckBoxes[BoxIndex].Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                CheckBoxes[BoxIndex].Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

                return CheckBoxes[BoxIndex].ActualWidth;
            }
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # #

        private void Return(object sender, RoutedEventArgs e)
        {
            Result = new Boolean[IntBoxes];

            for (Int32 I = 0; I < IntBoxes; ++I)
            {
                Result[I] = (Boolean)CheckBoxes[I].IsChecked;
            }

            Was_Canceled = false;

            Close();
        }
    }
}