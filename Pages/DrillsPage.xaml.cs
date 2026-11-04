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
        throw new NotImplementedException();
    }

    async void EditDrill_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    async void DeleteDrill_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    async void DeleteAllDrills_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
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