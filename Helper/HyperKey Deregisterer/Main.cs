using System;
using System.Security.Principal;
using System.ServiceProcess;

namespace Deregisterer
{
    internal static partial class Program
    {
        private static void Main(String[] args)
        {
            if (args.Length != 1)
            {
                Unregister();
                Environment.Exit(0);
            }

            switch (args[0])
            {
                case "/maintenance":
                    ServiceBase.Run(new Service());
                    return;

                case "/install":
                    if (IsElevated()) Installer.Install();
                    else Environment.Exit(-1);
                    return;

                case "/uninstall":
                    if (IsElevated()) Uninstaller.Uninstall();
                    else Environment.Exit(-1);
                    return;

                default:
                    Unregister();
                    return;
            }
        }

        private static Boolean IsElevated()
        {
            WindowsPrincipal principal = new(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}