using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Deregisterer
{
    internal static partial class MaintenanceService
    {
        internal const String USERINIT_VALUE_PATH = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon";
        internal const String USERINIT_VALUE_NAME = "Userinit";

        internal const String EXECUTABLE_REGEX = $",\\s*\"C:\\\\Program Files\\\\{Installer.INSTALLATION_DIRECTORY_NAME}\\\\{Installer.EXECUTABLE_NAME_WITHOUT_EXTENSION}\\.exe\"(\\s*,|\\s*$)";
        internal const String END_SEMICOLON_REGEX = ",+\\s*$";

        private static Boolean UserInitIsValid()
        {
            Object rawUserInitString = Registry.GetValue(USERINIT_VALUE_PATH, USERINIT_VALUE_NAME, null);

            if (rawUserInitString == null) throw new Exception("User init string not found!");
            if (rawUserInitString is not String) throw new Exception("User init had wrong type!");

            if (Regex.Match((String)rawUserInitString, EXECUTABLE_REGEX, RegexOptions.IgnoreCase).Success) return true;
            else return false;
        }

        internal static void FixUserInit()
        {
            Object rawUserInitString = Registry.GetValue(USERINIT_VALUE_PATH, USERINIT_VALUE_NAME, null);

            if (rawUserInitString == null) throw new Exception("User init string not found!");
            if (rawUserInitString is not String) throw new Exception("User init had wrong type!");

            String userInitString = (String)rawUserInitString;

            // if ends with ',' or ',,' etc
            if (Regex.Match(userInitString, END_SEMICOLON_REGEX, RegexOptions.IgnoreCase).Success)
            {
                userInitString = Regex.Replace(userInitString, END_SEMICOLON_REGEX, $", \"C:\\Program Files\\{Installer.INSTALLATION_DIRECTORY_NAME}\\{Installer.EXECUTABLE_NAME}\",", RegexOptions.IgnoreCase);
            }
            else // if no , was found
            {
                userInitString += $", \"C:\\Program Files\\{Installer.INSTALLATION_DIRECTORY_NAME}\\{Installer.EXECUTABLE_NAME}\",";
            }

            Registry.SetValue(USERINIT_VALUE_PATH, USERINIT_VALUE_NAME, userInitString);
        }
    }
}