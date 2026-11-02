using System.Collections.ObjectModel;

namespace Client_App;

public partial class LessonPlans : ContentPage
{
    private readonly List<LessonPlan> _allPlans =
    [
        new("Teamwork Through Movement", "Physical Education", "Grade 4", DateTime.Today.AddDays(1), 45,
            "Practice communication and cooperation through small-group movement challenges."),
        new("Building a Balanced Plate", "Health", "Grade 5", DateTime.Today.AddDays(2), 35,
            "Identify major food groups and create a balanced meal using familiar foods."),
        new("Locomotor Skills Circuit", "Physical Education", "Grade 2", DateTime.Today.AddDays(4), 40,
            "Develop balance, coordination, and safe movement through a rotating activity circuit."),
        new("Setting Personal Goals", "Wellness", "Grade 6", DateTime.Today.AddDays(5), 30,
            "Choose a realistic wellness goal and describe steps for tracking progress.")
    ];

    public ObservableCollection<LessonPlan> VisiblePlans { get; } = [];

    private LessonPlan? _editingPlan;

    public LessonPlans()
    {
        InitializeComponent();
        BindingContext = this;
        RefreshPlans();
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        _editingPlan = null;
        EditorHeading.Text = "New lesson plan";
        TitleEntry.Text = string.Empty;
        SubjectEntry.Text = string.Empty;
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
        SubjectEntry.Text = plan.Subject;
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

        _allPlans.Remove(plan);
        RefreshPlans();
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim() ?? string.Empty;
        var subject = SubjectEntry.Text?.Trim() ?? string.Empty;
        var grade = GradeEntry.Text?.Trim() ?? string.Empty;
        var objectives = ObjectivesEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(subject) ||
            string.IsNullOrWhiteSpace(grade) ||
            string.IsNullOrWhiteSpace(objectives))
        {
            ShowValidation("Enter a title, subject, grade, and lesson objectives.");
            return;
        }

        if (!int.TryParse(DurationEntry.Text, out var duration) || duration <= 0)
        {
            ShowValidation("Enter a duration greater than zero minutes.");
            return;
        }

        if (_editingPlan is null)
        {
            _allPlans.Insert(0, new LessonPlan(
                title,
                subject,
                grade,
                ScheduledDatePicker.Date ?? DateTime.Today,
                duration,
                objectives));
        }
        else
        {
            _editingPlan.Title = title;
            _editingPlan.Subject = subject;
            _editingPlan.GradeLevel = grade;
            _editingPlan.ScheduledDate = ScheduledDatePicker.Date ?? DateTime.Today;
            _editingPlan.DurationMinutes = duration;
            _editingPlan.Objectives = objectives;
        }

        EditorOverlay.IsVisible = false;
        RefreshPlans();
    }

    private void OnCancelEditorClicked(object? sender, EventArgs e)
    {
        EditorOverlay.IsVisible = false;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshPlans(e.NewTextValue);
    }

    private void RefreshPlans(string? searchText = null)
    {
        var query = (searchText ?? SearchEntry.Text)?.Trim();
        var plans = string.IsNullOrEmpty(query)
            ? _allPlans
            : _allPlans.Where(plan =>
                plan.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.Subject.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.GradeLevel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                plan.Objectives.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        VisiblePlans.Clear();
        foreach (var plan in plans)
            VisiblePlans.Add(plan);

        PlanCountLabel.Text = $"{_allPlans.Count} plans · local preview data";
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationLabel.IsVisible = true;
    }
}

public sealed class LessonPlan(
    string title,
    string subject,
    string gradeLevel,
    DateTime scheduledDate,
    int durationMinutes,
    string objectives)
{
    public string Title { get; set; } = title;
    public string Subject { get; set; } = subject;
    public string GradeLevel { get; set; } = gradeLevel;
    public DateTime ScheduledDate { get; set; } = scheduledDate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Objectives { get; set; } = objectives;
}
