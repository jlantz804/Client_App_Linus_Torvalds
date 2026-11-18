using System.Collections.ObjectModel;

namespace Client_App.Models;

public interface IDatabase
{
    public ObservableCollection<Drill> Drills { get; }
    ObservableCollection<ActivityItem> Activities { get; }
    public Task SelectAllDrills();
    public Task<DrillError> InsertDrill(Drill drillToInsert);
    Task<DrillError> UpdateDrill(Drill drillToEdit, long activityId, string activityName, string drillName, string description, string image, string video, List<string> steps, bool locked);
    public Task<DrillError> DeleteDrill(Drill drillToDelete);
    public Task<EquipmentModificationError> InsertEquipmentModification(EquipmentModification modification);
    public Task<ObservableCollection<EquipmentModification>> GetEquipmentModificationsAsync();
    public Task<EquipmentModificationError> DeleteEquipmentModificationsAsync(EquipmentModification modification);
    public Task<EquipmentModificationError> UpdateEquipmentModificationAsync(EquipmentModification modification);
    Task<IReadOnlyList<LessonPlan>> GetLessonPlansAsync();
    Task AddLessonPlanAsync(LessonPlan plan);
    Task UpdateLessonPlanAsync(LessonPlan plan);
    Task DeleteLessonPlanAsync(LessonPlan plan);

    // Activities: the contract for the Supabase activity queries.
    Task<IReadOnlyList<ActivityCategory>> GetActivityCategoriesAsync(string schoolLevel);
    Task<IReadOnlyList<ActivityItem>> GetActivitiesByCategoryAsync(long categoryId);
    Task<ActivityError> InsertActivityAsync(long categoryId, string name);
    Task<ActivityError> DeleteActivityByIdAsync(long activityId);
}