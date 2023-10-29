using System;

namespace WinUtil
{
    internal static class Machine
    {
        internal static HostRole Role;

        internal static String OSEdition = null;

        internal static Int32 OSMajorVersion = 0;

        internal static Int32 OSMinorVersion = 0;

        internal static OSPlatformFeatureCompliance WindowPlatformFeatureCompliance;

        internal static String NetBiosHostname = null;

        internal static String Hostname = null;

        internal static WindowsUIVersion UIVersion;

        internal static Boolean IsUEFI = false;

        internal static Boolean SecureBootEnabled = false;

        internal static Boolean IsInDomain = false;

        //# # # # # # # # # # # # # # # # # # # # # # # # #

        internal static String AdminGroupName = null;

        internal static String ExePath = null;

        internal static Boolean? IsActivated = null;

        internal static String User = null;

        internal static String UserPath = null;

        //# # # # # # # # # # # # # # # # # # # # # # # # #

        internal enum OSPlatformFeatureCompliance
        {
            Windows10_or_Older = 0,

            Windows11_Server2022 = 1,
        }

        internal enum HostRole
        {
            Server = 0,

            Client = 1,
        }

        internal enum WindowsUIVersion
        {
            Windows10 = 0,

            Windows11 = 1,
        }
    }
}