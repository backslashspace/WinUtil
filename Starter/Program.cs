using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Starter
{
    internal class Program
    {
        public static bool CompareHash256(string file, string hash)
        {
            string[] temp = file.Split('\\');
            string efile = temp[temp.Length - 1];
            temp = temp.Skip(0).Take((temp.Length) - 1).ToArray();
            string rpath = string.Join("\\", temp);

            if (file.Contains('\\'))
            {
                rpath = rpath + "\\";
            }

            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        if ((BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty)).Equals(hash, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            bool valide = false;
            bool ignore = false;

            if (CompareHash256("data\\WinUtil.exe", "1461988c26a78fd31a859c8651a549c2d80350218daaa36b68c6f0a3567a1359"))
            {
                valide = true;
            }
            do
            {
                if (valide || ignore)
                {
                    Process proc = new Process();
                    ProcessStartInfo info = new ProcessStartInfo()
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
                    var result = MessageBox.Show("Hash of 'WinUtil.exe' invalid, continue?", "Verification Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        ignore = true;
                    }
                }
            } while (ignore);
        }
    }
}
