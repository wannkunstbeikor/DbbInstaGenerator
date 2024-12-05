using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Test.ApiClasses;

public class MatchBoxscore
{
    [JsonPropertyName("homePlayerStats")]
    public List<Stats> HomePlayerStats { get; set; }

    [JsonPropertyName("homeTeamStats")]
    public object? HomeTeamStats { get; set; }

    [JsonPropertyName("homeTotalStats")]
    public Stats HomeTotalStats { get; set; }

    [JsonPropertyName("guestPlayerStats")]
    public List<Stats> GuestPlayerStats { get; set; }

    [JsonPropertyName("guestTeamStats")]
    public object? GuestTeamStats { get; set; }

    [JsonPropertyName("guestTotalStats")]
    public Stats GuestTotalStats { get; set; }
}