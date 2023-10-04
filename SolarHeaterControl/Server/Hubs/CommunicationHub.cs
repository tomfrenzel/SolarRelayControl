using Microsoft.AspNetCore.SignalR;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Hubs
{
    public class CommunicationHub : Hub
    {
        public async Task SendLog(LogEntry log)
        {
            await Clients.All.SendAsync("ReceiveLog", log);
        }
        public async Task SendRelayStatus(RelayStatusResponse status)
        {
            await Clients.All.SendAsync("ReceiveRelayStatus", status);
        }
    }
}
