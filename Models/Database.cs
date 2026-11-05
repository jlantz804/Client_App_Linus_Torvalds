using System.Collections.ObjectModel;

namespace Client_App.Models;

public partial class Database : IDatabase
{
    private static readonly string _url = "https://dsczwqvcbapcnparudmg.supabase.co";
    private static readonly string _key = "sb_publishable_pyIuG5PYJ7M-wle-Al6pWg_Zk2UzhNn";
    private readonly Supabase.Client _client;
    public ObservableCollection<Drill> Drills { get; private set; } = new();
    public ObservableCollection<ActivityItem> Activities { get; private set; } = new();

    public Database()
    {
        _client = new Supabase.Client(_url,_key);
    }

    private async Task InitializeClientAsync()
    {
        // Use an anonymous session when needed so Supabase row-level security can authorize requests.
        await _client.InitializeAsync();

        if (_client.Auth.CurrentSession is null)
        {
            await _client.Auth.SignInAnonymously();
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
}