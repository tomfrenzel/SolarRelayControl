using Serilog;
using SolarRelayControl.Server.Hubs;
using SolarRelayControl.Server.Interfaces;
using SolarRelayControl.Server.Stores;
using SolarRelayControl.Shared.Models;

namespace SolarRelayControl.Server.Services
{
    public class ControlService : BackgroundService
    {
        private readonly ISolarService solarService;
        private readonly IRelayService relayService;
        private readonly IConfiguration configuration;
        private readonly LogStore logStore;
        private readonly CommunicationHub communicationHub;

        private Settings Settings => configuration.Get<Settings>();

        public ControlService(IConfiguration configuration, ISolarService solarService, IRelayService relayService, LogStore logStore, CommunicationHub communicationHub)
        {
            this.configuration = configuration;
            this.solarService = solarService;
            this.relayService = relayService;
            this.logStore = logStore;
            this.communicationHub = communicationHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await ExecuteMeasurement();
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecuteMeasurement();
            }

        }

        private async Task ExecuteMeasurement()
        {
            try
            {
                var power = await solarService.GetPvInput();
                var soc = await solarService.GetSoc();
                Log.Information($"New measurement: PV Power = {power} kW, SOC = {soc} %");

                var currentStatus = await relayService.GetRelayStatus();
                RelayAction action;

                var requiredTresholdsReached = power >= Settings.PowerThreshold && soc >= Settings.SocThreshold;
                if (requiredTresholdsReached && !currentStatus.IsOn)
                {
                    action = RelayAction.Anschalten;
                }
                else if (!requiredTresholdsReached && currentStatus.IsOn)
                {
                    action = RelayAction.Ausschalten;
                }
                else
                {
                    return;
                }

                await relayService.SetRelayState(action);
                var entry = new LogEntry
                {
                    Timestamp = DateTimeOffset.Now,
                    CurrentPower = power,
                    CurrentSoc = soc,
                    Action = action
                };
                logStore.AddLogEntry(entry);
                await communicationHub.SendLog(entry);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occured");
            }
        }
    }
}
