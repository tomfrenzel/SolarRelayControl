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

        private Uri _relayBaseUri => new UriBuilder("http", Settings.RelayIp, 80, "activate").Uri;
        private Settings Settings => configuration.Get<Settings>();

        public ControlService(LogStore logStore, IConfiguration configuration)
        {
            this._logStore = logStore;
            this.configuration = configuration;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(Settings.RefreshPeriod));
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                var power = await GetValueFromRegister(32064, 2, 1) / 1000;
                var soc = await GetValueFromRegister(37760, 1, 0) / 10;

                if (power >= Settings.PowerThreshold && soc >= Settings.SocThreshold)
                {
                    await _httpClient.GetAsync(_relayBaseUri);
                    _logStore.AddLogEntry(new LogEntry
                    {
                        Timestamp = DateTimeOffset.Now,
                        CurrentPower = power,
                        CurrentSoc = soc,
                        Action = RelaisAction.PowerOn
                    });
                }
                else
                {
                    await _httpClient.GetAsync(_relayBaseUri);
                    _logStore.AddLogEntry(new LogEntry
                    {
                        Timestamp = DateTimeOffset.Now,
                        CurrentPower = power,
                        CurrentSoc = soc,
                        Action = RelaisAction.PowerOff
                    });
                }
            }
        }

        private async Task<int> GetValueFromRegister(ushort address, ushort quantity, int position)
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
    }
}
