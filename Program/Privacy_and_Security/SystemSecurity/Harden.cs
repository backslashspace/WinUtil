using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class SystemSecurity
    {
        private const String HARDEN_SOURCE = "Harden";

        private async static Task Harden()
        {
            OptionSelector.Option[] options =
            [
               new(true, false, "Activate Windows Defender network protection",                     null!),
               new(true, false, "Activate Windows Defender Sandbox",                                null!),
               new(false, false, "Add VeraCrypt as a trusted process",                              null!),
               new(true, false, "Activate PUA Protection (potentially unwanted applications)",      null!),
               new(true, false, "*Only initialize 'good' boot-start drivers",                       "This policy setting allows you to specify which boot-start drivers are initialized based on a classification determined by an Early Launch Antimalware boot-start driver. The Early Launch Antimalware boot-start driver can return the following classifications for each boot-start driver:\r\n- Good: The driver has been signed and has not been tampered with.\r\n- Bad: The driver has been identified as malware. It is recommended that you do not allow known bad drivers to be initialized.\r\n- Bad, but required for boot: The driver has been identified as malware, but the computer cannot successfully boot without loading this driver.\r\n- Unknown: This driver has not been attested to by your malware detection application and has not been classified by the Early Launch Antimalware boot-start driver.\r\n\r\nIf you enable this policy setting you will be able to choose which boot-start drivers to initialize the next time the computer is started.\r\n\r\nIf you disable or do not configure this policy setting, the boot start drivers determined to be Good, Unknown or Bad but Boot Critical are initialized and the initialization of drivers determined to be Bad is skipped.\r\n\r\nIf your malware detection application does not include an Early Launch Antimalware boot-start driver or if your Early Launch Antimalware boot-start driver has been disabled, this setting has no effect and all boot-start drivers are initialized. "),
               new(true, false, "*Activate Windows Defender Defender Exploit Guard",                "Set-Processmitigation -System -Enable DEP,EmulateAtlThunks,HighEntropy,SEHOP,SEHOPTelemetry,TerminateOnError\n\nhttps://learn.microsoft.com/de-de/defender-endpoint/customize-exploit-protection"),
               new(true, false, "*Deactivate WDigest for login",                                    "Old Challenge/Response-Protocol"),
               new(true, false, "Enable Audit mode for LSASS.exe",                                  null!),
               new(true, false, "Set default app for .hta to notepad",                              null!),
               new(true, false, "Set default app for .wsh to notepad",                              null!),
               new(true, false, "Set default app for .wsf to notepad",                              null!),
               new(true, false, "Set default app for .js to notepad",                               null!),
               new(true, false, "Set default app for .jse to notepad",                              null!),
               new(false, false, "Set default app for .bat to notepad",                             null!),
               new(false, false, "Set default app for .cmd to notepad",                             null!),
               new(true, false, "Set default app for .vbs to notepad",                              null!),
               new(true, false, "Set default app for .vbe to notepad",                              null!),
               new(true, false, "*Block system apps in firewall (see tooltip)",                 "notepad.exe, regsvr32.exe, mshta.exe, wscript.exe, cscript.exe, runscripthelper.exe, hh.exe"),
               new(true, false, "Deactivate anonymous Sam-Account enumeration",                     null!),
               new(true, false, "Activate Anti-spoof for facial recognition",                       null!),
               new(true, false, "Deactivate camera on locked screen",                               null!),
               new(true, false, "Deactivate app voice commands in locked state",                    null!),
               new(true, false, "Deactivate windows voice commands",                                null!),
            ];

            OptionSelector optionSelector = new("Harden Options", options, new(true, 0, "harden.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return;

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            await Task.Run(() =>
            {
                try
                {
                    if (optionSelector.Result.UserSelection[0])
                    {
                        Log.FastLog("Activating Windows Defender network protection", LogSeverity.Info, HARDEN_SOURCE);

                        PowerShell.Create().AddCommand("Set-MpPreference").AddParameter("-EnableNetworkProtection", "Enabled").Invoke();
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Activating Windows Defender network protection failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[1])
                    {
                        Log.FastLog("Activating Windows Defender Sandbox", LogSeverity.Info, HARDEN_SOURCE);

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\setx.exe", "/M MP_FORCE_USE_SANDBOX 1", true, true, true)).Success)
                        {
                            Log.FastLog("Activating Windows Defender Sandbox failed, unable to start setx.exe", LogSeverity.Error, HARDEN_SOURCE);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Activating Windows Defender Sandbox failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[2])
                    {
                        Log.FastLog("Making VeraCrypt a trusted process", LogSeverity.Info, HARDEN_SOURCE);

                        PowerShell.Create().AddCommand("Add-MpPreference").AddParameter("-ExclusionProcess", "C:\\Program Files\\VeraCrypt\\VeraCrypt.exe").Invoke();
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Making VeraCrypt a trusted process failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[3])
                    {
                        Log.FastLog("Activating PUA Protection (potentially unwanted applications)", LogSeverity.Info, HARDEN_SOURCE);

                        PowerShell.Create().AddCommand("Set-MpPreference").AddParameter("-PUAProtection", "Enabled").Invoke();
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Activating PUA Protection (potentially unwanted applications) failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[4])
                    {
                        Log.FastLog("Activating only initialize 'good' boot-start drivers", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Policies\EarlyLaunch", "DriverLoadPolicy", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Activating only initialize 'good' boot-start drivers failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[5])
                    {
                        Log.FastLog("Activating Windows Defender Defender Exploit Guard", LogSeverity.Info, HARDEN_SOURCE);

                        PowerShell.Create().AddCommand("Set-Processmitigation").AddParameter("-System").AddParameter("-Enable", new String[] { "DEP", "EmulateAtlThunks", "HighEntropy", "SEHOP", "SEHOPTelemetry", "TerminateOnError" }).Invoke();
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Activating Windows Defender Defender Exploit Guard failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[6])
                    {
                        Log.FastLog("Deactivating WDigest for login", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\WDigest", "UseLogonCredential", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Deactivating WDigest for login failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[7])
                    {
                        Log.FastLog("Enabling Audit mode for LSASS.exe", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\LSASS.exe", "AuditLevel", 8, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Enabling Audit mode for LSASS.exe failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                if (optionSelector.Result.UserSelection[8]) FTYPE("htafile", "Setting default app for .hta to notepad");
                if (optionSelector.Result.UserSelection[9]) FTYPE("wshfile", "Setting default app for .wsh to notepad");
                if (optionSelector.Result.UserSelection[10]) FTYPE("wsffile", "Setting default app for .wsf to notepad");
                if (optionSelector.Result.UserSelection[11]) FTYPE("jsfile", "Setting default app for .js to notepad");
                if (optionSelector.Result.UserSelection[12]) FTYPE("jsefile", "Setting default app for .jse to notepad");
                if (optionSelector.Result.UserSelection[13]) FTYPE("batfile", "Setting default app for .bat to notepad");
                if (optionSelector.Result.UserSelection[14]) FTYPE("cmdfile", "Setting default app for .cmd to notepad");
                if (optionSelector.Result.UserSelection[15]) FTYPE("vbsfile", "Setting default app for .vbs to notepad");
                if (optionSelector.Result.UserSelection[16]) FTYPE("vbefile", "Setting default app for .vbe to notepad");

                if (optionSelector.Result.UserSelection[17])
                {
                    Log.FastLog("Blocking apps in firewall", LogSeverity.Info, HARDEN_SOURCE);

                    try
                    {
                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block notepad.exe netconns\" program=\"%systemroot%\\system32\\notepad.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking notepad.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block regsvr32.exe netconns\" program=\"%systemroot%\\system32\\regsvr32.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking regsvr32.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block calc.exe netconns\" program=\"%systemroot%\\system32\\calc.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking calc.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block mshta.exe netconns\" program=\"%systemroot%\\system32\\mshta.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking mshta.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block wscript.exe netconns\" program=\"%systemroot%\\system32\\wscript.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking wscript.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block cscript.exe netconns\" program=\"%systemroot%\\system32\\cscript.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking cscript.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block runscripthelper.exe netconns\" program=\"%systemroot%\\system32\\runscripthelper.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking runscripthelper.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\netsh.exe", "advfirewall firewall add rule name=\"Block hh.exe netconns\" program=\"%systemroot%\\system32\\hh.exe\" protocol=tcp dir=out enable=yes action=block profile=any", true, true, true)).Success)
                        {
                            Log.FastLog("Blocking hh.exe failed, unable to start netsh.exe", LogSeverity.Error, HARDEN_SOURCE);
                            goto FWSKIP;
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog("Blocking apps in firewall failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                    }
                }

                FWSKIP:

                try
                {
                    if (optionSelector.Result.UserSelection[18])
                    {
                        Log.FastLog("Preventing anonymous Sam-Account enumeration", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RestrictAnonymousSAM", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "RestrictAnonymous", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "EveryoneIncludesAnonymous", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Preventing anonymous Sam-Account enumeration failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[19])
                    {
                        Log.FastLog("Activating Anti-spoof for facial recognition", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures", "EnhancedAntiSpoofing", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Activating Anti-spoof for facial recognition failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[20])
                    {
                        Log.FastLog("Deactivating camera on locked screen", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreenCamera", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Deactivating camera on locked screen failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[21])
                    {
                        Log.FastLog("Deactivating app voice commands in locked state", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoiceAboveLock", 2, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Deactivating app voice commands in locked state failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[22])
                    {
                        Log.FastLog("Deactivating windows voice commands", LogSeverity.Info, HARDEN_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsActivateWithVoice", 2, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Deactivating windows voice commands in locked state failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
                }
            });

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, HARDEN_SOURCE);
        }

        private static void FTYPE(String extension, String logMessage)
        {
            try
            {
                Log.FastLog(logMessage, LogSeverity.Info, HARDEN_SOURCE);

                Util.Execute.Process(new("c:\\windows\\system32\\cmd.exe", $"/c ftype {extension}=\"%SystemRoot%\\system32\\notepad.EXE\" \"%1\"", true, true, true));
            }
            catch (Exception exception)
            {
                Log.FastLog(logMessage + " failed with: " + exception.Message, LogSeverity.Error, HARDEN_SOURCE);
            }
        }
    }
}