using System;
using System.IO;
using System.Reflection;

namespace Deregisterer
{
    internal static partial class Installer
    {
        private static void CopyFiles()
        {
            String executingAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (Directory.Exists(INSTALLATION_DIRECTORY_PATH)) Directory.Delete(INSTALLATION_DIRECTORY_PATH, true);
            Directory.CreateDirectory(INSTALLATION_DIRECTORY_PATH);

            try
            {
                File.Copy(executingAssemblyPath + "\\" + EXECUTABLE_NAME_WITHOUT_EXTENSION + ".exe", INSTALLATION_DIRECTORY_PATH + EXECUTABLE_NAME_WITHOUT_EXTENSION + ".exe", true);
            }
            catch
            {
                Environment.Exit(-1);
            }

            try
            {
                File.Copy(executingAssemblyPath + "\\" + EXECUTABLE_NAME_WITHOUT_EXTENSION + ".exe.config", INSTALLATION_DIRECTORY_PATH + EXECUTABLE_NAME_WITHOUT_EXTENSION + ".exe.config", true);
            }
            catch { }
        }
    }
}