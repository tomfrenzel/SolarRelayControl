using EasyModbus;

namespace SolarHeaterControlApi
{
    public class ControlService : IHostedService, IDisposable
    {
        public static int RefreshPeriod = 10;
        public static string RelayIp = "192.168.3.153";
        public static string InverterIp = "192.168.3.152";
        public static int InverterPort = 502;
        public static int PowerThreshold = 2;
        public static int SocThreshold = 50;

        private readonly ILogger<ControlService> _logger;
        private readonly ModbusClient _modbusClient;
        private readonly HttpClient _httpClient;
        private readonly Uri _relayBaseUri = new UriBuilder("http", RelayIp, 80, "activate").Uri;
        private Timer? _timer = null;

        public ControlService(ILogger<ControlService> logger)
        {
            _logger = logger;
            _modbusClient = new ModbusClient(InverterIp, InverterPort);
            _httpClient = new HttpClient();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(RefreshPeriod));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var power = GetValueFromRegister(32064, 2, 1) / 1000;
            _logger.LogInformation($"Power: {power}kw");

            var soc = GetValueFromRegister(37760, 1, 0) / 10;
            _logger.LogInformation($"SOC: {soc}%");

            if (power >= PowerThreshold && soc >= SocThreshold)
            {
                _httpClient.GetAsync(_relayBaseUri);
                _logger.LogInformation("Heater started!");
            }
            else
            {
                _httpClient.GetAsync(_relayBaseUri);
                _logger.LogInformation("Heater stopped!");
            }
        }

        private int GetValueFromRegister(int address, int quantity, int position)
        {
            _modbusClient.Disconnect();
            Thread.Sleep(1000);
            _modbusClient.Connect();

            var value = 1;
            var valueReadoutSuccess = false;
            while (!valueReadoutSuccess)
            {
                try
                {
                    Thread.Sleep(1000);
                    var res = _modbusClient.ReadHoldingRegisters(address, quantity);
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

            _modbusClient.Disconnect();
            return value;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
