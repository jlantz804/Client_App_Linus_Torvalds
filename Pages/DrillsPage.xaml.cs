using System.Collections.ObjectModel;
using Client_App.Models;

namespace Client_App;

[QueryProperty(nameof(SchoolLevel), "schoolLevel")]
public partial class DrillsPage : ContentPage
{
    private readonly SupabaseService _supabase;
    private readonly ActivitiesService _activitiesService;
    private readonly List<ActivityCategory> _categories = [];
    private bool _busy;

    public string SchoolLevel { get; set; } = string.Empty;

    public ObservableCollection<StepItem> Steps { get; } = [new StepItem()];
    public ObservableCollection<Drill> Drills { get; } = [];
    public ObservableCollection<ActivityItem> Activities { get; } = [];

    public DrillsPage(SupabaseService supabase, ActivitiesService activitiesService)
    {
        _supabase = supabase;
        _activitiesService = activitiesService;
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    async Task LoadAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            _categories.Clear();
            _categories.AddRange(await _activitiesService.GetCategoriesAsync(SchoolLevel));
            CategoryPicker.ItemsSource = _categories.Select(c => c.Name).ToList();

            Drills.Clear();
            foreach (var drill in await _supabase.GetDrillsAsync())
            {
                Drills.Add(drill);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Could not load drills", ex.Message, "OK");
        }
        finally { _busy = false; }
    }

    // Category picks filter the activity picker
    async void CategoryPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        Activities.Clear();
        if (CategoryPicker.SelectedIndex < 0) return;

        try
        {
            var category = _categories[CategoryPicker.SelectedIndex];
            foreach (var activity in await _activitiesService.GetActivitiesAsync(category.Id))
            {
                Activities.Add(activity);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Could not load activities", ex.Message, "OK");
        }
    }

    // Selecting a row loads it into the form so Edit Drill has something to update
    void DrillsCV_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Drill drill) return;

        NameENT.Text = drill.Name;
        ImageENT.Text = drill.ImageUrl;
        VideoENT.Text = drill.VideoUrl;
        DescriptionENT.Text = drill.Description;

        Steps.Clear();
        foreach (var step in drill.Steps)
        {
            Steps.Add(new StepItem { Text = step });
        }
        if (Steps.Count == 0) Steps.Add(new StepItem());
    }

    void OnAddStep(object? sender, EventArgs e) => Steps.Add(new StepItem());

    void OnRemoveStep(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: StepItem step })
        {
            Steps.Remove(step);
        }
        if (Steps.Count == 0)
        {
            Steps.Add(new StepItem());
        }
    }

    List<string> GetStepTexts() =>
        Steps.Select(s => s.Text?.Trim() ?? "").Where(t => t.Length > 0).ToList();

    static string? NullIfBlank(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    async void AddDrill_Clicked(object? sender, EventArgs e)
    {
        if (_busy) return;

        if (ActivityPicker.SelectedItem is not ActivityItem activity)
        {
            await DisplayAlertAsync("Missing activity", "Please choose a category and an activity.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(NameENT.Text))
        {
            await DisplayAlertAsync("Missing name", "Please enter a drill name.", "OK");
            return;
        }

        _busy = true;
        try
        {
            var drill = new Drill
            {
                ActivityId = activity.Id,
                Name = NameENT.Text.Trim(),
                Description = DescriptionENT.Text?.Trim() ?? "",
                ImageUrl = NullIfBlank(ImageENT.Text),
                VideoUrl = NullIfBlank(VideoENT.Text),
                Steps = GetStepTexts()
            };
            await _supabase.AddDrillAsync(drill);
            Clear();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to add drill", ex.Message, "OK");
            return;
        }
        finally { _busy = false; }

        await LoadAsync();
    }

    async void EditDrill_Clicked(object? sender, EventArgs e)
    {
        if (_busy) return;

        if (DrillsCV.SelectedItem is not Drill selected)
        {
            await DisplayAlertAsync("No drill selected", "Tap a drill in the list first.", "OK");
            return;
        }

        _busy = true;
        try
        {
            var updated = new Drill
            {
                DrillId = selected.DrillId,
                // Keep the existing activity unless a new one was picked
                ActivityId = (ActivityPicker.SelectedItem as ActivityItem)?.Id ?? selected.ActivityId,
                Name = NameENT.Text?.Trim() ?? "",
                Description = DescriptionENT.Text?.Trim() ?? "",
                ImageUrl = NullIfBlank(ImageENT.Text),
                VideoUrl = NullIfBlank(VideoENT.Text),
                Steps = GetStepTexts()
            };
            await _supabase.UpdateDrillAsync(updated);
            Clear();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to edit drill", ex.Message, "OK");
            return;
        }
        finally { _busy = false; }

        await LoadAsync();
    }

    async void DeleteDrill_Clicked(object? sender, EventArgs e)
    {
        if (_busy || (sender as BindableObject)?.BindingContext is not Drill drill) return;

        bool confirm = await DisplayAlertAsync("Delete Drill", $"Delete '{drill.Name}'?", "Delete", "Cancel");
        if (!confirm) return;

        _busy = true;
        try
        {
            await _supabase.DeleteDrillAsync(drill.DrillId);
            Drills.Remove(drill);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to delete drill", ex.Message, "OK");
        }
        finally { _busy = false; }
    }

    async void DeleteAllDrills_Clicked(object? sender, EventArgs e)
    {
        if (_busy) return;

        bool confirm = await DisplayAlertAsync("Delete All", "Delete every drill? This can't be undone.", "Delete All", "Cancel");
        if (!confirm) return;

        _busy = true;
        try
        {
            await _supabase.DeleteAllDrillsAsync();
            Drills.Clear();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to delete drills", ex.Message, "OK");
        }
        finally { _busy = false; }
    }

    void ClearFields(object? sender, EventArgs e) => Clear();

    void Clear()
    {
        CategoryPicker.SelectedIndex = -1;
        ActivityPicker.SelectedIndex = -1;
        NameENT.Text = "";
        ImageENT.Text = "";
        VideoENT.Text = "";
        DescriptionENT.Text = "";
        DrillsCV.SelectedItem = null;

        Steps.Clear();
        Steps.Add(new StepItem());
    }
}