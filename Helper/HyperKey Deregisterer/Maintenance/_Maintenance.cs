using System;
using System.Threading;

namespace Deregisterer
{
    internal static partial class MaintenanceService
    {
        internal static void Run()
        {
            Boolean versionChanged = WindowsVersionChanged(); // test and set current version info
            Boolean userInitIsValid = UserInitIsValid();

            if (!versionChanged && userInitIsValid) goto END;

            if (!userInitIsValid) FixUserInit();

            if (versionChanged)
            {
                RegistryAssists(Context.Machine);

                SetHelpPaneAttributes();
            }

        END:
            Thread.Sleep(5120);
            Environment.Exit(0);
        }
    }
}