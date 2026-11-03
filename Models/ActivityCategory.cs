
using System.Text.Json.Serialization;

namespace Client_App.Models;

public sealed class ActivityCategory
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("school_level")]
    public string SchoolLevel { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
