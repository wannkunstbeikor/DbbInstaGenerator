using System.Text.Json.Serialization;

namespace Test.ApiClasses;

public class LigaData
{
    [JsonPropertyName("seasonId")]
    public int? SeasonId { get; set; }

    [JsonPropertyName("seasonName")]
    public string SeasonName { get; set; }

    [JsonPropertyName("actualMatchDay")]
    public MatchDay? ActualMatchDay { get; set; }

    [JsonPropertyName("ligaId")]
    public int? LigaId { get; set; }

    [JsonPropertyName("liganame")]
    public string Liganame { get; set; }

    [JsonPropertyName("liganr")]
    public int? Liganr { get; set; }

    [JsonPropertyName("skName")]
    public string SkName { get; set; }

    [JsonPropertyName("skNameSmall")]
    public string SkNameSmall { get; set; }

    [JsonPropertyName("skEbeneId")]
    public int? SkEbeneId { get; set; }

    [JsonPropertyName("skEbeneName")]
    public string SkEbeneName { get; set; }

    [JsonPropertyName("akName")]
    public string AkName { get; set; }

    [JsonPropertyName("geschlechtId")]
    public int? GeschlechtId { get; set; }

    [JsonPropertyName("geschlecht")]
    public string Geschlecht { get; set; }

    [JsonPropertyName("verbandId")]
    public int? VerbandId { get; set; }

    [JsonPropertyName("verbandName")]
    public string VerbandName { get; set; }

    [JsonPropertyName("bezirknr")]
    public int? Bezirknr { get; set; }

    [JsonPropertyName("bezirkName")]
    public string BezirkName { get; set; }

    [JsonPropertyName("kreisnr")]
    public int? Kreisnr { get; set; }

    [JsonPropertyName("kreisname")]
    public string? Kreisname { get; set; }

    [JsonPropertyName("statisticType")]
    public int? StatisticType { get; set; }

    [JsonPropertyName("vorabliga")]
    public bool? Vorabliga { get; set; }

    [JsonPropertyName("tableExists")]
    public bool? TableExists { get; set; }

    [JsonPropertyName("crossTableExists")]
    public bool? CrossTableExists { get; set; }
}