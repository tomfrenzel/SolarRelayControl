using Microsoft.AspNetCore.SignalR;
using Serilog;
using SolarRelayControl.Server.Hubs;
using SolarRelayControl.Server.Interfaces;

namespace SolarRelayControl.Server.Services
{
    /// <summary>
    /// Service to periodically check the relay status
    /// </summary>
    public class RelayStatusService : BackgroundService
    {
        private readonly IRelayService relayService;
        private readonly IHubContext<CommunicationHub> hubContext;

        public RelayStatusService(IRelayService relayService, IHubContext<CommunicationHub> hubContext)
        {
            this.relayService = relayService;
            this.hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var status = await relayService.GetRelayStatus();
                    await hubContext.Clients.All.SendAsync(nameof(CommunicationHub.SendRelayStatus), status);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An unexpected error occured");
                }
            }

        }
    }
}
