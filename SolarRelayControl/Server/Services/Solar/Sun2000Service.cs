using NModbus;
using SolarRelayControl.Server.Interfaces;
using SolarRelayControl.Shared.Models;
using System.Net.Sockets;

namespace SolarRelayControl.Server.Services.Solar
{
    /// <summary>
    /// Implementation of <see cref="ISolarService"/> for Huawei Sun2000 Inverters
    /// </summary>
    public class Sun2000Service : ISolarService
    {
        private readonly IConfiguration configuration;
        private Settings Settings => configuration.Get<Settings>();


        public Sun2000Service(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc />
        public async Task<double> GetPvInput()
        {
            double power = 0;
            if (Settings.Inverters.Inverter1.IsActive)
            {
                power += await getValueFromRegister(byte.Parse(Settings.Inverters.Inverter1.ModbusId.ToString()), 32064, 2, 1) / 1000;
            }
            if (Settings.Inverters.Inverter2.IsActive)
            {
                power += await getValueFromRegister(byte.Parse(Settings.Inverters.Inverter2.ModbusId.ToString()), 32064, 2, 1) / 1000;
            }
            if (Settings.Inverters.Inverter3.IsActive)
            {
                power += await getValueFromRegister(byte.Parse(Settings.Inverters.Inverter3.ModbusId.ToString()), 32064, 2, 1) / 1000;
            }
            return Math.Round(power, 3);
        }

        /// <inheritdoc />
        public async Task<double> GetSoc()
        {
            return await getValueFromRegister(byte.Parse(Settings.Inverters.Inverter1.ModbusId.ToString()), 37760, 1, 0) / 10;
        }

        private async Task<double> getValueFromRegister(byte slaveAddress, ushort address, ushort quantity, int position)
        {
            using (var client = new TcpClient(Settings.DongleIp, Settings.DongleModbusPort))
            {
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                Thread.Sleep(1000);
                var registers = await master.ReadHoldingRegistersAsync(slaveAddress, address, quantity);
                Thread.Sleep(1000);

                return registers[position];
            };
        }
    }
}
