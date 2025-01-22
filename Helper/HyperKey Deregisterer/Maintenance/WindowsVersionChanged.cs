using System;
using Microsoft.Win32;

namespace Deregisterer
{
    internal static partial class MaintenanceService
    {
        private const String WINDOWS_VERSION_PATH = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
        private const String PREVIOUS_VERSION_PATH = "HKEY_LOCAL_MACHINE\\SOFTWARE\\HyperKey-Deregisterer";
        private const String PREVIOUS_VERSION_VALUE_NAME = "PreviousVersionInfo";

        private static void ThrowUp() => throw new InvalidCastException("Windows version information in registry had wrong format??");

        private static Boolean WindowsVersionChanged()
        {
            Object rawCurrentBuildNumber = Registry.GetValue(WINDOWS_VERSION_PATH, "CurrentBuildNumber", null);
            Object rawUBR = Registry.GetValue(WINDOWS_VERSION_PATH, "UBR", null);
            Object rawPreviousVersionInfo = Registry.GetValue(PREVIOUS_VERSION_PATH, PREVIOUS_VERSION_VALUE_NAME, null);

            if (rawCurrentBuildNumber is not String) ThrowUp();
            if (!Int64.TryParse((String)rawCurrentBuildNumber, out Int64 currentBuildNumber)) ThrowUp();

            if (rawUBR is not Int32) ThrowUp();
            Int64 ubr = (Int64)(Int32)rawUBR;

            //

            if (rawPreviousVersionInfo is null or not Byte[])
            {
                SetPreviousVersionInfo(currentBuildNumber, ubr);
                return true;
            }

            Int64 storedCurrentBuildNumber = BitConverter.ToInt64((Byte[])rawPreviousVersionInfo, 0);
            Int64 storedUBR = BitConverter.ToInt64((Byte[])rawPreviousVersionInfo, 8);

            if (storedUBR != ubr || storedCurrentBuildNumber != currentBuildNumber)
            {
                SetPreviousVersionInfo(currentBuildNumber, ubr);
                return true;
            }

            return false;
        }

        private static void SetPreviousVersionInfo(Int64 currentBuildNumber, Int64 ubr)
        {
            Byte[] previousVersionInfoBuffer = new Byte[16];

            Buffer.BlockCopy(BitConverter.GetBytes(currentBuildNumber), 0, previousVersionInfoBuffer, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(ubr), 0, previousVersionInfoBuffer, 8, 8);

            Registry.SetValue(PREVIOUS_VERSION_PATH, PREVIOUS_VERSION_VALUE_NAME, previousVersionInfoBuffer, RegistryValueKind.Binary);
        }
    }
}