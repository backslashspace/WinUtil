using System;

namespace Deregisterer
{
    internal static partial class Installer
    {
        internal const String EXECUTABLE_NAME_WITHOUT_EXTENSION = "HyperKey Deregisterer";
        internal const String EXECUTABLE_NAME = $"{EXECUTABLE_NAME_WITHOUT_EXTENSION}.exe";
        internal const String INSTALLATION_DIRECTORY_NAME = "HyperKey Deregisterer";
        internal const String INSTALLATION_DIRECTORY_PATH = $"C:\\Program Files\\{INSTALLATION_DIRECTORY_NAME}\\";

        internal const String REGISTRY_APP_HIVE = "HKEY_LOCAL_MACHINE\\";
        internal const String REGISTRY_APP_TREE = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{EXECUTABLE_NAME_WITHOUT_EXTENSION}";
        internal const String REGISTRY_APP_FULL_PATH = $"{REGISTRY_APP_HIVE}{REGISTRY_APP_TREE}";

        internal static void Install()
        {
            CopyFiles();

            // apply user init
            MaintenanceService.FixUserInit();

            // pre-run registry tweaks
            MaintenanceService.RegistryAssists(MaintenanceService.Context.Machine);
            MaintenanceService.RegistryAssists(MaintenanceService.Context.User);

            RegisterWindowsApp();

            RegisterService();
        }
    }
}