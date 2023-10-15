using System;

namespace WinUtil
{
    public partial class MainWindow
    {
        #region Types

        internal enum OSType
        {
            Server = 0,

            Client = 1,
        }

        internal enum WindowsUIVersion
        {
            Windows10 = 0,

            Windows11 = 1,
        }

        internal enum WindowPlatformFeatureCompliance
        {
            Windows10_or_Older = 0,

            Windows11_Server2022 = 1,
        }

        #endregion

        #region AppVars

        internal static String AdminGroupName { get; set; }

        internal static String ExePath { get; set; }

        internal static Boolean IsActivated { get; set; }

        internal static String User { get; set; }

        internal static String UserPath { get; set; }

        internal static Boolean AllProfilesWLan { get; set; }

        internal static Int16 ActivationClicks { get; set; }

        internal sealed class ThisMachine
        {
            public static OSType OSType;

            public static String OSEdition = null;

            public static Int32 OSMajorVersion = 0;

            public static Int32 OSMinorVersion = 0;

            public static WindowPlatformFeatureCompliance WindowPlatformFeatureCompliance;

            public static String NetBiosHostname = null;

            public static String Hostname = null;

            public static WindowsUIVersion UIVersion;

            public static Boolean IsUEFI = false;

            public static Boolean SecureBootEnabled = false;

            public static Boolean IsInDomain = false;

            public static SByte WindowsLicenseStatus = SByte.MinValue;
        }

        #endregion

        #region External Resources

        private sealed class ExtResources
        {
            public const String CustomWinMessageBox = "4f383df5517addac328543b732cecd12f14aa93aa099e8a7d3cf00df45fe3805";
            public const String HashTools = "bd1add6c98b7406b779b3f2c20942f16523ce9841639918ee4c3ad719225d019";
            public const String PowershellHelper = "30154a0c4686bfc5e3e3efb0853f0e050db7d27ae59dffb8d54c06c9ec4f1d0c";
            public const String ProgramLauncher = "f4c3dedddeed357421c60ef17037c7536346aae985b33674f643e6c73eb8ab22";
            public const String RegistryTools = "0dcb3c87a8162acef6eb8c3ae5fa0ad62ebd43968ef69e0ce48d55bbc10aeb2f";
            public const String ServiceTools = "ba4add34b1aba3e01bd38dc284b60927d3b5c81645d3f63b1a9ae70fd59c64c0";
            public const String WinUser = "861e400ec6c99ae48ec4392cc10711f634f624cd8e8e82e829bc6d5e581c7a2f";

            public const String ManagedNativeWifi = "c590995858bc7f5624ae4161e7b22bfe834c17b6a6fc0c4dfd7207d1b11a3ca3";
            public const String SystemRuntimeCompilerServicesUnsafe = "37768488e8ef45729bc7d9a2677633c6450042975bb96516e186da6cb9cd0dcf";
            public const String SystemBuffersdll = "accccfbe45d9f08ffeed9916e37b33e98c65be012cfff6e7fa7b67210ce1fefb";
            public const String SystemDiagnosticsDiagnosticSourcedll = "5b8ccdda36486950de37484c25e1334376431e52176c32f87dd730690b273e3b";
            public const String SystemMemorydll = "bf3fb84664f4097f1a8a9bc71a51dcf8cf1a905d4080a4d290da1730866e856f";
            public const String SystemNumericsVectorsdll = "1d3ef8698281e7cf7371d1554afef5872b39f96c26da772210a33da041ba1183";
        }

        internal sealed class Const
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

        #endregion












        internal sealed class ThreadIsAlive
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
    }
}