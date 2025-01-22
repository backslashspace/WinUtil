using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable IDE0079
#pragma warning disable CS8618
#pragma warning disable CS8600

namespace BSS.Logging
{
    internal static class Log
    {
        [DllImport("kernel32.dll")]
        private static extern Boolean AllocConsole();

        internal static Boolean IsInitialized { get; private set; }

        private static readonly Object _fileLock = new();

        private static Options _configuration;
  
        internal sealed class Options
        {
            internal static Options Default() => new(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            internal const Int32 DEFAULT_PADDING_WIDTH = 52;
            internal const String FILENAME_FORMAT = "yyyy.MM.dd";
            internal const String TIME_FORMAT = "dd.MM.yyyy HH:mm:ss";

            internal Options(String logDirectoryPath, Int32 padding = DEFAULT_PADDING_WIDTH, String timeFormat = TIME_FORMAT, String filenameFormat = FILENAME_FORMAT, Boolean allocateConsoleInReleaseMode = false)
            {
                LogDirectoryPath = logDirectoryPath;
                Padding = padding;
                TimeFormat = timeFormat;
                FilenameFormat = filenameFormat;
                DefaultTextColor = Console.ForegroundColor;
                AllocateConsoleInReleaseMode = allocateConsoleInReleaseMode;

                Initialized = true;
            }

            internal readonly String LogDirectoryPath;
            internal readonly Int32 Padding;
            internal readonly String TimeFormat;
            internal readonly String FilenameFormat;
            internal readonly ConsoleColor DefaultTextColor;
            internal readonly Boolean AllocateConsoleInReleaseMode;

            internal readonly Boolean Initialized;
        }

        internal static Boolean Initialize(Options configuration)
        {
            if (IsInitialized || !configuration.Initialized) return false;

#if DEBUG
            if (Console.LargestWindowWidth == 0) AllocConsole();
#else
            if (configuration.AllocateConsoleInReleaseMode && Console.LargestWindowWidth == 0) AllocConsole();
#endif

            _configuration = configuration;

            if (Directory.Exists($"{configuration.LogDirectoryPath}\\logs"))
            {
                DateTime now = DateTime.Now;

                if (File.Exists($"{configuration.LogDirectoryPath}\\logs\\{now.ToString(configuration.FilenameFormat)}.txt"))
                {
                    using (StreamWriter streamWriter = new($"{configuration.LogDirectoryPath}\\logs\\{now.ToString(configuration.FilenameFormat)}.txt", true, Encoding.UTF8))
                    {
                        streamWriter.WriteLine();
                    }
                }
            }

            IsInitialized = true;

            return true;
        }

        // #######################################################################################

        internal static void FastLog(String message, LogSeverity severity, String source)
        {
            DateTime now = DateTime.Now;

            LogMessage logMessage;
            logMessage.Message = message;
            logMessage.Severity = severity;
            logMessage.Source = source;

            WriteFile(ref logMessage, ref now);
        }

        [Conditional("DEBUG")]
        [Obsolete("Will be omitted in RELEASE builds", false)]
        internal static void Debug(String message, String source)
        {
            DateTime now = DateTime.Now;

            LogMessage logMessage;
            logMessage.Message = message;
            logMessage.Severity = LogSeverity.Debug;
            logMessage.Source = source;

            WriteFile(ref logMessage, ref now);
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static void WriteFile(ref readonly LogMessage formattedLogMessage, ref readonly DateTime timeStamp)
        {
            if (!IsInitialized) throw new MethodAccessException("Logging class not initialized");
            if (!Directory.Exists($"{_configuration.LogDirectoryPath}\\logs")) Directory.CreateDirectory($"{_configuration.LogDirectoryPath}\\logs");

            //

            Int32 lineLength = 27 + formattedLogMessage.Source.Length;

            String source = $"]-[{formattedLogMessage.Source}]";
            String timeStampString = $"[{timeStamp.ToString(_configuration.TimeFormat)}] [";

            String severityString = null;
            switch (formattedLogMessage.Severity)
            {
                case LogSeverity.Info:
                    lineLength += 4;
                    severityString += "Info";
                    break;
                case LogSeverity.Debug:
                    lineLength += 5;
                    severityString += "Debug";
                    break;
                case LogSeverity.Warning:
                    lineLength += 7;
                    severityString += "Warning";
                    break;
                case LogSeverity.Verbose:
                    lineLength += 7;
                    severityString += "Verbose";
                    break;
                case LogSeverity.Error:
                    lineLength += 5;
                    severityString += "Error";
                    break;
                case LogSeverity.Critical:
                    lineLength += 8;
                    severityString += "Critical";
                    break;
                case LogSeverity.Alert:
                    lineLength += 5;
                    severityString += "Alert";
                    break;
            }

            String padding;
            if (lineLength < _configuration.Padding)
            {
                padding = new String(' ', _configuration.Padding - lineLength);
            }
            else
            {
                padding = " ";
            }

            String logLine = timeStampString + severityString + source + padding + formattedLogMessage.Message;

            try
            {
                lock (_fileLock)
                {
#if DEBUG
                    ColoredDebugPrint(timeStampString, formattedLogMessage.Severity, severityString!, source, padding, formattedLogMessage.Message);
#else
                    if (_configuration.AllocateConsoleInReleaseMode) ColoredDebugPrint(timeStampString, formattedLogMessage.Severity, severityString!, source, padding, formattedLogMessage.Message);
#endif

                    using (StreamWriter streamWriter = new($"{_configuration.LogDirectoryPath}\\logs\\{timeStamp.ToString(_configuration.FilenameFormat)}.txt", true, Encoding.UTF8))
                    {
                        streamWriter.WriteLine(logLine);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FieldAccessException($"Unable to write log file to:\n{_configuration.LogDirectoryPath}\\logs\\{timeStamp.ToString(_configuration.FilenameFormat)}.txt\n\nError: {ex.Message}");
            }
        }

        private static void ColoredDebugPrint(String timeStampString, LogSeverity logSeverity, String severityString, String source, String padding, String message)
        {
            ConsoleColor foreground = logSeverity switch
            {
                LogSeverity.Info => ConsoleColor.DarkCyan,
                LogSeverity.Debug => ConsoleColor.DarkGreen,
                LogSeverity.Warning => ConsoleColor.DarkYellow,
                LogSeverity.Verbose => ConsoleColor.Magenta,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Critical => ConsoleColor.DarkRed,
                LogSeverity.Alert => ConsoleColor.Yellow,
                _ => _configuration.DefaultTextColor,
            };

            Console.Write(timeStampString);
            Console.ForegroundColor = foreground;
            Console.Write(severityString);
            Console.ForegroundColor = _configuration.DefaultTextColor;
            Console.WriteLine(source + padding + message);
        }
    }
}