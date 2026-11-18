using System.Collections.ObjectModel;
using Client_App.Models;

namespace Client_App;

/*
Author: Rishikesh Dhanasekaran
Display school-specific activities in the same layout as Lesson Plans.
The existing BusinessLogic/Database layer handles the Supabase calls.
*/
[QueryProperty(nameof(SchoolLevel), "schoolLevel")]
public partial class ActivitiesPage : ContentPage
{
    private readonly BusinessLogic _activities;
    private readonly List<ActivityCategory> _categories = [];
    private readonly List<ActivityItem> _allActivities = [];

    private ActivityCategory? _selectedCategory;
    private string _schoolCode = string.Empty;
    private bool _loaded;
    private bool _busy;

    public ObservableCollection<ActivityItem> VisibleActivities { get; } = [];

    public string SchoolLevel
    {
        get => _schoolCode;
        set
        {
            _schoolCode = value;
            _loaded = false;
            SchoolLabel.Text = value switch
            {
                "elementary" => "Elementary School (PreK - 5)",
                "middle" => "Middle School (6 - 8)",
                "high" => "High School (9 - 12)",
                _ => value
            };
            ActivityCountLabel.Text = $"{SchoolLabel.Text} · Loading activities…";
        }
    }

    public ActivitiesPage(BusinessLogic activities)
    {
        _activities = activities;
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_loaded)
        {
            await LoadCategoriesAsync();
        }
    }

    // Load the school-specific categories using the existing database layer.
    private async Task LoadCategoriesAsync()
    {
        if (_busy)
        {
            return;
        }

        SetBusy(true, "Loading categories...");
        try
        {
            _categories.Clear();
            _categories.AddRange(await _activities.GetActivityCategoriesAsync(_schoolCode));

            if (_schoolCode == "elementary")
            {
                ShowCategorySelection();
            }
            else
            {
                _selectedCategory = _categories.FirstOrDefault(c => c.Name == "Sports & Activities");
                if (_selectedCategory is null)
                {
                    ShowCategorySelection();
                    CategoryPanel.IsVisible = false;
                    ActivityCountLabel.Text = "Category not found";
                    StatusLabel.Text = "Sports & Activities was not found for this school level.";
                    return;
                }

                ShowActivitySection();
                await ReadActivitiesAsync();
            }

            _loaded = true;
        }
        catch (Exception ex)
        {
            _loaded = false;
            StatusLabel.Text = "Could not load activities.";
            await DisplayAlertAsync("Supabase error", ex.Message, "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowCategorySelection()
    {
        _selectedCategory = null;
        _allActivities.Clear();
        VisibleActivities.Clear();
        CategoryPanel.IsVisible = true;
        ActivityPanel.IsVisible = false;
        NewActivityButton.IsVisible = false;
        SearchPanel.IsVisible = false;
        EditorOverlay.IsVisible = false;
        SearchEntry.Text = string.Empty;
        ActivityCountLabel.Text = $"{SchoolLabel.Text} · Choose a category";
        StatusLabel.Text = "";
    }

    private void ShowActivitySection()
    {
        if (_selectedCategory is null)
        {
            return;
        }

        CategoryPanel.IsVisible = false;
        ActivityPanel.IsVisible = true;
        NewActivityButton.IsVisible = true;
        SearchPanel.IsVisible = true;
        BackToCategoriesButton.IsVisible = _schoolCode == "elementary";
        CategoryTitle.Text = _selectedCategory.Name;
        SearchEntry.Text = string.Empty;
    }

    private async Task OpenCategoryAsync(string categoryName)
    {
        if (_busy)
        {
            return;
        }

        _selectedCategory = _categories.FirstOrDefault(c => c.Name == categoryName);
        if (_selectedCategory is null)
        {
            await DisplayAlertAsync("Category not found",
                $"{categoryName} was not found in Supabase.", "OK");
            return;
        }

        ShowActivitySection();
        await RefreshActivitiesAsync();
    }

    private async void OnObjectControlClicked(object? sender, EventArgs e)
    {
        await OpenCategoryAsync("Object Control Skills");
    }

    private async void OnLocomotorClicked(object? sender, EventArgs e)
    {
        await OpenCategoryAsync("Locomotor Movements");
    }

    private void OnBackToCategoriesClicked(object? sender, EventArgs e)
    {
        if (!_busy)
        {
            ShowCategorySelection();
        }
    }

    // Read activities and refresh the cards. Search is local to this category.
    private async Task ReadActivitiesAsync()
    {
        if (_selectedCategory is null)
        {
            return;
        }

        var activities = await _activities.GetActivitiesByCategoryAsync(_selectedCategory.Id);
        _allActivities.Clear();
        _allActivities.AddRange(activities);
        ApplySearchFilter();
        StatusLabel.Text = "";
    }

    private void ApplySearchFilter()
    {
        string searchText = SearchEntry.Text?.Trim() ?? string.Empty;
        VisibleActivities.Clear();
        foreach (var activity in _allActivities.OrderBy(a => a.Name,StringComparer.OrdinalIgnoreCase))
        {
            if (searchText.Length == 0
                || activity.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || (activity.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                VisibleActivities.Add(activity);
            }
        }

        ActivityCountLabel.Text = searchText.Length == 0
            ? $"{_allActivities.Count} activities · {SchoolLabel.Text}"
            : $"{VisibleActivities.Count} of {_allActivities.Count} activities · {SchoolLabel.Text}";
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_selectedCategory is not null)
        {
            ApplySearchFilter();
        }
    }

    private async Task RefreshActivitiesAsync()
    {
        if (_busy || _selectedCategory is null)
        {
            return;
        }

        SetBusy(true, "Loading activities...");
        try
        {
            await ReadActivitiesAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Could not refresh activities.";
            await DisplayAlertAsync("Supabase error", ex.Message, "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await RefreshActivitiesAsync();
    }

    // Show a matching popup editor, similar to the New Lesson Plan editor.
    private void OnAddClicked(object? sender, EventArgs e)
    {
        if (_busy || _selectedCategory is null)
        {
            return;
        }

        ActivityNameEntry.Text = string.Empty;
        EditorCategoryLabel.Text = $"{SchoolLabel.Text} · {_selectedCategory.Name}";
        ValidationLabel.IsVisible = false;
        EditorOverlay.IsVisible = true;
    }

    private void OnCancelEditorClicked(object? sender, EventArgs e)
    {
        if (!_busy)
        {
            EditorOverlay.IsVisible = false;
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_busy || _selectedCategory is null)
        {
            return;
        }

        string name = ActivityNameEntry.Text?.Trim() ?? string.Empty;
        if (name.Length == 0 || name.Length > 100)
        {
            ShowValidation("Enter an activity name between 1 and 100 characters.");
            return;
        }

        SetBusy(true, "Saving activity...");
        try
        {
            ActivityError result = await _activities.AddActivityAsync(_selectedCategory.Id, name);
            if (result != ActivityError.NoError)
            {
                ShowValidation(result.ToUserMessage());
                return;
            }

            EditorOverlay.IsVisible = false;
            await ReadActivitiesAsync();
        }
        catch (Exception ex)
        {
            ShowValidation(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationLabel.IsVisible = true;
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_busy || (sender as Button)?.CommandParameter is not ActivityItem activity)
        {
            return;
        }

        bool confirmed = await DisplayAlertAsync("Delete activity",
            $"Delete '{activity.Name}' from this school level?", "Delete", "Cancel");
        if (!confirmed)
        {
            return;
        }

        SetBusy(true, "Deleting activity...");
        try
        {
            ActivityError result = await _activities.RemoveActivityAsync(activity.Id);
            if (result == ActivityError.NoError)
            {
                await ReadActivitiesAsync();
            }
            else
            {
                await DisplayAlertAsync("Unable to delete activity", result.ToUserMessage(), "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to delete activity", ex.Message, "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy, string? status = null)
    {
        _busy = busy;
        BusySpinner.IsVisible = busy;
        BusySpinner.IsRunning = busy;
        NewActivityButton.IsEnabled = !busy;
        RefreshButton.IsEnabled = !busy;
        ObjectControlButton.IsEnabled = !busy;
        LocomotorButton.IsEnabled = !busy;
        SaveActivityButton.IsEnabled = !busy;

        if (status is not null)
        {
            StatusLabel.Text = status;
        }
    }

}
