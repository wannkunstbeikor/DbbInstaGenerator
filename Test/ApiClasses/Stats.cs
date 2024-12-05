using System.Text.Json.Serialization;

namespace Test.ApiClasses;

public class Stats
{
    [JsonPropertyName("esz")]
    public double Esz { get; set; }

    [JsonPropertyName("pts")]
    public double Pts { get; set; }

    [JsonPropertyName("twoPoints")]
    public Stat TwoPoints { get; set; }

    [JsonPropertyName("threePoints")]
    public Stat ThreePoints { get; set; }

    [JsonPropertyName("wt")]
    public Stat Wt { get; set; }

    [JsonPropertyName("onePoints")]
    public Stat OnePoints { get; set; }

    [JsonPropertyName("ro")]
    public double Ro { get; set; }

    [JsonPropertyName("rd")]
    public double Rd { get; set; }

    [JsonPropertyName("rt")]
    public double Rt { get; set; }

    [JsonPropertyName("as")]
    public double As { get; set; }

    [JsonPropertyName("st")]
    public double St { get; set; }

    [JsonPropertyName("to")]
    public double To { get; set; }

    [JsonPropertyName("bs")]
    public double Bs { get; set; }

    [JsonPropertyName("fouls")]
    public double Fouls { get; set; }

    [JsonPropertyName("eff")]
    public double Eff { get; set; }

    [JsonPropertyName("player")]
    public Player? Player { get; set; }

    [JsonPropertyName("isf")]
    public object Isf { get; set; }
}