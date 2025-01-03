using System.Text.Json.Serialization;

namespace DbbInstaGenerator.ApiClasses;

public class MatchResult
{
    [JsonPropertyName("heimEndstand")]
    public int HeimEndstand { get; set; }

    [JsonPropertyName("gastEndstand")]
    public int GastEndstand { get; set; }

    [JsonPropertyName("heimHalbzeitstand")]
    public int HeimHalbzeitstand { get; set; }

    [JsonPropertyName("gastHalbzeitstand")]
    public int GastHalbzeitstand { get; set; }

    [JsonPropertyName("heimV1stand")]
    public int HeimV1stand { get; set; }

    [JsonPropertyName("gastV1stand")]
    public int GastV1stand { get; set; }

    [JsonPropertyName("heimV3stand")]
    public int HeimV3stand { get; set; }

    [JsonPropertyName("gastV3stand")]
    public int GastV3stand { get; set; }

    [JsonPropertyName("heimV4stand")]
    public int HeimV4stand { get; set; }

    [JsonPropertyName("gastV4stand")]
    public int GastV4stand { get; set; }

    [JsonPropertyName("heimOt1stand")]
    public int? HeimOt1stand { get; set; }

    [JsonPropertyName("gastOt1stand")]
    public int? GastOt1stand { get; set; }

    [JsonPropertyName("heimOt2stand")]
    public int? HeimOt2stand { get; set; }

    [JsonPropertyName("gastOt2stand")]
    public int? GastOt2stand { get; set; }
}