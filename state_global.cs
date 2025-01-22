using System;
using System.Windows.Threading;

#pragma warning disable IDE0079
#pragma warning disable CS8625

namespace Stimulator
{
    internal static class UI
    {
        internal static Dispatcher Dispatcher = null;
    }

    internal static class RunContextInfo
    {
        internal static String ExecutablePath = null;

        internal struct Windows
        {
            internal static UInt32 MajorVersion = 0;
            internal static UInt32 MinorVersion = 0;

            internal static Boolean IsServer = false;

            internal static Boolean IsHomeEdition = false;

            internal static String HostName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            internal static String NetBiosHostname = Environment.MachineName;

            internal static Boolean IsDomainJoined = false;
            internal static String Domain = null;

            internal static String Username = null;
            internal static String UserHomePath = null;

            internal static String AdministratorGroupName = null;

            internal static Boolean IsUEFI = false;
            internal static Boolean SecureBootEnabled = false;
        }
    }

    // #####################################################################################

    
}