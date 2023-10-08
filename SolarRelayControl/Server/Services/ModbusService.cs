using NModbus;
using SolarRelayControl.Shared.Models;
using System.Net.Sockets;

namespace SolarRelayControl.Server.Services
{
    public class ModbusService
    {
        private readonly IConfiguration configuration;
        private Settings Settings => configuration.Get<Settings>();


        public ModbusService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<double> GetPvInput(int slaveAddress)
        {
            var inverterId = byte.Parse(slaveAddress.ToString());
            return await getValueFromRegister(inverterId, 32064, 2, 1) / 1000;
        }

        public async Task<double> GetSoc(int slaveAddress)
        {
            var inverterId = byte.Parse(slaveAddress.ToString());
            return await getValueFromRegister(inverterId, 37760, 1, 0) / 10;
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
