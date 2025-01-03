using System.Text.Json.Serialization;

namespace DbbInstaGenerator.ApiClasses;

public class Player
{
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }

    [JsonPropertyName("no")]
    public string No { get; set; }

    [JsonPropertyName("person")]
    public Person Person { get; set; }

    [JsonPropertyName("anonym")]
    public bool Anonym { get; set; }
}