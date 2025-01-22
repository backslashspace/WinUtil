using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Deregisterer
{
    internal static class Uninstaller
    {
        internal static void Uninstall()
        {
            Object rawUserInitString = Registry.GetValue(MaintenanceService.USERINIT_VALUE_PATH, MaintenanceService.USERINIT_VALUE_NAME, null);

            if (rawUserInitString == null) throw new Exception("User init string not found!");
            if (rawUserInitString is not String) throw new Exception("User init had wrong type!");

            String userInitString = Regex.Replace((String)rawUserInitString, MaintenanceService.EXECUTABLE_REGEX, ", ", RegexOptions.IgnoreCase);
            Registry.SetValue(MaintenanceService.USERINIT_VALUE_PATH, MaintenanceService.USERINIT_VALUE_NAME, userInitString);

            Tools.SC(Tools.SCAction.Delete);

            Registry.LocalMachine.DeleteSubKeyTree(Installer.REGISTRY_APP_TREE, false);

            Tools.Process(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", $"-c \"Start-Sleep 10; Remove-Item -Path '{Installer.INSTALLATION_DIRECTORY_PATH}' -Recurse:$true -Confirm:$false -Force:$true\"");
        }
    }
}