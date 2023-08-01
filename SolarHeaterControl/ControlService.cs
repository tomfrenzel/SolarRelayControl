using EasyModbus;

namespace SolarHeaterControl
{
    public class ControlService : IHostedService, IDisposable
    {
        public static int RefreshPeriod = 10;
        public static string InverterIp = "192.168.3.152";
        public static int InverterPort = 502;

        private readonly ILogger<ControlService> _logger;
        private readonly ModbusClient _client;
        private Timer? _timer = null;

        public ControlService(ILogger<ControlService> logger)
        {
            _logger = logger;
            _client = new ModbusClient(InverterIp, InverterPort);
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
            var soc = GetValueFromRegister(37760, 1, 0) / 10;

            _logger.LogInformation($"Power: {power}kw");
            _logger.LogInformation($"SOC: {soc}%");
        }

        private int GetValueFromRegister(int address, int qunatity, int position)
        {
            _client.Disconnect();
            Thread.Sleep(1000);
            _client.Connect();

            var value = 1;
            var valueReadoutSuccess = false;
            while (!valueReadoutSuccess)
            {
                try
                {
                    Thread.Sleep(1000);
                    var res = _client.ReadHoldingRegisters(address, qunatity);
                    if (res.Length == qunatity)
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

            _client.Disconnect();
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
