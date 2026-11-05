using System.Collections.ObjectModel;

namespace Client_App.Models;

public interface IDatabase
{
    public ObservableCollection<Drill> Drills { get; }
    ObservableCollection<ActivityItem> Activities { get; }
    public Task SelectAllDrills();
    public Task<DrillError> InsertDrill(Drill drillToInsert);
    public Task<DrillError> UpdateDrill(Drill currentDrill, String drillName);
    public Task<DrillError> DeleteDrill(Drill drillToDelete);
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