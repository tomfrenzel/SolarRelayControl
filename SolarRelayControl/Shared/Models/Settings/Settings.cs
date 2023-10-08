using System.ComponentModel.DataAnnotations;

namespace SolarRelayControl.Shared.Models.Settings
{
    public class Settings
    {
        [Required]
        public Sun2000Settings Sun2000Settings { get; set; } = new();

        [Required]
        public ShellySettings ShellySettings { get; set; } = new();

        [Required]
        public double PowerThreshold { get; set; } = 0;

        [Required]
        [Range(0, 100, ErrorMessage = "Der Wert muss zwischen 0 und 100 liegen")]
        public double SocThreshold { get; set; } = 0;
    }
}
