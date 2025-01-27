using BSS.Logging;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace Stimulator.SubWindows
{
    /* 
     * https://learn.microsoft.com/en-us/windows/security/identity-protection/credential-guard/configure?tabs=intune#disable-credential-guard-with-uefi-lock
     * mountvol X: /s
     * copy %WINDIR%\System32\SecConfig.efi X:\EFI\Microsoft\Boot\SecConfig.efi /Y
     * bcdedit /create {0cb3b571-2f2e-4343-a879-d86a476d7215} /d "DebugTool" /application osloader
     * bcdedit /set {0cb3b571-2f2e-4343-a879-d86a476d7215} path "\EFI\Microsoft\Boot\SecConfig.efi"
     * bcdedit /set {bootmgr} bootsequence {0cb3b571-2f2e-4343-a879-d86a476d7215}
     * bcdedit /set {0cb3b571-2f2e-4343-a879-d86a476d7215} loadoptions DISABLE-LSA-ISO,DISABLE-VBS
     * bcdedit /set {0cb3b571-2f2e-4343-a879-d86a476d7215} device partition=X:
     * mountvol X: /d
     * 
     * dmpstore -d VbsPolicy
     * 
     * https://github.com/pbatard/UEFI-Shell
     */

    public sealed partial class SystemSecurity
    {
        private const String VBS_SOURCE = "VBS";

        private static Task VBS()
        {
            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                   "Do you wish to enable VBS and its features:\n" +
                   "- Kernel Mode Hardware Enforced Stack Protection\n" +
                   "- Credential Guard\n" +
                   "- Hypervisor Enforced Code Integrity\n" +
                   "- Secure Launch\n" +
                   "- Machine Identity Isolation\n" +
                   "- Virtualization Based Protection of Code Integrity\n\n" +
                   "Press Yes to enable, and No to disable the features.",
                   "Virtualization based security",
                   System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                   System.Windows.Forms.MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Cancel) return Task.CompletedTask;
            else if (result == System.Windows.Forms.DialogResult.Yes) EnableVBS();
            else DisableVBS();

            Log.FastLog("Done", LogSeverity.Info, VBS_SOURCE);

            return Task.CompletedTask;
        }

        private static void EnableVBS()
        {
            Boolean useUEFILock = false;

            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                   "Enable with UEFI lock?\n",
                   "Virtualization based security",
                   System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                   System.Windows.Forms.MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Cancel) return;
            else if (result == System.Windows.Forms.DialogResult.Yes) useUEFILock = true;

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa", "LsaCfgFlags", useUEFILock ? 1 : 2, RegistryValueKind.DWord);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "RequirePlatformSecurityFeatures", 3, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "ConfigureKernelShadowStacksLaunch", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "ConfigureSystemGuardLaunch", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "MachineIdentityIsolation", 2, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "LsaCfgFlags", useUEFILock ? 1 : 2, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "HVCIMATRequired", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "HypervisorEnforcedCodeIntegrity", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DeviceGuard", "EnableVirtualizationBasedSecurity", useUEFILock ? 1 : 2, RegistryValueKind.DWord);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\ControlSet001\Control\DeviceGuard", "Locked", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\ControlSet001\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 1, RegistryValueKind.DWord);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\ControlSet001\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Locked", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\ControlSet001\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "HVCIMATRequired", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\ControlSet001\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", 1, RegistryValueKind.DWord);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\ControlSet001\Control\DeviceGuard\Scenarios\KernelShadowStacks", "Enabled", 1, RegistryValueKind.DWord);

            Log.FastLog($"Enabled virtualization based security features {(useUEFILock ? "with" : "without")} UEFI lock, a restart is needed to apply the changes. You may want check the status with msinfo32 | consider using 'bcdedit /set hypervisorlaunchtype on'", LogSeverity.Info, VBS_SOURCE);
        }

        private static void DisableVBS()
        {
            RegistryKey controlLsa = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Lsa", true);
            controlLsa?.DeleteValue("LsaCfgFlags", false);

            RegistryKey windowsDeviceGuard = Registry.LocalMachine.OpenSubKey("Software\\Policies\\Microsoft\\Windows\\DeviceGuard", true);
            windowsDeviceGuard?.DeleteValue("RequirePlatformSecurityFeatures", false);
            windowsDeviceGuard?.DeleteValue("ConfigureKernelShadowStacksLaunch", false);
            windowsDeviceGuard?.DeleteValue("ConfigureSystemGuardLaunch", false);
            windowsDeviceGuard?.DeleteValue("MachineIdentityIsolation", false);
            windowsDeviceGuard?.DeleteValue("LsaCfgFlags", false);
            windowsDeviceGuard?.DeleteValue("HVCIMATRequired", false);
            windowsDeviceGuard?.DeleteValue("HypervisorEnforcedCodeIntegrity", false);
            windowsDeviceGuard?.DeleteValue("EnableVirtualizationBasedSecurity", false);

            RegistryKey controlDeviceGuard = Registry.LocalMachine.OpenSubKey("System\\ControlSet001\\Control", true);
            controlDeviceGuard?.DeleteSubKeyTree("DeviceGuard", false);

            Log.FastLog($"Removed VBS config values from the registry, a restart is needed to apply the changes. You may want check the status with msinfo32 | consider using 'bcdedit /set hypervisorlaunchtype off'", LogSeverity.Info, VBS_SOURCE);

            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                   "You may or may not need to remove the UEFI lock (EFI variable)\n" +
                   "In EFI Shell> dmpstore -d VbsPolicy",
                   "Virtualization based security",
                   System.Windows.Forms.MessageBoxButtons.OK,
                   System.Windows.Forms.MessageBoxIcon.Information);
        }
    }
}