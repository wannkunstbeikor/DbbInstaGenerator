using System.Text.Json.Serialization;

namespace DbbInstaGenerator.ApiClasses;

public class MatchDay
{
    [JsonPropertyName("spieltag")]
    public int Spieltag { get; set; }
    
    [JsonPropertyName("bezeichnung")]
    public string Bezeichnung { get; set; }
}