using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    public sealed partial class BaseConfigWindow
    {
        private const String PACIFY_SOURCE = "Pacify";

        private async static Task Pacify()
        {
            OptionSelector.Option[] options =
            [
                new(true, false, "Show file extensions",                                                                    null!),
                new(true, false, "Set default explorer page to \"This PC\"",                                                null!),
                new(true, false, "Mark encrypted/compressed NTFS files/folders with colors",                                null!),
                new(true, false, "*Activate explorer compact mode",                                                         "This was the fault in windows 10."),
                new(true, false, "Explorer don't pretty path",                                                              null!),
                new(true, false, "*Show file operations details",                                                           "Enable the transfer speed graph."),
                new(true, false, "Deactivate search bar web search",                                                        null!),
                new(true, false, "Disable safe search",                                                                     null!),
                new(RunContextInfo.Windows.IsServer, !RunContextInfo.Windows.IsServer, "Enable disk usage in Task Manager", null!),
                new(true, false, "Disable 'No Microsoft Account Connected' Message in start menu",                          null!),
                new(true, false, "Deactivate narrator hotkey",                                                              null!),
                new(true, false, "Show Start menu Settings, Network and Explorer integration in start menu",                null!),
                new(true, false, "*Install HyperKey Deregisterer",                                                          "Starts right after the user initialization during logon and deregisters all hotkeys for a newly created shell process.\nThis process is not visible to the user and does not keep running in the background after initialization.\n\nExample: WIN+C\nSTRG+SHIFT+ALT+SPACE\nWIN+W\n etc"),
                new(true, false, "Disable Cortana",                                                                         null!),
                new(true, false, "*Disable GameDVR",                                                                        "Disables game bar."),
                new(true, false, "Deactivate StickyKeys",                                                                   null!),
                new(true, false, "Deactivate system ads / suggestions & auto app installation",                             null!),
                new(true, false, "Remove and deactivate Copilot",                                                           null!),
                new(true, false, "Remove and deactivate Recall",                                                            null!),
                new(true, false, "Don't ask for ways to get the most out of Windows",                                       null!),
                new(true, false, "Turn off auto installation of Microsoft Teams in the taskbar",                            null!),
                new(true, false, "Disable Windows Defender reminder to sign into a MS account",                             null!),
                new(true, false, "Disable Windows Defender reminder to turn on Edge SmartScreen filter",                    null!),
                new(true, false, "Disable News and Interest in taskbar",                                                    null!),
                new(true, false, "Disable OneDrive Notifications in Explorer",                                              null!),
                new(true, false, "Show more apps in the start menu",                                                        null!),
                new(true, false, "Enable Numlock on start",                                                                 null!),
                new(true, false, "*Don't automatically choose the appropriate folder type",                                 "This will speed up file explorer browsing in large directories, where normally it would first index all files and load all preview images of all videos"),
            ];

            OptionSelector optionSelector = new("Streamline & Pacify", options, new(true, 1, "pacify.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return;

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            await Task.Run(() =>
            {
                try
                {
                    if (optionSelector.Result.UserSelection[0])
                    {
                        Log.FastLog("[USER] Showing file extensions", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Showing file extensions failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[1])
                    {
                        Log.FastLog("[USER] Set default explorer page to \"This PC\"", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Set default explorer page to \"This PC\" failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[2])
                    {
                        Log.FastLog("[USER] Marking encrypted/compressed NTFS files/folders with colors", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowEncryptCompressedColor", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Marking encrypted/compressed NTFS files/folders with colors failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[3])
                    {
                        Log.FastLog("[USER] Activating explorer compact mode", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Activating explorer compact mode failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[4])
                    {
                        Log.FastLog("[USER] Explorer don't pretty path", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DontPrettyPath", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\CabinetState", "FullPath", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Explorer don't pretty path failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[5])
                    {
                        Log.FastLog("[USER] Showing file operations details", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager", "EnthusiastMode", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Showing file operations details failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[6])
                    {
                        Log.FastLog("[USER][Machine] Deactivating search bar web search", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowSearchToUseLocation", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWeb", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER][Machine] Deactivating search bar web search failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[7])
                    {
                        Log.FastLog("[USER] Disabling safe search", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\SearchSettings", "SafeSearchMode", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Disabling safe search failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (RunContextInfo.Windows.IsServer && optionSelector.Result.UserSelection[8])
                    {
                        Log.FastLog("[USER] Enabling disk performance counter", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Util.Execute.Process(new(@"C:\Windows\System32\diskperf.exe", "-Y", true, true, true));
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Enabling disk performance counter failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[9])
                    {
                        Log.FastLog("[USER] Disabling reminder to add Microsoft Account", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Disabling reminder to add Microsoft Account failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[10])
                    {
                        Log.FastLog("[USER] Deactivating narrator hotkey", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Narrator\NoRoam", "WinEnterLaunchEnabled", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating narrator hotkey failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[11])
                    {
                        Log.FastLog("[USER] Setting Start menu Settings, Network and Explorer integration", LogSeverity.Info, PACIFY_SOURCE);

                        Byte[] visiblePlacesData = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x86, 0x08, 0x73, 0x52, 0xaa, 0x51, 0x43, 0x42, 0x9f, 0x7b, 0x27, 0x76, 0x58, 0x46, 0x59, 0xd4, 0xbc, 0x24, 0x8a, 0x14, 0x0c, 0xd6, 0x89, 0x42, 0xa0, 0x80, 0x6e, 0xd9, 0xbb, 0xa2, 0x48, 0x82, 0x44, 0x81, 0x75, 0xfe, 0x0d, 0x08, 0xae, 0x42, 0x8b, 0xda, 0x34, 0xed, 0x97, 0xb6, 0x63, 0x94];
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Start", "VisiblePlaces", visiblePlacesData, RegistryValueKind.Binary);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Setting Start menu Settings, Network and Explorer integration failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[12])
                    {
                        Log.FastLog("[USER] Installing HyperKey Deregisterer", LogSeverity.Info, PACIFY_SOURCE);

                        Util.Execute.Result result = Util.Execute.Process(new(RunContextInfo.ExecutablePath + "\\assets\\HyperKey Deregisterer.exe", "/install", true, true, true));

                        if (!result.Success || result.ExitCode != 0)
                        {
                            Log.FastLog("[USER] Failed to install HyperKey Deregisterer, exited with non-zero exit code", LogSeverity.Error, PACIFY_SOURCE);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Installing HyperKey Deregisterer failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[13])
                    {
                        Log.FastLog("[MACHINE] Deactivating Cortana", LogSeverity.Info, PACIFY_SOURCE);

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortanaAboveLock", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\InputPersonalization", "AllowInputPersonalization", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Windows Search", "CortanaConsent", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Deactivating Cortana failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[14])
                    {
                        Log.FastLog("[MACHINE][USER] Deactivating GameDVR", LogSeverity.Info, PACIFY_SOURCE);
                       
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\GameBar", "AutoGameModeEnabled", 0, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_FSEBehaviorMode", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_FSEBehaviorMode", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE][USER] Deactivating GameDVR failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[15])
                    {
                        Log.FastLog("[USER] Deactivating StickyKeys", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "Sound on Activation", 0, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "Warning Sounds", 0, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility\\Keyboard Response", "Flags", "2", RegistryValueKind.String);
                        Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility\\StickyKeys", "Flags", "26", RegistryValueKind.String);
                        Registry.SetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility\\ToggleKeys", "Flags", "38", RegistryValueKind.String);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Deactivating StickyKeys failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[16])
                    {
                        Log.FastLog("[MACHINE][USER] Deactivating system ads / suggestions & auto app installation", LogSeverity.Info, PACIFY_SOURCE);
                        
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "Start_IrisRecommendations", 0, RegistryValueKind.DWord);

                        Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\CloudContent", "DisableSoftLanding", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\CloudContent", "DisableConsumerAccountStateContent", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\CloudContent", "DisableCloudOptimizedContent", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);

                        Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);

                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "ContentDeliveryAllowed", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "FeatureManagementEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "OemPreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEverEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SlideshowEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353698Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContentEnabled", 0, RegistryValueKind.DWord);
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE][USER] Deactivating system ads / suggestions & auto app installation failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[17])
                    {
                        Log.FastLog("[MACHINE][USER] Removing and deactivating Copilot", LogSeverity.Info, PACIFY_SOURCE);

                        Util.Execute.Process(new(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", "-c \"Get-AppxPackage -AllUsers | Where-Object {$_.Name -Like '*Microsoft.Copilot*'} | Remove-AppxPackage -AllUsers -ErrorAction Continue\"", true, true, true));
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Shell\\BrandedKey", "BrandedKeyChoiceType", "Search", RegistryValueKind.String);
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowCopilotButton", 0, RegistryValueKind.DWord);
                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Shell\\Copilot\\BingChat", "IsUserEligible", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE][USER] Removing and deactivating Copilot failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[18])
                    {
                        Log.FastLog("[MACHINE][USER] Removing and deactivating Recall", LogSeverity.Info, PACIFY_SOURCE);

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
                    Log.FastLog("[MACHINE][USER] Removing and deactivating Recall failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[19])
                    {
                        Log.FastLog("[USER] Don't ask for ways to get the most out of Windows", LogSeverity.Info, PACIFY_SOURCE);

                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\UserProfileEngagement", "ScoobeSystemSettingEnabled", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Don't ask for ways to get the most out of Windows failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[20])
                    {
                        Log.FastLog("[MACHINE] Turn off auto installation of Microsoft Teams in taskbar", LogSeverity.Info, PACIFY_SOURCE);

                        if (Util.UnlockRegistryKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Communications", RunContextInfo.Windows.AdministratorGroupName))
                        {
                            Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Communications", "ConfigureChatAutoInstall", 0, RegistryValueKind.DWord);
                        }
                        else
                        {
                            Log.FastLog("[MACHINE] Turn off auto installation of Microsoft Teams in taskbar failed with: failed to take ownership of HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Communications", LogSeverity.Error, PACIFY_SOURCE);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Turn off auto installation of Microsoft Teams in taskbar failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[21])
                    {
                        Log.FastLog("[USER] Disabling Windows Defender reminder to sign into a MS account", LogSeverity.Info, PACIFY_SOURCE);

                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows Security Health\\State", "AccountProtection_MicrosoftAccount_Disconnected", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Disabling Windows Defender reminder to sign into a MS account failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[22])
                    {
                        Log.FastLog("[USER] Disabling Windows Defender reminder to turn on Edge SmartScreen filter", LogSeverity.Info, PACIFY_SOURCE);

                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows Security Health\\State", "AppAndBrowser_EdgeSmartScreenOff", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Disabling Windows Defender reminder to turn on Edge SmartScreen filter failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[23])
                    {
                        Log.FastLog("[USER] Disabling News and Interest in taskbar", LogSeverity.Info, PACIFY_SOURCE);

                        if (Util.UnlockRegistryKey("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Feeds", RunContextInfo.Windows.AdministratorGroupName))
                        {
                            RegistryKey feedKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion", true);
                            feedKey.DeleteSubKeyTree("Feeds", false);

                            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2, RegistryValueKind.DWord);
                            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarOpenOnHover", 0, RegistryValueKind.DWord);
                        }
                        else
                        {
                            Log.FastLog("[USER] Disabling News and Interest in taskbar failed with: failed to take ownership of HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Communications", LogSeverity.Error, PACIFY_SOURCE);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Disabling News and Interest in taskbar failed with: " + exception.Message, LogSeverity.Warning, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[24])
                    {
                        Log.FastLog("[USER] Disabling OneDrive Notifications in Explorer", LogSeverity.Info, PACIFY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Disabling OneDrive Notifications in Explorer failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[25])
                    {
                        Log.FastLog("[USER] Showing more apps in the start menu", LogSeverity.Info, PACIFY_SOURCE);

                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", 1, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[USER] Showing more apps in the start menu failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[26])
                    {
                        Log.FastLog("[MACHINE] Enabling Numlock after boot", LogSeverity.Info, PACIFY_SOURCE);

                        Registry.SetValue("HKEY_USERS\\.DEFAULT\\Control Panel\\Keyboard", "InitialKeyboardIndicators", 2, RegistryValueKind.DWord);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[MACHINE] Enabling Numlock after boot failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                try
                {
                    if (optionSelector.Result.UserSelection[27])
                    {
                        Log.FastLog("[User] Don't automatically choose the appropriate folder type", LogSeverity.Info, PACIFY_SOURCE);
                        
                        RegistryKey shell = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell", true);
                        shell?.DeleteSubKeyTree("BagMRU", false);
                        shell?.DeleteSubKeyTree("Bags", false);

                        Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\Bags\\AllFolders\\Shell", "FolderType", "NotSpecified", RegistryValueKind.String);
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog("[User] Don't automatically choose the appropriate folder type failed with: " + exception.Message, LogSeverity.Error, PACIFY_SOURCE);
                }

                Util.RestartExplorerForUser();
            });

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            Log.FastLog("Done, restart to apply all changes", LogSeverity.Info, PACIFY_SOURCE);
        }
    }
}