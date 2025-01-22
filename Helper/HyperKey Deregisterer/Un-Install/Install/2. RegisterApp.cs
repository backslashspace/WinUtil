using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Deregisterer
{
    internal static partial class Installer
    {
        private static void RegisterWindowsApp()
        {
            UInt32 estimatedSize = 0;

            try
            {
                DirectoryInfo directoryInfo = new(INSTALLATION_DIRECTORY_PATH);

                UInt64 size = 0;

                FileInfo[] fileInfos = directoryInfo.GetFiles();

                for (Int32 i = 0; i < fileInfos.Length; ++i)
                {
                    size += (UInt64)fileInfos[i].Length;
                }

                estimatedSize = unchecked((UInt32)(size / 1024));
            }
            catch
            {
                Environment.Exit(-1);
            }

            String displayIcon = $"{INSTALLATION_DIRECTORY_PATH}{EXECUTABLE_NAME}";
            String displayName = "Windows Hyper Key Remover";
            String displayVersion = FileVersionInfo.GetVersionInfo($"{INSTALLATION_DIRECTORY_PATH}{EXECUTABLE_NAME}").FileVersion;
            String publisher = "https://github.com/backslashspace";
            String uninstallString = $"\"{INSTALLATION_DIRECTORY_PATH}{EXECUTABLE_NAME}\" /uninstall";

            try
            {
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "DisplayIcon", displayIcon, RegistryValueKind.String);
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "DisplayName", displayName, RegistryValueKind.String);
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "DisplayVersion", displayVersion, RegistryValueKind.String);
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "EstimatedSize", unchecked((Int32)estimatedSize), RegistryValueKind.DWord);
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "NoModify", 1, RegistryValueKind.DWord);
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "NoRepair", 1, RegistryValueKind.DWord);
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "Publisher", publisher, RegistryValueKind.String);
                Registry.SetValue(REGISTRY_APP_FULL_PATH, "UninstallString", uninstallString, RegistryValueKind.String);
            }
            catch
            {
                Environment.Exit(-1);
            }
        }
    }
}