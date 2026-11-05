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
    // DrillsPage Methods - Gavin
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

    public async Task<List<ActivityItem>> GetAllActivitiesAsync()
    {
        using var response = await SendRestAsync(HttpMethod.Get,
            "activities?select=id,category_id,name&order=name.asc");
        return await response.Content.ReadFromJsonAsync<List<ActivityItem>>() ?? [];
    }


    public async Task AddDrillAsync(Drill drill)
    {
        using var response = await SendRestAsync(HttpMethod.Post, "drills", drill);
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


    // Gavin: Drills create, update, and delete operations.
    public async Task AddDrillAsync(Drill drill)
    {
        // The database generates drillid.
        using var response = await SendRestAsync(HttpMethod.Post, "drills", new
        {
            activity_id = drill.ActivityId,
            name = drill.Name,
            description = drill.Description,
            image_url = drill.ImageUrl,
            video_url = drill.VideoUrl,
            steps = drill.Steps
        });
    }

    public async Task UpdateDrillAsync(Drill drill)
    {
        using var response = await SendRestAsync(HttpMethod.Patch,
            $"drills?drillid=eq.{drill.DrillId}", new
            {
                activity_id = drill.ActivityId,
                name = drill.Name,
                description = drill.Description,
                image_url = drill.ImageUrl,
                video_url = drill.VideoUrl,
                steps = drill.Steps
            });
    }

    public async Task DeleteDrillAsync(long drillId)
    {
        using var response = await SendRestAsync(HttpMethod.Delete,
            $"drills?drillid=eq.{drillId}");
    }

}
