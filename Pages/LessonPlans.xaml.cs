using System.Collections.ObjectModel;
using Client_App.Models;

namespace Client_App;

public partial class LessonPlans : ContentPage
{
    private readonly Lessons _lessons;
    private LessonPlan? _editingPlan;

    public ObservableCollection<LessonPlan> VisiblePlans { get; } = [];

    public LessonPlans(IDatabase database)
    {
        _lessons = new Lessons(database);
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
            await _lessons.DeletePlanAsync(plan);
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
            var validationMessage = await _lessons.SavePlanAsync(
                _editingPlan,
                TitleEntry.Text,
                SchoolLevelPicker.SelectedItem as string,
                CategoryEntry.Text,
                ActivityPicker.SelectedItem as string,
                GradeEntry.Text,
                ScheduledDatePicker.Date,
                DurationEntry.Text,
                ObjectivesEditor.Text);

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
        RefreshPlans();
    }

    private void RefreshPlans(string? searchText = null)
    {
        var plans = _lessons.SearchPlans(searchText ?? SearchEntry.Text);

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
