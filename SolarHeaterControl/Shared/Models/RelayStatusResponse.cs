using System.Text.Json.Serialization;

namespace SolarHeaterControl.Shared.Models
{
    public record RelayStatusResponse(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("output")] bool Output,
        [property: JsonPropertyName("apower")] double? Apower,
        [property: JsonPropertyName("voltage")] double? Voltage,
        [property: JsonPropertyName("freq")] double? Freq,
        [property: JsonPropertyName("current")] double? Current,
        [property: JsonPropertyName("pf")] double? Pf,
        [property: JsonPropertyName("aenergy")] Aenergy? Aenergy,
        [property: JsonPropertyName("temperature")] Temperature Temperature
    );


    public record Aenergy(
        [property: JsonPropertyName("total")] double Total,
        [property: JsonPropertyName("by_minute")] IReadOnlyList<double> ByMinute,
        [property: JsonPropertyName("minute_ts")] int MinuteTs
    );

    public record Temperature(
        [property: JsonPropertyName("tC")] double TC,
        [property: JsonPropertyName("tF")] double TF
    );
}
