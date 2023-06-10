using System;
using System.Text;
using System.Runtime.InteropServices;

namespace WinUtil_Main
{
    public class MessageBoxManager
    {
        private delegate IntPtr HookProc(Int32 nCode, IntPtr wParam, IntPtr lParam);
        private delegate  Boolean  EnumChildProc(IntPtr hWnd, IntPtr lParam);

        private const Int32 WH_CALLWNDPROCRET = 12;
        //private const Int32 WM_DESTROY = 0x0002;
        private const Int32 WM_INITDIALOG = 0x0110;
        //private const Int32 WM_TIMER = 0x0113;
        //private const Int32 WM_USER = 0x400;
        //private const Int32 DM_GETDEFID = WM_USER + 0;

        private const Int32 MBOK = 1;
        private const Int32 MBCancel = 2;
        private const Int32 MBAbort = 3;
        private const Int32 MBRetry = 4;
        private const Int32 MBIgnore = 5;
        private const Int32 MBYes = 6;
        private const Int32 MBNo = 7;


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(Int32 idHook, HookProc lpfn, IntPtr hInstance, Int32 threadId);

        [DllImport("user32.dll")]
        private static extern Int32 UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, Int32 nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLengthW", CharSet = CharSet.Unicode)]
        private static extern Int32 GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
        private static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder text, Int32 maxLength);

        [DllImport("user32.dll")]
        private static extern Int32 EndDialog(IntPtr hDlg, IntPtr nResult);

        [DllImport("user32.dll")]
        private static extern  Boolean  EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)]
        private static extern Int32 GetClassName(IntPtr hWnd, StringBuilder lpClassName, Int32 nMaxCount);

        [DllImport("user32.dll")]
        private static extern Int32 GetDlgCtrlID(IntPtr hwndCtl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, Int32 nIDDlgItem);

        [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode)]
        private static extern  Boolean  SetWindowText(IntPtr hWnd, String  lpString);


        [StructLayout(LayoutKind.Sequential)]
        public struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public UInt32 message;
            public IntPtr hwnd;
        };

        private static readonly HookProc hookProc;
        private static readonly EnumChildProc enumProc;
        [ThreadStatic]
        private static IntPtr hHook;
        [ThreadStatic]
        private static Int32 nButton;

        /// <summary>
        /// OK text
        /// </summary>
        public static String  OK = "&OK";
        /// <summary>
        /// Cancel text
        /// </summary>
        public static String  Cancel = "&Cancel";
        /// <summary>
        /// Abort text
        /// </summary>
        public static String  Abort = "&Abort";
        /// <summary>
        /// Retry text
        /// </summary>
        public static String  Retry = "&Retry";
        /// <summary>
        /// Ignore text
        /// </summary>
        public static String  Ignore = "&Ignore";
        /// <summary>
        /// Yes text
        /// </summary>
        public static String  Yes = "&Yes";
        /// <summary>
        /// No text
        /// </summary>
        public static String  No = "&No";

        static MessageBoxManager()
        {
            hookProc = new HookProc(MessageBoxHookProc);
            enumProc = new EnumChildProc(MessageBoxEnumProc);
            hHook = IntPtr.Zero;
        }

        /// <summary>
        /// Enables MessageBoxManager functionality
        /// </summary>
        /// <remarks>
        /// MessageBoxManager functionality is enabled on current thread only.
        /// Each thread that needs MessageBoxManager functionality has to call this method.
        /// </remarks>
        public static void Register()
        {
            if (hHook != IntPtr.Zero)
                throw new NotSupportedException("One hook per thread allowed.");
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, hookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
#pragma warning restore CS0618 // Typ oder Element ist veraltet
        }

        /// <summary>
        /// Disables MessageBoxManager functionality
        /// </summary>
        /// <remarks>
        /// Disables MessageBoxManager functionality on current thread only.
        /// </remarks>
        public static void Unregister()
        {
            if (hHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hHook);
                hHook = IntPtr.Zero;
            }
        }

        private static IntPtr MessageBoxHookProc(Int32 nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return CallNextHookEx(hHook, nCode, wParam, lParam);

            CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            IntPtr hook = hHook;

            if (msg.message == WM_INITDIALOG)
            {
                Int32 nLength = GetWindowTextLength(msg.hwnd);
                StringBuilder className = new(10);
                GetClassName(msg.hwnd, className, className.Capacity);
                if (className.ToString() == "#32770")
                {
                    nButton = 0;
                    EnumChildWindows(msg.hwnd, enumProc, IntPtr.Zero);
                    if (nButton == 1)
                    {
                        IntPtr hButton = GetDlgItem(msg.hwnd, MBCancel);
                        if (hButton != IntPtr.Zero)
                            SetWindowText(hButton, OK);
                    }
                }
            }

            return CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private static  Boolean  MessageBoxEnumProc(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder className = new(10);
            GetClassName(hWnd, className, className.Capacity);
            if (className.ToString() == "Button")
            {
                Int32 ctlId = GetDlgCtrlID(hWnd);
                switch (ctlId)
                {
                    case MBOK:
                        SetWindowText(hWnd, OK);
                        break;
                    case MBCancel:
                        SetWindowText(hWnd, Cancel);
                        break;
                    case MBAbort:
                        SetWindowText(hWnd, Abort);
                        break;
                    case MBRetry:
                        SetWindowText(hWnd, Retry);
                        break;
                    case MBIgnore:
                        SetWindowText(hWnd, Ignore);
                        break;
                    case MBYes:
                        SetWindowText(hWnd, Yes);
                        break;
                    case MBNo:
                        SetWindowText(hWnd, No);
                        break;
                }
                nButton++;
            }

            return true;
        }
    }
}