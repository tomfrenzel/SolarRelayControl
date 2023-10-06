using Microsoft.AspNetCore.SignalR;
using SolarHeaterControl.Server.Services;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;
using System.Collections.Generic;

namespace SolarHeaterControl.Server.Hubs
{
    public class CommunicationHub : Hub
    {
        private readonly RelayService relayService;
        private readonly LogStore logStore;

        public CommunicationHub(RelayService relayService, LogStore logStore)
        {
            this.relayService = relayService;
            this.logStore = logStore;
        }

        public async Task SendLog(LogEntry log)
        {
            await Clients.All.SendAsync("ReceiveLog", log);
        }

        public async Task SendRelayStatus(RelayStatusResponse status)
        {
            await Clients.All.SendAsync("ReceiveRelayStatus", status);
        }

        public async Task<IEnumerable<LogEntry>> GetLogs()
        {
            return await Task.Run(() => logStore.GetLogs());
        }
        public async Task<RelayStatusResponse> GetRelayStatus()
        {
            return await relayService.GetRelayStatus();
        }
    }
}
