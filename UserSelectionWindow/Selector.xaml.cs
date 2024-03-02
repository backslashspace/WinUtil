using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WinUtil
{
    public partial class Selector : Window
    {
        /// <summary>Input order -> output order</summary>
        internal Boolean[] Result;

        /// <summary><see langword="null"/> until window was closed via 'close', 'cancel' or 'continue'</summary>
        internal Boolean? Was_Canceled = null;

        private Style CheckBoxStyle;
        private CheckBox[] CheckBoxes;
        private static readonly SolidColorBrush FontColor = new((Color)ColorConverter.ConvertFromString("#dddddd"));

        public readonly struct Field
        {
            public Field(Boolean userCanChange, Boolean defaultValue, String fieldName, String tooltip)
            {
                UserCanChange = userCanChange;
                DefaultValue = defaultValue;
                Fieldname = fieldName;
                Tooltip = tooltip;
            }

            public readonly Boolean UserCanChange;
            public readonly Boolean DefaultValue;
            public readonly String Fieldname;
            public readonly String Tooltip;
        }

        /// <summary>Builds a dynamic customizable window with checkboxes and description.</summary>
        public Selector(String windowTitle, String iconUriPath, String headLine, String description, Field[] fields)
        {
            if (fields.Length < 2 || fields.Length > Byte.MaxValue)
            {
                throw new InvalidDataException($"Number of fields provided was invalid.\n accepted range: 2 - {Byte.MaxValue}");
            }

            InitializeComponent();

            Title = windowTitle;
            Window_Title.Text = windowTitle;

            Head.Text = headLine;
            Description.Text = description;

            if (iconUriPath != null)
            {
                Window_Icon.Source = new BitmapImage(new Uri(iconUriPath, UriKind.RelativeOrAbsolute));
            }

            CheckBoxStyle = (Style)FindResource("CheckBox");
            CheckBoxes = new CheckBox[fields.Length];

            UInt16 Y = 15;

            for (Byte i = 0; i < fields.Length; i++)
            {
                CheckBoxes[i] = new()
                {
                    Style = CheckBoxStyle,
                    Content = fields[i].Fieldname,
                    ToolTip = fields[i].Tooltip,
                    IsChecked = fields[i].DefaultValue,
                    IsEnabled = fields[i].UserCanChange,
                    Margin = new Thickness(20, Y, 0, 0),
                    Height = 25,
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Foreground = FontColor,
                };

                ScrollGrid.Children.Add(CheckBoxes[i]);

                Y += 35;
            }

            ScrollGrid.Height = Y + 5;
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private void Return(object sender, RoutedEventArgs e)
        {
            Result = new Boolean[CheckBoxes.Length];

            for (Byte i = 0; i < CheckBoxes.Length; ++i)
            {
                Result[i] = (Boolean)CheckBoxes[i].IsChecked;
            }

            Was_Canceled = false;

            Close();
        }

        private void Cancel_Button(object sender, RoutedEventArgs e)
        {
            Was_Canceled = true;

            Close();
        }
    }
}