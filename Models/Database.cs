using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Client_App.Models;

public partial class Database : IDatabase
{
    private static readonly string _url = "https://dsczwqvcbapcnparudmg.supabase.co";
    private static readonly string _key = "sb_publishable_pyIuG5PYJ7M-wle-Al6pWg_Zk2UzhNn";
    private readonly Supabase.Client _client;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;
    public ObservableCollection<Drill> Drills { get; private set; } = new();
    public ObservableCollection<ActivityItem> Activities { get; private set; } = new();

    public Database()
    {
        _client = new Supabase.Client(_url,_key);
    }

    private async Task InitializeClientAsync()
    {
        // Rishi: One shared Supabase session for Activities, Drills, and Lesson Plans.
        await _initializationLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                await _client.InitializeAsync();
                _initialized = true;
            }

            if (_client.Auth.CurrentSession is null)
            {
                await _client.Auth.SignInAnonymously();
            }
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task SelectAllDrills()
    {
        try
        {
            await InitializeClientAsync();

            var activities = (await _client.From<ActivityItem>().Get()).Models;
            var drills = (await _client.From<Drill>().Get()).Models;

            var names = activities.ToDictionary(a => a.Id, a => a.Name);

            Activities.Clear();
            foreach (var a in activities) {
                Activities.Add(a);
            }

            Drills.Clear();
            foreach (var drill in drills)
            {
                drill.ActivityName = names.GetValueOrDefault(drill.ActivityId ?? -1, "");
                Drills.Add(drill);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching drills: {ex.Message}");
        }
    }

    public async Task<DrillError> InsertDrill(Drill drillToInsert)
    {
        try
        {
            var response = await _client.From<Drill>().Insert(drillToInsert);
            if (response.Models.Any())
            {
                var addedDrill = response.Models.First();
                addedDrill.ActivityName = drillToInsert.ActivityName;
                Drills.Add(addedDrill);
            }
            else
            {
                return DrillError.DuplicateDrill;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Insert failed: {ex}");
            return ex.Message.Contains("already exists") || ex.Message.Contains("duplicate key") ? DrillError.DuplicateDrill : DrillError.DBAdditionError;
        }
        return DrillError.NoError;
    }

    public async Task<IReadOnlyList<LessonPlan>> GetLessonPlansAsync()
    {
        await InitializeClientAsync();
        // Fetch in bounded batches to stay within Supabase/PostgREST response limits.
        const int pageSize = 1000;
        var plans = new List<LessonPlan>();

        for (var offset = 0; ; offset += pageSize)
        {
            var response = await _client.From<LessonPlan>()
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
        await InitializeClientAsync();
        await _client.From<LessonPlan>().Insert(plan);
    }

    public async Task UpdateLessonPlanAsync(LessonPlan plan)
    {
        await InitializeClientAsync();
        await _client.From<LessonPlan>().Update(plan);
    }

    public async Task DeleteLessonPlanAsync(LessonPlan plan)
    {
        await InitializeClientAsync();
        await _client.From<LessonPlan>().Delete(plan);
    }

    public Task<DrillError> UpdateDrill(Drill currentDrill, string drillName)
    {
        throw new NotImplementedException();
    }

    public Task<DrillError> DeleteDrill(Drill drillToDelete)
    {
        throw new NotImplementedException();
    }

    //Alex Method for EquipmentModification
    public async Task<EquipmentModificationError> InsertEquipmentModification(EquipmentModification modification)
    {
        try
        {
            await InitializeClientAsync();
            await _client.From<EquipmentModification>().Insert(modification);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Insert failed: {ex}");
            if (ex.Message.Contains("already exists") || ex.Message.Contains("duplicate key"))
            {
                return EquipmentModificationError.DuplicateEquipmentModiifcation;
            }

            return EquipmentModificationError.DBAdditionError;
        }
        return EquipmentModificationError.NoError;
    }
    // Rishi: Supabase activities queries. Other database methods are unchanged.
    private static readonly HttpClient ActivityHttp = new();

    private async Task<HttpRequestMessage> CreateActivityRequestAsync(HttpMethod method, string route)
    {
        await InitializeClientAsync();
        string token = _client.Auth.CurrentSession?.AccessToken
            ?? throw new InvalidOperationException("Could not start a Supabase session. Enable Anonymous Sign-Ins in Supabase Authentication settings.");

        var request = new HttpRequestMessage(method,
            $"{SupabaseSettings.ProjectUrl.TrimEnd('/')}/rest/v1/{route}");
        request.Headers.TryAddWithoutValidation("apikey", SupabaseSettings.PublishableKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public async Task<IReadOnlyList<ActivityCategory>> GetActivityCategoriesAsync(string schoolLevel)
    {
        string code = Uri.EscapeDataString(schoolLevel);
        using var request = await CreateActivityRequestAsync(HttpMethod.Get,
            $"activity_categories?select=id,school_level,name&school_level=eq.{code}&order=id.asc");
        using var response = await ActivityHttp.SendAsync(request);
        string json = await ReadActivityResponseAsync(response);
        return JsonSerializer.Deserialize<List<ActivityCategory>>(json) ?? [];
    }

    public async Task<IReadOnlyList<ActivityItem>> GetActivitiesByCategoryAsync(long categoryId)
    {
        using var request = await CreateActivityRequestAsync(HttpMethod.Get,
            $"activities?select=id,category_id,name,description&category_id=eq.{categoryId}&order=id.asc");
        using var response = await ActivityHttp.SendAsync(request);
        string json = await ReadActivityResponseAsync(response);
        return JsonSerializer.Deserialize<List<ActivityItem>>(json) ?? [];
    }

    public async Task<ActivityError> InsertActivityAsync(long categoryId, string name)
    {
        try
        {
            using var request = await CreateActivityRequestAsync(HttpMethod.Post, "activities");
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { category_id = categoryId, name }),
                Encoding.UTF8, "application/json");
            request.Headers.TryAddWithoutValidation("Prefer", "return=minimal");

            using var response = await ActivityHttp.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return ActivityError.NoError;
            }

            string error = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Add activity failed ({(int)response.StatusCode}): {error}");
            if (response.StatusCode == HttpStatusCode.Conflict || error.Contains("23505", StringComparison.OrdinalIgnoreCase))
            {
                return ActivityError.DuplicateActivity;
            }
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                return ActivityError.PermissionDenied;
            }
            if (error.Contains("23503", StringComparison.OrdinalIgnoreCase))
            {
                return ActivityError.InvalidCategory;
            }
            return ActivityError.DBAdditionError;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Add activity failed: {ex}");
            return ActivityError.DBAdditionError;
        }
    }

    public async Task<ActivityError> DeleteActivityByIdAsync(long activityId)
    {
        try
        {
            // Delete by ID, not name, so only the chosen activity gets removed.
            using var request = await CreateActivityRequestAsync(HttpMethod.Delete,
                $"activities?id=eq.{activityId}&select=id");
            request.Headers.TryAddWithoutValidation("Prefer", "return=representation");
            using var response = await ActivityHttp.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Delete activity failed ({(int)response.StatusCode}): {error}");
                return response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized
                    ? ActivityError.PermissionDenied : ActivityError.DBDeletionError;
            }

            string json = await response.Content.ReadAsStringAsync();
            var deleted = JsonSerializer.Deserialize<List<ActivityItem>>(json) ?? [];
            return deleted.Count > 0 ? ActivityError.NoError : ActivityError.ActivityNotFound;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delete activity failed: {ex}");
            return ActivityError.DBDeletionError;
        }
    }

    private static async Task<string> ReadActivityResponseAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return body;
        }

        string detail = body;
        try
        {
            using var json = JsonDocument.Parse(body);
            foreach (string field in new[] { "message", "msg", "error_description", "details", "error" })
            {
                if (json.RootElement.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    detail = value.GetString() ?? body;
                    break;
                }
            }
        }
        catch (JsonException) { }

        throw new InvalidOperationException($"Supabase error {(int)response.StatusCode}: {detail}");
    }

}