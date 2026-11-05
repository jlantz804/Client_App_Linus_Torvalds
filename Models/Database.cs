using System.Collections.ObjectModel;
using System.Diagnostics;
namespace Client_App.Models;

public partial class Database : IDatabase
{
    private static string url = "https://dsczwqvcbapcnparudmg.supabase.co";
    private static string key = "sb_publishable_pyIuG5PYJ7M-wle-Al6pWg_Zk2UzhNn";
    Supabase.Client client;
    public ObservableCollection<Drill> Drills { get; private set; } = new();
    public ObservableCollection<ActivityItem> Activities { get; private set; } = new();

    public Database()
    {
        client = new Supabase.Client(url,key);
    }

    public async Task SelectAllDrills()
    {
        try
        {
            await client.InitializeAsync();

            if (client.Auth.CurrentSession is null)
            {
                await client.Auth.SignInAnonymously();
            }

            var activities = (await client.From<ActivityItem>().Get()).Models;
            var drills = (await client.From<Drill>().Get()).Models;

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
            var response = await client.From<Drill>().Insert(drillToInsert);
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

    public Task<DrillError> UpdateDrill(Drill currentDrill, string drillName)
    {
        throw new NotImplementedException();
    }

    public Task<DrillError> DeleteDrill(Drill drillToDelete)
    {
        throw new NotImplementedException();
    }
}