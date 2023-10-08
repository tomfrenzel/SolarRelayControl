using System.ComponentModel.DataAnnotations;

namespace SolarRelayControl.Shared.Models.Settings
{
    public class Sun2000Settings
    {
        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? Ip { get; set; }

        [Required]
        public int ModbusPort { get; set; } = 502;

        [Required]
        public Inverters Inverters { get; set; } = new();
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
