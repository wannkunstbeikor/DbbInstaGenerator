using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Test.ApiClasses;

public class MatchInfo
{
    [JsonPropertyName("topPerformances")]
    public List<TopPerformance> TopPerformances { get; set; }

    [JsonPropertyName("spielfeld")]
    public Spielfeld Spielfeld { get; set; }

    [JsonPropertyName("srList")]
    public object? SrList { get; set; }
}