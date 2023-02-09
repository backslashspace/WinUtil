using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Security;
using System.Management;
using System.ComponentModel;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Management.Automation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using WinUtil.Properties;
using System.Net;
//using ManagedNativeWifi;

namespace WinUtil
{
    public partial class MainWindow : Window
    {
        public static class SetServiceStartType
        {
            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern Boolean ChangeServiceConfig(
                IntPtr hService,
                UInt32 nServiceType,
                UInt32 nStartType,
                UInt32 nErrorControl,
                String lpBinaryPathName,
                String lpLoadOrderGroup,
                IntPtr lpdwTagId,
                [In] char[] lpDependencies,
                String lpServiceStartName,
                String lpPassword,
                String lpDisplayName);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern IntPtr OpenService(
                IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr OpenSCManager(
                string machineName, string databaseName, uint dwAccess);

            [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
            public static extern int CloseServiceHandle(IntPtr hSCObject);

            private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
            private const uint SERVICE_QUERY_CONFIG = 0x00000001;
            private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
            private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

            public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
            {
                var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
                if (scManagerHandle == IntPtr.Zero)
                {
                    throw new ExternalException("Open Service Manager Error");
                }

                try
                {
                    var serviceHandle = OpenService(
                        scManagerHandle,
                        svc.ServiceName,
                        SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

                    if (serviceHandle == IntPtr.Zero)
                    {
                        throw new ExternalException("Open Service Error");
                    }

                    var result = ChangeServiceConfig(
                        serviceHandle,
                        SERVICE_NO_CHANGE,
                        (uint)mode,
                        SERVICE_NO_CHANGE,
                        null,
                        null,
                        IntPtr.Zero,
                        null,
                        null,
                        null,
                        null);

                    if (result == false)
                    {
                        int nError = Marshal.GetLastWin32Error();
                        var win32Exception = new Win32Exception(nError);
                        throw new ExternalException("Could not change service start type: "
                            + win32Exception.Message);
                    }

                    CloseServiceHandle(serviceHandle);
                    CloseServiceHandle(scManagerHandle);

                }
                catch (System.InvalidOperationException)
                {

                }
            }
        }
        public class Const
        {
            public const string FirefoxImageName = "Firefox Setup 109.0.1.exe";
            public const string FirefoxImageHash = "bcce91389b3c2f91871589016e565e4c57b480e1c67c13ad2af24b7d19cf0b42";
            public const string FSH41 = "6ebaa64f760ab886ca21436c96d7ec1e8e8fda87de787d625a733af147946ff2";
            public const string FSH31 = "4ab65f6689aaadb5b424ff01b0831fbfe934aedf4d5e7be36b8db2314e7a2e09";
            public const string FSH30 = "33d71e18a599864dc7aedcb951790d9e04e675340bd330c6f2057a804170dda4";
            public const string FSH21 = "b45cc85bd6be76f5133860f97631cdc95275d4e0c39699e60bc2c8d0a7130b58";
            public const string FSH20 = "52a431827e3e3b2f57eedc3749e2d2e799f6de2b6b3974d9f6617dda28bca3b0";
            public const string FSH11 = "a6ab07ce3251a2b4eca183e7ea32b2a48529ebd5194c4ec7061cf3f7d548bb18";
            public const string FSH10 = "e6643eeb74602e883ad26ae71e64555c2d760b5d27f420572d044c54741a6e71";
            public const string FSH00 = "dfc03623cb52de39daca1baea32024a7d5fa9e7e3efdfbf2a0122a2236d922d9";

            public const string CurBlack = "453a6f4eec2396d9e14a1c0a64c39bb9e96ec7523bf886ba2130d3165302ee19";
            public const string CurBlackMono = "fb72e7e8f9dbc27539ec98193d82cf2c1904c4089153117a98fb39c7191971bc";
            public const string CurBlackHybrid = "11a46ef84853b2f6e9f5f1e99d889be3d223aafc4162d8e1b88f9eb79485d659";
            public const string CurDef = "3b1e6fac6fc1bab4e8d19394b0dd5344077a0aa6899e093e569a20c0dfeeacb2";
            public const string CurDefMono = "e173444a7b01c124a9772b2895bfa23ca1a6068c41512a11756dfea052ebec44";
            public const string CurDefHybrid = "cc4129dce37027d2beed40e57cb51633ff64a93fe78e5ae88bfb7ee7a052fc1f";

            public const string VCLibs = "9bfde6cfcc530ef073ab4bc9c4817575f63be1251dd75aaa58cb89299697a569";
            public const string VCLibsName = "Microsoft.VCLibs.140.00.appx";

            public const string WT = "6c477c3ef5acc64114e76ee1a31f4995638120076669adf84996326b12371e91";
            public const string WTName = "5f46166a368a44909385945e4262010d.msixbundle";

            public const string WinGetName = "Win-Get.1.4.10173.msixbundle";
            public const string WinGetHash = "091899dff6a9b4d7ff364e03e408cfc1efac382ddfd945e69e952bb364e2b208";
            public const string Xaml27Name = "Microsoft.UI.Xaml.2.7.appx";
            public const string Xaml27Hash = "efa0b1396a134a654c41bd9f4b9f084a5dfe19ad6d492cd99313b0b898a0767f";

            public const string su10exe = "OOSU10-v1.9.1434.exe";
            public const string su10exeHash = "d2b99302d0acdf47256a654c28d20f9730175125d8c66bee2074c7357a8f50e2";
            public const string su10settings = "Settings-v1.9.1434.v2.cfg";
            public const string su10settingsHash = "ff580efdf26b1f7c235125663b52769e6a6e784a443b62fa286cc7955320913d";

            public const string ACLHash = "4efc87b7e585fcbe4eaed656d3dbadaec88beca7f92ca7f0089583b428a6b221";

            public const string Nano = "8b31e1e10f3383ff6ca49e4f65e738805015351984ad67517448e3e7c53c43a2";

            public const string zip7exe = "254cf6411d38903b2440819f7e0a847f0cfee7f8096cfad9e90fea62f42b0c23";
            public const string zip7dll = "73578f14d50f747efa82527a503f1ad542f9db170e2901eddb54d6bce93fc00e";

            public const string zip7installHash = "b055fee85472921575071464a97a79540e489c1c3a14b9bdfbdbab60e17f36e4";
            public const string zip7installName = "7z2201-x64.exe";

            public const string vc64Hash = "ce6593a1520591e7dea2b93fd03116e3fc3b3821a0525322b0a430faa6b3c0b4";
            public const string vc86Hash = "cf92a10c62ffab83b4a2168f5f9a05e5588023890b5c0cc7ba89ed71da527b0f";

            public const string javaName = "jdk-17.0.5_windows-x64_bin.exe";
            public const string javaHash = "6f5f3ff459f4f5a35866622b2de6638c16eaad7d52c89a31d846a435bda39621";
        }
        public class ThreadIsAlive
        {
            public static bool ActivateWindows { get; set; }
            public static bool LoginBlur { get; set; }
            public static bool General { get; set; }
            public static bool N0Telemetry { get; set; }
            public static bool Curser { get; set; }
            public static bool SystemTheme { get; set; }
            public static bool Restart_Explorer { get; set; }
            public static bool WT { get; set; }
            public static bool Debloat { get; set; }
            public static bool ClearTaskBar { get; set; }
            public static bool RemoveXbox { get; set; }
            public static bool RMOneDrive { get; set; }
            public static bool AppFilehistory { get; set; }
            public static bool Cmen { get; set; }
            public static bool Ribbon { get; set; }
            public static bool Nano { get; set; }
            public static bool RMEdge { get; set; }
            public static bool BootStuff { get; set; }
            public static bool NumLock { get; set; }
            public static bool BootSound { get; set; }
            public static bool Install { get; set; }
            public static bool SMB { get; set; }
            public static bool LockScreen { get; set; }
            public static bool LockScreenNotifications { get; set; }
            public static bool UAC { get; set; }
            public static bool Pagefile { get; set; }
            public static bool RequireCtrl { get; set; }
            public static bool AutoLogin { get; set; }
            public static bool Harden { get; set; }
            public static bool ApplicationGuard { get; set; }
            public static bool VBS { get; set; }
            public static bool USBExecution { get; set; }
            public static bool CFG { get; set; }
            public static bool BMP { get; set; }
            public static bool MaxFailedLoginAttempts { get; set; }
            public static bool ScreenTimeOut { get; set; }
            public static bool SystemLock { get; set; }
            public static bool SystemCheck { get; set; }
            public static bool IOWLanConfig { get; set; }
            public static bool WebView { get; set; }
            public static bool WGetAction { get; set; }
            public static bool Backgroundapps { get; set; }
            public static bool WindowsUpdate { get; set; }
        }

        //#######################################################################################################################
        public static string AdminGroupName { get; set; }
        public static string ExePath { get; set; }
        public static bool IsWin11OrNewer { get; set; }
        public static bool IsServer { get; set; }
        public static bool IsUEFI { get; set; }
        public static bool IsActivated { get; set; }
        public static string SessionUser { get; set; }
        public static bool AllProfilesWLan { get; set; }
        public static short ActivationClicks { get; set; }

        //#######################################################################################################################

        public static string GetServiceStatus(string servicename)
        {
            ServiceController sc = new ServiceController(servicename);

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    return "running";
                case ServiceControllerStatus.Stopped:
                    return "stopped";
                case ServiceControllerStatus.Paused:
                    return "paused";
                case ServiceControllerStatus.StopPending:
                    return "stopping";
                case ServiceControllerStatus.StartPending:
                    return "starting";
                default:
                    return "status changing";
            }
        }

        public static bool InternetIsAvailable()
        {
            try
            {
                foreach (IPAddress theaddress in Dns.GetHostAddresses("1dot1dot1dot1.cloudflare-dns.com")) {}
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static int WinGetIsInstalled(bool InstallOnMissing)
        {
            //returns 0 when not found
            //returns 1 when found / after installing
            //returns 2 when install fails
            try
            {
                Start("winget.exe", "", true, true);

                return 1;
            }
            catch (Exception)
            {
                if (InstallOnMissing)
                {
                    Write("Installing WinGet..\n", "darkgreen", null);

                    if (CompareHash256("assets\\" + Const.VCLibsName, Const.VCLibs, true)[0] == 1 && CompareHash256("assets\\WinGet\\" + Const.WinGetName, Const.WinGetHash, true)[0] == 1 && CompareHash256("assets\\WinGet\\" + Const.Xaml27Name, Const.Xaml27Hash, true)[0] == 1)
                    {
                        PowerShell.Create().AddCommand("Add-AppxPackage")
                            .AddParameter("-path", "assets\\" + Const.VCLibsName)
                            .Invoke();

                        PowerShell.Create().AddCommand("Add-AppxPackage")
                            .AddParameter("-path", "assets\\WinGet\\" + Const.Xaml27Name)
                            .Invoke();

                        PowerShell.Create().AddCommand("Add-AppxPackage")
                            .AddParameter("-path", "assets\\WinGet\\" + Const.WinGetName)
                            .Invoke();

                        try
                        {
                            Start("winget.exe", null, true, true);

                            Write("Done\n\n", "darkcyan", null);

                            return 1;
                        }
                        catch (Exception)
                        {
                            Write("Error\n\n", "red", null);

                            return 2;
                        }
                    }
                    else
                    {
                        Write("Nothing changed, skipping\n\n", "darkyellow", null);

                        return 2;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public static long SafeReadRegQDword(string pathpre, string path, string value, RegistryValueKind type, short notfoundmode)
        {
            ///not found mode
            //0 = no action
            //1 = del when wrong type
            //2 = replace with correct, empty value

            PrepareRegAccess(pathpre, path, value, type, notfoundmode);

            if (pathpre.ToLower() == "hklm")
            {
                pathpre = "HKEY_LOCAL_MACHINE\\";
            }
            else if (pathpre.ToLower() == "hkcu")
            {
                pathpre = "HKEY_CURRENT_USER\\";
            }

            if (type == RegistryValueKind.DWord)
            {
                try
                {
                    return (int)Registry.GetValue(pathpre + path, value, null);
                }
                catch (Exception ex) when (ex is System.ArgumentNullException || ex is System.NullReferenceException)
                {
                    return -1;
                }
            }
            else if (type == RegistryValueKind.QWord)
            {
                try
                {
                    return (long)Registry.GetValue(pathpre + path, value, null);
                }
                catch (Exception ex) when (ex is System.ArgumentNullException || ex is System.NullReferenceException)
                {
                    return -1;
                }
            }
            return -2;
        }

        public static string SafeReadRegString(string pathpre, string path, string value, short notfoundmode)
        {
            ///not found mode
            //0 = no action
            //1 = del when wrong type
            //2 = replace with correct, empty value

            PrepareRegAccess(pathpre, path, value, RegistryValueKind.String, notfoundmode);

            if (pathpre.ToLower() == "hklm")
            {
                pathpre = "HKEY_LOCAL_MACHINE\\";
            }
            else if (pathpre.ToLower() == "hkcu")
            {
                pathpre = "HKEY_CURRENT_USER\\";
            }

            try
            {
                return (string)Registry.GetValue(pathpre + path, value, null);
            }
            catch (Exception ex) when (ex is System.ArgumentNullException || ex is System.NullReferenceException)
            {
                return null;
            }
        }

        public static int PrepareRegAccess(string pathprefix, string path, string value, RegistryValueKind type, short replacewithtypeordelete)
        {
            ///correction
            //0 = no action
            //1 = del when wrong type
            //2 = replace with correct, empty value

            ///return
            // !! only hklm and hkcu atm !!
            //return 0 = value is missing
            //return 1 = existing value has target type
            //return 2 = wrong type value
            //return 3 = permission error

            RegistryKey key;

            if (pathprefix.ToLower() == "hklm")
            {
                key = Registry.LocalMachine.OpenSubKey(path, true);
            }
            else if (pathprefix.ToLower() == "hkcu")
            {
                key = Registry.CurrentUser.OpenSubKey(path, true);
            }
            else
            {
                return -1;
            }

            try
            {
                if (type == RegistryValueKind.String)
                {
                    if (key.GetValueKind(value) == RegistryValueKind.String)
                    {
                        return 1;
                    }
                    else if (replacewithtypeordelete == 2)
                    {
                        key.DeleteValue(value, false);
                        key.SetValue(value, "", RegistryValueKind.String);
                        return 1;
                    }
                    else if (replacewithtypeordelete == 1)
                    {
                        key.DeleteValue(value, false);
                        return 0;
                    }
                    return 2;
                }
                else if (type == RegistryValueKind.DWord)
                {

                    if (key.GetValueKind(value) == RegistryValueKind.DWord)
                    {
                        return 1;
                    }
                    else if (replacewithtypeordelete == 2)
                    {
                        key.DeleteValue(value, false);
                        key.SetValue(value, 0, type);
                        return 1;
                    }
                    else if (replacewithtypeordelete == 1)
                    {
                        key.DeleteValue(value, false);
                        return 0;
                    }
                    return 2;
                }
                else if (type == RegistryValueKind.QWord)
                {
                    if (key.GetValueKind(value) == RegistryValueKind.DWord)
                    {
                        return 1;
                    }
                    else if (replacewithtypeordelete == 2)
                    {
                        key.DeleteValue(value, false);
                        key.SetValue(value, 0, type);
                        return 1;
                    }
                    else if (replacewithtypeordelete == 1)
                    {
                        key.DeleteValue(value, false);
                        return 0;
                    }
                    return 2;
                }
            }
            catch (System.IO.IOException)
            {
                if (replacewithtypeordelete == 2)
                {
                    if (type == RegistryValueKind.DWord || type == RegistryValueKind.QWord)
                    {
                        key.SetValue(value, 0, type);
                        return 1;
                    }

                    key.SetValue(value, "", type);

                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                return 3;
            }

            //compatibility
            catch (Exception)
            {
                return 0;
            }
            //compatibility

            return -1;
        }

        public static void Write(string txt, string foreground, string background)
        {
            if (background == null) { }

            else if (background.ToLower() == "black")
            {
                Console.BackgroundColor = ConsoleColor.Black;
            }
            else if (background.ToLower() == "white")
            {
                Console.BackgroundColor = ConsoleColor.White;
            }
            else if (background.ToLower() == "red")
            {
                Console.BackgroundColor = ConsoleColor.Red;
            }
            else if (background.ToLower() == "magenta")
            {
                Console.BackgroundColor = ConsoleColor.Magenta;
            }
            else if (background.ToLower() == "cyan")
            {
                Console.BackgroundColor = ConsoleColor.Cyan;
            }
            else if (background.ToLower() == "green")
            {
                Console.BackgroundColor = ConsoleColor.Green;
            }
            else if (background.ToLower() == "gray")
            {
                Console.BackgroundColor = ConsoleColor.Gray;
            }
            else if (background.ToLower() == "blue")
            {
                Console.BackgroundColor = ConsoleColor.Blue;
            }
            else if (background.ToLower() == "darkblue")
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }
            else if (background.ToLower() == "darkcyan")
            {
                Console.BackgroundColor = ConsoleColor.DarkCyan;
            }
            else if (background.ToLower() == "darkgray")
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
            }
            else if (background.ToLower() == "darkgreen")
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            }
            else if (background.ToLower() == "darkmagenta")
            {
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
            }
            else if (background.ToLower() == "darkred")
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
            }
            else if (background.ToLower() == "darkyellow")
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
            }
            else if (background.ToLower() == "magenta")
            {
                Console.BackgroundColor = ConsoleColor.Magenta;
            }
            else if (background.ToLower() == "yellow")
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
            }
            //#######
            if (foreground == null) { }

            else if (foreground.ToLower() == "black")
            {
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (foreground.ToLower() == "white")
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (foreground.ToLower() == "red")
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (foreground.ToLower() == "magenta")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }
            else if (foreground.ToLower() == "cyan")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else if (foreground.ToLower() == "green")
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (foreground.ToLower() == "gray")
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (foreground.ToLower() == "blue")
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (foreground.ToLower() == "darkblue")
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
            }
            else if (foreground.ToLower() == "darkcyan")
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
            }
            else if (foreground.ToLower() == "darkgray")
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            else if (foreground.ToLower() == "darkgreen")
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            else if (foreground.ToLower() == "darkmagenta")
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            }
            else if (foreground.ToLower() == "darkred")
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else if (foreground.ToLower() == "darkyellow")
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            else if (foreground.ToLower() == "magenta")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }
            else if (foreground.ToLower() == "yellow")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.Write(txt);
            Console.ResetColor();
        }

