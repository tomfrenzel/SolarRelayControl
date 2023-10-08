namespace SolarRelayControl.Server.Interfaces
{
    /// <summary>
    /// Service for communicating with the solar system
    /// </summary>
    public interface ISolarService
    {
        /// <summary>
        /// Gets the power which is currently getting produced by the solar system 
        /// </summary>
        /// <returns>Power in kW</returns>
        Task<double> GetPvInput();

        /// <summary>
        /// Gets the current battery SOC
        /// </summary>
        /// <returns>SOC in %</returns>
        Task<double> GetSoc();
    }
}