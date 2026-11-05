using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Client_App.Models;

public class BusinessLogic
{
    private IDatabase Database { get; set; }

    public ObservableCollection<Drill> Drills
    {
        get { return Database.Drills; }
    }

    public ObservableCollection<StepItem> Steps { get; } = new();

    public ObservableCollection<ActivityItem> Activities
    {
        get { return Database.Activities; }
    }

    public BusinessLogic()
    {
        Database = new Database();
        InitializeDrillsAsync();
    }

    public async void InitializeDrillsAsync()
    {
        await Database.SelectAllDrills();
    }

    public async Task<DrillError> AddDrill(ActivityItem activity, String name, String description,  String image, String video, List<string> steps)
    {
        DrillError fieldsValid = FieldsValid(name, description, image, video, steps);
        if (fieldsValid == DrillError.NoError)
        {
            return await Database.InsertDrill(new Drill(activity.Id ,activity.Name, name,description,image,video,steps));
        }
        else
        {
            return fieldsValid;
        }
    }

    private DrillError FieldsValid(string name, string description, string image, string video, List<string> steps)
    {
        return DrillError.NoError;
    }

    // Rishi: Activities logic 
    public Task<IReadOnlyList<ActivityCategory>> GetActivityCategoriesAsync(string schoolLevel)
    {
        if (schoolLevel != "elementary" && schoolLevel != "middle" && schoolLevel != "high")
        {
            return Task.FromResult<IReadOnlyList<ActivityCategory>>([]);
        }

        return Database.GetActivityCategoriesAsync(schoolLevel);
    }

    public Task<IReadOnlyList<ActivityItem>> GetActivitiesByCategoryAsync(long categoryId)
    {
        if (categoryId <= 0)
        {
            return Task.FromResult<IReadOnlyList<ActivityItem>>([]);
        }

        return Database.GetActivitiesByCategoryAsync(categoryId);
    }

    public async Task<ActivityError> AddActivityAsync(long categoryId, string? name)
    {
        if (categoryId <= 0)
        {
            return ActivityError.InvalidCategory;
        }

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
        {
            return ActivityError.InvalidActivityName;
        }

        string cleanName = name.Trim();
        var activities = await Database.GetActivitiesByCategoryAsync(categoryId);
        if (activities.Any(a => string.Equals(a.Name, cleanName, StringComparison.OrdinalIgnoreCase)))
        {
            return ActivityError.DuplicateActivity;
        }

        return await Database.InsertActivityAsync(categoryId, cleanName);
    }

    public Task<ActivityError> RemoveActivityAsync(long activityId)
    {
        if (activityId <= 0)
        {
            return Task.FromResult(ActivityError.InvalidActivityId);
        }

        return Database.DeleteActivityByIdAsync(activityId);
    }

}