using System.Text.Json.Serialization;

namespace DbbInstaGenerator.ApiClasses;

public class TopPerformance
{
    [JsonPropertyName("performance")]
    public string Performance { get; set; }

    [JsonPropertyName("playerA")]
    public Player PlayerA { get; set; }

    [JsonPropertyName("playerB")]
    public Player PlayerB { get; set; }

    [JsonPropertyName("performanceA")]
    public double PerformanceA { get; set; }

    [JsonPropertyName("performanceB")]
    public double PerformanceB { get; set; }
}