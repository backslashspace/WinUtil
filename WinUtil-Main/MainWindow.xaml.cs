using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace WinUtil_Main
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
                [In] Char[] lpDependencies,
                String lpServiceStartName,
                String lpPassword,
                String lpDisplayName);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern IntPtr OpenService(
                IntPtr hSCManager, String lpServiceName, UInt32 dwDesiredAccess);

            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr OpenSCManager(
                String machineName, String databaseName, UInt32 dwAccess);

            [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
            public static extern Int32 CloseServiceHandle(IntPtr hSCObject);

            private const UInt32 SERVICE_NO_CHANGE = 0xFFFFFFFF;
            private const UInt32 SERVICE_QUERY_CONFIG = 0x00000001;
            private const UInt32 SERVICE_CHANGE_CONFIG = 0x00000002;
            private const UInt32 SC_MANAGER_ALL_ACCESS = 0x000F003F;

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
                        (UInt32)mode,
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
                        Int32 nError = Marshal.GetLastWin32Error();
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
            public const String FirefoxImageName = "Firefox Setup 114.0.1.exe";
            public const String FirefoxImageHash = "378e9bea3123218dbbf448a6da9b4a1d06f0e76e2a1267297f6123abf0995151";
            public const String FSH41 = "6ebaa64f760ab886ca21436c96d7ec1e8e8fda87de787d625a733af147946ff2";
            public const String FSH31 = "4ab65f6689aaadb5b424ff01b0831fbfe934aedf4d5e7be36b8db2314e7a2e09";
            public const String FSH30 = "33d71e18a599864dc7aedcb951790d9e04e675340bd330c6f2057a804170dda4";
            public const String FSH21 = "b45cc85bd6be76f5133860f97631cdc95275d4e0c39699e60bc2c8d0a7130b58";
            public const String FSH20 = "52a431827e3e3b2f57eedc3749e2d2e799f6de2b6b3974d9f6617dda28bca3b0";
            public const String FSH11 = "a6ab07ce3251a2b4eca183e7ea32b2a48529ebd5194c4ec7061cf3f7d548bb18";
            public const String FSH10 = "e6643eeb74602e883ad26ae71e64555c2d760b5d27f420572d044c54741a6e71";
            public const String FSH00 = "dfc03623cb52de39daca1baea32024a7d5fa9e7e3efdfbf2a0122a2236d922d9";

            public const String CurBlack = "1b6cc8dded20f9b52913ee9d5fb8dc055900d3379521e582101cce5c1a3ffc55";
            public const String CurBlackMono = "deefa39c9ffb1e37db469938f443d54ffdab4c90216fde7687a07b9edac30e1a";
            public const String CurBlackHybrid = "7fa9477d1adee85911f113a89036bdaa4bc3c07de48b1d0b70da144fb1ebcc4c";
            public const String CurDef = "4fc875bd5ee33dbec614c85fca7358033fdcb398e61c5442b21ae45d52a8cb1c";
            public const String CurDefMono = "612e141b508c50831230118bb310fa22ab3624842c6bd0ac799f1c210ca4225a";
            public const String CurDefHybrid = "6e845c04e853967a0f3c71c7853bd7a5f7b6e74feb9769e340e0147708d751ce";

            public const String VCLibs = "9bfde6cfcc530ef073ab4bc9c4817575f63be1251dd75aaa58cb89299697a569";
            public const String VCLibsName = "Microsoft.VCLibs.140.00.appx";

            public const String WT = "ba6fc6854e713094b4009cf2021e8b4887cff737ab4b9c4f9390462dd2708298";
            public const String WTName = "efc17b22f5144899a8ae73644add609f.msixbundle";
            public const String WTLicenseName = "efc17b22f5144899a8ae73644add609f_License1.xml";
            public const String WTLicenseHash = "f42e88d519acb2e4e15f929d5fd1fd07acb6ba4e2fe797501a329a33a5564675";

            public const String WinGetName = "Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle";
            public const String WinGetHash = "060cf918cd11feea62a25b8c594c81def5f35e4f53aa3c92a3ef1806e902a2e8";
            public const String WinGetLicenseName = "7b7c9a02f424442cafec8d108f644d61_License1.xml";
            public const String WinGetLicenseHash = "6e0e9c662a17d7e8f0e3595d7abe8798b5872383030d5587d267cea234d07beb";
            public const String Xaml27Name = "Microsoft.UI.Xaml.2.7_7.2208.15002.0_x64__8wekyb3d8bbwe.appx";
            public const String Xaml27Hash = "efa0b1396a134a654c41bd9f4b9f084a5dfe19ad6d492cd99313b0b898a0767f";

            public const String su10exe = "OOSU10-v1.9.1435.exe";
            public const String su10exeHash = "22d3a45792b749e70b908088e95c19abae0707b248fcb83744b23bc6f662425b";
            public const String su10settings = "Settings-v1.9.1435.cfg";
            public const String su10settingsHash = "1a2792ac835df3cd2a3f6097bdb8abefce3200eb87afbf6f2e1a44b4f0685220";

            public const String ACLHash = "4efc87b7e585fcbe4eaed656d3dbadaec88beca7f92ca7f0089583b428a6b221";

            public const String Nano = "8b31e1e10f3383ff6ca49e4f65e738805015351984ad67517448e3e7c53c43a2";

            public const String zip7exe = "254cf6411d38903b2440819f7e0a847f0cfee7f8096cfad9e90fea62f42b0c23";
            public const String zip7dll = "73578f14d50f747efa82527a503f1ad542f9db170e2901eddb54d6bce93fc00e";

            public const String zip7installHash = "23ab1f43a0ed6a022b441995a8dcf9b9cd08046f73fb66042bdb7eabaf87b7b2";
            public const String zip7installName = "7z2300-x64.exe";

            public const String vc64Hash = "ce6593a1520591e7dea2b93fd03116e3fc3b3821a0525322b0a430faa6b3c0b4";
            public const String vc86Hash = "cf92a10c62ffab83b4a2168f5f9a05e5588023890b5c0cc7ba89ed71da527b0f";

            public const String javaName = "jdk-17.0.7_windows-x64_bin.exe";
            public const String javaHash = "f41cfb7fd675f9f74b76217a2c0940b76f4676f053fddb62a464eacffa4a773b";
        }

        public class ThreadIsAlive
        {
            public static Boolean ActivateWindows { get; set; }
            public static Boolean BlockTCP { get; set; }
            public static Boolean LoginBlur { get; set; }
            public static Boolean General { get; set; }
            public static Boolean N0Telemetry { get; set; }
            public static Boolean Curser { get; set; }
            public static Boolean SystemTheme { get; set; }
            public static Boolean Restart_Explorer { get; set; }
            public static Boolean WT { get; set; }
            public static Boolean Debloat { get; set; }
            public static Boolean ClearTaskBar { get; set; }
            public static Boolean RemoveXbox { get; set; }
            public static Boolean RMOneDrive { get; set; }
            public static Boolean AppFilehistory { get; set; }
            public static Boolean Cmen { get; set; }
            public static Boolean Ribbon { get; set; }
            public static Boolean Nano { get; set; }
            public static Boolean RMEdge { get; set; }
            public static Boolean BootStuff { get; set; }
            public static Boolean NumLock { get; set; }
            public static Boolean BootSound { get; set; }
            public static Boolean Install { get; set; }
            public static Boolean SMB { get; set; }
            public static Boolean LockScreen { get; set; }
            public static Boolean LockScreenNotifications { get; set; }
            public static Boolean UAC { get; set; }
            public static Boolean Pagefile { get; set; }
            public static Boolean RequireCtrl { get; set; }
            public static Boolean AutoLogin { get; set; }
            public static Boolean Harden { get; set; }
            public static Boolean ApplicationGuard { get; set; }
            public static Boolean VBS { get; set; }
            public static Boolean USBExecution { get; set; }
            public static Boolean CFG { get; set; }
            public static Boolean BMP { get; set; }
            public static Boolean MaxFailedLoginAttempts { get; set; }
            public static Boolean ScreenTimeOut { get; set; }
            public static Boolean SystemLock { get; set; }
            public static Boolean SystemCheck { get; set; }
            public static Boolean IOWLanConfig { get; set; }
            public static Boolean WebView { get; set; }
            public static Boolean WGetAction { get; set; }
            public static Boolean Backgroundapps { get; set; }
            public static Boolean WindowsUpdate { get; set; }
            public static Boolean NoWUDrivers { get; set; }
            public static Boolean LSASS { get; set; }
            public static Boolean TCPTune { get; set; }
        }

        //#######################################################################################################################
        public static String AdminGroupName { get; set; }
        public static String ExePath { get; set; }
        public static Boolean IsUEFI { get; set; }
        public static Boolean IsActivated { get; set; }
        public static String User { get; set; }
        public static Boolean IsInDomain { get; set; }
        public static String UserPath { get; set; }
        public static Boolean AllProfilesWLan { get; set; }
        public static Int16 ActivationClicks { get; set; }
        //
        public static Boolean IsServer { get; set; }
        public static Boolean IsWin11Server22Complient { get; set; }

        public static Boolean Win10UI = true;
        public static Int32 WindowsMajorV { get; set; }
        public static Int32 WindowsMinorV { get; set; }

        //#######################################################################################################################

        ///<summary>Handles Registry access.</summary>
        public class RegistryIO
        {
            ///<summary>Converts Human-Readable Registry path to '<c>RegistryKey</c>' type.</summary>
            ///<returns>'<c>null</c>' when RegistryHive not found.</returns>
            private static RegistryKey PathToKey(String Path)
            {
                String DPath = Path.Split('\\')[0];
                Path = Regex.Match(Path, "(?<=\\\\).*").ToString();

                return DPath.ToUpper() switch
                {
                    "HKEY_LOCAL_MACHINE" => Registry.LocalMachine.OpenSubKey(Path, true),
                    "HKEY_CURRENT_USER" => Registry.CurrentUser.OpenSubKey(Path, true),
                    "HKEY_CLASSES_ROOT" => Registry.ClassesRoot.OpenSubKey(Path, true),
                    "HKEY_USERS" => Registry.Users.OpenSubKey(Path, true),
                    "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig.OpenSubKey(Path, true),
                    _ => throw new Exception("Invalid RegistryHive"),
                };
            }

            //######################################################

            ///<summary>Reads a value from the Registry.</summary>
            ///<returns>
            ///Returns data in form of specified '<c>RegistryValueKind</c>'.<br/>
            ///Returns '<c>null</c>' when value is not present.<br/>
            ///Returns '<c>-1</c>' when value has the wrong type and '<c>DeleteWrongType</c>' is set to '<c>false</c>'.
            ///</returns>
            public static dynamic GetValue(String Path, String Value, RegistryValueKind ExpectedType, Boolean DeleteWrongType)
            {
                var Out = Registry.GetValue(Path, Value, null);

                if (Out == null)
                {
                    return null;
                }

                switch (ExpectedType)
                {
                    case RegistryValueKind.String:
                        if (Out is String)
                        {
                            return Out;
                        }
                        return Fallback();
                    case RegistryValueKind.DWord:
                        if (Out is Int32)
                        {
                            return Out;
                        }

                        return Fallback();
                    case RegistryValueKind.QWord:
                        if (Out is Int64)
                        {
                            return Out;
                        }

                        return Fallback(); ;
                    case RegistryValueKind.MultiString:
                        if (Out is String[])
                        {
                            return Out;
                        }

                        return Fallback();
                    case RegistryValueKind.Binary:
                        if (Out is Byte[] || Out is Int16[] || Out is Int32[] || Out is Int64[])
                        {
                            return Out;
                        }

                        return Fallback();
                    default:
                        return null;
                }

                dynamic Fallback()
                {
                    if (DeleteWrongType)
                    {
                        RegistryKey Key = PathToKey(Path);

                        try
                        {
                            using (Key)
                            {
                                Key.DeleteValue(Value, false);
                            }
                            Key.Close();
                            Key.Dispose();
                        }
                        catch (System.ArgumentException) { }

                        return null;
                    }

                    return -1;
                }
            }

            ///<summary>Removes values from the Registry.</summary>
            ///<remarks>Takes a List of values and removes them in a specified path.</remarks>
            ///<returns>'<c>true</c>' when at least one error occurred.</returns>
            public static Boolean DeleteValues(String Path, String[] Values, Boolean ContinueOnError = true)
            {
                RegistryKey Key = PathToKey(Path);

                if (Key == null) { return false; }

                Boolean RT = false;

                using (Key)

                    foreach (String Value in Values)
                    {
                        try
                        {

                            Key.DeleteValue(Value, true);

                        }
                        catch
                        {
                            if (ContinueOnError == false)
                            {
                                return true;
                            }

                            RT = true;
                        }
                    }

                Key.Close();
                Key.Dispose();

                return RT;
            }

            ///<summary>Removes Key from the Registry.</summary>
            ///<remarks>Takes a List of Keys and removes them in a specified path.</remarks>
            ///<returns>'<c>true</c>' when at least one error occurred.</returns>
            public static Boolean DeleteSubKeyTree(String KeyPath, String[] Keys, Boolean ContinueOnError = true)
            {
                RegistryKey MKey = PathToKey(KeyPath);

                if (MKey == null) { return false; }

                Boolean RT = false;

                using (MKey)

                    foreach (String Key in Keys)
                    {
                        try
                        {
                            MKey.DeleteSubKeyTree(Key);
                        }
                        catch
                        {
                            if (ContinueOnError == false)
                            {
                                return true;
                            }

                            RT = true;
                        }
                    }

                MKey.Close();
                MKey.Dispose();

                return RT;
            }

            ///<summary>Tests if a value exists in the Registry.</summary>
            ///<returns>'<c>false</c>' if not present.</returns>
            public static Boolean TestRegValuePresense(String Path, String Value)
            {
                try
                {
                    if ((String)Registry.GetValue(Path, Value, null).ToString() == null)
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
        }

        ///<summary>Eggsecutes stuff</summary>
        public class Execute
        {
            ///<summary>Allows to start executables with commands.</summary>
            public static void EXE(String Path, String Args = null, Boolean HiddenExecute = false, Boolean WaitForExit = false)
            {
                Process Proc;
                ProcessStartInfo info;

                if (HiddenExecute)
                {
                    Proc = new Process();
                    info = new ProcessStartInfo()
                    {
                        FileName = Path,
                        Arguments = Args,
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas"
                    };

                    Proc.StartInfo = info;
                    Proc.Start();
                }
                else
                {
                    Proc = new Process();
                    info = new ProcessStartInfo()
                    {
                        FileName = Path,
                        Arguments = Args,
                        UseShellExecute = true,
                        Verb = "runas"

                    };
                    Proc.StartInfo = info;
                    Proc.Start();
                }

                if (WaitForExit)
                {
                    Proc.WaitForExit();
                }
            }

            public static String[] PowerShell(String RawPSCommand, Boolean OutPut = false, Boolean FormatCustom = false, Int32 RMTop = 0, Int32 RMBottom = 0)
            {
                if (!OutPut)
                {
                    System.Management.Automation.PowerShell.Create().AddScript(RawPSCommand).Invoke();

                    return null;
                }

                List<String> Output;
                String PSOut;

                if (FormatCustom)
                {
                    PSOut = System.Management.Automation.PowerShell.Create().AddScript(RawPSCommand + " | Format-Custom | Out-String").Invoke()[0].ToString();
                }
                else
                {
                    PSOut = System.Management.Automation.PowerShell.Create().AddScript(RawPSCommand + " | Out-String").Invoke()[0].ToString();
                }

                PSOut = PSOut.Replace("\r", "");
                PSOut = PSOut.Replace(" ", "");

                Output = new List<String>(PSOut.Split('\n'));

                for (Int32 i = 0; i < RMTop; i++)
                {
                    Output.RemoveAt(0);
                }

                for (Int32 i = 0; i < RMBottom; i++)
                {
                    Output.RemoveAt(Output.Count - 1);
                }

                return Output.ToArray();
            }
        }

        //######################################################

        ///<summary>Retrieves Windows License status from the Windows Management Engine and saves it in the Registry.</summary>
        private static void SetRegLicense()
        {
            PowerShell.Create().AddScript("$test = Get-CimInstance SoftwareLicensingProduct -Filter \"Name like 'Windows%'\" | where { $_.PartialProductKey } | select LicenseStatus; $test = $test -replace \"@{LicenseStatus=\"; $test = $test -replace \"}\"; reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil\" /v \"Windows Activation Status\" /t reg_dword /d $test /f").Invoke();
        }

        ///<summary>Returns the status of a given service.</summary>
        public static String GetServiceStatus(String servicename)
        {
            ServiceController sc = new(servicename);

            return sc.Status switch
            {
                ServiceControllerStatus.Running => "running",
                ServiceControllerStatus.Stopped => "stopped",
                ServiceControllerStatus.Paused => "paused",
                ServiceControllerStatus.StopPending => "stopping",
                ServiceControllerStatus.StartPending => "starting",
                _ => "status changing",
            };
        }

        ///<summary>Tests internet connectivity.</summary>
        ///<remarks>(Attempts to resolve a hostname)</remarks>
        public static Boolean InternetIsAvailable()
        {
            try
            {
                IPAddress[] theaddress = Dns.GetHostAddresses("1dot1dot1dot1.cloudflare-dns.com");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        ///<summary>Checks if a script friendly version of '<c>Microsoft.DesktopAppInstaller</c>' is installed.</summary>
        ///<returns>
        ///<c>0</c> = not usable<br/>
        ///<c>1</c> = usable (ready or after install)<br/>
        ///<c>2</c> = install failed
        ///</returns>
        public static Int32 WinGetIsInstalled(Boolean InstallOnMissing = false)
        {
            String[] Folders = Directory.EnumerateDirectories("C:\\Program Files\\WindowsApps").ToArray();

            for (Int32 i = 0; i < Folders.Length; i++)
            {
                if (Folders[i].Contains("Microsoft.DesktopAppInstaller"))
                {
                    if (Regex.IsMatch(Folders[i], "([0-9]+)\\.([0-9]+)"))
                    {
                        String[] arr = Regex.Split(Folders[i], "([0-9]+)\\.([0-9]+)");

                        try
                        {
                            if (Int32.Parse(arr[1]) > 1 || (Int32.Parse(arr[1]) == 1 && Int32.Parse(arr[2]) >= 20))
                            {
                                return 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch { }
                    }
                }
            }

            if (InstallOnMissing)
            {
                Write("Installing WinGet..\n", "darkgreen", null);

                if (CompareHash256("assets\\" + Const.VCLibsName, Const.VCLibs, true)[0] == 1 && CompareHash256("assets\\WinGet\\" + Const.WinGetName, Const.WinGetHash, true)[0] == 1 && CompareHash256("assets\\WinGet\\" + Const.Xaml27Name, Const.Xaml27Hash, true)[0] == 1 && CompareHash256("assets\\WinGet\\" + Const.WinGetLicenseName, Const.WinGetLicenseHash, true)[0] == 1)
                {
                    PowerShell.Create().AddScript("Add-AppxPackage -Path \"assets\\" + Const.VCLibsName + "\"").Invoke();

                    PowerShell.Create().AddScript("Add-AppxPackage -Path \"assets\\WinGet\\" + Const.Xaml27Name + "\"").Invoke();

                    PowerShell.Create().AddScript("Add-ProvisionedAppPackage -Online -PackagePath \"assets\\WinGet\\" + Const.WinGetName + "\" -LicensePath \"assets\\WinGet\\" + Const.WinGetLicenseName + "\"").Invoke();

                    Thread.Sleep(5000);

                    try
                    {
                        Execute.EXE("winget.exe", null, true, true);

                        Write("Done\n\n", "darkcyan", null);

                        return 1;
                    }
                    catch (Exception ex)
                    {
                        Write(ex.ToString() + "\n\n", "red", null);

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

        ///<summary>Print to console with color.</summary>
        public static void Write(String txt, String foreground, String background)
        {
            if (background != null)
            {
                switch (background.ToLower())
                {
                    case "back":
                        Console.BackgroundColor = ConsoleColor.Black;
                        break;
                    case "white":
                        Console.BackgroundColor = ConsoleColor.White;
                        break;
                    case "red":
                        Console.BackgroundColor = ConsoleColor.Red;
                        break;
                    case "cyan":
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        break;
                    case "green":
                        Console.BackgroundColor = ConsoleColor.Green;
                        break;
                    case "gray":
                        Console.BackgroundColor = ConsoleColor.Gray;
                        break;
                    case "blue":
                        Console.BackgroundColor = ConsoleColor.Blue;
                        break;
                    case "darkblue":
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        break;
                    case "darkcyan":
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        break;
                    case "darkgray":
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        break;
                    case "darkgreen":
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        break;
                    case "darkmagenta":
                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                        break;
                    case "darkred":
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        break;
                    case "darkyellow":
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        break;
                    case "magenta":
                        Console.BackgroundColor = ConsoleColor.Magenta;
                        break;
                    case "yellow":
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        break;
                }
            }
            //#######
            if (foreground != null)
            {
                switch (foreground.ToLower())
                {
                    case "back":
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    case "white":
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case "red":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case "cyan":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case "green":
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case "gray":
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case "blue":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case "darkblue":
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;
                    case "darkcyan":
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case "darkgray":
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case "darkgreen":
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    case "darkmagenta":
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        break;
                    case "darkred":
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                    case "darkyellow":
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case "magenta":
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case "yellow":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }
            }

            Console.Write(txt);
            Console.ResetColor();
        }

        ///<summary>Extracts embeded files.</summary>
        public static void InternExtract(String Path, String OutFile, String EmbededFileName, String AlternativNamespace = null)
        {
            //get namespace
            String Namespace = AlternativNamespace ?? "WinUtil_Main";

            ValidateInput();

            //extract
            try
            {
                Stream RawFile = Assembly.GetExecutingAssembly().GetManifestResourceStream(Namespace + "." + EmbededFileName);
                FileStream FileStream = new(Path + "\\" + OutFile, FileMode.Create);

                for (Int32 i = 0; i < RawFile.Length; i++)
                {
                    FileStream.WriteByte((Byte)RawFile.ReadByte());
                }

                FileStream.Close();
                FileStream.Dispose();
            }
            catch (UnauthorizedAccessException)
            {
                var attributes = File.GetAttributes(Path + "\\" + OutFile);
                if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    attributes &= ~FileAttributes.Hidden;
                    File.SetAttributes(Path + "\\" + OutFile, attributes);
                }
                InternExtract(Path, OutFile, EmbededFileName);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path);
                InternExtract(Path, OutFile, EmbededFileName);
            }

            void ValidateInput()
            {
                //get all embeded resources
                String[] EmbededResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                foreach (String Item in EmbededResources)
                {
                    if (Item == Namespace + "." + EmbededFileName) return;
                }

                throw new Exception("Embeded resource not found:\nProvided resource: " + Namespace + "." + EmbededFileName);
            }
        }

        ///<summary>Calculates SHA256 of a given file.</summary>
        ///<returns>'<c>Byte[2]</c>' { IsValid, IsPresent }</returns>
        public static Byte[] CompareHash256(String file, String hash, Boolean verbose)
        {
            String[] temp = file.Split('\\');
            String efile = temp[temp.Length - 1];
            temp = temp.Skip(0).Take((temp.Length) - 1).ToArray();
            String rpath = String.Join("\\", temp);

            if (file.Contains('\\'))
            {
                rpath += "\\";
            }

            try
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                using var stream = File.OpenRead(file);
                if ((BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", String.Empty)).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return new Byte[] { 1, 1 };
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
                    return new Byte[] { 0, 1 };
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
                return new Byte[] { 0, 0 };
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
                    Write("Direct start\n", "red", "yellow");
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                Write("Direct start\n", "red", null);
            }

            ////// logo //////
            {

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
            }

            try
            {
                //get user infos
                if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName != "")
                {
                    IsInDomain = true;
                }

                User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];

                UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Split('\\')[2];

                Write("Loaded User: " + User, "darkgray", null);
                Write("\nLoaded User-DIR: " + UserPath + "\n\n", "darkgray", null);

                //check if system is activated [set reg value]
                if (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "Windows Activation Status", RegistryValueKind.DWord, true) != 1)
                {
                    ThreadStart cr = new(SetRegLicense);
                    Thread ct = new(cr);
                    ct.Start();
                }
                else
                {
                    IsActivated = true;
                }

                //check systemtype
                if (Execute.PowerShell("$env:firmware_type", true, false, 0, 1)[0].ToUpper() == "UEFI")
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
                if (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID", RegistryValueKind.String, false) == "Core" && (RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 2 || RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\WinUtil", "GPO Status", RegistryValueKind.DWord, true) != 1))
                {
                    ThreadStart childref = new(Functions.InstallGPO);
                    Thread childThread = new(childref);
                    childThread.Start();
                }

                //gets local admingroup name
                String temp = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount)).Value;
                String[] temp2 = temp.Split('\\');
                AdminGroupName = temp2[1];

                //gets exepath
                ExePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                //get OS details
                //get version
                try
                {
                    Int32 CBN = Int32.Parse(RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", RegistryValueKind.String, false));
                    Int32 UBR = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", RegistryValueKind.DWord, false);

                    WindowsMajorV = CBN;
                    WindowsMinorV = UBR;
                }
                catch
                {
                    Write("Failed to retrieve system information\n", "red", null);
                    Write("CurrentBuildNumber | UBR\n\n", "darkgray", null);
                }

                //get type/edition
                String IType = RegistryIO.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallationType", RegistryValueKind.String, false).ToLower();

                switch (IType)
                {
                    case "server":
                        IsServer = true;
                        Write("Detected Windows Server\n\n", "cyan", null);
                        if (WindowsMajorV >= 20348)
                        {
                            IsWin11Server22Complient = true;
                        }
                        break;
                    case "client":
                        if (WindowsMajorV >= 22000)
                        {
                            Win10UI = false;
                            IsWin11Server22Complient = true;
                            Write("Detected Windows 11 or newer\n\n", "cyan", null);
                        }
                        break;
                    default:
                        Write("Failed to retrieve system information\n", "red", null);
                        Write("Installation type\n\n", "darkgray", null);
                        break;
                }
            }
            catch (Exception ex)
            {
                Write(ex.Message + "\n\n", "red", null);
            }

            //      🥄

            //starts gui centered
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            //sets default dir box
            wlanconfigdir.Text = "C:\\Users\\" + UserPath + "\\Desktop";
            AllProfilesWLan = true;
        }

        //#######################################################################################################################

        private void Restart_Explorer(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Restart_Explorer)
            {
                ThreadIsAlive.Restart_Explorer = true;
                new Thread(Functions.Restart_Explorer).Start();
            }
        }

        private void ActivateWindows(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.ActivateWindows)
            {
                ThreadIsAlive.ActivateWindows = true;
                new Thread(Functions.ActivateWindows).Start();
            }
        }

        private void General(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.General)
            {
                ThreadIsAlive.General = true;
                new Thread(Functions.General).Start();
            }
        }

        private void YesLoginBlur(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LoginBlur)
            {
                ThreadIsAlive.LoginBlur = true;
                new Thread(Functions.YesLoginBlur).Start();
            }
        }

        private void NoLoginBlur(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LoginBlur)
            {
                ThreadIsAlive.LoginBlur = true;
                new Thread(Functions.NoLoginBlur).Start();
            }
        }

        private void N0Telemetry(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.N0Telemetry)
            {
                ThreadIsAlive.N0Telemetry = true;
                new Thread(Functions.DeacTelemetry).Start();
            }
        }

        private void DarkMode(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemTheme)
            {
                ThreadIsAlive.SystemTheme = true;
                new Thread(Functions.DarkMode).Start();
            }
        }

        private void LightMode(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemTheme)
            {
                ThreadIsAlive.SystemTheme = true;
                new Thread(Functions.LightMode).Start();
            }
        }

        private void BetterWT(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WT)
            {
                ThreadIsAlive.WT = true;
                new Thread(Functions.BetterWT).Start();
            }
        }

        private void NormalWT(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WT)
            {
                ThreadIsAlive.WT = true;
                new Thread(Functions.NormalWT).Start();
            }
        }

        private void BlockTCP(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BlockTCP)
            {
                ThreadIsAlive.BlockTCP = true;
                new Thread(Functions.BlockTCP).Start();
            }
        }

        private void Debloat(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Debloat)
            {
                ThreadIsAlive.Debloat = true;
                new Thread(Functions.Debloat).Start();
            }
        }

        private void ClearTaskBar(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.ClearTaskBar)
            {
                ThreadIsAlive.ClearTaskBar = true;
                new Thread(Functions.ClearTaskBar).Start();
            }
        }

        private void RemoveXbox(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RemoveXbox)
            {
                ThreadIsAlive.RemoveXbox = true;
                new Thread(Functions.RemoveXbox).Start();
            }
        }

        private void RMOneDrive(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RMOneDrive)
            {
                ThreadIsAlive.RMOneDrive = true;
                new Thread(Functions.RMOneDrive).Start();
            }
        }

        private void NoAppFilehistory(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AppFilehistory)
            {
                ThreadIsAlive.AppFilehistory = true;
                new Thread(Functions.NoAppFilehistory).Start();
            }
        }

        private void YesAppFilehistory(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AppFilehistory)
            {
                ThreadIsAlive.AppFilehistory = true;
                new Thread(Functions.YesAppFilehistory).Start();
            }
        }

        private void LegacyCmen(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Cmen)
            {
                ThreadIsAlive.Cmen = true;
                new Thread(Functions.LegacyCmen).Start();
            }
        }

        private void DefaultCmen(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Cmen)
            {
                ThreadIsAlive.Cmen = true;
                new Thread(Functions.DefaultCmen).Start();
            }
        }

        private void LegacyRibbon(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Ribbon)
            {
                ThreadIsAlive.Ribbon = true;
                new Thread(Functions.LegacyRibbon).Start();
            }
        }

        private void RMEdge(Object sender, RoutedEventArgs e)
        {
            if (ThreadIsAlive.WebView)
            {
                Write("Wait for WebView to uninstall\n\n", "yellow", null);
            }
            else if (!ThreadIsAlive.RMEdge)
            {
                ThreadIsAlive.RMEdge = true;
                new Thread(Functions.RMEdge).Start();
            }
        }

        private void DefaultRibbon(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Ribbon)
            {
                ThreadIsAlive.Ribbon = true;
                new Thread(Functions.DefaultRibbon).Start();
            }
        }

        private void NoWUDrivers(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.NoWUDrivers)
            {
                ThreadIsAlive.NoWUDrivers = true;
                new Thread(Functions.NoWUDrivers).Start();
            }
        }

        private void SafeBoot(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootStuff)
            {
                ThreadIsAlive.BootStuff = true;
                new Thread(Functions.SafeBoot).Start();
            }
        }

        private void UEFIBoot(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootStuff)
            {
                ThreadIsAlive.BootStuff = true;
                new Thread(Functions.UEFIBoot).Start();
            }
        }

        private void ActivateNumLock(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.NumLock)
            {
                ThreadIsAlive.NumLock = true;
                new Thread(Functions.ActivateNumLock).Start();
            }
        }

        private void ActivateBootSound(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootSound)
            {
                ThreadIsAlive.BootSound = true;
                new Thread(Functions.ActivateBootSound).Start();
            }
        }

        private void DeactivateBootSound(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BootSound)
            {
                ThreadIsAlive.BootSound = true;
                new Thread(Functions.DeactivateBootSound).Start();
            }
        }

        private void Nano(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Nano)
            {
                ThreadIsAlive.Nano = true;
                new Thread(Functions.Nano).Start();
            }
        }

        private void NotepadPlusPlus(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WGetAction)
            {
                ThreadIsAlive.WGetAction = true;
                new Thread(Functions.NotepadPlusPlus).Start();
            }
        }

        private void ResetUpdate(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.ResetUpdate).Start();
            }
        }

        private void SecurityUpdatesOnly(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.SecurityUpdatesOnly).Start();
            }
        }

        private void DeactivateWindowsUpdate(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.DeactivateWindowsUpdate).Start();
            }
        }

        private void DeactivateNumLock(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.NumLock)
            {
                ThreadIsAlive.NumLock = true;
                new Thread(Functions.DeactivateNumLock).Start();
            }
        }

        private void PowerSettings(Object sender, RoutedEventArgs e)
        {
            Execute.EXE("powercfg.cpl", null, false, false);
        }

        private void NetworkSettings(Object sender, RoutedEventArgs e)
        {
            Execute.EXE("ncpa.cpl", null, false, false);
        }

        private void SystemSettings(Object sender, RoutedEventArgs e)
        {
            Execute.EXE("sysdm.cpl", null, false, false);
        }

        private void OptionalFeatures(Object sender, RoutedEventArgs e)
        {
            if (!IsServer)
            {
                Execute.EXE("OptionalFeatures.exe", null, false, false);
            }
            else
            {
                Execute.EXE("ServerManager.exe", null, false, false);
            }
        }

        private void Firefox(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Install)
            {
                ThreadIsAlive.Install = true;
                ThreadStart childref = new(Functions.Firefox);
                Thread childThread = new(childref);
                childThread.SetApartmentState(ApartmentState.STA);
                childThread.Start();
            }
            else
            {
                Write("\nWait for program to finish installing\n\n", "darkyellow", null);
            }
        }

        private void EncryptSMB(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SMB)
            {
                ThreadIsAlive.SMB = true;

                SMBhardenMessage subWindow = new();
                subWindow.Show();
            }
        }

        private void ActivateLockScreen(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreen)
            {
                ThreadIsAlive.LockScreen = true;
                new Thread(Functions.ActivateLockScreen).Start();
            }
        }

        private void ActivateLockScreenNotifications(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreenNotifications)
            {
                ThreadIsAlive.LockScreenNotifications = true;
                new Thread(Functions.ActivateLockScreenNotifications).Start();
            }
        }

        private void DeactivateLockScreenNotifications(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreenNotifications)
            {
                ThreadIsAlive.LockScreenNotifications = true;
                new Thread(Functions.DeactivateLockScreenNotifications).Start();
            }
        }

        private void ListUsers(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                ThreadStart childref = new(Functions.ListUsers);
                new Thread(Functions.ListUsers).Start();
            }
        }

        private void ActivateAutologin(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.ActivateAutologin).Start(new string[] { Username.Text, Password.Password });
                Username.Text = null;
                Password.Password = null;
            }
        }

        private void DeactivateAutologin(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.DeactivateAutologin).Start();
            }
        }

        private void HideUser(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.HideUser).Start(hideactionusername.Text);
                hideactionusername.Text = null;
            }
        }

        private void ShowUser(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.AutoLogin)
            {
                ThreadIsAlive.AutoLogin = true;
                new Thread(Functions.ShowUser).Start(hideactionusername.Text);
                hideactionusername.Text = null;
            }
        }

        private void DeactivateLockScreen(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LockScreen)
            {
                ThreadIsAlive.LockScreen = true;
                new Thread(Functions.DeactivateLockScreen).Start();
            }
        }

        private void DeactivateEncryptSMB(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SMB)
            {
                ThreadIsAlive.SMB = true;
                new Thread(Functions.DeactivateEncryptSMB).Start();
            }
        }

        private void VerboseUAC(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.UAC)
            {
                ThreadIsAlive.UAC = true;
                new Thread(Functions.VerboseUAC).Start();
            }
        }

        private void DefaultUAC(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.UAC)
            {
                ThreadIsAlive.UAC = true;
                new Thread(Functions.DefaultUAC).Start();
            }
        }

        private void PagefileClear(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Pagefile)
            {
                ThreadIsAlive.Pagefile = true;
                new Thread(Functions.PagefileClear).Start();
            }
        }

        private void PagefileDefault(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Pagefile)
            {
                ThreadIsAlive.Pagefile = true;
                new Thread(Functions.PagefileDefault).Start();
            }
        }

        private void RequireCtrl(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RequireCtrl)
            {
                ThreadIsAlive.RequireCtrl = true;
                new Thread(Functions.RequireCtrl).Start();
            }
        }

        private void Backgroundappsno(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Backgroundapps)
            {
                ThreadIsAlive.Backgroundapps = true;
                new Thread(Functions.Backgroundappsno).Start();
            }
        }

        private void Backgroundappsyes(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Backgroundapps)
            {
                ThreadIsAlive.Backgroundapps = true;
                new Thread(Functions.Backgroundappsyes).Start();
            }
        }

        private void DontRequireCtrl(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.RequireCtrl)
            {
                ThreadIsAlive.RequireCtrl = true;
                new Thread(Functions.DontRequireCtrl).Start();

            }
        }

        private void Zip7(Object sender, RoutedEventArgs e)
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

        private void VCRedist(Object sender, RoutedEventArgs e)
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

        private void Java(Object sender, RoutedEventArgs e)
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

        private void Codecs(Object sender, RoutedEventArgs e)
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

        private void Harden(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Harden)
            {
                ThreadIsAlive.Harden = true;
                new Thread(Functions.Harden).Start();
            }
        }

        //private void ActivateApplicationGuard(Object sender, RoutedEventArgs e)
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

        //private void DeactivateApplicationGuard(Object sender, RoutedEventArgs e)
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

        //private void DeactivateVBS(Object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.VBS)
        //    {
        //        ThreadIsAlive.VBS = true;
        //        new Thread(Functions.DeactivateVBS).Start();
        //    }
        //}

        //private void ActivateVBS(Object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.VBS)
        //    {
        //        ThreadIsAlive.VBS = true;
        //        new Thread(Functions.ActivateVBS).Start();
        //    }
        //}

        private void AllowUSBCode(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.USBExecution)
            {
                ThreadIsAlive.USBExecution = true;
                new Thread(Functions.AllowUSBCode).Start();
            }
        }

        private void FastWUPDATE(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.WindowsUpdate)
            {
                ThreadIsAlive.WindowsUpdate = true;
                new Thread(Functions.FastWUPDATE).Start();
            }
        }

        private void BlockUSBCode(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.USBExecution)
            {
                ThreadIsAlive.USBExecution = true;
                new Thread(Functions.BlockUSBCode).Start();
            }
        }

        //private void ActivateCFG(Object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.CFG)
        //    {
        //        ThreadIsAlive.CFG = true;
        //        new Thread(Functions.ActivateCFG).Start();
        //    }
        //}

        //private void DeactivateCFG(Object sender, RoutedEventArgs e)
        //{
        //    if (!ThreadIsAlive.CFG)
        //    {
        //        ThreadIsAlive.CFG = true;
        //        new Thread(Functions.DeactivateCFG).Start();
        //    }
        //}

        private void UserAutostart(Object sender, RoutedEventArgs e)
        {
            new Thread(Functions.UserAutostart).Start();
        }

        private void GlobalAutostart(Object sender, RoutedEventArgs e)
        {
            new Thread(Functions.GlobalAutostart).Start();
        }

        private void StandartBootMenuePolicy(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BMP)
            {
                ThreadIsAlive.BMP = true;
                new Thread(Functions.StandartBootMenuePolicy).Start();
            }
        }

        private void LegacyBootMenuePolicy(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.BMP)
            {
                ThreadIsAlive.BMP = true;
                new Thread(Functions.LegacyBootMenuePolicy).Start();
            }
        }

        private void SetMaxFailedLoginAttemptsBoot(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.MaxFailedLoginAttempts)
            {
                ThreadIsAlive.MaxFailedLoginAttempts = true;
                new Thread(Functions.SetMaxFailedLoginAttemptsBoot).Start(MfLan.Text);
                MfLan.Text = null;
            }
        }

        private void SetScreenTimeout(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.ScreenTimeOut)
            {
                ThreadIsAlive.ScreenTimeOut = true;
                new Thread(Functions.SetScreenTimeout).Start(Timeout.Text);
                Timeout.Text = null;
            }
        }

        private void DefaultCMDBehavior(Object sender, RoutedEventArgs e)
        {
            Execute.EXE("cmd.exe", "/c ftype batfile=\"%1\" %*", true, false);
            Execute.EXE("cmd.exe", "/c ftype cmdfile=\"%1\" %*", true, false);
            Write("Default .bat and .cmd behaviour\n\n", "darkcyan", null);
        }

        private void SystemLock(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.MaxFailedLoginAttempts)
            {
                ThreadIsAlive.SystemLock = true;
                new Thread(Functions.SystemLock).Start(new string[] { attempts.Text, duration.Text });
                attempts.Text = null;
                duration.Text = null;
            }
        }

        private void CheckHealth(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.CheckHealth).Start();
            }
        }

        private void ScanHealth(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.ScanHealth).Start();
            }
        }

        private void RestoreHealth(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.RestoreHealth).Start();
            }
        }

        private void SFCScan(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.SystemCheck)
            {
                ThreadIsAlive.SystemCheck = true;
                new Thread(Functions.SFCScan).Start();
            }
        }

        public void SelectWLanDir(Object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new()
            {
                SelectedPath = "C:\\Users\\" + UserPath
            };
            DialogResult result = ofd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                wlanconfigdir.Text = ofd.SelectedPath;
            }
        }

        private void AllProfilesWLanFalse(Object sender, RoutedEventArgs e)
        {
            AllProfilesWLan = false;
        }

        private void AllProfilesWLanTrue(Object sender, RoutedEventArgs e)
        {
            AllProfilesWLan = true;
        }

        private void ExportWLanConfig(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.IOWLanConfig)
            {
                ThreadIsAlive.IOWLanConfig = true;
                new Thread(Functions.ExportWLanConfig).Start(wlanconfigdir.Text);
            }
        }

        private void ImportWLanConfig(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.IOWLanConfig)
            {
                ThreadIsAlive.IOWLanConfig = true;
                new Thread(Functions.ImportWLanConfig).Start(wlanconfigdir.Text);
            }
        }

        private void RemWebView(Object sender, RoutedEventArgs e)
        {
            if (ThreadIsAlive.RMEdge)
            {
                Write("Wait for Edge to uninstall\n\n", "yellow", null);
            }
            else if (!ThreadIsAlive.WebView)
            {
                ThreadIsAlive.WebView = true;
                new Thread(Functions.RemWebView).Start();
            }
        }

        private void Curser(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.Curser)
            {
                ThreadIsAlive.Curser = true;
                new Thread(Functions.Curser).Start();
            }
        }

        private void LSASS_ON(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LSASS)
            {
                ThreadIsAlive.LSASS = true;
                new Thread(Functions.LSASS_ON).Start();
            }
        }

        private void LSASS_OFF(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.LSASS)
            {
                ThreadIsAlive.LSASS = true;
                new Thread(Functions.LSASS_OFF).Start();
            }
        }

        private void NoTCPTune(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.TCPTune)
            {
                ThreadIsAlive.TCPTune = true;
                new Thread(Functions.NoTCPTune).Start();
            }
        }

        private void YesTCPTune(Object sender, RoutedEventArgs e)
        {
            if (!ThreadIsAlive.TCPTune)
            {
                ThreadIsAlive.TCPTune = true;
                new Thread(Functions.YesTCPTune).Start();
            }
        }

        //

        private void Info(Object sender, RoutedEventArgs e)
        {
            Write("═══════════════ Info Start ═══════════════\n\n", "green", null);

            Write("═══ General Settings ═══\n", "darkgreen", null);

            Write("Activate Windows Defender Sandbox\n", "gray", null);

            Write("Set VeraCrypt as Trusted Process\n", "gray", null);

            Write("Properly handling Ethernet connections (IgnoreNonRoutableEthernet=1)\n", "gray", null);

            Write("Preventing Wi-Fi disconnecting when Ethernet\n", "gray", null);

            Write("Disable lockscreen tips and tricks\n", "gray", null);

            Write("Show encrypted or compressed NTFS files in color\n", "gray", null);

            Write("Set automatic crash dump creation to 3 (small memory dump file)\n", "gray", null);

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

            Write("Protect common Objects\n", "gray", null);

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
            Write("(", "darkgray", null);
            Write("v1.8", "gray", null);
            Write(") with permission from 'WindowsAddict'\n\n", "darkgray", null);

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

        private void Debug(Object sender, RoutedEventArgs e)
        {
            //String[] EmbededResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            try
            {
                PowerShell.Create().AddCommand("Add-MpPreference").Invoke();
            }
            catch
            {
                Console.WriteLine("s");
            }

        }

        //###############################################################################

        //Window UI buttons

        //minimize
        private void Button_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MinimizeButtonMouseIsOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimizeButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2d2d2d"));
        }

        private void MinimizeButtonMouseIsNotOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimizeButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
        }

        private void MinimizeButtonMouseClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MinimizeButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a2a"));
        }
        
        //close button

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseButtonColorMouseIsOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c42b1c"));
        }

        private void CloseButtonColorMouseIsNotOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
        }

        private void CloseButtonColorMouseClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButtonColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b22a1b"));
        }
    }
}
