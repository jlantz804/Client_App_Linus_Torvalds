using System.Collections.ObjectModel;

namespace Client_App;

public partial class LessonPlans : ContentPage
{
    private static readonly string[] SchoolLevels = ["Elementary", "Middle School", "High School"];

    private static readonly Dictionary<string, string> ElementaryCategories = new(StringComparer.Ordinal)
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

    private static readonly string[] MiddleHighActivities =
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

    private readonly List<LessonPlan> _allPlans = [];
    private readonly SupabaseService _supabase;
    private LessonPlan? _editingPlan;
    private string _connectionStatus = "Connecting to Supabase…";

    public ObservableCollection<LessonPlan> VisiblePlans { get; } = [];

    public LessonPlans(SupabaseService supabase)
    {
        _supabase = supabase;
        InitializeComponent();
        BindingContext = this;
        SchoolLevelPicker.ItemsSource = SchoolLevels;
        RefreshPlans();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPlansAsync();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        _editingPlan = null;
        EditorHeading.Text = "New lesson plan";
        TitleEntry.Text = string.Empty;
        SchoolLevelPicker.SelectedIndex = -1;
        ActivityPicker.ItemsSource = Array.Empty<string>();
        ActivityPicker.SelectedIndex = -1;
        CategoryEntry.Text = string.Empty;
        CategoryEntry.IsReadOnly = false;
        GradeEntry.Text = string.Empty;
        ScheduledDatePicker.Date = DateTime.Today;
        DurationEntry.Text = string.Empty;
        ObjectivesEditor.Text = string.Empty;
        ValidationLabel.IsVisible = false;
        EditorOverlay.IsVisible = true;
    }

    private void OnEditClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not LessonPlan plan)
            return;

        _editingPlan = plan;
        EditorHeading.Text = "Edit lesson plan";
        TitleEntry.Text = plan.Title;
        SchoolLevelPicker.SelectedItem = plan.SchoolLevel;
        PopulateActivities();
        ActivityPicker.SelectedItem = plan.Activity;
        CategoryEntry.Text = plan.Category;
        CategoryEntry.IsReadOnly = plan.SchoolLevel == "Elementary";
        GradeEntry.Text = plan.GradeLevel;
        ScheduledDatePicker.Date = plan.ScheduledDate;
        DurationEntry.Text = plan.DurationMinutes.ToString();
        ObjectivesEditor.Text = plan.Objectives;
        ValidationLabel.IsVisible = false;
        EditorOverlay.IsVisible = true;
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not LessonPlan plan)
            return;

        var confirm = await DisplayAlertAsync("Delete lesson plan", $"Delete ‘{plan.Title}’?", "Delete", "Cancel");
        if (!confirm)
            return;

        try
        {
            await _supabase.DeleteLessonPlanAsync(plan);
            await LoadPlansAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Unable to delete lesson plan", exception.Message, "OK");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim() ?? string.Empty;
        var schoolLevel = SchoolLevelPicker.SelectedItem as string ?? string.Empty;
        var category = CategoryEntry.Text?.Trim() ?? string.Empty;
        var activity = ActivityPicker.SelectedItem as string ?? string.Empty;
        var grade = GradeEntry.Text?.Trim() ?? string.Empty;
        var objectives = ObjectivesEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(schoolLevel) ||
            string.IsNullOrWhiteSpace(category) ||
            string.IsNullOrWhiteSpace(activity) ||
            string.IsNullOrWhiteSpace(grade) ||
            string.IsNullOrWhiteSpace(objectives))
        {
            ShowValidation("Complete the title, school level, category, activity, grade, and objectives.");
            return;
        }

        if (!int.TryParse(DurationEntry.Text, out var duration) || duration <= 0)
        {
            ShowValidation("Enter a duration greater than zero minutes.");
            return;
        }

        var plan = new LessonPlan
        {
            Title = title,
            SchoolLevel = schoolLevel,
            Category = category,
            Activity = activity,
            GradeLevel = grade,
            ScheduledDate = ScheduledDatePicker.Date ?? DateTime.Today,
            DurationMinutes = duration,
            Objectives = objectives
        };

        try
        {
            if (_editingPlan is null)
            {
                await _supabase.AddLessonPlanAsync(plan);
            }
            else
            {
                plan.Id = _editingPlan.Id;
                await _supabase.UpdateLessonPlanAsync(plan);
            }

            EditorOverlay.IsVisible = false;
            await LoadPlansAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Unable to save lesson plan", exception.Message, "OK");
        }
    }

    private void OnCancelEditorClicked(object? sender, EventArgs e)
    {
        EditorOverlay.IsVisible = false;
    }

    private void OnSchoolLevelChanged(object? sender, EventArgs e)
    {
        PopulateActivities();
        CategoryEntry.Text = string.Empty;
        CategoryEntry.IsReadOnly = SchoolLevelPicker.SelectedItem as string == "Elementary";
    }

    private void OnActivityChanged(object? sender, EventArgs e)
    {
        if (SchoolLevelPicker.SelectedItem as string == "Elementary" &&
            ActivityPicker.SelectedItem is string activity &&
            ElementaryCategories.TryGetValue(activity, out var category))
        {
            CategoryEntry.Text = category;
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshPlans(e.NewTextValue);
    }

    private async Task LoadPlansAsync()
    {
        try
        {
            var plans = await _supabase.GetLessonPlansAsync();
            _allPlans.Clear();
            _allPlans.AddRange(plans);
            _connectionStatus = $"{_allPlans.Count} lesson plans available";
        }
        catch (Exception exception)
        {
            _allPlans.Clear();
            _connectionStatus = $"Supabase: {exception.Message}";
        }

        RefreshPlans();
    }

    private void PopulateActivities()
    {
        var level = SchoolLevelPicker.SelectedItem as string;
        ActivityPicker.ItemsSource = level switch
        {
            "Elementary" => ElementaryCategories.Keys.ToArray(),
            "Middle School" or "High School" => MiddleHighActivities,
            _ => Array.Empty<string>()
        };
    }

    private void RefreshPlans(string? searchText = null)
    {
        var query = (searchText ?? SearchEntry.Text)?.Trim();
        var plans = string.IsNullOrEmpty(query)
            ? _allPlans
            : _allPlans.Where(plan =>
                plan.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.SchoolLevel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.Category.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.Activity.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.GradeLevel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.Objectives.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        VisiblePlans.Clear();
        foreach (var plan in plans)
            VisiblePlans.Add(plan);

        PlanCountLabel.Text = _connectionStatus;
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationLabel.IsVisible = true;
    }
}
