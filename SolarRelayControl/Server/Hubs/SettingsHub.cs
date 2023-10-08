using Microsoft.AspNetCore.SignalR;
using SolarRelayControl.Server.Extensions;
using SolarRelayControl.Server.Services;
using SolarRelayControl.Server.Stores;
using SolarRelayControl.Shared.Models;
using System.Collections.Generic;

namespace SolarRelayControl.Server.Hubs
{
    public class SettingsHub : Hub
    {
        private readonly IConfiguration configuration;

        public SettingsHub(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task UpdateSettings(Settings settings)
        {
            await Task.Run(() => configuration.Set(settings));
        }

        public async Task<Settings> GetSettings()
        {
            return await Task.Run(() => configuration.Get<Settings>());
        }
    }
}
