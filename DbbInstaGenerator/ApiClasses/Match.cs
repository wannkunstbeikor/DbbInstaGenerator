using System.Text.Json.Serialization;

namespace DbbInstaGenerator.ApiClasses;

public class Match
{
    [JsonPropertyName("ligaData")]
    public LigaData LigaData { get; set; }

    [JsonPropertyName("matchId")]
    public int? MatchId { get; set; }

    [JsonPropertyName("matchDay")]
    public int? MatchDay { get; set; }

    [JsonPropertyName("matchNo")]
    public int? MatchNo { get; set; }

    [JsonPropertyName("kickoffDate")]
    public string KickoffDate { get; set; }

    [JsonPropertyName("kickoffTime")]
    public string KickoffTime { get; set; }

    [JsonPropertyName("homeTeam")]
    public Team HomeTeam { get; set; }

    [JsonPropertyName("guestTeam")]
    public Team GuestTeam { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; }

    [JsonPropertyName("ergebnisbestaetigt")]
    public bool? Ergebnisbestaetigt { get; set; }

    [JsonPropertyName("statisticType")]
    public int? StatisticType { get; set; }

    [JsonPropertyName("verzicht")]
    public bool? Verzicht { get; set; }

    [JsonPropertyName("abgesagt")]
    public bool? Abgesagt { get; set; }

    [JsonPropertyName("matchResult")]
    public MatchResult? MatchResult { get; set; }

    [JsonPropertyName("matchInfo")]
    public MatchInfo? MatchInfo { get; set; }

    [JsonPropertyName("matchBoxscore")]
    public object MatchBoxscore { get; set; }

    [JsonPropertyName("playByPlay")]
    public object? PlayByPlay { get; set; }

    [JsonPropertyName("hasPlayByPlay")]
    public bool? HasPlayByPlay { get; set; }
}