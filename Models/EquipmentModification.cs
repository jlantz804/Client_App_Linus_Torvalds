using System.Text.Json.Serialization;

namespace Client_App.Models;

public sealed class EquipmentModification
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Id { get; set; }

    [JsonPropertyName("activity_id")]
    public long ActivityId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public string Steps { get; set; } = string.Empty;
}