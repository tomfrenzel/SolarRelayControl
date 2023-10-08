using System.ComponentModel.DataAnnotations;

namespace SolarRelayControl.Shared.Models.Settings
{
    public class ShellySettings
    {
        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? Ip { get; set; }
    }
}
