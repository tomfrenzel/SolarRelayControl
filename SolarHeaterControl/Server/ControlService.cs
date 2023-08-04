using NModbus;
using SolarHeaterControl.Server;
using SolarHeaterControl.Shared.Models;
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
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(Settings.RefreshPeriod));
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                var power = await getValueFromRegister(32064, 2, 1) / 1000;
                var soc = await getValueFromRegister(37760, 1, 0) / 10;

                var result = await _httpClient.GetAsync(_relayBaseUri);
                var currentState = await result.Content.ReadFromJsonAsync<RelayResponse>();

                if (power >= Settings.PowerThreshold && soc >= Settings.SocThreshold)
                {
                    if (currentState.ison)
                    {
                        createLog(power, soc);
                    }
                    else
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
                    else
                    {
                        createLog(power, soc);
                    }
                }
            }
        }

        private async Task<double> getValueFromRegister(ushort address, ushort quantity, int position)
        {
            using (var client = new TcpClient(Settings.InverterIp, Settings.InverterPort))
            {
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                Thread.Sleep(1000);
                var registers = await master.ReadHoldingRegistersAsync(1, address, quantity);
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

        private void createLog(double power, double soc, RelayAction? action = null)
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
