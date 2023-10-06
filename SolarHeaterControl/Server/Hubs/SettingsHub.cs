using Microsoft.AspNetCore.SignalR;
using SolarHeaterControl.Server.Extensions;
using SolarHeaterControl.Server.Services;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;
using System.Collections.Generic;

namespace SolarHeaterControl.Server.Hubs
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
