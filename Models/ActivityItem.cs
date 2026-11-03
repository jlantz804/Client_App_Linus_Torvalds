
using System.Text.Json.Serialization;

namespace Client_App.Models;

public sealed class ActivityItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("category_id")]
    public long CategoryId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
