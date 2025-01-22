using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Deregisterer
{
    internal static partial class Program
    {
        private const UInt32 MOD_ALT = 0x0001;
        private const UInt32 MOD_CONTROL = 0x0002;
        private const UInt32 MOD_SHIFT = 0x0004;
        private const UInt32 MOD_WIN = 0x0008;
        private const UInt32 MOD_NOREPEAT = 0x4000;

        private const Byte NO_VK = 0x0;
        private const Byte vKEY_C = 0x43;
        private const Byte vKEY_W = 0x57;
        private const Byte vKEY_T = 0x54;
        private const Byte vKEY_Y = 0x59;
        private const Byte vKEY_O = 0x4F;
        private const Byte vKEY_P = 0x50;
        private const Byte vKEY_D = 0x44;
        private const Byte vKEY_L = 0x4C;
        private const Byte vKEY_X = 0x58;
        private const Byte vKEY_N = 0x4E;
        private const Byte vKEY_SPACE = 0x20;

        [DllImport("User32.dll")]
        private static extern void RegisterHotKey(IntPtr hwnd, Int32 id, UInt32 fsModifiers, UInt32 vk);

        [DllImport("User32.dll")]
        private static extern void UnregisterHotKey(IntPtr hwnd, Int32 id);

        // #################################################################################################################

        private static void Unregister()
        {
            MaintenanceService.RegistryAssists(MaintenanceService.Context.User);

            // hyper keys
            Byte[] keys = [NO_VK, vKEY_W, vKEY_T, vKEY_Y, vKEY_O, vKEY_P, vKEY_D, vKEY_L, vKEY_X, vKEY_N, vKEY_SPACE];

            KillExplorer();

            for (Byte b = 0; b < keys.Length; b++)
            {
                RegisterHotKey(IntPtr.Zero, b, (MOD_ALT + MOD_CONTROL + MOD_SHIFT + MOD_WIN) | MOD_NOREPEAT, keys[b]);
            }

            // teams
            RegisterHotKey(IntPtr.Zero, 11, MOD_WIN | MOD_NOREPEAT, vKEY_C);

            // widgets
            RegisterHotKey(IntPtr.Zero, 12, MOD_WIN | MOD_NOREPEAT, vKEY_W);

            StartExplorer();

            Thread.Sleep(1024);

            // 12 is count of keys
            for (Byte id = 0; id < 13; ++id)
            {
                UnregisterHotKey(IntPtr.Zero, id);
            }
        }

        // #################################################################################################################

        private static void KillExplorer()
        {
            Process process = new();

            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "C:\\Windows\\System32\\taskkill.exe";
            process.StartInfo.Arguments = "/IM explorer.exe /F";

            process.Start();
            process.WaitForExit();
        }

        private static void StartExplorer()
        {
            Process process = new();

            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "C:\\Windows\\explorer.exe";

            process.Start();
        }
    }
}