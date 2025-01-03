using System.Text.Json.Serialization;

namespace DbbInstaGenerator.ApiClasses;

public class ApiResponse<T>
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public T Data { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("dateFormat")]
    public string DateFormat { get; set; }

    [JsonPropertyName("timeFormat")]
    public string TimeFormat { get; set; }

    [JsonPropertyName("timeFormatShort")]
    public string TimeFormatShort { get; set; }

    [JsonPropertyName("serverInstance")]
    public string ServerInstance { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("appContext")]
    public string AppContext { get; set; }
}