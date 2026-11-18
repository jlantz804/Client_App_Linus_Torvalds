using Client_App.Models;

namespace Client_App;

public partial class EquipmentModificationsPage : ContentPage
{
    private long? _editingModificationId;
    public EquipmentModificationsPage()
    {
        InitializeComponent();
        BindingContext= MauiProgram.BusinessLogic;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadModificationsAsync();
    }

    private async Task LoadModificationsAsync()
    {
        try
        {
            var modifications = await MauiProgram.BusinessLogic.GetEquipmentModificationsAsync();
            ModificationsCollection.ItemsSource = modifications
                .OrderBy(modification => modification.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Could not load modifications", exception.Message, "OK");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (ActivityPicker.SelectedItem is not ActivityItem activity)
        {
            await DisplayAlertAsync(
                "Missing activity", "Please choose an activity.", "OK");
            return;
        }
        var name = NameEntry.Text ?? "";
        var description = DescriptionEntry.Text ?? "";
        var image = ImageEntry.Text ?? "";
        var steps = ModificationStepsEntry.Text ?? "";

        EquipmentModificationError result;

        if (_editingModificationId.HasValue)
        {
            result = await MauiProgram.BusinessLogic.UpdateEquipmentModificationAsync(
                _editingModificationId.Value, activity, name, description, image, steps);
        }
        else
        {
            result = await MauiProgram.BusinessLogic.AddEquipmentModification(activity, name, description, image, steps);
        }

        if(result != EquipmentModificationError.NoError)
        {
            await DisplayAlertAsync("Save failed", "Could not save the equipment modification.", "OK");
            return;
        }

        OnClearClicked(sender, e);
        OpenEditorToggle.IsChecked = false;
        await LoadModificationsAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var modification = button?.CommandParameter as EquipmentModification;

        if (modification == null)
        {
            return;
        }
        bool confirmed = await DisplayAlertAsync( "Delete equipment modification",
            $"Delete {modification.Name}?", "Delete", "Cancel");

        if (!confirmed)
        {
            return;
        }

        var result = await MauiProgram.BusinessLogic
            .DeleteEquipmentModificationsAsync(modification);

        if (result == EquipmentModificationError.NoRowDeleted)
        {
            await DisplayAlertAsync("Delete failed", "No row was deleted.", "OK");
            return;
        }

        if (result != EquipmentModificationError.NoError)
        {
            await DisplayAlertAsync( "Delete failed",
                "Could not delete the equipment modification.", "OK");
            return;
        }

        await LoadModificationsAsync();
    }

    private void OnEditClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var selectedItem = button?.CommandParameter;
        var modification = selectedItem as EquipmentModification;

        if (modification == null)
        {
            return;
        }

        _editingModificationId = modification.Id;

        var activities = MauiProgram.BusinessLogic.Activities;
        ActivityPicker.SelectedItem = activities.FirstOrDefault( 
            activity => activity.Id == modification.ActivityId);

        NameEntry.Text = modification.Name;
        DescriptionEntry.Text = modification.Description;
        ImageEntry.Text = modification.Image;
        ModificationStepsEntry.Text = modification.Steps;

        OpenEditorToggle.IsChecked = true;
    }

    private void OnEditorCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value)
        {
            _editingModificationId = null;
            OnClearClicked(sender, e);
        }
    }


    private void OnClearClicked(object? sender, EventArgs e)
    {
        ActivityPicker.SelectedIndex = -1;
        NameEntry.Text = "";
        ImageEntry.Text = "";
        DescriptionEntry.Text = "";
        ModificationStepsEntry.Text = "";
    }
    
}