using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace HyperKey_UnReg
{
    public static class UnReg
    {
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

        private static void Main()
        {
            Byte[] KeysToUnregister = new[] { vKEY_W, vKEY_T, vKEY_Y, vKEY_O, vKEY_P, vKEY_D, vKEY_L, vKEY_X, vKEY_N, vKEY_SPACE };

            Process("explorer.exe", ProcessAction.Kill);

            for (Byte b = 0; b < 10; b++)
            {
                RegisterHotKey(IntPtr.Zero, b, 0x1 + 0x2 + 0x4 + 0x8 | 0x4000, KeysToUnregister[b]);
            }

            RegisterHotKey(IntPtr.Zero, 10, 0x8 | 0x4000, vKEY_C);

            Process("C:\\Windows\\explorer.exe", ProcessAction.Start);

            Thread.Sleep(1024);

            for (Byte ID = 0; ID < 11; ++ID)
            {
                UnregisterHotKey(IntPtr.Zero, ID);
            }

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\ms-officeapp\\Shell\\Open\\Command", "", "rundll32", RegistryValueKind.String);
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        enum ProcessAction
        {
            Start = 0,
            Kill = 1
        }

        private static void Process(String PName, ProcessAction Action)
        {
            Process process = new();

            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;

            if (Action == ProcessAction.Kill)
            {
                process.StartInfo.FileName = "C:\\Windows\\System32\\taskkill.exe";
                process.StartInfo.Arguments = $"/IM {PName} /F";

                process.Start();
                process.WaitForExit();
            }
            else
            {
                process.StartInfo.FileName = PName;

                process.Start();
            }
        }
    }
}