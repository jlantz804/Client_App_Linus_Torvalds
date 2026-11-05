using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Client_App.Models;

// Maps lesson-plan properties to the Supabase table and columns.
[Table("lesson_plans")]
public sealed class LessonPlan : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id
    {
        get; set;
    }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("school_level")]
    public string SchoolLevel { get; set; } = string.Empty;

    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Column("activity")]
    public string Activity { get; set; } = string.Empty;

    [Column("grade_level")]
    public string GradeLevel { get; set; } = string.Empty;

    [Column("scheduled_date")]
    public DateTime ScheduledDate
    {
        get; set;
    }

    [Column("duration_minutes")]
    public int DurationMinutes
    {
        get; set;
    }

    [Column("objectives")]
    public string Objectives { get; set; } = string.Empty;
}
