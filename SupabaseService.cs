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

            if (SupabaseSettings.ProjectUrl.Contains("YOUR_PROJECT_REF", StringComparison.Ordinal) ||
                SupabaseSettings.PublishableKey.Contains("YOUR_SUPABASE_PUBLISHABLE_KEY", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Add your Supabase project URL and publishable key in SupabaseSettings.cs.");
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

    // Methods for DrillsPage - GAVIN
    // 
    private async Task<HttpResponseMessage> SendRestAsync(HttpMethod method, string path, object? body = null)
    {
        string token = await GetActivitiesAccessTokenAsync();

        using var request = new HttpRequestMessage(method, SupabaseSettings.ProjectUrl + "/rest/v1/" + path);
        request.Headers.Add("apikey", SupabaseSettings.PublishableKey);
        request.Headers.Add("Authorization", "Bearer " + token);
        if (method != HttpMethod.Get)
        {
            request.Headers.Add("Prefer", "return=minimal");
        }
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await _equipmentHttp.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            response.Dispose();
            throw new HttpRequestException($"Supabase error {status}: {error}");
        }
        return response;
    }

    // Needs category_id too, so drills can show their category
    public async Task<List<ActivityItem>> GetAllActivitiesAsync()
    {
        using var response = await SendRestAsync(HttpMethod.Get,
            "activities?select=id,category_id,name&order=name.asc");
        return await response.Content.ReadFromJsonAsync<List<ActivityItem>>() ?? [];
    }

    public async Task<List<Drill>> GetDrillsAsync()
    {
        using var response = await SendRestAsync(HttpMethod.Get, "drills?select=*&order=drillid.asc");
        var drills = await response.Content.ReadFromJsonAsync<List<Drill>>() ?? [];

        var activities = (await GetAllActivitiesAsync()).ToDictionary(a => a.Id);
        foreach (var drill in drills)
        {
            if (drill.ActivityId is long id && activities.TryGetValue(id, out var activity))
            {
                drill.ActivityName = activity.Name;
                drill.CategoryId = activity.CategoryId;
            }
            else
            {
                drill.ActivityName = "No activity";
            }
        }
        return drills;
    }

    public async Task AddDrillAsync(Drill drill)
    {
        // DrillId is 0, so JsonIgnore(WhenWritingDefault) leaves it out and Postgres generates it
        using var response = await SendRestAsync(HttpMethod.Post, "drills", drill);
    }

    public async Task UpdateDrillAsync(Drill drill)
    {
        // Anonymous object so drillid is never in the body (an ALWAYS identity column rejects it)
        var body = new
        {
            activity_id = drill.ActivityId,
            name = drill.Name,
            description = drill.Description,
            image_url = drill.ImageUrl,
            video_url = drill.VideoUrl,
            steps = drill.Steps
        };
        using var response = await SendRestAsync(HttpMethod.Patch, $"drills?drillid=eq.{drill.DrillId}", body);
    }

    public async Task DeleteDrillAsync(long drillId)
    {
        using var response = await SendRestAsync(HttpMethod.Delete, $"drills?drillid=eq.{drillId}");
    }

    public async Task DeleteAllDrillsAsync()
    {
        // PostgREST refuses a DELETE with no filter, so match every real id
        using var response = await SendRestAsync(HttpMethod.Delete, "drills?drillid=gt.0");
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