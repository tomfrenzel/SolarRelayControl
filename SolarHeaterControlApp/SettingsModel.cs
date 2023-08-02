﻿using System.ComponentModel.DataAnnotations;

namespace SolarHeaterControlApp
{
    public class SettingsModel
    {
        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? ServerIp { get; set; }

        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? InverterIp { get; set; }

        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Eingabe hat ein falsches Format")]
        public string? RelayIp { get; set; }

        [Required]
        public int? InverterPort { get; set; }

        [Required]
        public int? RefreshPeriod { get; set; }

        [Required]
        public int? PowerThreshold { get; set; }

        [Required]
        [Range(0,100, ErrorMessage = "Der Wert muss zwischen 0 und 100 liegen")]
        public int? SocThreshold { get; set; }
    }
}
