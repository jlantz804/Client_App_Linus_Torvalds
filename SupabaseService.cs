using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Client_App;

public static class SupabaseSettings
{
    public const string ProjectUrl = "https://dsczwqvcbapcnparudmg.supabase.co";
    public const string PublishableKey = "sb_publishable_pyIuG5PYJ7M-wle-Al6pWg_Zk2UzhNn";
}

public sealed class SupabaseService
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private Supabase.Client? _client;

    public bool IsAuthenticated => _client?.Auth.CurrentSession is not null;

    public async Task InitializeAsync()
    {
        if (_client is not null)
            return;

        await _initializationLock.WaitAsync();
        try
        {
            if (_client is not null)
                return;

            if (SupabaseSettings.ProjectUrl.Contains("YOUR_PROJECT_REF", StringComparison.Ordinal) ||
                SupabaseSettings.PublishableKey.Contains("YOUR_SUPABASE_PUBLISHABLE_KEY", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Add your Supabase project URL and publishable key in SupabaseSettings.cs.");
            }

            var client = new Supabase.Client(
                SupabaseSettings.ProjectUrl,
                SupabaseSettings.PublishableKey,
                new Supabase.SupabaseOptions
                {
                    AutoRefreshToken = true
                });

            await client.InitializeAsync();
            _client = client;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<IReadOnlyList<LessonPlan>> GetLessonPlansAsync()
    {
        await InitializeAsync();
        const int pageSize = 1000;
        var plans = new List<LessonPlan>();

        for (var offset = 0; ; offset += pageSize)
        {
            var response = await _client!.From<LessonPlan>()
                .Range(offset, offset + pageSize - 1)
                .Get();

            plans.AddRange(response.Models);
            if (response.Models.Count < pageSize)
                break;
        }

        return plans;
    }

    public async Task SignInAsync(string email, string password)
    {
        await InitializeAsync();
        await _client!.Auth.SignIn(email, password);
    }

    public async Task SignOutAsync()
    {
        if (_client is not null && IsAuthenticated)
            await _client.Auth.SignOut();
    }

    public async Task AddLessonPlanAsync(LessonPlan plan)
    {
        EnsureSignedIn();
        await _client!.From<LessonPlan>().Insert(plan);
    }

    public async Task UpdateLessonPlanAsync(LessonPlan plan)
    {
        EnsureSignedIn();
        await _client!.From<LessonPlan>().Update(plan);
    }

    public async Task DeleteLessonPlanAsync(LessonPlan plan)
    {
        EnsureSignedIn();
        await _client!.From<LessonPlan>().Delete(plan);
    }

    private void EnsureSignedIn()
    {
        if (!IsAuthenticated)
            throw new InvalidOperationException("Sign in with a teacher account before changing lesson plans.");
    }
}

[Table("lesson_plans")]
public sealed class LessonPlan : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

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
    public DateTime ScheduledDate { get; set; }

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; }

    [Column("objectives")]
    public string Objectives { get; set; } = string.Empty;
}
