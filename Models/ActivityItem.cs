using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace Client_App.Models;

[Table("activities")]
public sealed class ActivityItem : BaseModel
{
    [JsonPropertyName("id")]
    public long Id
    {
        get; set;
    }

    [JsonPropertyName("category_id")]
    public long CategoryId
    {
        get; set;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description
    {
        get; set;
    }
}
