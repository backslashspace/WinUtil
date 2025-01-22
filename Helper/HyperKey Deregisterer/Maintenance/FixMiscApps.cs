using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Deregisterer
{
    internal static partial class MaintenanceService
    {
        internal enum Context : Int32
        {
            Machine = 0,
            User = 1
        }

        internal static void RegistryAssists(Context context)
        {
            if (context == Context.Machine)
            {
                // deactivate windows widgets (WIN + W)
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
            }
            else
            {
                // prevents office app from opening
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\ms-officeapp\\Shell\\Open\\Command", "", "rundll32", RegistryValueKind.String);

                // remove desktop 'more info about this picture'
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{2cc5ca98-6485-489a-920e-b3e88a6ccce3}", 1, RegistryValueKind.DWord);
            }
        }

        private static void SetHelpPaneAttributes()
        {
            try
            {
                String helpPanePath = "C:\\Windows\\HelpPane.exe";

                Tools.Process("C:\\Windows\\System32\\takeown.exe", $"/F {helpPanePath}");
                Tools.Process("C:\\Windows\\System32\\icacls.exe", $"{helpPanePath} /grant 'S-1-5-32-544':(F)");

                // explorer F1
                FileInfo file = new("C:\\Windows\\HelpPane.exe");
                AuthorizationRuleCollection accessRules = file.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));

                FileSecurity fileSecurity = file.GetAccessControl();
                List<FileSystemAccessRule> existsList = new();

                foreach (FileSystemAccessRule rule in existsList)
                {
                    fileSecurity.RemoveAccessRuleAll(rule);
                }

                file.SetAccessControl(fileSecurity);
            }
            catch
            {
                Environment.Exit(-3);
            }
        }
    }
}