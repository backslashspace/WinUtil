using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
//libs
using static PowershellHelper.PowershellHelper;
using ProgramLauncher;
using static HashTools.FileHashTools;

namespace WinUtil
{
    public partial class MainWindow
    {
        ///<returns><see langword="Boolean"/><see href="[2]"/> { IsValid, IsPresent }</returns>
        protected Boolean[] VerboseHashCheck(String FilePath, String IsHash, String Algorithm = "SHA256")
        {
            String efile;
            String rpath;

            try
            {
                if (CompareHash(FilePath, IsHash, Algorithm))
                {
                    return new Boolean[] { true, true };
                }
                else
                {
                    CreatePathString();

                    DispatchedLogBoxAdd(rpath, Brushes.Gray);
                    DispatchedLogBoxAdd(efile, Brushes.OrangeRed, StayInLine: true);
                    DispatchedLogBoxAdd(" ── ", Brushes.Gray, StayInLine: true);
                    DispatchedLogBoxAdd("Invalide Hash", Brushes.Red, StayInLine: true);

                    return new Boolean[] { false, true };
                }
            }
            catch
            {
                CreatePathString();

                DispatchedLogBoxAdd(rpath, Brushes.Gray);
                DispatchedLogBoxAdd(efile, Brushes.OrangeRed, StayInLine: true);
                DispatchedLogBoxAdd(" ── ", Brushes.Gray, StayInLine: true);
                DispatchedLogBoxAdd("File missing", Brushes.Red, StayInLine: true);

                return new Boolean[] { false, false };
            }

            void CreatePathString()
            {
                String[] temp = FilePath.Split('\\');
                efile = temp[temp.Length - 1];
                temp = temp.Skip(0).Take((temp.Length) - 1).ToArray();
                rpath = String.Join("\\", temp);

                if (FilePath.Contains('\\'))
                {
                    rpath += "\\";
                }
            }
        }

        ///<summary>Tests internet connectivity.</summary>
        ///<remarks>(Attempts to resolve a hostname)</remarks>
        protected static Boolean InternetIsAvailable()
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
        protected Int32 WinGetIsInstalled(Boolean InstallOnMissing = false)
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
                DispatchedLogBoxAdd("Installing WinGet..", Brushes.DarkGreen);

                if (VerboseHashCheck("assets\\" + Const.VCLibsName, Const.VCLibs)[0] && VerboseHashCheck("assets\\WinGet\\" + Const.WinGetName, Const.WinGetHash)[0] && VerboseHashCheck("assets\\WinGet\\" + Const.Xaml27Name, Const.Xaml27Hash)[0] && VerboseHashCheck("assets\\WinGet\\" + Const.WinGetLicenseName, Const.WinGetLicenseHash)[0])
                {
                    PowerShell("Add-AppxPackage -Path \"assets\\" + Const.VCLibsName + "\"");

                    PowerShell("Add-AppxPackage -Path \"assets\\WinGet\\" + Const.Xaml27Name + "\"");

                    PowerShell("Add-ProvisionedAppPackage -Online -PackagePath \"assets\\WinGet\\" + Const.WinGetName + "\" -LicensePath \"assets\\WinGet\\" + Const.WinGetLicenseName + "\"");

                    Task.Delay(5000).Wait();

                    try
                    {
                        Execute.EXE("winget.exe", RunAs: true, HiddenExecute: true);

                        DispatchedLogBoxAdd("Done\n", Brushes.DarkCyan);

                        return 1;
                    }
                    catch (Exception ex)
                    {
                        DispatchedLogBoxAdd("[ERR] " + ex.ToString() + "\n", Brushes.Red);

                        return 2;
                    }
                }
                else
                {
                    DispatchedLogBoxAdd("[WARN] Nothing changed, skipping\n", Brushes.OrangeRed);

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