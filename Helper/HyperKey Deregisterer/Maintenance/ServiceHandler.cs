using System;
using System.ServiceProcess;
using System.Threading;

namespace Deregisterer
{
    internal sealed class Service : ServiceBase
    {
        internal static readonly Thread Worker = new(MaintenanceService.Run) { Name = "MainWorkerThread" };

        public Service()
        {
            AutoLog = false;
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = false;
            CanStop = false;
            ServiceName = $"{Installer.EXECUTABLE_NAME_WITHOUT_EXTENSION} Maintenance Service";
        }

        protected override void OnStart(String[] args) => Worker.Start();

        protected override void OnStop() => Worker.Join();

        protected override void OnShutdown() => Worker.Join();
    }
}