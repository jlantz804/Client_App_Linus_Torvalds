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

    //Equipment modification methods
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
}