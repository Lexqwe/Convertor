using System.Text.Json.Serialization;

namespace Api.Services;

internal class CbrResponse
{
    [JsonPropertyName("Date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("Valute")]
    public Dictionary<string, CbrValute> Valute { get; set; } = new();
}

internal class CbrValute
{
    [JsonPropertyName("CharCode")]
    public string CharCode { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Nominal")]
    public int Nominal { get; set; }

    [JsonPropertyName("Value")]
    public decimal Value { get; set; }
}