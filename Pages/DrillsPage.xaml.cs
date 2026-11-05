using System.Collections.ObjectModel;
using Client_App.Models;

namespace Client_App;

public partial class DrillsPage : ContentPage
{
    private readonly SupabaseService _supabase;
    private bool _loaded;

    public ObservableCollection<Drill> Drills { get; } = [];
    public ObservableCollection<ActivityItem> Activities { get; } = [];
    public ObservableCollection<StepItem> Steps { get; } = [];

    public DrillsPage(SupabaseService supabase)
    {
        _supabase = supabase;
        InitializeComponent();
        BindingContext = this;
        Steps.Add(new StepItem());
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = new Command(async () =>
            {
                try
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Navigation failed", ex.ToString(), "OK");
                }
            })
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
        {
            return;
        }

        try
        {
            foreach (var activity in await _supabase.GetAllActivitiesAsync())
            {
                Activities.Add(activity);
            }
            foreach (var drill in await _supabase.GetDrillsAsync())
            {
                Drills.Add(drill);
            }
            _loaded = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to load drills", ex.Message, "OK");
        }
    }

    void OnAddStep(object? sender, EventArgs e) => Steps.Add(new StepItem());

    void OnRemoveStep(object? sender, EventArgs e)
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

    async void AddDrill_Clicked(object? sender, EventArgs e)
    {
        if (ActivityPicker.SelectedItem is not ActivityItem activity)
        {
            await DisplayAlertAsync("Missing activity", "Please choose an activity.", "OK");
            return;
        }

        var drill = new Drill
        {
            ActivityId = activity.Id,
            ActivityName = activity.Name,
            CategoryId = activity.CategoryId,
            Name = NameENT.Text ?? string.Empty,
            ImageUrl = ImageENT.Text,
            VideoUrl = VideoENT.Text,
            Description = DescriptionENT.Text ?? string.Empty,
            Steps = Steps.Select(s => s.Text).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
        };

        try
        {
            await _supabase.AddDrillAsync(drill);
            Drills.Add(drill);
            Clear();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Unable to add drill", ex.Message, "OK");
        }
    }

    async void EditDrill_Clicked(object? sender, EventArgs e)
    {
        var drill = new Drill
        {
            Name = NameENT.Text,
            Description = DescriptionENT.Text,
            ImageUrl = ImageENT.Text,
            VideoUrl = VideoENT.Text,
            Steps = Steps.Select(s => s.Text).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
        };

        // TODO: send `drill` to Supabase here

        RemoveAllSteps();
        Clear();
    }

    async void DeleteDrill_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    async void DeleteAllDrills_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    void ClearFields(object? sender, EventArgs e)
    {
        Clear();
    }

    void Clear() {
        //ActivityPicker.ClearValue();
        NameENT.Text="";
        ImageENT.Text="";
        VideoENT.Text="";
        DescriptionENT.Text="";
        RemoveAllSteps();
    }

    private void RemoveAllSteps()
    {
        Steps.Clear();
        Steps.Add(new StepItem());
    }
}
