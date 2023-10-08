using System.ComponentModel.DataAnnotations;

namespace SolarRelayControl.Shared.Models
{
    public class Settings
    {
        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? DongleIp { get; set; }

        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? RelayIp { get; set; }

        [Required]
        public int DongleModbusPort { get; set; } = 502;

        [Required]
        public Inverters Inverters { get; set; } = new();

        [Required]
        public double? PowerThreshold { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Der Wert muss zwischen 0 und 100 liegen")]
        public double? SocThreshold { get; set; }
    }

    public class Inverters
    {
        public Inverter Inverter1 { get; set; } = new();
        public Inverter Inverter2 { get; set; } = new();
        public Inverter Inverter3 { get; set; } = new();
    }

    public class Inverter
    {
        public bool IsActive { get; set; }
        public int ModbusId { get; set; }
    }
}
