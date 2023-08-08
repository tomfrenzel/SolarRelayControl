using NModbus;
using Serilog;
using SolarHeaterControl.Server;
using SolarHeaterControl.Shared.Models;
using System;
using System.Net.Sockets;

namespace SolarHeaterControl.Client
{
    public class ControlService : BackgroundService
    {
        private readonly LogStore _logStore;
        private readonly IConfiguration configuration;
        private readonly HttpClient _httpClient;

        private Uri _relayBaseUri => new UriBuilder("http", Settings.RelayIp, 80, "relay/0").Uri;
        private Settings Settings => configuration.Get<Settings>();
        private record struct RelayResponse(bool ison);

        public ControlService(LogStore logStore, IConfiguration configuration)
        {
            this._logStore = logStore;
            this.configuration = configuration;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExecuteMeasurement();

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
            for (byte inverterNumber = 1; inverterNumber <= Settings.InverterCount; inverterNumber++)
            {
                power += await getValueFromRegister(inverterNumber, 32064, 2, 1) / 1000;
            }
            power = Math.Round(power, 3);

            var soc = await getValueFromRegister(1, 37760, 1, 0) / 10;
            Log.Information($"New measurement: PV Power = {power} kW, SOC = {soc} %");

            var result = await _httpClient.GetAsync(_relayBaseUri);
            var currentState = await result.Content.ReadFromJsonAsync<RelayResponse>();

            if (power >= Settings.PowerThreshold && soc >= Settings.SocThreshold)
            {
                if (!currentState.ison)
                {
                    await setRelayState("on");
                    createLog(power, soc, RelayAction.Anschalten);
                }
            }
            else
            {
                if (currentState.ison)
                {
                    await setRelayState("off");
                    createLog(power, soc, RelayAction.Ausschalten);
                }
            }

        }

        private async Task<double> getValueFromRegister(byte slaveAddress, ushort address, ushort quantity, int position)
        {
            using (var client = new TcpClient(Settings.InverterIp, Settings.InverterPort))
            {
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                Thread.Sleep(1000);
                var registers = await master.ReadHoldingRegistersAsync(slaveAddress, address, quantity);
                Thread.Sleep(1000);

                return registers[position];
            };
        }

        private async Task setRelayState(string state)
        {
            var uri = new UriBuilder(_relayBaseUri);
            uri.Query = $"turn={state}";

            await _httpClient.GetAsync(uri.Uri);
        }

        private void createLog(double power, double soc, RelayAction action)
        {
            _logStore.AddLogEntry(new LogEntry
            {
                Timestamp = DateTimeOffset.Now,
                CurrentPower = power,
                CurrentSoc = soc,
                Action = action
            });
        }
    }
}
