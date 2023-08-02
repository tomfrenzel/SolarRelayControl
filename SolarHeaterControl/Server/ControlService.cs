using EasyModbus;
using SolarHeaterControl.Server;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Client
{
    public class ControlService : IHostedService, IDisposable
    {
        public static Settings Settings { get; set; } = new Settings
        {
            RefreshPeriod = 10,
            RelayIp = "192.168.3.153",
            InverterIp = "192.168.3.152",
            InverterPort = 502,
            PowerThreshold = 2,
            SocThreshold = 50
        };

        private readonly LogStore _logStore;
        private readonly HttpClient _httpClient;
        private readonly Uri _relayBaseUri = new UriBuilder("http", Settings.RelayIp, 80, "activate").Uri;
        private Timer? _timer = null;

        public ControlService(LogStore logStore)
        {
            this._logStore = logStore;
            _httpClient = new HttpClient();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(Settings.RefreshPeriod));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var modbusClient = new ModbusClient(Settings.InverterIp, Settings.InverterPort);
            var power = GetValueFromRegister(modbusClient, 32064, 2, 1) / 1000;
            var soc = GetValueFromRegister(modbusClient, 37760, 1, 0) / 10;

            if (power >= Settings.PowerThreshold && soc >= Settings.SocThreshold)
            {
                _httpClient.GetAsync(_relayBaseUri);
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
                _httpClient.GetAsync(_relayBaseUri);
                _logStore.AddLogEntry(new LogEntry
                {
                    Timestamp = DateTimeOffset.Now,
                    CurrentPower = power,
                    CurrentSoc = soc,
                    Action = RelaisAction.PowerOff
                });
            }
        }

        private int GetValueFromRegister(ModbusClient modbusClient, int address, int quantity, int position)
        {
            Thread.Sleep(1000);
            modbusClient.Connect();

            var value = 1;
            var valueReadoutSuccess = false;
            while (!valueReadoutSuccess)
            {
                try
                {
                    Thread.Sleep(1000);
                    var res = modbusClient.ReadHoldingRegisters(address, quantity);
                    if (res.Length == quantity)
                    {
                        var newValue = res[position];
                        if (newValue == 0)
                        {
                            if (value == 0)
                            {
                                valueReadoutSuccess = true;
                            }
                            else
                            {
                                value = 0;
                                valueReadoutSuccess = false;
                            }
                        }
                        else
                        {
                            value = newValue;
                            valueReadoutSuccess = true;
                        }
                    }
                }
                catch (Exception) { }
            }

            modbusClient.Disconnect();
            return value;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
