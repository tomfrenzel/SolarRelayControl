using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server
{
    public class LogStore
    {
        private readonly List<LogEntry> _logs = new();
        public List<LogEntry> GetLogs() => _logs;
        public void AddLogEntry(LogEntry entry) { _logs.Add(entry); }
    }
}
