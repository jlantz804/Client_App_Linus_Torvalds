using System.Collections.ObjectModel;
using Client_App.Models;

namespace Client_App;

[QueryProperty(nameof(SchoolLevel), "schoolLevel")]
public partial class LessonPlans : ContentPage
{
    private readonly Lessons _lessons;
    private readonly SupabaseService _supabase;
    public string? SchoolLevel { get; set; }
    private LessonPlan? _editingPlan;

    public ObservableCollection<LessonPlan> VisiblePlans { get; } = [];

    public LessonPlans(IDatabase database, SupabaseService supabase)
    {
        _lessons = new Lessons(database);
        _supabase = supabase;
        InitializeComponent();
        BindingContext = this;
        SchoolLevelPicker.ItemsSource = _lessons.SchoolLevels.ToArray();
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
        SchoolLevelPicker.SelectedItem = SchoolLevel;
        if (SchoolLevel is null)
        {
            SchoolLevelPicker.SelectedIndex = -1;
        }
        var selectedSchoolLevel = SchoolLevelPicker.SelectedItem as string;
        LockedSwitch.IsToggled = false;
        LockedSwitch.IsEnabled = true;
        ActivityPicker.ItemsSource = _lessons.GetActivities(selectedSchoolLevel);
        ActivityPicker.SelectedIndex = -1;
        CategoryEntry.Text = string.Empty;
        CategoryEntry.IsReadOnly = _lessons.IsCategoryReadOnly(selectedSchoolLevel);
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
        {
            return;
        }

        _editingPlan = plan;
        EditorHeading.Text = "Edit lesson plan";
        TitleEntry.Text = plan.Title;
        SchoolLevelPicker.SelectedItem = plan.SchoolLevel;
        ActivityPicker.ItemsSource = _lessons.GetActivities(plan.SchoolLevel);
        ActivityPicker.SelectedItem = plan.Activity;
        CategoryEntry.Text = plan.Category;
        CategoryEntry.IsReadOnly = _lessons.IsCategoryReadOnly(plan.SchoolLevel);
        GradeEntry.Text = plan.GradeLevel;
        ScheduledDatePicker.Date = plan.ScheduledDate;
        DurationEntry.Text = plan.DurationMinutes.ToString();
        ObjectivesEditor.Text = plan.Objectives;
        LockedSwitch.IsToggled = plan.Locked;
        LockedSwitch.IsEnabled = plan.CanManageLock;
        ValidationLabel.IsVisible = false;
        EditorOverlay.IsVisible = true;
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not LessonPlan plan)
        {
            return;
        }

        var confirm = await DisplayAlertAsync("Delete lesson plan", $"Delete ‘{plan.Title}’?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        try
        {
            var currentUser = _supabase.CurrentUserId;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                await DisplayAlertAsync("Sign in required", "Sign in before deleting lesson plans.", "OK");
                return;
            }

            var isAdmin = await _supabase.IsCurrentUserAdminAsync();
            var error = await _lessons.DeletePlanAsync(plan, currentUser, isAdmin);
            if (error is not null)
            {
                await DisplayAlertAsync("Unable to delete lesson plan", error, "OK");
                return;
            }

            await LoadPlansAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Unable to delete lesson plan", exception.Message, "OK");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var currentUser = _supabase.CurrentUserId;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                await DisplayAlertAsync("Sign in required", "Sign in before saving lesson plans.", "OK");
                return;
            }

            var isAdmin = await _supabase.IsCurrentUserAdminAsync();
            var validationMessage = await _lessons.SavePlanAsync(
                _editingPlan,
                TitleEntry.Text,
                SchoolLevelPicker.SelectedItem as string,
                CategoryEntry.Text,
                ActivityPicker.SelectedItem as string,
                GradeEntry.Text,
                ScheduledDatePicker.Date,
                DurationEntry.Text,
                ObjectivesEditor.Text,
                LockedSwitch.IsToggled,
                currentUser,
                isAdmin);

            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                return;
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
        var schoolLevel = SchoolLevelPicker.SelectedItem as string;
        ActivityPicker.ItemsSource = _lessons.GetActivities(schoolLevel);
        CategoryEntry.Text = string.Empty;
        CategoryEntry.IsReadOnly = _lessons.IsCategoryReadOnly(schoolLevel);
    }

    private void OnActivityChanged(object? sender, EventArgs e)
    {
        if (_lessons.IsCategoryReadOnly(SchoolLevelPicker.SelectedItem as string))
        {
            CategoryEntry.Text = _lessons.GetCategory(ActivityPicker.SelectedItem as string) ?? string.Empty;
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshPlans(e.NewTextValue);
    }

    private async Task LoadPlansAsync()
    {
        await _lessons.LoadPlansAsync();

        var plans = _lessons.SearchPlans(null);
        var currentUser = _supabase.CurrentUserId;
        var isAdmin = false;
        if (!string.IsNullOrWhiteSpace(currentUser))
        {
            try
            {
                isAdmin = await _supabase.IsCurrentUserAdminAsync();
            }
            catch (Exception)
            {
            }
        }

        foreach (var plan in plans)
        {
            plan.CanManageLock = isAdmin || string.Equals(currentUser, plan.CreatedBy, StringComparison.Ordinal);
            plan.CanEditOrDelete = !plan.Locked || plan.CanManageLock;
        }

        try
        {
            var profiles = (await _supabase.Client.From<UserProfile>().Get()).Models;
            var namesByUserId = profiles.ToDictionary(
                profile => profile.Uid,
                profile => profile.Name,
                StringComparer.OrdinalIgnoreCase);

            foreach (var plan in plans)
            {
                plan.CreatorName = plan.CreatedBy is not null && namesByUserId.TryGetValue(plan.CreatedBy, out var creatorName)
                    ? creatorName
                    : "Unknown teacher";
            }
        }
        catch (Exception)
        {
            foreach (var plan in plans)
            {
                plan.CreatorName = "Unknown teacher";
            }
        }

        RefreshPlans();
    }

    private void RefreshPlans(string? searchText = null)
    {
        var plans = _lessons.SearchPlans(searchText ?? SearchEntry.Text);
        if (!string.IsNullOrWhiteSpace(SchoolLevel))
        {
            plans = plans
                .Where(plan => string.Equals(plan.SchoolLevel, SchoolLevel, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        VisiblePlans.Clear();
        foreach (var plan in plans)
        {
            VisiblePlans.Add(plan);
        }

        PlanCountLabel.Text = _lessons.ConnectionStatus;
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationLabel.IsVisible = true;
    }
}
