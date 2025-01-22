using BSS.Logging;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

#pragma warning disable IDE0079
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace Stimulator.SubWindows
{
     // saved file layout = version | data
     //                      ^^^^^
     //                      Int32

    public sealed partial class OptionSelector : Window
    {
        [Obsolete("Empty Constructor not supported.", true)]
        public OptionSelector() { }

        internal readonly struct Configuration
        {
            [Obsolete("Empty Constructor not supported.", true)]
            public Configuration() { }

            internal Configuration(Boolean allowConfiguration, Int32 version = 0, String filename = null)
            {
                if (allowConfiguration && filename == null)
                {
                    Log.Debug("allowConfiguration was true but filename was null, this should not happen", "Stimulator.SubWindows.OptionSelector._configuration.ctor()");
                    throw new InvalidOperationException("_configuration.Filename was null");
                }

                AllowConfiguration = allowConfiguration;
                Version = version;
                Filename = filename;
            }

            internal readonly Boolean AllowConfiguration;
            internal readonly Int32 Version;
            internal readonly String Filename;
        }

        internal readonly struct Option
        {
            [Obsolete("Empty Constructor not supported.", true)]
            public Option() {}

            internal Option(Boolean defaultValue, Boolean readOnly, String fieldName, String tooltip)
            {
                DefaultValue = defaultValue;
                ReadOnly = readOnly;
                FieldName = fieldName;
                Tooltip = tooltip;
            }

            internal readonly Boolean DefaultValue;
            internal readonly Boolean ReadOnly;
            internal readonly String FieldName;
            internal readonly String Tooltip;
        }

        internal struct SelectionResult()
        {
            internal Boolean CommitSelection = false;
            internal Boolean[] UserSelection = null;
        }

        // ######################################################################################

        private readonly Configuration _configuration;

        private readonly Option[] _options;
        private readonly Int32 _optionsLength;

        private SelectionResult _result;
        internal SelectionResult Result { get => _result; }

        internal OptionSelector(String title, Option[] options, Configuration configuration)
        {
            if (title == null || options == null) throw new InvalidOperationException("title or options[] was null");

            _configuration = configuration;
            _options = options;
            _optionsLength = options.Length;

            InitializeComponent();

            Loaded += BuildWindowContent;

            Title = title;

            if (!configuration.AllowConfiguration)
            {
                SaveButton.Visibility = Visibility.Collapsed;
                LoadButton.Visibility = Visibility.Collapsed;
            }
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private CheckBox[] _checkBoxes;

        private void BuildWindowContent(Object sender, RoutedEventArgs e)
        {
            _checkBoxes = new CheckBox[_optionsLength];

            Double yPosition = 20;

            for (Int32 i = 0; i < _optionsLength; ++i)
            {
                _checkBoxes[i] = new();
                _checkBoxes[i].Content = _options[i].FieldName;
                _checkBoxes[i].ToolTip = _options[i].Tooltip;
                _checkBoxes[i].IsEnabled = !_options[i].ReadOnly;
                _checkBoxes[i].IsChecked = _options[i].DefaultValue;
                _checkBoxes[i].Margin = new(25, yPosition, 0, 0);
                _checkBoxes[i].VerticalAlignment = VerticalAlignment.Top;
                _checkBoxes[i].HorizontalAlignment = HorizontalAlignment.Left;

                yPosition += 30;

                OptionsGrid.Children.Add(_checkBoxes[i]);
            }

            // hack: grid uses auto height, use dummy to have some even spacing on the bottom
            CheckBox bottomSpacer = new();
            bottomSpacer.Visibility = Visibility.Hidden;
            bottomSpacer.Margin = new(25, yPosition - 10, 0, 0);
            bottomSpacer.VerticalAlignment = VerticalAlignment.Top;
            bottomSpacer.HorizontalAlignment = HorizontalAlignment.Left;

            OptionsGrid.Children.Add(bottomSpacer);
        }

        // ######################################################################################

        private void Commit_Click(Object sender, RoutedEventArgs e)
        {
            _result.UserSelection = new Boolean[_optionsLength];
            
            for (Int32 i = 0; i < _optionsLength; ++i)
            {
                _result.UserSelection[i] = (Boolean)_checkBoxes[i].IsChecked!;
            }

            _result.CommitSelection = true;
            Close();
        }

        private void Cancel_Click(Object sender, RoutedEventArgs e) => Close();

        private void All_Click(Object sender, RoutedEventArgs e)
        {
            for (Int32 i = 0; i < _optionsLength; ++i)
            {
                if (!_checkBoxes[i].IsEnabled) continue;

                _checkBoxes[i].IsChecked = true;
            }
        }

        private void None_Click(Object sender, RoutedEventArgs e)
        {
            for (Int32 i = 0; i < _optionsLength; ++i)
            {
                if (!_checkBoxes[i].IsEnabled) continue;

                _checkBoxes[i].IsChecked = false;
            }
        }

        private unsafe void Save_Click(Object sender, RoutedEventArgs e)
        {
        redo:
            OpenFileDialog openFileDialog = new();
            openFileDialog.InitialDirectory = RunContextInfo.ExecutablePath;
            openFileDialog.FileName = _configuration.Filename;
            openFileDialog.CheckFileExists = false;
            openFileDialog.Filter = "Configuration file (*.cfg)|*.cfg|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Title = "Select " + Title + " file";

            if (!(Boolean)openFileDialog.ShowDialog()!) return;

            FileStream fileStream;

            try
            {
                fileStream = new(openFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, false);
            }
            catch (Exception exception)
            {
                Log.FastLog($"Failed to open '{openFileDialog.FileName}' in write mode: {exception.Message}", LogSeverity.Error, "SaveConfig");
                goto redo;
            }

            fileStream.Write(BitConverter.GetBytes(_configuration.Version), 0, 4);

            for (Int32 i = 0; i < _optionsLength; ++i)
            {
                Boolean isChecked = (Boolean)_checkBoxes[i].IsChecked!;
                fileStream.WriteByte(*(Byte*)&isChecked);
            }

            fileStream.Flush(true);

            Log.FastLog($"Wrote config for '{Title}' to {openFileDialog.FileName}", LogSeverity.Info, "SaveConfig");

            fileStream.Close();
            fileStream.Dispose();
        }

        private unsafe void Load_Click(Object sender, RoutedEventArgs e)
        {
        redo:
            OpenFileDialog openFileDialog = new();
            openFileDialog.InitialDirectory = RunContextInfo.ExecutablePath;
            openFileDialog.FileName = _configuration.Filename;
            openFileDialog.CheckFileExists = false;
            openFileDialog.Filter = "Configuration file (*.cfg)|*.cfg|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Title = "Select " + Title + " file";

            if (!(Boolean)openFileDialog.ShowDialog()!) return;

            FileStream fileStream;

            try
            {
                fileStream = new(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, false);
            }
            catch (Exception exception)
            {
                Log.FastLog($"Failed to open '{openFileDialog.FileName}' in read mode: {exception.Message}", LogSeverity.Error, "LoadConfig");
                goto redo;
            }

            Byte[] versionBytes = new Byte[4];
            fileStream.Read(versionBytes, 0, 4);
            Int32 configurationVersion = BitConverter.ToInt32(versionBytes, 0);

            if (configurationVersion != _configuration.Version)
            {
                Log.FastLog($"Failed to load '{openFileDialog.FileName}', version mismatch, config version: {configurationVersion}, required version: {_configuration.Version}", LogSeverity.Error, "LoadConfig");
                goto redo;
            }

            for (Int32 i = 0; i < _optionsLength; ++i)
            {
                if (!_checkBoxes[i].IsEnabled) continue;

                Byte isChecked = (Byte)fileStream.ReadByte();
                _checkBoxes[i].IsChecked = *(Boolean*)&isChecked;
            }

            Log.FastLog($"Successfully loaded config for '{Title}' from {openFileDialog.FileName}", LogSeverity.Info, "LoadConfig");

            fileStream.Close();
            fileStream.Dispose();
        }
    }
}