        public static void Start(string program, string command, bool hiddenexecute, bool waitforexit)
        {
#pragma warning disable IDE0059
            Process proc = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
#pragma warning restore IDE0059
            if (hiddenexecute)
            {
                proc = new Process();
                info = new ProcessStartInfo()
                {
                    FileName = program,
                    Arguments = command,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas"
                };
                proc.StartInfo = info;
                proc.Start();
            }
            else
            {
                proc = new Process();
                info = new ProcessStartInfo()
                {
                    FileName = program,
                    Arguments = command,
                    UseShellExecute = true,
                    Verb = "runas"

                };
                proc.StartInfo = info;
                proc.Start();
            }

            if (waitforexit)
            {
                proc.WaitForExit();
            }
        }

        public static bool TestRegValuePresense(string key, string value)
        {
            try
            {
                if ((string)Registry.GetValue(key, value, null).ToString() == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex is System.ArgumentNullException || ex is System.NullReferenceException)
            {
                return false;
            }
        }

        public static void InternExtract(string path, string outfile, string embededfile)
        {
            string writepfilepath;

            if (path == null || path == "")
            {
                writepfilepath = outfile;
            }
            else
            {
                writepfilepath = path + outfile;
            }


            try
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinUtil." + embededfile);
                FileStream fileStream = new FileStream(writepfilepath, FileMode.Create);
                for (int i = 0; i < stream.Length; i++)
                    fileStream.WriteByte((byte)stream.ReadByte());
                fileStream.Close();
            }
            catch (UnauthorizedAccessException)
            {
                var attributes = File.GetAttributes(writepfilepath);
                if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    attributes &= ~FileAttributes.Hidden;
                    File.SetAttributes(writepfilepath, attributes);
                }
                InternExtract(path, outfile, embededfile);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path);
                InternExtract(path, outfile, embededfile);
            }

        }

        public static short[] CompareHash256(string file, string hash, bool verbose)
        {
            ///return
            // [2] {valide, presens}


            string[] temp = file.Split('\\');
            string efile = temp[temp.Length - 1];
            temp = temp.Skip(0).Take((temp.Length) - 1).ToArray();
            string rpath = string.Join("\\", temp);

            if (file.Contains('\\'))
            {
                rpath += "\\";
            }

            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        if ((BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty)).Equals(hash, StringComparison.OrdinalIgnoreCase))
                        {
                            return new short[] { 1, 1 };
                        }
                        else
                        {
                            if (verbose)
                            {
                                Write(".\\" + rpath, "gray", null);
                                Write(efile, "darkyellow", null);
                                Write(" ── ", "gray", null);
                                Write("Invalide Hash\n", "red", null);
                            }
                            return new short[] { 0, 1 };
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
            {
                if (verbose)
                {
                    Write(".\\" + rpath, "gray", null);
                    Write(efile, "darkyellow", null);
                    Write(" ── ", "gray", null);
                    Write("File missing\n", "red", null);
                }
                return new short[] { 0, 0 };
            }
        }

        //#######################################################################################################################

        public MainWindow()
        {
            //test for proper start
            try
            {
                if (!Environment.GetCommandLineArgs()[1].Equals("e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9"))
                {
                    Write("Direct start\n", "cyan", null);
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                Write("Direct start\n", "red", null);
            }

            ////// logo //////

            Write("\n  ██", "blue", null);
            Write("╗       ", "darkgray", null);

            Write("██", "blue", null);
            Write("╗", "darkgray", null);

            Write("██", "blue", null);
            Write("╗", "darkgray", null);
            Write("███", "blue", null);
            Write("╗  ", "darkgray", null);
            Write("██", "blue", null);
            Write("╗", "darkgray", null);

            Write("██", "red", null);
            Write("╗   ", "darkgray", null);
            Write("██", "red", null);
            Write("╗", "darkgray", null);

            Write("████████", "green", null);
            Write("╗", "darkgray", null);

            Write("██", "cyan", null);
            Write("╗", "darkgray", null);

            Write("██", "darkyellow", null);
            Write("╗\n", "darkgray", null);

            //

            Write("  ██", "blue", null);
            Write("║  ", "darkgray", null);
            Write("██", "blue", null);           //W
            Write("╗  ", "darkgray", null);
            Write("██", "blue", null);
            Write("║", "darkgray", null);

            Write("██", "blue", null);           //I
            Write("║", "darkgray", null);

            Write("████", "blue", null);
            Write("╗ ", "darkgray", null);
            Write("██", "blue", null);         //N
            Write("║", "darkgray", null);

            Write("██", "red", null);
            Write("║   ", "darkgray", null);
            Write("██", "red", null);            //U
            Write("║", "darkgray", null);

            Write("╚══", "darkgray", null);       //T
            Write("██", "green", null);
            Write("╔══╝", "darkgray", null);

            Write("██", "cyan", null);
            Write("║", "darkgray", null);        //I

            Write("██", "darkyellow", null);         //L
            Write("║\n", "darkgray", null);

            //

            Write("  ╚", "darkgray", null);
            Write("██", "blue", null);           //W
            Write("╗", "darkgray", null);
            Write("████", "blue", null);
            Write("╗", "darkgray", null);
            Write("██", "blue", null);
            Write("╔╝", "darkgray", null);

            Write("██", "blue", null);               //I
            Write("║", "darkgray", null);

            Write("██", "blue", null);
            Write("╔", "darkgray", null);
            Write("██", "blue", null);               //N
            Write("╗", "darkgray", null);
            Write("██", "blue", null);
            Write("║", "darkgray", null);

            Write("██", "red", null);
            Write("║   ", "darkgray", null);
            Write("██", "red", null);                //U
            Write("║   ", "darkgray", null);

            Write("██", "green", null);
            Write("║   ", "darkgray", null);       //T

            Write("██", "cyan", null);
            Write("║", "darkgray", null);            //I

            Write("██", "darkyellow", null);              //L
            Write("║\n", "darkgray", null);

            //

            Write("   ████", "blue", null);           //W
            Write("╔═", "darkgray", null);
            Write("████", "blue", null);
            Write("║ ", "darkgray", null);

            Write("██", "blue", null);               //I
            Write("║", "darkgray", null);

            Write("██", "blue", null);               //N
            Write("║╚", "darkgray", null);
            Write("████", "blue", null);
            Write("║", "darkgray", null);

            Write("██", "red", null);
            Write("║   ", "darkgray", null);
            Write("██", "red", null);                //U
            Write("║   ", "darkgray", null);

            Write("██", "green", null);
            Write("║   ", "darkgray", null);       //T

            Write("██", "cyan", null);
            Write("║", "darkgray", null);            //I

            Write("██", "darkyellow", null);              //L
            Write("║\n", "darkgray", null);

            //

            Write("   ╚", "darkgray", null);           //W
            Write("██", "blue", null);
            Write("╔╝ ╚", "darkgray", null);
            Write("██", "blue", null);
            Write("╔╝ ", "darkgray", null);

            Write("██", "blue", null);               //I
            Write("║", "darkgray", null);

            Write("██", "blue", null);               //N
            Write("║ ╚", "darkgray", null);
            Write("███", "blue", null);
            Write("║", "darkgray", null);

            Write("╚", "darkgray", null);
            Write("██████", "red", null);                //U
            Write("╔╝   ", "darkgray", null);

            Write("██", "green", null);
            Write("║   ", "darkgray", null);       //T

            Write("██", "cyan", null);
            Write("║", "darkgray", null);            //I

            Write("███████", "darkyellow", null);              //L
            Write("╗\n", "darkgray", null);

            //

            Write("    ╚═╝   ╚═╝  ╚═╝╚═╝  ╚══╝ ╚═════╝    ╚═╝   ╚═╝╚══════╝ \n\n", "darkgray", null);

            ////// logo end ////// 

            //check if system is activated [set reg value]
            if (SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "Windows Activation Status", RegistryValueKind.DWord, 1) == -1)
            {
                Start("cmd.exe", "/c powershell -command \"$test = Get-CimInstance SoftwareLicensingProduct -Filter \\\"Name like 'Windows%'\\\" | where { $_.PartialProductKey } | select LicenseStatus; $test = $test -replace \\\"@{LicenseStatus=\\\"; $test = $test -replace \\\"}\\\"; reg add \\\"HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil\\\" /v \\\"Windows Activation Status\\\" /t reg_dword /d $test /f\"", true, false);
            }

            //check check systemtype [set reg value]
            if (SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "IsUEFI", RegistryValueKind.DWord, 1) == -1)
            {
                Start("cmd.exe", "/c powershell -command \"if ($env:firmware_type -eq \\\"UEFI\\\") {reg add \\\"HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil\\\" /v \\\"IsUEFI\\\" /t reg_dword /d 1 /f} else {reg add \\\"HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil\\\" /v \\\"IsUEFI\\\" /t reg_dword /d 0 /f}\"", true, true);
            }

            //get system type in memory
            if (SafeReadRegQDword("hklm", "SYSTEM\\WinUtil", "IsUEFI", RegistryValueKind.DWord, 1) == 1)
            {
                IsUEFI = true;
                Write("Detected UEFI\n\n", "cyan", null);
            }
            else
            {
                IsUEFI = false;
                Write("Detected legacy Bios\n\n", "cyan", null);
            }

            //install GPO on Home systems
            if (SafeReadRegString("hklm", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "EditionID", 1) == "Core")
            {
                ThreadStart childref = new ThreadStart(Functions.InstallGPO);
                Thread childThread = new Thread(childref);
                childThread.Start();
            }

            //gets local admingroup name
            string temp = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount)).Value;
            string[] temp2 = temp.Split('\\');
            AdminGroupName = temp2[1];

            //gets exepath
            ExePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            //check os details
            if (SafeReadRegString("hklm", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallationType", 1).ToLower().Contains("server"))
            {
                IsServer = true;

                Write("Detected Windows Server\n\n", "cyan", null);
            }

            //      🥄
            if (TestRegValuePresense("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber") && !IsServer)
            {

                if (int.Parse(SafeReadRegString("hklm", "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", 1)) >= 22000)
                {
                    IsWin11OrNewer = true;
                    Write("Detected Windows 11 or newer\n\n", "cyan", null);
                }
            }
            else if (!IsServer)
            {
                Write("Systemversion information missing\n", "red", null);
                Write("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion \"CurrentBuildNumber\"\n", "darkgray", null);
            }

            //get current logon user    (ask if rdp)
            try
            {
                ManagementObjectSearcher search1 = new ManagementObjectSearcher("select * from win32_ComputerSystem");
                foreach (ManagementObject obj in search1.Get().Cast<ManagementObject>())
                {
                    SessionUser = (string)obj["UserName"];
                    temp2 = SessionUser.Split('\\');
                    SessionUser = temp2[1];
                }
            }
            catch (System.NullReferenceException)
            {
                Write("Detected remote session!\n", "darkyellow", null);
                Write("===== Local-Users =====\n", "darkgray", null);
                Functions.ListUsers();

                while (true)
                {
                    //get user from user
                    string user = Microsoft.VisualBasic.Interaction.InputBox("\n\nRemote session detected, please specify a username to continue.",
                               "Select User",
                               "",
                               0,
                               0);

                    bool UserIsValide = false;

                    if (user == "")
                    {
                        Write("Exiting", "red", null);

                        Environment.Exit(0);
                    }

                    string[] usernames = Functions.GetSystemUserList();

                    for (int i = 0; i < usernames.Length; i++)
                    {
                        if (user == usernames[i])
                        {
                            SessionUser = user;
                            UserIsValide = true;
                            break;
                        }
                    }

                    if (UserIsValide)
                    {
                        Write("Using user: " + user + "\n\n", "cyan", null);
                        
                        break;
                    }
                    else
                    {
                        Write("Invalide user\n\n", "darkyellow", null);
                    }
                }
            }

            //starts gui centered
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            //sets default dir box
            wlanconfigdir.Text = "C:\\Users\\" + SessionUser + "\\Desktop";
            AllProfilesWLan = true;
        }

        //#######################################################################################################################

        private void Restart_Explorer(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Restart_Explorer)
            {
                ThreadIsAlive.Restart_Explorer = true;
                new Thread(Functions.Restart_Explorer).Start();
            }
        }

        private void ActivateWindows(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.ActivateWindows)
            {
                ThreadIsAlive.ActivateWindows = true;
                new Thread(Functions.ActivateWindows).Start();
            }
        }

        private void General(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.General)
            {
                ThreadIsAlive.General = true;
                new Thread(Functions.General).Start();
            }
        }

        private void YesLoginBlur(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LoginBlur)
            {
                ThreadIsAlive.LoginBlur = true;
                new Thread(Functions.YesLoginBlur).Start();
            }
        }

        private void NoLoginBlur(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LoginBlur)
            {
                ThreadIsAlive.LoginBlur = true;
                new Thread(Functions.NoLoginBlur).Start();
            }
        }

        private void N0Telemetry(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.N0Telemetry)
            {
                ThreadIsAlive.N0Telemetry = true;
                new Thread(Functions.DeacTelemetry).Start();
            }
        }

        private void DarkMode(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemTheme)
            {
                ThreadIsAlive.SystemTheme = true;
                new Thread(Functions.DarkMode).Start();
            }
        }

        private void LightMode(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemTheme)
            {
                ThreadIsAlive.SystemTheme = true;
                new Thread(Functions.LightMode).Start();
            }
        }

        private void BetterWT(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WT)
            {
                ThreadIsAlive.WT = true;
                new Thread(Functions.BetterWT).Start();
            }
        }

        private void NormalWT(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WT)
            {
                ThreadIsAlive.WT = true;
                new Thread(Functions.NormalWT).Start();
            }
        }

        private void Debloat(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Debloat)
            {
                ThreadIsAlive.Debloat = true;
                new Thread(Functions.Debloat).Start();
            }
        }

        private void ClearTaskBar(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.ClearTaskBar)
            {
                ThreadIsAlive.ClearTaskBar = true;
                new Thread(Functions.ClearTaskBar).Start();
            }
        }

        private void RemoveXbox(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RemoveXbox)
            {
                ThreadIsAlive.RemoveXbox = true;
                new Thread(Functions.RemoveXbox).Start();
            }
        }

        private void RMOneDrive(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RMOneDrive)
            {
                ThreadIsAlive.RMOneDrive = true;
                new Thread(Functions.RMOneDrive).Start();
            }
        }

        private void NoAppFilehistory(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AppFilehistory)
            {
                ThreadIsAlive.AppFilehistory = true;
                new Thread(Functions.NoAppFilehistory).Start();
            }
        }

        private void YesAppFilehistory(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AppFilehistory)
            {
                ThreadIsAlive.AppFilehistory = true;
                new Thread(Functions.YesAppFilehistory).Start();
            }
        }

        private void LegacyCmen(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Cmen)
            {
                ThreadIsAlive.Cmen = true;
                new Thread(Functions.LegacyCmen).Start();
            }
        }

        private void DefaultCmen(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Cmen)
            {
                ThreadIsAlive.Cmen = true;
                new Thread(Functions.DefaultCmen).Start();
            }
        }

        private void LegacyRibbon(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Ribbon)
            {
                ThreadIsAlive.Ribbon = true;
                new Thread(Functions.LegacyRibbon).Start();
            }
        }

        private void RMEdge(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RMEdge)
            {
                ThreadIsAlive.RMEdge = true;
                new Thread(Functions.RMEdge).Start();
            }
        }

        private void DefaultRibbon(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Ribbon)
            {
                ThreadIsAlive.Ribbon = true;
                new Thread(Functions.DefaultRibbon).Start();
            }
        }

        private void SafeBoot(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootStuff)
            {
                ThreadIsAlive.BootStuff = true;
                new Thread(Functions.SafeBoot).Start();
            }
        }

        private void UEFIBoot(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootStuff)
            {
                ThreadIsAlive.BootStuff = true;
                new Thread(Functions.UEFIBoot).Start();
            }
        }

        private void ActivateNumLock(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.NumLock)
            {
                ThreadIsAlive.NumLock = true;
                new Thread(Functions.ActivateNumLock).Start();
            }
        }

        private void ActivateBootSound(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootSound)
            {
                ThreadIsAlive.BootSound = true;
                new Thread(Functions.ActivateBootSound).Start();
            }
        }

        private void DeactivateBootSound(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootSound)
            {
                ThreadIsAlive.BootSound = true;
                new Thread(Functions.DeactivateBootSound).Start();
            }
        }

        private void Nano(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Nano)
            {
                ThreadIsAlive.Nano = true;
                new Thread(Functions.Nano).Start();
            }
        }

        private void NotepadPlusPlus (object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WGetAction)
            {
                ThreadIsAlive.WGetAction = true;
                new Thread(Functions.NotepadPlusPlus).Start();
            }
        }

        private void ResetUpdate(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.ResetUpdate).Start();
            }
        }

        private void SecurityUpdatesOnly(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.SecurityUpdatesOnly).Start();
            }
        }

        private void DeactivateWindowsUpdate(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.DeactivateWindowsUpdate).Start();
            }
        }

        private void DeactivateNumLock(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.NumLock)
            {
                ThreadIsAlive.NumLock = true;
                new Thread(Functions.DeactivateNumLock).Start();
            }
        }

        private void PowerSettings(object sender, RoutedEventArgs e)
        {
            Start("powercfg.cpl", null, false, false);
        }

        private void NetworkSettings(object sender, RoutedEventArgs e)
        {
            Start("ncpa.cpl", null, false, false);
        }

        private void SystemSettings(object sender, RoutedEventArgs e)
        {
            Start("sysdm.cpl", null, false, false);
        }

        private void OptionalFeatures(object sender, RoutedEventArgs e)
        {
            if (!IsServer)
            {
                Start("OptionalFeatures.exe", null, false, false);
            }
            else
            {
                Start("ServerManager.exe", null, false, false);
            }
        }

        private void Firefox(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Install)
            {
                ThreadIsAlive.Install = true;
                ThreadStart childref = new ThreadStart(Functions.Firefox);
                Thread childThread = new Thread(childref);
                childThread.SetApartmentState(ApartmentState.STA);
                childThread.Start();
            }
            else
            {
                Write("\nWait for program to finish installing\n\n", "darkyellow", null);
            }
        }

        private void EncryptSMB(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SMB)
            {
                ThreadIsAlive.SMB = true;
                new Thread(Functions.EncryptSMB).Start();
            }
        }

        private void ActivateLockScreen(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreen)
            {
                ThreadIsAlive.LockScreen = true;
                new Thread(Functions.ActivateLockScreen).Start();
            }
        }

        private void ActivateLockScreenNotifications(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreenNotifications)
            {
                ThreadIsAlive.LockScreenNotifications = true;
                new Thread(Functions.ActivateLockScreenNotifications).Start();
            }
        }

        private void DeactivateLockScreenNotifications(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreenNotifications)
            {
                ThreadIsAlive.LockScreenNotifications = true;
                new Thread(Functions.DeactivateLockScreenNotifications).Start();
            }
        }

        private void ListUsers(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                ThreadStart childref = new ThreadStart(Functions.ListUsers);
                new Thread(Functions.ListUsers).Start();
            }
        }

        private void ActivateAutologin(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.ActivateAutologin).Start(new string[] { Username.Text, Password.Password });
                Username.Text = null;
                Password.Password = null;
            }
        }

        private void DeactivateAutologin(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.DeactivateAutologin).Start();
            }
        }

        private void HideUser(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.HideUser).Start(hideactionusername.Text);
                hideactionusername.Text = null;
            }
        }

        private void ShowUser(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.ShowUser).Start(hideactionusername.Text);
                hideactionusername.Text = null;
            }
        }

        private void DeactivateLockScreen(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreen)
            {
                ThreadIsAlive.LockScreen = true;
                new Thread(Functions.DeactivateLockScreen).Start();
            }
        }

        private void DeactivateEncryptSMB(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SMB)
            {
                ThreadIsAlive.SMB = true;
                new Thread(Functions.DeactivateEncryptSMB).Start();
            }
        }

        private void VerboseUAC(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.UAC)
            {
                ThreadIsAlive.UAC = true;
                new Thread(Functions.VerboseUAC).Start();
            }
        }

        private void DefaultUAC(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.UAC)
            {
                ThreadIsAlive.UAC = true;
                new Thread(Functions.DefaultUAC).Start();
            }
        }

        private void PagefileClear(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Pagefile)
            {
                ThreadIsAlive.Pagefile = true;
                new Thread(Functions.PagefileClear).Start();
            }
        }

        private void PagefileDefault(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Pagefile)
            {
                ThreadIsAlive.Pagefile = true;
                new Thread(Functions.PagefileDefault).Start();
            }
        }

        private void RequireCtrl(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RequireCtrl)
            {
                ThreadIsAlive.RequireCtrl = true;
                new Thread(Functions.RequireCtrl).Start();
            }
        }

        private void Backgroundappsno(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Backgroundapps)
            {
                ThreadIsAlive.Backgroundapps = true;
                new Thread(Functions.Backgroundappsno).Start();
            }
        }

        private void Backgroundappsyes(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Backgroundapps)
            {
                ThreadIsAlive.Backgroundapps = true;
                new Thread(Functions.Backgroundappsyes).Start();
            }
        }

        private void DontRequireCtrl(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RequireCtrl)
            {
                ThreadIsAlive.RequireCtrl = true;
                new Thread(Functions.DontRequireCtrl).Start();

            }
        }

        private void Zip7(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Install)
            {
                ThreadIsAlive.Install = true;
                new Thread(Functions.Zip7).Start();
            }
            else
            {
                Write("\nWait for program to finish installing\n\n", "darkyellow", null);
            }
        }

        private void VCRedist(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Install)
            {
                ThreadIsAlive.Install = true;
                new Thread(Functions.VCRedist).Start();
            }
            else
            {
                Write("\nWait for program to finish installing\n\n", "darkyellow", null);
            }
        }

        private void Java(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Install)
            {
                ThreadIsAlive.Install = true;
                new Thread(Functions.Java).Start();
            }
            else
            {
                Write("\nWait for program to finish installing\n\n", "darkyellow", null);
            }
        }

        private void Codecs(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Install)
            {
                ThreadIsAlive.Install = true;
                new Thread(Functions.Codecs).Start();
            }
            else
            {
                Write("\nWait for program to finish installing\n\n", "darkyellow", null);
            }
        }

        private void Harden(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Harden)
            {
                ThreadIsAlive.Harden = true;
                new Thread(Functions.Harden).Start();
            }
        }

        //private void ActivateApplicationGuard(object sender, RoutedEventArgs e)
        //{
        //    if (SafeReadRegString("hklm", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "EditionID", 1) == "Core")
        //    {
        //        ThreadIsAlive.ApplicationGuard = true;
        //        var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Windows-Defender-ApplicationGuard\nnot supported on Windows Home editions",
        //        "",
        //        MessageBoxButtons.OK,
        //        MessageBoxIcon.Error);
        //        return;
        //    }
        //    if (!ThreadIsAlive.ApplicationGuard)
        //    {
        //        ThreadIsAlive.ApplicationGuard = true;
        //        new Thread(Functions.ActivateApplicationGuard).Start();
        //        return;
        //    }
        //    else
        //    {
        //        Write("Application Guard feature change pending, please restart\n\n", "darkyellow", null);
        //        return;
        //    }
        //}

        //private void DeactivateApplicationGuard(object sender, RoutedEventArgs e)
        //{
        //    if (SafeReadRegString("hklm", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "EditionID", 1) == "Core")
        //    {
        //        ThreadIsAlive.ApplicationGuard = true;
        //        var result0 = System.Windows.Forms.MessageBox.Show(
        //        "Windows-Defender-ApplicationGuard\nnot supported on Windows Home editions",
        //        "",
        //        MessageBoxButtons.OK,
        //        MessageBoxIcon.Error);
        //        return;
        //    }
        //    if (!ThreadIsAlive.ApplicationGuard)
        //    {
        //        ThreadIsAlive.ApplicationGuard = true;
        //        new Thread(Functions.DeactivateApplicationGuard).Start();
        //    }
        //    else
        //    {
        //        Write("Application Guard feature change pending, please restart\n\n", "darkyellow", null);
        //    }
        //}

        //private void DeactivateVBS(object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.VBS)
        //    {
        //        ThreadIsAlive.VBS = true;
        //        new Thread(Functions.DeactivateVBS).Start();
        //    }
        //}

        //private void ActivateVBS(object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.VBS)
        //    {
        //        ThreadIsAlive.VBS = true;
        //        new Thread(Functions.ActivateVBS).Start();
        //    }
        //}

        private void AllowUSBCode(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.USBExecution)
            {
                ThreadIsAlive.USBExecution = true;
                new Thread(Functions.AllowUSBCode).Start();
            }
        }

        private void FastWUPDATE(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.FastWUPDATE).Start();
            }
        }

        private void BlockUSBCode(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.USBExecution)
            {
                ThreadIsAlive.USBExecution = true;
                new Thread(Functions.BlockUSBCode).Start();
            }
        }

        //private void ActivateCFG(object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.CFG)
        //    {
        //        ThreadIsAlive.CFG = true;
        //        new Thread(Functions.ActivateCFG).Start();
        //    }
        //}

        //private void DeactivateCFG(object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.CFG)
        //    {
        //        ThreadIsAlive.CFG = true;
        //        new Thread(Functions.DeactivateCFG).Start();
        //    }
        //}

        private void UserAutostart(object sender, RoutedEventArgs e)
        {
            new Thread(Functions.UserAutostart).Start();
        }

        private void GlobalAutostart(object sender, RoutedEventArgs e)
        {
            new Thread(Functions.GlobalAutostart).Start();
        }

        private void StandartBootMenuePolicy(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BMP)
            {
                ThreadIsAlive.BMP = true;
                new Thread(Functions.StandartBootMenuePolicy).Start();
            }
        }

        private void LegacyBootMenuePolicy(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BMP)
            {
                ThreadIsAlive.BMP = true;
                new Thread(Functions.LegacyBootMenuePolicy).Start();
            }
        }

        private void SetMaxFailedLoginAttemptsBoot(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.MaxFailedLoginAttempts)
            {
                ThreadIsAlive.MaxFailedLoginAttempts = true;
                new Thread(Functions.SetMaxFailedLoginAttemptsBoot).Start(MfLan.Text);
                MfLan.Text = null;
            }
        }

        private void SetScreenTimeout(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.ScreenTimeOut)
            {
                ThreadIsAlive.ScreenTimeOut = true;
                new Thread(Functions.SetScreenTimeout).Start(Timeout.Text);
                Timeout.Text = null;
            }
        }

        private void DefaultCMDBehavior(object sender, RoutedEventArgs e)
        {
            Start("cmd.exe", "/c ftype batfile=\"%1\" %*", true, false);
            Start("cmd.exe", "/c ftype cmdfile=\"%1\" %*", true, false);
            Write("Default .bat and .cmd behaviour\n\n", "darkcyan", null);
        }

        private void SystemLock(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.MaxFailedLoginAttempts)
            {
                ThreadIsAlive.SystemLock = true;
                new Thread(Functions.SystemLock).Start(new string[] { attempts.Text, duration.Text });
                attempts.Text = null;
                duration.Text = null;
            }
        }

        private void CheckHealth(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.CheckHealth).Start();
            }
        }

        private void ScanHealth(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.ScanHealth).Start();
            }
        }

        private void RestoreHealth(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.RestoreHealth).Start();
            }
        }

        private void SFCScan(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.SFCScan).Start();
            }
        }

        public void SelectWLanDir(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            ofd.SelectedPath = "C:\\Users";
            DialogResult result = ofd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                wlanconfigdir.Text = ofd.SelectedPath;
            }
        }

        private void AllProfilesWLanFalse(object sender, RoutedEventArgs e)
        {
            AllProfilesWLan = false;
        }

        private void AllProfilesWLanTrue(object sender, RoutedEventArgs e)
        {
            AllProfilesWLan = true;
        }

        private void ExportWLanConfig(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.IOWLanConfig)
            {
                ThreadIsAlive.IOWLanConfig = true;
                new Thread(Functions.ExportWLanConfig).Start(wlanconfigdir.Text);
            }
        }

        private void ImportWLanConfig(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.IOWLanConfig)
            {
                ThreadIsAlive.IOWLanConfig = true;
                new Thread(Functions.ImportWLanConfig).Start(wlanconfigdir.Text);
            }
        }

        private void RemWebView(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WebView)
            {
                ThreadIsAlive.WebView = true;
                new Thread(Functions.RemWebView).Start();
            }
        }

        private void Curser(object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Curser)
            {
                ThreadIsAlive.Curser = true;
                new Thread(Functions.Curser).Start();
            }
        }

        private void Info(object sender, RoutedEventArgs e)
        {
            Write("═══════════════ Info Start ═══════════════\n\n", "green", null);

            Write("═══ General Settings ═══\n", "darkgreen", null);

            Write("Activate Windows Defender Sandbox\n", "gray", null);

            Write("Set VeraCrypt as Trusted Process\n", "gray", null);

            Write("Disable lockscreen tips and tricks\n", "gray", null);

            Write("Show encrypted or compressed NTFS files in color\n", "gray", null);

            Write("Disable Local Security Questions\n", "gray", null);

            Write("Enable NumLock on startup\n", "gray", null);

            Write("Disable auto reboot (while users are logged in)\n", "gray", null);

            Write("Enable NTFSLongpaths\n", "gray", null);

            Write("Set Desktop-background-quality to 100%\n", "gray", null);

            Write("Disable SafeSearchMode\n", "gray", null);

            Write("Disable HibernationBoot\n", "gray", null);

            Write("Enable clipboard history\n", "gray", null);

            Write("Disable Cortana\n", "gray", null);

            Write("Disable thirdParty suggestions\n", "gray", null);

            Write("Show filename extensions\n", "gray", null);

            Write("Set default explorer page to \"This PC\"\n", "gray", null);

            Write("Enable Explorer Process separation\n", "gray", null);

            Write("Disable Explorer Compact Mode\n", "gray", null);

            Write("Remove DynLinks\n", "gray", null);

            Write("Show task manager details\n", "gray", null);

            Write("Show file operations details\n", "gray", null);

            Write("Set various services to manual..\n", "gray", null);

            Write("*Various performance tweaks*\n", "gray", null);

            Write("Remove AutoLogger file and restricting directory\n\n", "gray", null);

            //

            Write("═══ Deactivating telemetry ═══\n", "darkgreen", null);

            Write("Run ShutUp10\n", "gray", null);

            Write("Deactivate GameDVR\n", "gray", null);

            Write("Disable Storage Sense\n", "gray", null);

            Write("Disable system telemetry\n", "gray", null);

            Write("Disable Application suggestions\n", "gray", null);

            Write("Disable Feedback\n", "gray", null);

            Write("Disable Tailored Experiences\n", "gray", null);

            Write("Disable Advertising ID\n", "gray", null);

            Write("Disable Tailored Experiences\n", "gray", null);

            Write("Stopp and Disable Diagnostics Tracking Service\n", "gray", null);

            Write("Stopp and Disable Superfetch service\n", "gray", null);

            Write("Disable News and Interests\n", "gray", null);

            Write("Disable Wi-Fi Sense\n", "gray", null);

            Write("Adds blacklisted Domains\n\n", "darkgray", null);

            //

            Write("═══ Better Windows Terminal Integration ═══\n", "darkgreen", null);

            Write("Installs Windows Terminal on Windows Server or Windows 10\n", "gray", null);

            Write("Adds 'Run As Administrator' to extended context menu\n\n", "gray", null);

            //

            Write("═══ Windows Hardening ═══\n", "darkgreen", null);

            Write("Deactivate Local Security Questions\n", "gray", null);

            Write("Require authentification after sleep\n", "gray", null);

            Write("Deactivate auto hotspot connect\n", "gray", null);

            Write("Enable UAC\n", "gray", null);

            Write("Preventing credential theft\n", "gray", null);

            Write("Activate Windows Defender network protection\n", "gray", null);

            Write("Activate Windows Defender Sandbox\n", "gray", null);

            Write("Making VeraCrypt a trusted process\n", "gray", null);

            Write("Activate PUA\n", "gray", null);

            Write("Activate Windows Defender periodic scan\n", "gray", null);

            Write("Scan system boot drivers\n", "gray", null);

            Write("Prevent Office child process creation\n", "gray", null);

            Write("Block Office process injection\n", "gray", null);

            Write("Block Win32-API calls from Makros\n", "gray", null);

            Write("Block Office from creating executables\n", "gray", null);

            Write("Block potentially obfuscated scripts\n", "gray", null);

            Write("Block executable content from E-Mail client and Webmail\n", "gray", null);

            Write("Block execution of downloaded content via Javascript or VBSscripts\n", "gray", null);

            Write("Block Adobe Reader childprocess creation\n", "gray", null);

            Write("Prevent WMI misuse\n", "gray", null);

            Write("Deactivate useractivity upload\n", "gray", null);

            Write("Deactivate GameDVR\n", "gray", null);

            Write("Protect common objects\n", "gray", null);

            Write("Deactivate http printing\n", "gray", null);

            Write("Securing Netlogon traffic\n", "gray", null);

            Write("Activate Windows Defender Exploit Guard\n", "gray", null);

            Write("LSASS hardening\n", "magenta", null);
            Write("Running LSA as protected process\n", "darkgray", null);

            Write("Deactivate WDigest for login\n", "darkgray", null);

            Write("Mimikatz protection (check access requests)\n", "darkgray", null);

            Write("Activate Remote Credential Guard-Mode (credentials delegation)\n", "darkgray", null);

            Write("Changes default file opener to notepad.exe (ransomware protection)\n", "magenta", null);
            Write("hta\n", "darkgray", null);
            Write("wsh\n", "darkgray", null);
            Write("wsf\n", "darkgray", null);
            Write("bat\n", "darkgray", null);
            Write("js\n", "darkgray", null);
            Write("jse\n", "darkgray", null);
            Write("vbe\n", "darkgray", null);
            Write("vbs\n", "darkgray", null);
            Write("cmd\n", "darkgray", null);

            Write("Firewall settings\n", "magenta", null);
            Write("Activating logging", "darkgray", null);
            Write("   (%systemroot%\\system32\\LogFiles\\Firewall\\pfirewall.log)\n", "darkgray", null);
            Write("Seting max length to 4096\n", "darkgray", null);

            Write("Blocks Notepad.exe netconns\n", "darkgray", null);
            Write("Blocks regsvr32.exe netconns\n", "darkgray", null);
            Write("Blocks calc.exe netconns\n", "darkgray", null);
            Write("Blocks mshta.exe netconns\n", "darkgray", null);
            Write("Blocks wscript.exe netconns\n", "darkgray", null);
            Write("Blocks cscript.exe netconns\n", "darkgray", null);
            Write("Blocks runscripthelper.exe netconns\n", "darkgray", null);
            Write("Blocks hh.exe netconns\n", "darkgray", null);

            Write("IE and Edge hardening\n", "magenta", null);
            Write("Activate Smartscreen for Edge\n", "darkgray", null);

            Write("IE software install notifications\n", "darkgray", null);

            Write("Deactivate Edge build-in passwordmanager\n", "darkgray", null);

            Write("Prevente anonymous Sam-Account enumeration\n", "darkgray", null);

            Write("Biometric security\n", "magenta", null);
            Write("Anti-spoof for facial recognition\n", "darkgray", null);
            Write("Deactivate camera on locked screen", "darkgray", null);
            Write("Deactivate app voice commands in locked state\n", "darkgray", null);
            Write("Deactivate windows voice commands in locked state\n", "darkgray", null);

            Write("Activate advanced Windows Event-Logging\n", "magenta", null);
            Write("Sette various log files to 1024000kb\n", "darkgray", null);
            Write("Activate Power-Shell logging\n\n", "darkgray", null);

            //

            Write("═══ Windows Activator ═══\n", "darkgreen", null);

            Write("Windows Client: HWID Universal-Ticket\n", "gray", null);
            Write("Windows Server: KMS38-Protected\n", "gray", null);

            Write("Implemented from ", "darkgray", null);
            Write("https://github.com/massgravel/Microsoft-Activation-Scripts ", "cyan", null);
            Write("with permission from 'WindowsAddict'\n\n", "darkgray", null);

            //

            Write("═══ Legacy Context Menu ═══\n", "darkgreen", null);
            Write("Reverts the Right-Click context menu back to the Windows 10 style\n\n", "gray", null);

            //

            Write("═══ Legacy Explorer Ribbon ═══\n", "darkgreen", null);
            Write("Reverts the Explorer Ribbon back to the Windows 10 style\n\n", "gray", null);

            //

            Write("╔═════════════════ Info End ════════════════╗\n", "green", null);
            Write("║ ", "green", null);
            Write("https://github.com/backslashspace/WinUtil", "cyan", null);
            Write(" ║\n", "green", null);
            Write("╚═══════════════════════════════════════════╝\n", "green", null);
        }

        //#######################################################################################################################

        private void Debug(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
