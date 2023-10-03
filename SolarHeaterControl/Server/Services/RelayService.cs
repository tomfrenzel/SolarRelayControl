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

        private Settings Settings => configuration.Get<Settings>();
        private Uri relayControlUri => new UriBuilder("http", Settings.RelayIp, 80, "relay/0").Uri;
        private Uri relayStatuslUri => new UriBuilder("http", Settings.RelayIp, 80, "rpc/Switch.GetStatus").Uri;

        public RelayService(HttpClient httpClient, IConfiguration configuration, LogStore logStore)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.logStore = logStore;
        }
        public async Task SetRelayState(RelayAction action, double power, double soc)
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

            logStore.AddLogEntry(new LogEntry
            {
                Timestamp = DateTimeOffset.Now,
                CurrentPower = power,
                CurrentSoc = soc,
                Action = action
            });
        }

        public async Task<RelayStatusResponse> GetRelayStatus()
        {
            var uri = new UriBuilder(relayStatuslUri);
            uri.Query = $"id=0";

            var result = await httpClient.GetAsync(uri.Uri);
            var staus = await result.Content.ReadFromJsonAsync<RelayStatusResponse>();
            if (staus == null)
            {
                throw new ArgumentNullException(nameof(staus));
            }

            return staus;
        }
    }
}
