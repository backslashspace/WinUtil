using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class SecurityConfigWindow
    {
        private const String PRIVACY_SOURCE = "Privacy";

        private async static Task Privacy()
        {
            OptionSelector.Option[] options =
            [
                new(true, false, "Don't track process starts for suggestions",                                      null!),
                new(true, false, "Don't allow websites to access language lists",                                   null!),
                new(true, false, "Remove advertisement ID",                                                         null!),
                new(true, false, "Disable online voice recognition",                                                null!),
                new(true, false, "Don't collect/send telemetry",                                                    null!),
                new(true, false, "Deactivate event transcripts",                                                    null!),
                new(true, false, "Deactivate tailored experience",                                                  null!),
                new(true, false, "Deactivate feedback",                                                             null!),
                new(true, false, "Deactivate recent search history",                                                null!),
                new(true, false, "Deactivate search highlights",                                                    null!),
                new(true, false, "Deactivate safe search",                                                          null!),
                new(true, false, "Deactivate browsing history in Search",                                           null!),
                new(true, false, "Remove Copilot and Recall",                                                       null!),
                new(true, false, "*Disable/Manual potential services",                                              "M:HomeGroupListener, D:AJRouter, D:AssignedAccessManagerSvc, M:CDPSvc, D:diagnosticshub.standardcollector.service, M:EdgeUpdate, M:PcaSvc, D:RemoteRegistry, M:StateRepository"),
                new(true, false, "Turn off lock screen notifications",                                              null!),
                new(true, false, "Turn off reminders and calls on the lock screen",                                 null!),
                new(true, false, "Deactivate remote control",                                                       null!),
                new(true, false, "Deactivate app access to account information",                                    null!),
                new(true, false, "Deactivate upload of user activities",                                            null!),
                new(true, false, "Deactivate handwriting error reports",                                            null!),
                new(true, false, "Deactivate advertising via Bluetooth",                                            null!),
                new(true, false, "Deactivate cross device message and clipboard sync",                              null!),
                new(true, false, "Deactivate Cortana",                                                              null!),
                new(true, false, "Deactivate reports to SpyNet",                                                    null!),
                new(true, false, "Don't remember recently opened files",                                            null!),
            ];

            OptionSelector optionSelector = new("Privacy", options, new(true, 0, "privacy.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return;

            // # # # # # # # # # # # # # # # # # # # # # # # # #
            
            await Task.Run(() =>
            {
                try
                {
                    if (optionSelector.Result.UserSelection[0])
                    {
                        Log.FastLog("[USER] Don't track process starts for suggestions", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Don't track process starts for suggestions failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[1])
                    {
                        Log.FastLog("[USER] Don't allow websites to access language lists", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Don't allow websites to access language lists failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[2])
                    {
                        Log.FastLog("[USER] Removing advertisement ID", LogSeverity.Info, PRIVACY_SOURCE);

                        RegistryKey advertisingInfo = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion", true);
                        advertisingInfo.DeleteSubKeyTree("AdvertisingInfo", false);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Removing advertisement ID failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[3])
                    {
                        Log.FastLog("[USER] Disabling online voice recognition", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Disabling online voice recognition failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[4])
                    {
                        Log.FastLog("[MACHINE] Don't collect/send telemetry - full disable only possible on Windows Enterprise, Education and Server", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CPSS\Store\AllowTelemetry", "Value", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CPSS\Store", "AllowTelemetry", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CPSS\DevicePolicy\AdvertisingInfo", "DefaultValue", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CPSS\DevicePolicy\AllowTelemetry", "DefaultValue", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CPSS\UserPolicy\AdvertisingInfo", "DefaultValue", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CPSS\Store", "AllowTelemetry", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "MaxTelemetryAllowed", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\AppCompat", "AITEnable", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Don't collect/send telemetry failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[5])
                    {
                        Log.FastLog("[MACHINE] Deactivating event transcripts", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack\EventTranscriptKey", "EnableEventTranscript", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating event transcripts failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[6])
                    {
                        Log.FastLog("[USER] Deactivating tailored experience", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating tailored experience failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[7])
                    {
                        Log.FastLog("[USER] Deactivating feedback", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating feedback failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[8])
                    {
                        Log.FastLog("[USER] Deactivating recent search history", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsAADCloudSearchEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsMSACloudSearchEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDeviceSearchHistoryEnabled", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating recent search history failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[9])
                    {
                        Log.FastLog("[USER][MACHINE] Deactivating search highlights", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\Windows Search", "EnableDynamicContentInWSB", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating search highlights failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[10])
                    {
                        Log.FastLog("[USER] Deactivating safe search", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "SafeSearchMode", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating safe search failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[11])
                    {
                        Log.FastLog("[USER] Deactivating browsing history in Search", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_RecoPersonalizedSites", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating browsing history in Search failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[12])
                    {
                        Log.FastLog("Removing Copilot and Recall", LogSeverity.Info, PRIVACY_SOURCE);

                        Util.Execute.Process(new(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", "-c \"Get-AppxPackage -AllUsers | Where-Object {$_.Name -Like '*Microsoft.Copilot*'} | Remove-AppxPackage -AllUsers -ErrorAction Continue\"", true, true, true));
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Shell\\BrandedKey", "BrandedKeyChoiceType", "Search", RegistryValueKind.String);
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowCopilotButton", 0, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Shell\\Copilot\\BingChat", "IsUserEligible", 0, RegistryValueKind.DWord);

                        Util.Execute.Process(new(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", "-c \"Disable-WindowsOptionalFeature -Online -FeatureName 'Recall'\"", true, true, true));
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\WindowsAI\\DisableAIDataAnalysis", "value", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI\\DisableAIDataAnalysis", "value", 1, RegistryValueKind.DWord);

                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Paint", "DisableCocreator", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Paint", "DisableGenerativeFill", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Paint", "DisableImageCreator", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI", "AllowRecallEnablement", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Removing Copilot and Recall failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[13])
                    {
                        Log.FastLog("Changing services", LogSeverity.Info, PRIVACY_SOURCE);

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config HomeGroupListener start=demand", true, true, true)).Success)
                        {
                            Log.FastLog("HomeGroupListener->demand failed", LogSeverity.Error, PRIVACY_SOURCE);
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config AJRouter start=disabled", true, true, true)).Success)
                        {
                            Log.FastLog("AJRouter->demand disabled", LogSeverity.Error, PRIVACY_SOURCE);
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config AssignedAccessManagerSvc start=disabled", true, true, true)).Success)
                        {
                            Log.FastLog("AssignedAccessManagerSvc->disabled failed", LogSeverity.Error, PRIVACY_SOURCE);
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config CDPSvc start=demand", true, true, true)).Success)
                        {
                            Log.FastLog("CDPSvc->demand failed", LogSeverity.Error, PRIVACY_SOURCE);
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config diagnosticshub.standardcollector.service start=disabled", true, true, true)).Success)
                        {
                            Log.FastLog("diagnosticshub.standardcollector.service->disabled failed", LogSeverity.Error, PRIVACY_SOURCE);
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config EdgeUpdate start=demand", true, true, true)).Success)
                        {
                            Log.FastLog("EdgeUpdate->demand failed", LogSeverity.Error, PRIVACY_SOURCE);
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config RemoteRegistry start=disabled", true, true, true)).Success)
                        {
                            Log.FastLog("RemoteRegistry->disabled failed", LogSeverity.Error, PRIVACY_SOURCE);
                        }

                        if (!Util.Execute.Process(new("c:\\windows\\system32\\sc.exe", "config StateRepository start=demand", true, true, true)).Success)
                        {
                            Log.FastLog("StateRepository->demand failed", LogSeverity.Error, PRIVACY_SOURCE);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("Failed to change some services: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[14])
                    {
                        Log.FastLog("[USER] Turning off lock screen notifications", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PushNotifications", "LockScreenToastEnabled", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Turning off lock screen notifications failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[15])
                    {
                        Log.FastLog("[USER] Turning off reminders and calls on the lock screen", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings", "NOC_GLOBAL_SETTING_ALLOW_CRITICAL_TOASTS_ABOVE_LOCK", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Turning off reminders and calls on the lock screen failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[16])
                    {
                        Log.FastLog("[MACHINE] Deactivating remote control", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Remote Assistance", "fAllowToGetHelp", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Remote Assistance", "fAllowFullControl", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating remote control failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[17])
                    {
                        Log.FastLog("[MACHINE] Deactivating app access to account information", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", "Value", "Deny", RegistryValueKind.String);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Remote Assistance", "fAllowFullControl", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating app access to account information failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[18])
                    {
                        Log.FastLog("[MACHINE] Deactivating the upload of user activities", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating the upload of user activities failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[19])
                    {
                        Log.FastLog("[MACHINE] Deactivating handwriting error reports", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports", "PreventHandwritingErrorReports", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\TabletPC", "PreventHandwritingDataSharing", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating handwriting error reports failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[20])
                    {
                        Log.FastLog("[MACHINE] Deactivating advertising via Bluetooth", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Bluetooth", "AllowAdvertising", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating advertising via Bluetooth failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[21])
                    {
                        Log.FastLog("[MACHINE] Deactivating cross device message and clipboard sync", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Messaging", "AllowMessageSync", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Messaging", "AllowCrossDeviceClipboard", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating cross device message and clipboard sync failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[22])
                    {
                        Log.FastLog("[MACHINE][USER] Deactivating Cortana", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana ", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortanaAboveLock ", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\InputPersonalization", "AllowInputPersonalization ", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Windows Search", "CortanaConsent ", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy ", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization", "RestrictImplicitTextCollection ", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection ", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts ", 0, RegistryValueKind.DWord);


                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE][USER] Deactivating narrator hotkey failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[23])
                    {
                        Log.FastLog("[MACHINE] Deactivating reports to SpyNet", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows Defender\Spynet", "SubmitSamplesConsent ", 2, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows Defender\Spynet", "SpyNetReporting ", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\MRT", "DontReportInfectionInformation ", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating reports to SpyNet failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[24])
                    {
                        Log.FastLog("[USER] Don't remember recently opened files", LogSeverity.Info, PRIVACY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs ", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Don't remember recently opened files failed with: " + exception.Message, LogSeverity.Error, PRIVACY_SOURCE);
                }

                Util.RestartExplorerForUser();
            });

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, PRIVACY_SOURCE);
        }
    }
}