using System;

namespace WinUtil
{
    internal class CustomAppEntry
    {
        //this 'overrides' Main() in App.g.cs
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            try
            {
                WinUtil.App app = new();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                Debug.AllocConsole();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Error message: ");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(ex.Message);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nStackTrace: ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(ex.StackTrace);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("\nAn unknown error occurred in the application.");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("\nPress return to exit: ");

                Console.ReadLine();

                Environment.Exit(1);
            }   
        }
    }
}