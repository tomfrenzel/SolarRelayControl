using SolarRelayControl.Shared.Models;

namespace SolarRelayControl.Server.Interfaces
{
    /// <summary>
    /// Service for communicating wit a relay
    /// </summary>
    public interface IRelayService
    {
        /// <summary>
        /// Gets the current status of the relay
        /// </summary>
        /// <returns></returns>
        Task<RelayStatus> GetRelayStatus();

        /// <summary>
        /// Sets the relay Output
        /// </summary>
        /// <param name="action">Action to da with the relay</param>
        /// <returns></returns>
        Task SetRelayState(RelayAction action);
    }
}