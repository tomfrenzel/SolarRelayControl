﻿namespace SolarRelayControl.Shared.Models
{
    public record RelayStatus(
        bool IsOn,
        double? CurrentPower,
        double? TotalPower,
        double? Temperature
    );
}
