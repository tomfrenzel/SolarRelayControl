﻿using Serilog;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Services
{
    public class ControlService : BackgroundService
    {
        private readonly ModbusService modbusService;
        private readonly RelayService relayService;
        private readonly IConfiguration configuration;
 
        private Settings Settings => configuration.Get<Settings>();

        public ControlService(IConfiguration configuration, ModbusService modbusService, RelayService relayService)
        {
            this.configuration = configuration;
            this.modbusService = modbusService;
            this.relayService = relayService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await ExecuteMeasurement();
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ExecuteMeasurement();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An unexpected error occured");
                }
            }

        }

        private async Task ExecuteMeasurement()
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

            if (power >= Settings.PowerThreshold && soc >= Settings.SocThreshold)
            {
                await relayService.SetRelayState(RelayAction.Anschalten, power, soc);
            }
            else
            {
                await relayService.SetRelayState(RelayAction.Ausschalten, power, soc);
            }

        }
    }
}