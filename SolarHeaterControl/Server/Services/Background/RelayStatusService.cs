﻿using Microsoft.AspNetCore.SignalR;
using Serilog;
using SolarHeaterControl.Server.Hubs;
using SolarHeaterControl.Server.Interfaces;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Services.Background
{
    public class RelayStatusService : BackgroundService
    {
        private readonly IRelayService relayService;
        private readonly CommunicationHub communicationHub;

        public RelayStatusService(IRelayService relayService, CommunicationHub communicationHub)
        {
            this.relayService = relayService;
            this.communicationHub = communicationHub;
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
                    await communicationHub.SendRelayStatus(status);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An unexpected error occured");
                }
            }

        }
    }
}
