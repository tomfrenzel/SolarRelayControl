using Microsoft.AspNetCore.SignalR;
using Serilog;
using SolarHeaterControl.Server.Hubs;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;
using System.Net.Http;

namespace SolarHeaterControl.Server.Services
{
    public class RelayService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly IHubContext<CommunicationHub> hubContext;

        private Settings Settings => configuration.Get<Settings>();
        private Uri relayControlUri => new UriBuilder("http", Settings.RelayIp, 80, "relay/0").Uri;
        private Uri relayStatuslUri => new UriBuilder("http", Settings.RelayIp, 80, "rpc/Switch.GetStatus").Uri;

        public RelayService(HttpClient httpClient, IConfiguration configuration, IHubContext<CommunicationHub> hubContext)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.hubContext = hubContext;
        }
        public async Task SetRelayState(RelayAction action)
        {
            string state = string.Empty;
            RelayStatusResponse currentStatus = await GetRelayStatus();

            if (!currentStatus.Output && action == RelayAction.Anschalten)
            {
                state = "on";
            }
            else if (currentStatus.Output && action == RelayAction.Ausschalten)
            {
                state = "off";
            }
            else
            {
                return;
            }

            var uri = new UriBuilder(relayControlUri);
            uri.Query = $"turn={state}";

            await httpClient.GetAsync(uri.Uri);

            var newRelayStatus = await GetRelayStatus();
            await hubContext.Clients.All.SendAsync("ReceiveRelayStatus", newRelayStatus);
        }

        public async Task<RelayStatusResponse> GetRelayStatus()
        {
            var uri = new UriBuilder(relayStatuslUri);
            uri.Query = $"id=0";

            var result = await httpClient.GetAsync(uri.Uri);
            var status = await result.Content.ReadFromJsonAsync<RelayStatusResponse>();
            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            Log.Information($"Relay Status: Output = {status.Output}, Temperature = {status.Temperature.TC} °C, Power = {status.Apower?.ToString() ?? "N/A"}");
            return status;
        }
    }
}
