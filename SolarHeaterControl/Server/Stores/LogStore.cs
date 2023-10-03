﻿using Serilog;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Stores
{
    public class LogStore
    {
        private readonly List<LogEntry> _logs = new();
        public List<LogEntry> GetLogs() => _logs;
        public void AddLogEntry(LogEntry entry)
        {
            _logs.Add(entry);
            Log.Information($"New measurement: PV Power = {entry.CurrentPower} kW, SOC = {entry.CurrentSoc} %, Action = {entry.Action}");
        }
    }
}