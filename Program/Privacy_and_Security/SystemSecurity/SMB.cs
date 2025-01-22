using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class SystemSecurity
    {
        private const String SMB_SOURCE = "SMB";

        private async static Task SMB()
        {
            OptionSelector.Option[] options =
            [
               new(true, false, "Set minimum protocol version to 3.1.1",             null!),
               new(true, false, "*Set specific cipher suit order (hover for info)",  "Order: AES_256_GCM, AES_128_GCM, AES_256_CCM, AES_128_CCM"),
               new(true, false, "Only allow AES_256_GCM",                            null!),
               new(true, false, "Honor above mentioned cipher suite order",          null!),
               new(true, false, "Deactivate server auto-shares",                     null!),
               new(true, false, "Deactivate workstation auto-shares",                null!),
               new(true, false, "Hide server",                                       null!),
               new(true, false, "Enable security signatures",                        null!),
               new(true, false, "Enable encryption",                                 null!),
               new(true, false, "Require security signatures",                       null!),
               new(true, false, "Require encryption",                                null!),
            ];

            OptionSelector optionSelector = new("SMB Server Options", options, new(true, 0, "smb.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return;

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            await Task.Run(() =>
            {
                try
                {
                    if (optionSelector.Result.UserSelection[0])
                    {
                        Log.FastLog("Setting minimum protocol version to 3.1.1", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LanmanServer", "MinSmb2Dialect", 0x311, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to set minimum protocol version to 3.1.1: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (!optionSelector.Result.UserSelection[2] && optionSelector.Result.UserSelection[1])
                    {
                        Log.FastLog("Setting specific cipher suit order", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LanmanServer", "CipherSuiteOrder", new String[] { "AES_256_GCM", "AES_128_GCM", "AES_256_CCM", "AES_128_CCM" }, RegistryValueKind.MultiString);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "CipherSuiteOrder", new String[] { "AES_256_GCM", "AES_128_GCM", "AES_256_CCM", "AES_128_CCM" }, RegistryValueKind.MultiString);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Setting specific cipher suit order: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[2])
                    {
                        Log.FastLog("Only allowing AES_256_GCM", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LanmanServer", "CipherSuiteOrder", new String[] { "AES_256_GCM" }, RegistryValueKind.MultiString);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "CipherSuiteOrder", new String[] { "AES_256_GCM" }, RegistryValueKind.MultiString);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Only allowing AES_256_GCM: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[3])
                    {
                        Log.FastLog("Setting honor cipher suite order", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LanmanServer", "HonorCipherSuiteOrder", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to set honor cipher suite order: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[4])
                    {
                        Log.FastLog("Deactivating server auto-shares", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "AutoShareServer", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to deactivate server auto-shares: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[5])
                    {
                        Log.FastLog("Deactivating workstation auto-shares", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "AutoShareWks", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to deactivate workstation auto-shares: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[6])
                    {
                        Log.FastLog("Hiding server", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "Hidden", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to hide server: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[7])
                    {
                        Log.FastLog("Enabling security signature", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "enablesecuritysignature", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to enable security signature: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[8])
                    {
                        Log.FastLog("Enabling encryption", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "EncryptData", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to enable encryption: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[9])
                    {
                        Log.FastLog("Setting require security signature", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "RequireSecuritySignature", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to set require security signature: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[10])
                    {
                        Log.FastLog("Rejecting unencrypted access", LogSeverity.Info, SMB_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\LanmanServer\Parameters", "RejectUnencryptedAccess", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to set reject unencrypted access: " + exception.Message, LogSeverity.Error, SMB_SOURCE);
                }

                Log.FastLog("Restarting smb server service", LogSeverity.Info, SMB_SOURCE);

                Util.Execute.Process(new("c:\\windows\\system32\\net.exe", "stop server", true, true, true));
                Util.Execute.Process(new("c:\\windows\\system32\\net.exe", "start server", true, true, true));
            });

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            Log.FastLog("Done", LogSeverity.Info, SMB_SOURCE);
        }
    }
}