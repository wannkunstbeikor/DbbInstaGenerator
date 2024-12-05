using System.Text.Json.Serialization;

namespace Test.ApiClasses;

public class Team
{
    [JsonPropertyName("seasonTeamId")]
    public int? SeasonTeamId { get; set; }

    [JsonPropertyName("teamCompetitionId")]
    public int? TeamCompetitionId { get; set; }

    [JsonPropertyName("teamPermanentId")]
    public int? TeamPermanentId { get; set; }

    [JsonPropertyName("teamname")]
    public string Teamname { get; set; }

    [JsonPropertyName("teamnameSmall")]
    public string TeamnameSmall { get; set; }

    [JsonPropertyName("clubId")]
    public int? ClubId { get; set; }

    [JsonPropertyName("verzicht")]
    public bool? Verzicht { get; set; }
}