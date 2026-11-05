using System.Collections.ObjectModel;
using Client_App;

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
}