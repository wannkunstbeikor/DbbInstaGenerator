using System.Text.Json.Serialization;

namespace Test.ApiClasses;

public class Spielfeld
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("bezeichnung")]
    public string Bezeichnung { get; set; }

    [JsonPropertyName("strasse")]
    public string Strasse { get; set; }

    [JsonPropertyName("plz")]
    public string Plz { get; set; }

    [JsonPropertyName("ort")]
    public string Ort { get; set; }
}