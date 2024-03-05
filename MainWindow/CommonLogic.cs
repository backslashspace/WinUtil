using System;
using System.Threading.Tasks;
using BSS.HashTools;
using System.Windows.Media;
//
using BSS.Launcher;

namespace WinUtil
{
    internal static class Common
    {
        internal static async Task RestartExplorer()
        {
            xProcess.Run("C:\\Windows\\System32\\taskkill.exe", "/IM explorer.exe /F", waitForExit: true, hiddenExecute: true);
            await Task.Delay(1000).ConfigureAwait(false);
            xProcess.Run("C:\\Windows\\explorer.exe");
        }

        ///<returns><see langword="bool"/>[] { IsValid, IsPresent }</returns>
        internal static Boolean[] VerboseHashCheck(String filePath, String expectedHash, xHash.Algorithm algorithm = xHash.Algorithm.SHA256)
        {
            String fileName;
            String path;

            try
            {
                if (xHash.CompareHash(filePath, expectedHash, algorithm))
                {
                    return new Boolean[] { true, true };
                }
                else
                {
                    (path, fileName) = CreatePathString(ref filePath);

                    LogBox.Add(path, Brushes.Gray);
                    LogBox.Add(fileName, Brushes.OrangeRed, stayInLine: true);
                    LogBox.Add(" ── ", Brushes.Gray, stayInLine: true);
                    LogBox.Add("Invalide Hash", Brushes.Red, stayInLine: true);

                    return new Boolean[] { false, true };
                }
            }
            catch
            {
                (path, fileName) = CreatePathString(ref filePath);

                LogBox.Add(path, Brushes.Gray);
                LogBox.Add(fileName, Brushes.OrangeRed, stayInLine: true);
                LogBox.Add(" ── ", Brushes.Gray, stayInLine: true);
                LogBox.Add("File missing", Brushes.Red, stayInLine: true);

                return new Boolean[] { false, false };
            }

            static (String path, String fileName) CreatePathString(ref String filePath)
            {
                String fileName;
                String path = "";

                String[] pathParts = filePath.Split('\\');

                fileName = pathParts[pathParts.Length - 1];

                for (UInt16 i = 0; i < pathParts.Length - 1; ++i)
                {
                    path += pathParts[i] + "\\";
                }

                return (path, fileName);
            }
        }


    }
}