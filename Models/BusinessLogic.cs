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

    public async Task<DrillError> AddDrill(ActivityItem activity, String name, String description,  String image, String video, List<string> steps, Boolean locked, String user)
    {
        DrillError fieldsValid = FieldsValid(name, description, image, video, steps);
        if (fieldsValid == DrillError.NoError)
        {
            return await Database.InsertDrill(new Drill(activity.Id ,activity.Name, name, description, image, video, steps, locked, user));
        }
        else
        {
            return fieldsValid;
        }
    }

    public async Task<DrillError> EditDrill(Drill drillToEdit, ActivityItem activity, String name, String description,  String image, String video, List<string> steps, Boolean locked, String currentUser)
    {
        // Change once login is working
        if (drillToEdit.Locked && currentUser != drillToEdit.CreatedBy || drillToEdit.Locked != locked && currentUser != drillToEdit.CreatedBy)
        {
            return DrillError.PermissionDenied;
        }
        DrillError fieldsValid = FieldsValid(name, description, image, video, steps);
        if (fieldsValid == DrillError.NoError)
        {
            return await Database.UpdateDrill(drillToEdit, activity.Id, activity.Name, 
                name, description, image, video, steps, locked);
        }
        else
        {
            return fieldsValid;
        }
    }

    public async Task<DrillError> DeleteDrill(Drill drillToDelete, String currentUser)
    {
        // Change once login is working
        if (drillToDelete.Locked && currentUser != drillToDelete.CreatedBy)
        {
            return DrillError.PermissionDenied;
        }
        return await Database.DeleteDrill(drillToDelete);
    }

    private DrillError FieldsValid(string name, string description, string image, string video, List<string> steps)
    {
        if (name.Length == 0)
        {
            return DrillError.NameTooShort;
        }
        else if (description.Length == 0)
        {
            return DrillError.DescriptionTooShort;
        }
        else if (steps.Count == 0)
        {
            return DrillError.StepsEmpty;
        }
        else
        {
            foreach(string step in steps)
            {
                if (step == "")
                {
                    return DrillError.BlankStep;
                }
            }
        }
        return DrillError.NoError;
    }

    // Alex Equipment modification methods
    public async Task<EquipmentModificationError> AddEquipmentModification(ActivityItem activity,
    string name, string description, string image, string steps)
    {
        EquipmentModification modification = new EquipmentModification();

        modification.ActivityId = activity.Id;
        modification.Name = name;
        modification.Description = description;
        modification.Image = image;
        modification.Steps = steps;

        return await Database.InsertEquipmentModification(modification);
    }

    public Task<ObservableCollection<EquipmentModification>> GetEquipmentModificationsAsync()
    {
        return Database.GetEquipmentModificationsAsync();
    }

    public Task<EquipmentModificationError> DeleteEquipmentModificationsAsync(EquipmentModification modification)
    {
        return Database.DeleteEquipmentModificationsAsync(modification);
    }

    public Task<EquipmentModificationError> UpdateEquipmentModificationAsync(long id, ActivityItem activity,
    string name, string description, string image, string steps)
    {
        EquipmentModification modification = new EquipmentModification();

        modification.Id = id;
        modification.ActivityId = activity.Id;
        modification.Name = name;
        modification.Description = description;
        modification.Image = image;
        modification.Steps = steps;

        return Database.UpdateEquipmentModificationAsync(modification);
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