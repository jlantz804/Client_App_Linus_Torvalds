using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Client_App.Models;

[Table("users")]
public sealed class UserProfile : BaseModel
{
    [PrimaryKey("uid", false)]
    public string Uid { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("is_admin")]
    public bool IsAdmin { get; set; }
}
