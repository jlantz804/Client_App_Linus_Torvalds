namespace Client_App.Models;

public class Lessons
{
    private static readonly string[] _schoolLevels = ["Elementary", "Middle School", "High School"];

    private static readonly Dictionary<string, string> _elementaryCategories = new(StringComparer.Ordinal)
    {
        ["Overhand throwing"] = "Object control skills",
        ["Underhand tossing"] = "Object control skills",
        ["Kicking"] = "Object control skills",
        ["Striking"] = "Object control skills",
        ["Catching"] = "Object control skills",
        ["Run"] = "Locomotor movements",
        ["Walk"] = "Locomotor movements",
        ["Gallop"] = "Locomotor movements",
        ["Hop"] = "Locomotor movements",
        ["Leap"] = "Locomotor movements",
        ["Side side"] = "Locomotor movements",
        ["Skip"] = "Locomotor movements",
        ["Jump"] = "Locomotor movements"
    };

    private static readonly string[] _middleHighActivities =
    [
        "Soccer",
        "Football",
        "Basketball",
        "Gymnastics",
        "Swimming",
        "Baseball/softball",
        "Volleyball",
        "Badminton",
        "Pickleball",
        "Team activities",
        "Tchoukball",
        "Weightlifting",
        "Yoga"
    ];

    private readonly IDatabase _database;
    private readonly List<LessonPlan> _allPlans = [];

    public IReadOnlyList<string> SchoolLevels => _schoolLevels;
    public string ConnectionStatus { get; private set; } = "Connecting to Supabase…";

    public Lessons(IDatabase database)
    {
        _database = database;
    }

    public string[] GetActivities(string? schoolLevel)
    {
        return schoolLevel switch
        {
            "Elementary" => _elementaryCategories.Keys.ToArray(),
            "Middle School" or "High School" => _middleHighActivities,
            _ => Array.Empty<string>()
        };
    }

    public string? GetCategory(string? activity)
    {
        return activity is not null && _elementaryCategories.TryGetValue(activity, out var category)
            ? category
            : null;
    }

    public bool IsCategoryReadOnly(string? schoolLevel)
    {
        return schoolLevel == "Elementary";
    }

    public async Task LoadPlansAsync()
    {
        try
        {
            var plans = await _database.GetLessonPlansAsync();
            _allPlans.Clear();
            _allPlans.AddRange(plans);
            ConnectionStatus = $"{_allPlans.Count} lesson plans available";
        }
        catch (Exception exception)
        {
            // Keep the page usable and surface fetch failures through the connection-status label.
            _allPlans.Clear();
            ConnectionStatus = $"Supabase: {exception.Message}";
        }
    }

    public IReadOnlyList<LessonPlan> SearchPlans(string? searchText)
    {
        var query = searchText?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            return _allPlans;
        }

        return _allPlans.Where(plan =>
            plan.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            plan.SchoolLevel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            plan.Category.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            plan.Activity.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            plan.GradeLevel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            plan.Objectives.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task DeletePlanAsync(LessonPlan plan)
    {
        await _database.DeleteLessonPlanAsync(plan);
    }

    public async Task<string?> SavePlanAsync(
        LessonPlan? editingPlan,
        string? title,
        string? schoolLevel,
        string? category,
        string? activity,
        string? grade,
        DateTime? scheduledDate,
        string? durationText,
        string? objectives)
    {
        title = title?.Trim() ?? string.Empty;
        schoolLevel = schoolLevel?.Trim() ?? string.Empty;
        category = category?.Trim() ?? string.Empty;
        activity = activity?.Trim() ?? string.Empty;
        grade = grade?.Trim() ?? string.Empty;
        objectives = objectives?.Trim() ?? string.Empty;

        // Return a validation message for the form; null means the input is ready to save.
        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(schoolLevel) ||
            string.IsNullOrWhiteSpace(category) ||
            string.IsNullOrWhiteSpace(activity) ||
            string.IsNullOrWhiteSpace(grade) ||
            string.IsNullOrWhiteSpace(objectives))
        {
            return "Complete the title, school level, category, activity, grade, and objectives.";
        }

        if (!int.TryParse(durationText, out var duration) || duration <= 0)
        {
            return "Enter a duration greater than zero minutes.";
        }

        var plan = new LessonPlan
        {
            Title = title,
            SchoolLevel = schoolLevel,
            Category = category,
            Activity = activity,
            GradeLevel = grade,
            ScheduledDate = scheduledDate ?? DateTime.Today,
            DurationMinutes = duration,
            Objectives = objectives
        };

        if (editingPlan is null)
        {
            await _database.AddLessonPlanAsync(plan);
        }
        else
        {
            // Preserve the existing primary key so Supabase updates the intended row.
            plan.Id = editingPlan.Id;
            await _database.UpdateLessonPlanAsync(plan);
        }

        return null;
    }
}
