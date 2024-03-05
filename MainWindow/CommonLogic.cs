using System.Threading.Tasks;
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
        



    }
}