using BSS.Logging;
using System;
using System.IO;
using System.Reflection;

namespace Stimulator
{
    internal static class Entry
    {
        [STAThread]
        private static void Main()
        {
            Console.WriteLine("\n ██████╗ ███████╗    ███████╗████████╗██╗███╗   ███╗██╗   ██╗██╗      █████╗ ████████╗ ██████╗ ██████╗     \r\n██╔═══██╗██╔════╝    ██╔════╝╚══██╔══╝██║████╗ ████║██║   ██║██║     ██╔══██╗╚══██╔══╝██╔═══██╗██╔══██╗    \r\n██║   ██║███████╗    ███████╗   ██║   ██║██╔████╔██║██║   ██║██║     ███████║   ██║   ██║   ██║██████╔╝    \r\n██║   ██║╚════██║    ╚════██║   ██║   ██║██║╚██╔╝██║██║   ██║██║     ██╔══██║   ██║   ██║   ██║██╔══██╗    \r\n╚██████╔╝███████║    ███████║   ██║   ██║██║ ╚═╝ ██║╚██████╔╝███████╗██║  ██║   ██║   ╚██████╔╝██║  ██║    \r\n ╚═════╝ ╚══════╝    ╚══════╝   ╚═╝   ╚═╝╚═╝     ╚═╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝    \r\n                                                                                                           ");

            RunContextInfo.ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Log.Initialize(new(RunContextInfo.ExecutablePath, Log.Options.DEFAULT_PADDING_WIDTH, Log.Options.TIME_FORMAT, Log.Options.FILENAME_FORMAT, true));

            App app = new();

            UI.Dispatcher = app.Dispatcher;

            try
            {
                app.InitializeComponent();

                Log.FastLog("Starting MainWindow", LogSeverity.Info, "Main()");

                app.Run();
            }
            catch (Exception exception)
            {
                Log.FastLog($"An unhandled exception was thrown: {exception.Message}\n" + exception.StackTrace, LogSeverity.Critical, "Main()");
            }

            Log.FastLog("MainWindow closed, press return to exit:", LogSeverity.Info, "Main()");

            Console.ReadLine();
        }
    }
}