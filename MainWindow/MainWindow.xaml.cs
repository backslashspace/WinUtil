using System;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Windows.Media;
using System.Security.Cryptography;

namespace WinUtil
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            SoftDebug();
#endif

            //pin whole application (?????)
            Application.Dispatcher = Dispatcher.CurrentDispatcher;
            Application.Object = this;

            Boolean hadErrors = VerifyDLLs();

            if (hadErrors)
            {
                DeactivateWorker();

                LogBoxAdd("\nTerminating application", Brushes.Red);

                Task.Run(() => ExitOnError());

                return;
            }
            else
            {
                LogBoxAdd("Verified program files\n", Brushes.LightGreen, stayInLine: true);
            }

            Loaded += InitiateApplication;
        }

        private static void ExitOnError()
        {
            Task.Delay(7000).Wait();

            Environment.Exit(1);
        }

        private Boolean VerifyDLLs()
        {
            ActivateWorker();

            Boolean hadErrors = false;
            String errorString = "";

            if (!Validator("ManagedNativeWifi.dll").Equals(EXT_DLL.ManagedNativeWifi, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid ManagedNativeWifi.dll");
            }

            if (!Validator("BSS.HashTools.dll").Equals(EXT_DLL.BSS_HashTools, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid WinX64_HashTools.dll");
            }

            if (!Validator("BSS.Launcher.dll").Equals(EXT_DLL.BSS_Launcher, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid WinX64_Launcher.dll");
            }

            if (!Validator("BSS.System.dll").Equals(EXT_DLL.BSS_System, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid WinX64_System.dll");
            }

            if (!Validator("System.CodeDom.dll").Equals(EXT_DLL.CodeDom, StringComparison.OrdinalIgnoreCase))
            {
                hadErrors = true;
                BuildErrorString(ref errorString, "[Critical] invalid System.CodeDom.dll");
            }

            if (hadErrors)
            {
                LogBoxAdd(errorString, Brushes.OrangeRed, stayInLine: true);
            }

            return hadErrors;

            //# # # # # # # # # #

            static String Validator(String filePath)
            {
                if (!File.Exists(filePath))
                {
                    return "";
                }

                using SHA256 sha256 = SHA256.Create();

                String hash;

                try
                {
                    using FileStream fileStream = File.OpenRead(filePath);

                    hash = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", String.Empty);
                }
                catch
                {
                    hash = null;
                }

                return hash;
            }

            static void BuildErrorString(ref String errorString, String line)
            {
                errorString += errorString == "" ? line : "\n" + line;
            }
        }

        private async void InitiateApplication(object sender, EventArgs e)
        {
            await Task.Run(() => SystemInfo.Load());

            DeactivateWorker();
        }
        
        //#######################################################################################################

        private async void Restart_Explorer_Button(object sender, RoutedEventArgs e)
        {
            Restart_Explorer.IsEnabled = false;
            
            await Common.RestartExplorer().ConfigureAwait(true);

            Restart_Explorer.IsEnabled = true;
        }
    }
}