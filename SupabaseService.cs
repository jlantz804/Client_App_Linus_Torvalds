using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Client_App.Models;
using System.Net.Http.Json;

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
    private HttpClient _equipmentHttp = new HttpClient();
    public async Task InitializeAsync()
    {
        if (_client is not null)
        {
            return;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_client is not null)
            {
                return;
            }

            var client = new Supabase.Client(
                SupabaseSettings.ProjectUrl,
                SupabaseSettings.PublishableKey);

            await client.InitializeAsync();
            _client = client;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private readonly SemaphoreSlim _activitiesAuthLock = new(1, 1);

    // Rishi: Sign in anonymously for Activities while the teacher login is not ready.
    // This uses the authenticated RLS role, without needing email or password.
    public async Task<string> GetActivitiesAccessTokenAsync()
    {
        await InitializeAsync();
        await _activitiesAuthLock.WaitAsync();
        try
        {
            var session = _client!.Auth.CurrentSession;
            if (session is null)
            {
                session = await _client.Auth.SignInAnonymously();
            }

            return session?.AccessToken ?? throw new InvalidOperationException(
                "Could not start an Activities session. Enable Anonymous Sign-Ins in Supabase Authentication settings.");
        }
        finally
        {
            _activitiesAuthLock.Release();
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
            {
                break;
            }
        }

        return plans;
    }

    public async Task AddLessonPlanAsync(LessonPlan plan)
    {
        await InitializeAsync();
        await _client!.From<LessonPlan>().Insert(plan);
    }

    public async Task UpdateLessonPlanAsync(LessonPlan plan)
    {
        await InitializeAsync();
        await _client!.From<LessonPlan>().Update(plan);
    }

    public async Task DeleteLessonPlanAsync(LessonPlan plan)
    {
        await InitializeAsync();
        await _client!.From<LessonPlan>().Delete(plan);
    }

    //Methods for EquipmentModifications - ALEX
    public async Task<List<ActivityItem>?> GetEquipmentActivitiesAsync()
    {
        string token = await GetActivitiesAccessTokenAsync();

        string url = SupabaseSettings.ProjectUrl
            + "/rest/v1/activities?select=id,name&order=name.asc";

        using HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("apikey", SupabaseSettings.PublishableKey);
        request.Headers.Add("Authorization", "Bearer " + token);

        using HttpResponseMessage response =
            await _equipmentHttp.SendAsync(request);

        return await response.Content
            .ReadFromJsonAsync<List<ActivityItem>>();
    }

    public async Task AddEquipmentModificationAsync(
        EquipmentModification modification)
    {
        string token = await GetActivitiesAccessTokenAsync();

        string url = SupabaseSettings.ProjectUrl
            + "/rest/v1/equipment_modifications";

        using HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("apikey", SupabaseSettings.PublishableKey);
        request.Headers.Add("Authorization", "Bearer " + token);
        request.Headers.Add("Prefer", "return=minimal");

        request.Content = JsonContent.Create(modification);

        using HttpResponseMessage response =
            await _equipmentHttp.SendAsync(request);
    }
}

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