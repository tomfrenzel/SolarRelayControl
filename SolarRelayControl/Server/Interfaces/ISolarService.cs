namespace SolarRelayControl.Server.Interfaces
{
    public interface ISolarService
    {
        Task<double> GetPvInput(int slaveAddress);
        Task<double> GetSoc(int slaveAddress);
    }
}