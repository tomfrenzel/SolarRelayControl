using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Interfaces
{
    public interface IRelayService
    {
        Task<RelayStatus> GetRelayStatus();
        Task SetRelayState(RelayAction action);
    }
}