﻿using Microsoft.AspNetCore.SignalR;
using SolarHeaterControl.Server.Interfaces;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;
using System.Collections.Generic;

namespace SolarHeaterControl.Server.Hubs
{
    public class CommunicationHub : Hub
    {
        private readonly IRelayService relayService;
        private readonly LogStore logStore;

        public CommunicationHub(IRelayService relayService, LogStore logStore)
        {
            this.relayService = relayService;
            this.logStore = logStore;
        }

        public async Task SendLog(LogEntry log)
        {
            await Clients.All.SendAsync("ReceiveLog", log);
        }

        public async Task SendRelayStatus(RelayStatus status)
        {
            await Clients.All.SendAsync("ReceiveRelayStatus", status);
        }

        public async Task<IEnumerable<LogEntry>> GetLogs()
        {
            return await Task.Run(() => logStore.GetLogs());
        }
        public async Task<RelayStatus> GetRelayStatus()
        {
            return await relayService.GetRelayStatus();
        }
    }
}
