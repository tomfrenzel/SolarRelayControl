using Microsoft.AspNetCore.SignalR;
using Serilog;
using SolarRelayControl.Server.Hubs;
using SolarRelayControl.Server.Interfaces;
using SolarRelayControl.Server.Stores;
using SolarRelayControl.Shared.Models;
using SolarRelayControl.Shared.Models.Settings;

namespace SolarRelayControl.Server.Services
{
    /// <summary>
    /// Service to periodically check the inverter data and control the relay accordingly
    /// </summary>
    public class ControlService : BackgroundService
    {
        private readonly ISolarService solarService;
        private readonly IRelayService relayService;
        private readonly IConfiguration configuration;
        private readonly LogStore logStore;
        private readonly IHubContext<CommunicationHub> hubContext;

        private Settings Settings => configuration.Get<Settings>();

        public ControlService(IConfiguration configuration, ISolarService solarService, IRelayService relayService, LogStore logStore, IHubContext<CommunicationHub> hubContext)
        {
            this.configuration = configuration;
            this.solarService = solarService;
            this.relayService = relayService;
            this.logStore = logStore;
            this.hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExecuteMeasurement();
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
                    action = RelayAction.PowerOn;
                }
                else if (!requiredTresholdsReached && currentStatus.IsOn)
                {
                    action = RelayAction.PowerOff;
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
                await hubContext.Clients.All.SendAsync(nameof(CommunicationHub.SendLog), entry);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occured");
            }
        }
    }
}
