using Client_App.Model;
using Microsoft.IdentityModel.Tokens;

namespace Client_App;

public partial class DrillsPage : ContentPage
{

    public DrillsPage()
    {
        InitializeComponent();
        BindingContext = MauiProgram.BusinessLogic;

        MauiProgram.BusinessLogic.Steps.Add(new StepItem());
    }

    void OnAddStep(object? sender, EventArgs e) => MauiProgram.BusinessLogic.Steps.Add(new StepItem());

    void OnRemoveStep(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: StepItem step })
            MauiProgram.BusinessLogic.Steps.Remove(step);
        if (MauiProgram.BusinessLogic.Steps.IsNullOrEmpty())
        {
            MauiProgram.BusinessLogic.Steps.Add(new StepItem());
        }
    }

    async void AddDrill_Clicked(object? sender, EventArgs e)
    {
        if (ActivityPicker.SelectedItem is not Activity activity)
        {
            await DisplayAlertAsync("Missing activity", "Please choose an activity.", "OK");
            return;
        }

        DrillError result = await MauiProgram.BusinessLogic.AddDrill(activity, NameENT.Text, ImageENT.Text, VideoENT.Text, 
            DescriptionENT.Text, MauiProgram.BusinessLogic.Steps.Select(s => s.Text).Where(t => !string.IsNullOrWhiteSpace(t)).ToList());
        if (result != DrillError.NoError)
        {
            await DisplayAlertAsync("RuhRoh Raggy", result.ToString(), "OK");
        }
        else
        {
            RemoveAllSteps();
            Clear();
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
            Steps = MauiProgram.BusinessLogic.Steps.Select(s => s.Text).Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
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
        MauiProgram.BusinessLogic.Steps.Clear();
        MauiProgram.BusinessLogic.Steps.Add(new StepItem());
    }
}
