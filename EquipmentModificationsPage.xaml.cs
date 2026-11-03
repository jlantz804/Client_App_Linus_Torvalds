using Client_App.Models;

namespace Client_App;

public partial class EquipmentModificationsPage : ContentPage
{
    private SupabaseService _supabase;

    public EquipmentModificationsPage(SupabaseService supabase)
    {
        _supabase = supabase;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ActivityPicker.ItemsSource =
            await _supabase.GetEquipmentActivitiesAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        
        if (ActivityPicker.SelectedItem == null)
        {
            return;
        }

        ActivityItem activity =
            (ActivityItem)ActivityPicker.SelectedItem;

        EquipmentModification modification = new EquipmentModification();

        modification.ActivityId = activity.Id;
        modification.Name = NameEntry.Text ?? "";
        modification.Image = ImageEntry.Text ?? "";
        modification.Description = DescriptionEntry.Text ?? "";
        modification.Steps = ModificationStepsEntry.Text ?? "";

        await _supabase.AddEquipmentModificationAsync(modification);
        await Shell.Current.GoToAsync("..");
    }
}