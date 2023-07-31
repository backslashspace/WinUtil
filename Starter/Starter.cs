using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starter
{
    internal class Starter
    {
        public static Boolean CompareHash256(string file, String hash)
        {
            try
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                using var stream = File.OpenRead(file);
                if ((BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty)).Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
            {
                return false;
            }
        }

        static void Main()
        {
            Boolean valide = false;
            Boolean ignore = false;

            if (CompareHash256("data\\WinUtil.exe", "5d1a10b6e82c351c9cf1b165ff2e30664deaa8edb5ad249cc615736ff445425f"))
            {
                valide = true;
            }
            do
            {
                if (valide || ignore)
                {
                    Process proc = new();
                    ProcessStartInfo info = new()
                    {
                        FileName = "WinUtil.exe",

                        Arguments = "e22afd680ce7b8f23fad799fa3beef2dbce66e42e8877a9f2f0e3fd0b55619c9",
                        UseShellExecute = true,
                        WorkingDirectory = "data",
                        Verb = "runas"

                    };
                    proc.StartInfo = info;
                    proc.Start();
                    break;
                }
                else
                {
                    var result = System.Windows.Forms.MessageBox.Show("Hash of 'WinUtil.exe' invalid, continue?", "Verification Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        ignore = true;
                    }
                }
            } while (ignore);
        }
    }
}
