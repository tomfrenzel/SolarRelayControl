using Microsoft.AspNetCore.SignalR;
using Serilog;
using SolarHeaterControl.Server.Hubs;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Services.Background
{
    public class ControlService : BackgroundService
    {
        private readonly ModbusService modbusService;
        private readonly RelayService relayService;
        private readonly IConfiguration configuration;
        private readonly LogStore logStore;
        private readonly CommunicationHub communicationHub;

        private Settings Settings => configuration.Get<Settings>();

        public ControlService(IConfiguration configuration, ModbusService modbusService, RelayService relayService, LogStore logStore, CommunicationHub communicationHub)
        {
            this.configuration = configuration;
            this.modbusService = modbusService;
            this.relayService = relayService;
            this.logStore = logStore;
            this.communicationHub = communicationHub;
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
                double power = 0;
                if (Settings.Inverters.Inverter1.IsActive)
                {
                    power += await modbusService.GetPvInput(Settings.Inverters.Inverter1.ModbusId);
                }
                if (Settings.Inverters.Inverter2.IsActive)
                {
                    power += await modbusService.GetPvInput(Settings.Inverters.Inverter2.ModbusId);
                }
                if (Settings.Inverters.Inverter3.IsActive)
                {
                    power += await modbusService.GetPvInput(Settings.Inverters.Inverter3.ModbusId);
                }
                power = Math.Round(power, 3);

                var soc = await modbusService.GetSoc(Settings.Inverters.Inverter1.ModbusId);
                Log.Information($"New measurement: PV Power = {power} kW, SOC = {soc} %");

                RelayAction action;
                if (power >= Settings.PowerThreshold && soc >= Settings.SocThreshold)
                {
                    action = RelayAction.Anschalten;
                }
                else
                {
                    action = RelayAction.Ausschalten;
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
