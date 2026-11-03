using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Client_App.Models;

namespace Client_App;

// Rishi: Keep Activities database code separate from Joey's Lesson Plans code.
public sealed class ActivitiesService
{
    private static readonly HttpClient Http = new();
    private readonly SupabaseService _supabase;

    public ActivitiesService(SupabaseService supabase)
    {
        _supabase = supabase;
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string route)
    {
        var token = await _supabase.GetActivitiesAccessTokenAsync();
        var request = new HttpRequestMessage(method,
            $"{SupabaseSettings.ProjectUrl.TrimEnd('/')}/rest/v1/{route}");
        request.Headers.TryAddWithoutValidation("apikey", SupabaseSettings.PublishableKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public async Task<List<ActivityCategory>> GetCategoriesAsync(string schoolLevel)
    {
        string code = Uri.EscapeDataString(schoolLevel);
        using var request = await CreateRequestAsync(HttpMethod.Get,
            $"activity_categories?select=id,school_level,name&school_level=eq.{code}&order=id.asc");
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<List<ActivityCategory>>(await ReadResponseAsync(response)) ?? [];
    }

    public async Task<List<ActivityItem>> GetActivitiesAsync(long categoryId)
    {
        using var request = await CreateRequestAsync(HttpMethod.Get,
            $"activities?select=id,category_id,name,description&category_id=eq.{categoryId}&order=id.asc");
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<List<ActivityItem>>(await ReadResponseAsync(response)) ?? [];
    }

    public async Task AddActivityAsync(long categoryId, string name)
    {
        using var request = await CreateRequestAsync(HttpMethod.Post, "activities");
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { category_id = categoryId, name }),
            Encoding.UTF8, "application/json");
        request.Headers.TryAddWithoutValidation("Prefer", "return=minimal");
        using var response = await Http.SendAsync(request);
        await ReadResponseAsync(response);
    }

    public async Task DeleteActivityAsync(long id)
    {
        // Always delete by the activity's exact ID.
        using var request = await CreateRequestAsync(HttpMethod.Delete,
            $"activities?id=eq.{id}&select=id");
        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");
        using var response = await Http.SendAsync(request);
        string json = await ReadResponseAsync(response);
        var deleted = JsonSerializer.Deserialize<List<ActivityItem>>(json) ?? [];
        if (deleted.Count == 0)
            throw new InvalidOperationException("No activity was deleted. Check the Supabase RLS policies.");
    }

    private static async Task<string> ReadResponseAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode) return body;
        string detail = body;
        try
        {
            using var data = JsonDocument.Parse(body);
            var root = data.RootElement;
            foreach (string field in new[] { "message", "msg", "error_description", "details", "error" })
            {
                if (root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String)
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
