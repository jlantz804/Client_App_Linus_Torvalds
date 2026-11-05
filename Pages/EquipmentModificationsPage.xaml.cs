using Client_App.Models;

namespace Client_App;

public partial class EquipmentModificationsPage : ContentPage
{
    public EquipmentModificationsPage()
    {
        InitializeComponent();
        BindingContext= MauiProgram.BusinessLogic;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (ActivityPicker.SelectedItem is not ActivityItem activity)
        {
            await DisplayAlertAsync(
                "Missing activity", "Please choose an activity.", "OK");
            return;
        }
        EquipmentModificationError result = await MauiProgram.BusinessLogic.AddEquipmentModification(
        activity, NameEntry.Text ?? "", DescriptionEntry.Text ?? "",
        ImageEntry.Text ?? "", ModificationStepsEntry.Text ?? "");
    }
    
}