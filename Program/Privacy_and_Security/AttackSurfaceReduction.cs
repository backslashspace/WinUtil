using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    // https://learn.microsoft.com/en-us/defender-endpoint/microsoft-defender-antivirus-compatibility
    // https://techcommunity.microsoft.com/blog/microsoftdefenderatpblog/demystifying-attack-surface-reduction-rules---part-1/1306420
    // https://techcommunity.microsoft.com/blog/microsoftdefenderatpblog/demystifying-attack-surface-reduction-rules---part-2/1326565
    // https://techcommunity.microsoft.com/blog/microsoftdefenderatpblog/demystifying-attack-surface-reduction-rules---part-3/1360968
    // https://techcommunity.microsoft.com/blog/microsoftdefenderatpblog/demystifying-attack-surface-reduction-rules---part-4/1384425
    // https://learn.microsoft.com/en-us/defender-endpoint/enable-attack-surface-reduction
    // https://learn.microsoft.com/en-us/defender-endpoint/attack-surface-reduction-rules-reference
    // https://learn.microsoft.com/en-us/defender-endpoint/attack-surface-reduction-faq

    public sealed partial class SecurityConfigWindow
    {
        private const String ATTACK_SURFACE_REDUCTION_SOURCE = "ASR";
        private const String ATTACK_SURFACE_REDUCTION_RULES = "56a863a9-875e-4185-98a7-b882c64b5ce5, 7674ba52-37eb-4a4f-a9a1-f0f9a1619a2c, d4f940ab-401b-4efc-aadc-ad5f3c50688a, 9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2, be9ba2d9-53ea-4cdc-84e5-9b1eeee46550, 01443614-cd74-433a-b99e-2ecdc07bfc25, 5beb7efe-fd9a-4556-801d-275e5ffc04cc, d3e037e1-3eb8-44c8-a917-57927947596d, 3b576869-a4ec-4529-8536-b80a7769e899, 75668c1f-73b5-4cf0-bb93-3ecf5cb7cc84, 26190899-1602-49e8-8b27-eb1d0a1ce869, e6db77e5-3df2-4cf1-b95a-636979351e5b, d1e49aac-8f56-4280-b9ba-993a6d77406c, 33ddedf1-c6e0-47cb-833e-de6133960387, b2b3f03d-6a65-4f7b-a9c7-1c7ef74a9ba4, c0033c00-d16d-4114-a5a0-dc9b3a7d2ceb, a8f5898e-1dc8-49a9-9878-85004b8a61e6, 92e97fa1-2edf-4476-bdd6-9dd0b4dddc7b, c1db55ab-c21a-4637-bb3f-a12568109d35";

        private static Task AttackSurfaceReduction()
        {
            try
            {
                PowerShell.Create().AddCommand("Get-MpPreference").Invoke();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show(
                    "Unable to set ASR rules, Windows Defender not installed or broken powershell integration.",
                    "Not supported",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);

                return Task.CompletedTask;
            }

            Object rawDefenderMode = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Advanced Threat Protection", "ForceDefenderPassiveMode", null);
            if (rawDefenderMode == null || rawDefenderMode.GetType() != typeof(Int32) || (Int32)rawDefenderMode == 1)
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                    "It appears that Windows Defender is not explicitly set as the primary protection solution.\nIn order to use attack surface reduction rules, Windows Defender is required to run in 'active mode'.\n\nContinue?",
                    ATTACK_SURFACE_REDUCTION_SOURCE,
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

                if (result != System.Windows.Forms.DialogResult.Yes) return Task.CompletedTask;

                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Advanced Threat Protection", "ForceDefenderPassiveMode", 0, RegistryValueKind.DWord);
                Log.FastLog("Set ForceDefenderPassiveMode to 0", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
            }

            //

            OptionSelector.Option[] options =
            [
                new(false, false, "*Block abuse of exploited vulnerable signed drivers",                                                  "breaks MSI Afterburner"),
                new(false, false, "Block Adobe Reader from creating child processes",                                                    null!),
                new(false, false, "Block all Office applications from creating child processes",                                         null!),
                new(false, false, "Block credential stealing from the Windows local security authority subsystem (lsass.exe)",           null!),
                new(false, false, "Block executable content from email client and webmail",                                              null!),
                new(false, false, "*Block executable files from running unless they meet a prevalence, age, or trusted list criterion",   "breaks things"),
                new(false, false, "Block execution of potentially obfuscated scripts",                                                   null!),
                new(false, false, "Block JavaScript or VBScript from launching downloaded executable content",                           null!),
                new(false, false, "Block Office applications from creating executable content",                                          null!),
                new(false, false, "Block Office applications from injecting code into other processes",                                  null!),
                new(false, false, "Block Office communication application from creating child processes",                                null!),
                new(false, false, "Block persistence through WMI event subscription. (file and folder exclusions not supported)",        null!),
                new(false, false, "*Block process creations originating from PSExec and WMI commands",                                    "breaks Windows Server Manager"),
                new(false, false, "Block rebooting machine in Safe Mode (preview)",                                                      null!),
                new(false, false, "Block untrusted and unsigned processes that run from USB",                                            null!),
                new(false, false, "*Block use of copied or impersonated system tools (preview)",                                         "breaks VeraCrypt"),
                new(false, false, "Block Webshell creation for Servers",                                                                 null!),
                new(false, false, "Block Win32 API calls from Office macros",                                                            null!),
                new(false, false, "Use advanced protection against ransomware",                                                          null!),
            ];

            OptionSelector optionSelector = new("Attack Surface Reduction", options, new(true, 0, "asr.cfg"));
            optionSelector.ShowDialog();

            if (!optionSelector.Result.CommitSelection) return Task.CompletedTask;

            // # # # # # # # # # # # # # # # # # # # # # # # # #

            Boolean[] rules = new Boolean[19];

            if (optionSelector.Result.UserSelection[0])
            {
                Log.FastLog("Blocking abuse of exploited vulnerable signed drivers", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[0] = true;
            }

            if (optionSelector.Result.UserSelection[1])
            {
                Log.FastLog("Blocking Adobe Reader from creating child processes", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[1] = true;
            }

            if (optionSelector.Result.UserSelection[2])
            {
                Log.FastLog("Blocking all Office applications from creating child processes", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[2] = true;
            }

            if (optionSelector.Result.UserSelection[3])
            {
                Log.FastLog("Blocking credential stealing from the Windows local security authority subsystem (lsass.exe)", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[3] = true;
            }

            if (optionSelector.Result.UserSelection[4])
            {
                Log.FastLog("Blocking executable content from email client and webmail", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[4] = true;
            }

            if (optionSelector.Result.UserSelection[5])
            {
                Log.FastLog("Blocking executable files from running unless they meet a prevalence, age, or trusted list criterion", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[5] = true;
            }

            if (optionSelector.Result.UserSelection[6])
            {
                Log.FastLog("Blocking execution of potentially obfuscated scripts", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[6] = true;
            }

            if (optionSelector.Result.UserSelection[7])
            {
                Log.FastLog("Blocking JavaScript or VBScript from launching downloaded executable content", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[7] = true;
            }

            if (optionSelector.Result.UserSelection[8])
            {
                Log.FastLog("Blocking Office applications from creating executable content", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[8] = true;
            }

            if (optionSelector.Result.UserSelection[9])
            {
                Log.FastLog("Blocking Office applications from injecting code into other processes", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[9] = true;
            }

            if (optionSelector.Result.UserSelection[10])
            {
                Log.FastLog("Blocking Office communication application from creating child processes", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[10] = true;
            }

            if (optionSelector.Result.UserSelection[11])
            {
                Log.FastLog("Blocking persistence through WMI event subscription. (file and folder exclusions not supported)", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[11] = true;
            }

            if (optionSelector.Result.UserSelection[12])
            {
                Log.FastLog("Blocking process creations originating from PSExec and WMI commands", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[12] = true;
            }

            if (optionSelector.Result.UserSelection[13])
            {
                Log.FastLog("Blocking rebooting machine in Safe Mode (preview)", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[13] = true;
            }

            if (optionSelector.Result.UserSelection[14])
            {
                Log.FastLog("Blocking untrusted and unsigned processes that run from USB", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[14] = true;
            }

            if (optionSelector.Result.UserSelection[15])
            {
                Log.FastLog("Blocking use of copied or impersonated system tools (preview)", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[15] = true;
            }

            if (optionSelector.Result.UserSelection[16])
            {
                Log.FastLog("Blocking Webshell creation for Servers", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[16] = true;
            }

            if (optionSelector.Result.UserSelection[17])
            {
                Log.FastLog("Blocking Win32 API calls from Office macros", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[17] = true;
            }

            if (optionSelector.Result.UserSelection[18])
            {
                Log.FastLog("Blocking advanced protection against ransomware", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
                rules[18] = true;
            }

            // build command

            String commandEnabled = null!;

            for (Int16 i = 0; i < rules.Length; ++i)
            {
                if (commandEnabled == null) commandEnabled = rules[i] ? "1" : "0";
                else commandEnabled += rules[i] ? ",1" : ",0";
            }

            Log.Debug($"commandEnabled = '{commandEnabled}'", ATTACK_SURFACE_REDUCTION_SOURCE);

            // apply

            try
            {
                Util.Execute.Process(new(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", $"-c \"Set-MpPreference -AttackSurfaceReductionRules_Ids {ATTACK_SURFACE_REDUCTION_RULES} -AttackSurfaceReductionRules_Actions {commandEnabled}\"", true, true, true));

                Log.FastLog("Applied rules", LogSeverity.Info, ATTACK_SURFACE_REDUCTION_SOURCE);
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to apply asr rules: " + exception.Message, LogSeverity.Error, ATTACK_SURFACE_REDUCTION_SOURCE);
            }

            return Task.CompletedTask;
        }
    }
}