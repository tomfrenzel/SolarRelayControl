using Microsoft.AspNetCore.SignalR;
using Serilog;
using SolarRelayControl.Server.Hubs;
using SolarRelayControl.Server.Interfaces;
using SolarRelayControl.Shared.Models;
using SolarRelayControl.Shared.Models.Settings;
using System.Text.Json.Serialization;

namespace SolarRelayControl.Server.Services.Relay
{
    /// <summary>
    /// Implementation of <see cref="IRelayService"/> for Shelly Pro Relays
    /// </summary>
    public class ShellyRelayService : IRelayService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly IHubContext<CommunicationHub> hubContext;

        private Settings Settings => configuration.Get<Settings>();
        private Uri relayControlUri => new UriBuilder("http", Settings.ShellySettings.Ip, 80, "relay/0").Uri;
        private Uri relayStatuslUri => new UriBuilder("http", Settings.ShellySettings.Ip, 80, "rpc/Switch.GetStatus").Uri;

        public ShellyRelayService(HttpClient httpClient, IConfiguration configuration, IHubContext<CommunicationHub> hubContext)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.hubContext = hubContext;
        }

        /// <inheritdoc />
        public async Task SetRelayState(RelayAction action)
        {
            var uri = new UriBuilder(relayControlUri);
            var state = action == RelayAction.Anschalten ? "on" : "off";
            uri.Query = $"turn={state}";

            await httpClient.GetAsync(uri.Uri);

            var newRelayStatus = await GetRelayStatus();
            await hubContext.Clients.All.SendAsync("ReceiveRelayStatus", newRelayStatus);
        }

        /// <inheritdoc />
        public async Task<RelayStatus> GetRelayStatus()
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
            return new RelayStatus(status.Output, status.Current, status.Aenergy?.Total, status.Temperature.TC);
        }

        private record RelayStatusResponse(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("source")] string Source,
            [property: JsonPropertyName("output")] bool Output,
            [property: JsonPropertyName("apower")] double? Apower,
            [property: JsonPropertyName("voltage")] double? Voltage,
            [property: JsonPropertyName("freq")] double? Freq,
            [property: JsonPropertyName("current")] double? Current,
            [property: JsonPropertyName("pf")] double? Pf,
            [property: JsonPropertyName("aenergy")] Aenergy? Aenergy,
            [property: JsonPropertyName("temperature")] Temperature Temperature
        );

        private record Aenergy(
            [property: JsonPropertyName("total")] double Total,
            [property: JsonPropertyName("by_minute")] IReadOnlyList<double> ByMinute,
            [property: JsonPropertyName("minute_ts")] int MinuteTs
        );

        private record Temperature(
            [property: JsonPropertyName("tC")] double TC,
            [property: JsonPropertyName("tF")] double TF
        );
    }
}
