using System.Text.Json.Serialization;

namespace Test.ApiClasses;

public class Club
{
    [JsonPropertyName("vereinId")]
    public int? VereinId { get; set; }

    [JsonPropertyName("vereinsname")]
    public string Vereinsname { get; set; }

    [JsonPropertyName("vereinsnummer")]
    public string Vereinsnummer { get; set; }

    [JsonPropertyName("kontaktData")]
    public object KontaktData { get; set; }
}