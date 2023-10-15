using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiSyPHuS_Win10_2022_Host_INIT
{
    internal class Hostfile
    {
        internal static readonly String[] TelemetryData =
        {
            "au-v10.events.data.microsoft.com",
            "au-v20.events.data.microsoft.com",
            "au.vortex-win.data.microsoft.com",
            "de-v20.events.data.microsoft.com",
            "de.vortex-win.data.microsoft.com",
            "eu-v10.events.data.microsoft.com",
            "eu-v20.events.data.microsoft.com",
            "eu.vortex-win.data.microsoft.com",
            "events-sandbox.data.microsoft.com",
            "events.data.microsoft.com",
            "jp-v10.events.data.microsoft.com",
            "jp-v20.events.data.microsoft.com",
            "settings-win.data.microsoft.com",
            "telecommand.telemetry.microsoft.com",
            "uk-v20.events.data.microsoft.com",
            "uk.vortex-win.data.microsoft.com",
            "us-v10.events.data.microsoft.com",
            "us-v20.events.data.microsoft.com",
            "us.vortex-win.data.microsoft.com",
            "us4-v20.events.data.microsoft.com",
            "us5-v20.events.data.microsoft.com",
            "v10.vortex-win.data.microsoft.com",
            "v20.events.data.microsoft.com",
            "v20.vortex-win.data.microsoft.com",
            "vortex-win-sandbox.data.microsoft.com",
            "vortex-win.data.microsoft.com",
            "watson.ppe.telemetry.microsoft.com",
            "watson.telemetry.microsoft.com",
            "watson.telemetry.microsoft.com.nsatc.net",
            "modern.watson.data.microsoft.com.akadns.net"
        };

        private static String[] Read_File()
        {
            String CLine;
            List<String> Content = new();

            using FileStream FS = File.OpenRead("C:\\Windows\\System32\\drivers\\etc\\hosts");
            using StreamReader SR = new(FS, Encoding.UTF8, true, 512);

            while ((CLine = SR.ReadLine()) != null)
            {
                Content.Add(CLine);
            }

            FS.Close();
            FS.Dispose();

            SR.Close();
            SR.Dispose();

            return Content.ToArray();
        }

        internal static Task Get_New_Data()
        {
            MainWindow.Current_File_Content = Read_File();

            List<String> ToAdd = new();

            foreach (String Entry in TelemetryData)
            {
                foreach (String Line in MainWindow.Current_File_Content)
                {
                    if (Line == "" || Line[0] == '#' || Entry == " ")
                    {
                        continue;
                    }

                    if (Line.Contains(Entry))
                    {
                        goto skip;
                    }
                }

                ToAdd.Add(Entry);

            skip:;
            }

            MainWindow.Data_To_Be_Writen = ToAdd.ToArray();

            return Task.CompletedTask;
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static void Write_File()
        {
            using StreamWriter sw = new("C:\\Windows\\System32\\drivers\\etc\\hosts", false, Encoding.UTF8, 512);

            foreach (String s in MainWindow.Current_File_Content)
            {
                sw.WriteLine(s);
            }

            if (MainWindow.Data_To_Be_Writen.Length == TelemetryData.Length)
            {
                sw.WriteLine("\n#<https://www.bsi.bund.de/SharedDocs/Downloads/DE/BSI/Cyber-Sicherheit/SiSyPHus/Telemetrie-Endpunkte_Windows10_Build_Build_21H2.html>\n");
            }
            else
            {
                sw.WriteLine();
            }

            foreach (String s in MainWindow.Data_To_Be_Writen)
            {
                sw.WriteLine($"127.0.0.1 {s}");
            }

            if (MainWindow.Data_To_Be_Writen.Length == TelemetryData.Length)
            {
                sw.WriteLine("\n#</https://www.bsi.bund.de/SharedDocs/Downloads/DE/BSI/Cyber-Sicherheit/SiSyPHus/Telemetrie-Endpunkte_Windows10_Build_Build_21H2.html>");
            }

            sw.Close();
            sw.Dispose();
        }
    }
}