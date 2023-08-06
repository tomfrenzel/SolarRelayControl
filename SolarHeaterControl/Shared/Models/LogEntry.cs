namespace SolarHeaterControl.Shared.Models
{
    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public double CurrentPower { get; set; }
        public double CurrentSoc { get; set; }
        public RelayAction Action { get; set; }
    }
}
