namespace SolarHeaterControl.Shared.Models
{
    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public int CurrentPower { get; set; }
        public int CurrentSoc { get; set; }
        public RelaisAction? Action { get; set; }
    }
}
