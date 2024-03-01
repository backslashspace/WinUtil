using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
//libs
using EXT.HashTools;
using EXT.Launcher.Powershell;
using EXT.Launcher.Process;

namespace WinUtil
{
    internal static class Global
    {
        /*
        internal static void Prepare_DEFAULTUSER_Edit()
        {
            if (ApplicationState.TargetNewUsers)
            {
                if (ApplicationState.DEFAULT_USER_Edit_In_Progress)
                {
                    while (ApplicationState.DEFAULT_USER_Edit_In_Progress)
                    {
                        Task.Delay(100).Wait();
                    }
                }

                LogBox.Add("Creating backup of NTUSER.DAT", Brushes.DarkOrange);
            }
        }

        internal static void Load_DEFAULTUSER_Hive()
        {
            ApplicationState.DEFAULT_USER_Edit_In_Progress = true;

            xProcess.Run("C:\\Windows\\System32\\reg.exe", $"load {ApplicationState.DEFAULT_USER_Edit_WorkingDirectory} C:\\Users\\Default\\NTUSER.DAT", WaitForExit: true, HiddenExecute: true);    
        }

        internal static void Unload_DEFAULTUSER_Hive()
        {
            xProcess.Run("C:\\Windows\\System32\\taskkill.exe", "/im regedit.exe /f", WaitForExit: true, HiddenExecute: true);

            GC.Collect();

            xProcess.Run("C:\\Windows\\System32\\reg.exe", $"unload {ApplicationState.DEFAULT_USER_Edit_WorkingDirectory}", WaitForExit: true, HiddenExecute: true);

            ApplicationState.DEFAULT_USER_Edit_In_Progress = false;
        }
        */






        ///<returns><see langword="Boolean"/>[2] { IsValid, IsPresent }</returns>
        internal static Boolean[] VerboseHashCheck(String filePath, String IsHash, xHash.Algorithm algorithm = xHash.Algorithm.SHA256)
        {
            String efile;
            String rpath;

            try
            {
                if (xHash.CompareHash(filePath, IsHash, algorithm))
                {
                    return new Boolean[] { true, true };
                }
                else
                {
                    CreatePathString();

                    LogBox.Add(rpath, Brushes.Gray);
                    LogBox.Add(efile, Brushes.OrangeRed, StayInLine: true);
                    LogBox.Add(" ── ", Brushes.Gray, StayInLine: true);
                    LogBox.Add("Invalide Hash", Brushes.Red, StayInLine: true);

                    return new Boolean[] { false, true };
                }
            }
            catch
            {
                CreatePathString();

                LogBox.Add(rpath, Brushes.Gray);
                LogBox.Add(efile, Brushes.OrangeRed, StayInLine: true);
                LogBox.Add(" ── ", Brushes.Gray, StayInLine: true);
                LogBox.Add("File missing", Brushes.Red, StayInLine: true);

                return new Boolean[] { false, false };
            }

            void CreatePathString()
            {
                String[] temp = filePath.Split('\\');
                efile = temp[temp.Length - 1];
                temp = temp.Skip(0).Take((temp.Length) - 1).ToArray();
                rpath = String.Join("\\", temp);

                if (filePath.Contains('\\'))
                {
                    rpath += "\\";
                }
            }
        }

        ///<summary>Tests internet connectivity.</summary>
        ///<remarks>(Attempts to resolve a hostname)</remarks>
        internal static Boolean InternetIsAvailable()
        {
            try
            {
                _ = Dns.GetHostAddresses("1dot1dot1dot1.cloudflare-dns.com");
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
        internal static Int32 WinGetIsInstalled(Boolean installOnMissing = false)
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

            if (installOnMissing)
            {
                LogBox.Add("Installing WinGet..", Brushes.DarkGreen);

                if (VerboseHashCheck("assets\\" + Resource_Assets.VCLibs_PathName, Resource_Assets.VCLibs_Hash)[0] && VerboseHashCheck("assets\\WinGet\\" + Resource_Assets.WinGetName, Resource_Assets.WinGetHash)[0] && VerboseHashCheck("assets\\WinGet\\" + Resource_Assets.Xaml27Name, Resource_Assets.Xaml27Hash)[0] && VerboseHashCheck("assets\\WinGet\\" + Resource_Assets.WinGetLicenseName, Resource_Assets.WinGetLicenseHash)[0])
                {
                    xPowershell.Run($"Add-AppxPackage -Path \"assets\\{Resource_Assets.VCLibs_PathName}\"");

                    xPowershell.Run("Add-AppxPackage -Path \"assets\\WinGet\\" + Resource_Assets.Xaml27Name + "\"");

                    xPowershell.Run("Add-ProvisionedAppPackage -Online -PackagePath \"assets\\WinGet\\" + Resource_Assets.WinGetName + "\" -LicensePath \"assets\\WinGet\\" + Resource_Assets.WinGetLicenseName + "\"");

                    Task.Delay(5000).Wait();

                    try
                    {
                        xProcess.Run("winget.exe", RunAs: true, HiddenExecute: true);

                        LogBox.Add("Done\n", Brushes.DarkCyan);

                        return 1;
                    }
                    catch (Exception ex)
                    {
                        LogBox.Add("[ERR] " + ex.ToString() + "\n", Brushes.Red);

                        return 2;
                    }
                }
                else
                {
                    LogBox.Add("[WARN] Nothing changed, skipping\n", Brushes.OrangeRed);

                    return 2;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}