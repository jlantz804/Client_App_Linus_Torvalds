using System.Collections.ObjectModel;
using Client_App.Models;

namespace Client_App;

// Gavin: Display and manage drills from Supabase.
public partial class DrillsPage : ContentPage
{
    private readonly SupabaseService _supabase;
    private Drill? _selectedDrill;
    private bool _busy;

    public ObservableCollection<Drill> Drills { get; } = new();
    public ObservableCollection<ActivityItem> Activities { get; } = new();
    public ObservableCollection<StepItem> Steps { get; } = new();

    public DrillsPage(SupabaseService supabase)
    {
        _supabase = supabase;
        InitializeComponent();
        BindingContext = this;
        Steps.Add(new StepItem());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        try
        {
            var activities = await _supabase.GetAllActivitiesAsync();
            var drills = await _supabase.GetDrillsAsync();

            Activities.Clear();
            foreach (var activity in activities)
            {
                Activities.Add(activity);
            }

            Drills.Clear();
            foreach (var drill in drills)
            {
                Drills.Add(drill);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Drills error", ex.Message, "OK");
        }
        finally
        {
            _busy = false;
        }
    }

    // Select an existing drill to edit it.
    private void OnDrillSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selectedDrill = e.CurrentSelection.FirstOrDefault() as Drill;
        if (_selectedDrill is null)
        {
            return;
        }

        ActivityPicker.SelectedItem = Activities.FirstOrDefault(
            a => a.Id == _selectedDrill.ActivityId);
        NameENT.Text = _selectedDrill.Name;
        ImageENT.Text = _selectedDrill.ImageUrl;
        VideoENT.Text = _selectedDrill.VideoUrl;
        DescriptionENT.Text = _selectedDrill.Description;

        Steps.Clear();
        foreach (string text in _selectedDrill.Steps)
        {
            Steps.Add(new StepItem { Text = text });
        }
        if (Steps.Count == 0)
        {
            Steps.Add(new StepItem());
        }
    }

    private void OnAddStep(object? sender, EventArgs e)
    {
        Steps.Add(new StepItem());
    }

    private void OnRemoveStep(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: StepItem step })
        {
            Steps.Remove(step);
        }
        if (Steps.Count == 0)
        {
            Steps.Add(new StepItem());
        }
    }

    private Drill? ReadForm()
    {
        if (ActivityPicker.SelectedItem is not ActivityItem activity)
        {
            return null;
        }
        return new Drill
        {
            ActivityId = activity.Id,
            Name = NameENT.Text?.Trim() ?? string.Empty,
            ImageUrl = ImageENT.Text?.Trim(),
            VideoUrl = VideoENT.Text?.Trim(),
            Description = DescriptionENT.Text?.Trim() ?? string.Empty,
            Steps = Steps.Select(step => (step.Text ?? string.Empty).Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList()
        };
    }

    private async void AddDrill_Clicked(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }
        Drill? drill = ReadForm();
        if (drill is null || string.IsNullOrWhiteSpace(drill.Name))
        {
            await DisplayAlertAsync("Missing information",
                "Choose an activity and enter a drill name.", "OK");
            return;
        }

        _busy = true;
        try
        {
            await _supabase.AddDrillAsync(drill);
            Clear();
            _busy = false;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to add drill", ex.Message, "OK");
        }
        finally
        {
            _busy = false;
        }
    }

    private async void EditDrill_Clicked(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }
        if (_selectedDrill is null)
        {
            await DisplayAlertAsync("Select a drill",
                "Tap an existing drill first.", "OK");
            return;
        }
        Drill? drill = ReadForm();
        if (drill is null || string.IsNullOrWhiteSpace(drill.Name))
        {
            await DisplayAlertAsync("Missing information",
                "Choose an activity and enter a drill name.", "OK");
            return;
        }
        drill.DrillId = _selectedDrill.DrillId;

        _busy = true;
        try
        {
            await _supabase.UpdateDrillAsync(drill);
            Clear();
            _busy = false;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to edit drill", ex.Message, "OK");
        }
        finally
        {
            _busy = false;
        }
    }

    private async void DeleteDrill_Clicked(object? sender, EventArgs e)
    {
        if (_busy || sender is not Button { BindingContext: Drill drill })
        {
            return;
        }
        bool confirmed = await DisplayAlertAsync("Remove drill",
            $"Remove '{drill.Name}'?", "Remove", "Cancel");
        if (!confirmed)
        {
            return;
        }

        _busy = true;
        try
        {
            await _supabase.DeleteDrillAsync(drill.DrillId);
            Clear();
            _busy = false;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to remove drill", ex.Message, "OK");
        }
        finally
        {
            _busy = false;
        }
    }

    private async void DeleteAllDrills_Clicked(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }
        bool confirmed = await DisplayAlertAsync("Remove all drills",
            "Delete ALL drills from the shared Supabase database?",
            "Delete All", "Cancel");
        if (!confirmed)
        {
            return;
        }

        _busy = true;
        try
        {
            // Delete only IDs we could read, rather than a blanket database delete.
            foreach (var drill in Drills.ToList())
            {
                await _supabase.DeleteDrillAsync(drill.DrillId);
            }
            Clear();
            _busy = false;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to remove drills", ex.Message, "OK");
        }
        finally
        {
            _busy = false;
        }
    }

    private void ClearFields(object? sender, EventArgs e)
    {
        Clear();
    }

    private void Clear()
    {
        _selectedDrill = null;
        DrillsCV.SelectedItem = null;
        ActivityPicker.SelectedItem = null;
        NameENT.Text = string.Empty;
        ImageENT.Text = string.Empty;
        VideoENT.Text = string.Empty;
        DescriptionENT.Text = string.Empty;
        Steps.Clear();
        Steps.Add(new StepItem());
    }
}
