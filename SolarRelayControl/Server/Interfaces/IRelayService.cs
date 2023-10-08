using SolarRelayControl.Shared.Models;

namespace SolarRelayControl.Server.Interfaces
{
    public interface IRelayService
    {
        Task<RelayStatus> GetRelayStatus();
        Task SetRelayState(RelayAction action);
    }
}