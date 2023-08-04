using System.ComponentModel.DataAnnotations;

namespace SolarHeaterControl.Shared.Models
{
    public class Settings
    {
        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? InverterIp { get; set; }

        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? RelayIp { get; set; }

        [Required]
        public int InverterPort { get; set; } = 502;

        [Required]
        public int? PowerThreshold { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Der Wert muss zwischen 0 und 100 liegen")]
        public int? SocThreshold { get; set; }
    }
}
