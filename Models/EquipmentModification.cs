using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Client_App.Models;

[Table("equipment_modifications")]
public sealed class EquipmentModification : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("activity_id")]
    public long ActivityId { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("image")]
    public string Image { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("steps")]
    public string Steps { get; set; } = "";
}