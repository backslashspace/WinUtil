using System.Threading.Tasks;
//
using EXT.Launcher.Process;

namespace WinUtil
{
    internal static class Common
    {
        internal static async Task RestartExplorer()
        {
            xProcess.Run("C:\\Windows\\System32\\taskkill.exe", "/IM explorer.exe /F", WaitForExit: true, HiddenExecute: true);
            await Task.Delay(1000).ConfigureAwait(false);
            xProcess.Run("C:\\Windows\\explorer.exe", null, WaitForExit: true, HiddenExecute: true);
        }
        



    }
}