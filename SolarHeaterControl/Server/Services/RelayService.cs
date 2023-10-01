using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;
using System.Net.Http;

namespace SolarHeaterControl.Server.Services
{
    public class RelayService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly LogStore logStore;

        private record struct RelayResponse(bool ison);
        private Settings Settings => configuration.Get<Settings>();
        private Uri _relayBaseUri => new UriBuilder("http", Settings.RelayIp, 80, "relay/0").Uri;

        public RelayService(HttpClient httpClient, IConfiguration configuration, LogStore logStore)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.logStore = logStore;
        }
        public async Task SetRelayState(RelayAction action, double power, double soc)
        {
            string state = string.Empty;
            var result = await httpClient.GetAsync(_relayBaseUri);
            var currentState = await result.Content.ReadFromJsonAsync<RelayResponse>();

            if (!currentState.ison && action == RelayAction.Anschalten)
            {
                state = "on";
            }
            else if (currentState.ison && action == RelayAction.Ausschalten)
            {
                state = "off";
            }

            var uri = new UriBuilder(_relayBaseUri);
            uri.Query = $"turn={state}";

            await httpClient.GetAsync(uri.Uri);

            logStore.AddLogEntry(new LogEntry
            {
                Timestamp = DateTimeOffset.Now,
                CurrentPower = power,
                CurrentSoc = soc,
                Action = action
            });
        }
    }
}
