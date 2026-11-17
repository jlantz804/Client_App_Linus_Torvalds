using Client_App.Models;

namespace Client_App;

public partial class DrillsPage : ContentPage
{
    private Drill? _editing;

    public DrillsPage()
    {
        InitializeComponent();
        BindingContext = MauiProgram.BusinessLogic;
        EnsureOneStep();
        UpdateCount();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateCount();
    }

    // ---------- Header / list ----------

    private void UpdateCount()
    {
        PlanCountLabel.Text = $"{MauiProgram.BusinessLogic.Drills.Count()} drills available";
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.Trim();

        if (string.IsNullOrEmpty(q))
        {
            // Back to the live binding so adds/deletes show up immediately
            DrillsCollection.ItemsSource = null;
            DrillsCollection.SetBinding(ItemsView.ItemsSourceProperty, "Drills");
            return;
        }

        DrillsCollection.RemoveBinding(ItemsView.ItemsSourceProperty);
        DrillsCollection.ItemsSource = MauiProgram.BusinessLogic.Drills
            .Where(d =>
                d.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.ActivityName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.Steps.Any(s => s.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    // ---------- Open / close editor ----------

    private void OnAddClicked(object? sender, EventArgs e)
    {
        _editing = null;
        EditorHeading.Text = "New drill";
        ClearForm();
        EditorOverlay.IsVisible = true;
    }

    private void OnEditClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Drill drill) {
            return;
        }

        _editing = drill;
        EditorHeading.Text = "Edit drill";
        // Sets Editor Display with Drill's details
        ActivityPicker.SelectedItem = MauiProgram.BusinessLogic.Activities
            .FirstOrDefault(a => a.Id == drill.ActivityId);
        NameEntry.Text = drill.Name;
        DescriptionEditor.Text = drill.Description;
        ImageEntry.Text = drill.ImageUrl;
        VideoEntry.Text = drill.VideoUrl;
        LockedSwitch.IsToggled = drill.Locked;

        MauiProgram.BusinessLogic.Steps.Clear();
        foreach (var s in drill.Steps)
        {
            MauiProgram.BusinessLogic.Steps.Add(new StepItem { Text = s });
        }
        EnsureOneStep();

        ValidationLabel.IsVisible = false;
        EditorOverlay.IsVisible = true;
    }

    private void OnCancelEditorClicked(object? sender, EventArgs e)
    {
        EditorOverlay.IsVisible = false;
    }

    // ---------- Save / delete ----------

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (ActivityPicker.SelectedItem is not ActivityItem activity)
        {
            ShowValidation("Please choose an activity.");
            return;
        }

        var steps = MauiProgram.BusinessLogic.Steps
                .Select(s => s.Text)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        // Change once login is working
        string currentUser = "Developer";

        DrillError result = _editing is null
            ? await MauiProgram.BusinessLogic.AddDrill(
                activity, NameEntry.Text, DescriptionEditor.Text,
                ImageEntry.Text, VideoEntry.Text, steps,
                LockedSwitch.IsToggled, currentUser)
            : await MauiProgram.BusinessLogic.EditDrill(
                _editing, activity, NameEntry.Text, DescriptionEditor.Text,
                ImageEntry.Text, VideoEntry.Text, steps,
                LockedSwitch.IsToggled, currentUser);

        if (result != DrillError.NoError)
        {
            await DisplayAlertAsync("Oops something went wrong", result.ToString(), "OK");
            return;
        }

        _editing = null;
        EditorOverlay.IsVisible = false;
        ClearForm();
        UpdateCount();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not Drill drill) {
            return;
        }

        var confirm = await DisplayAlertAsync("Delete Drill", $"Delete ‘{drill.Name}’?", "Delete", "Cancel");
        if (!confirm) {
            return;
        }
        // Change once login is working
        string currentUser = "Developer";

        DrillError result = await MauiProgram.BusinessLogic.DeleteDrill(drill, currentUser);
        if (result != DrillError.NoError)
        {
            await DisplayAlertAsync("Oops something went wrong", result.ToString(), "OK");
            return;
        }
        UpdateCount();
    }

    // ---------- Steps ----------

    private void OnAddStep(object? sender, EventArgs e)
    {
        MauiProgram.BusinessLogic.Steps.Add(new StepItem());
    }

    private void OnRemoveStep(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: StepItem step })
        {
            MauiProgram.BusinessLogic.Steps.Remove(step);
        }
        EnsureOneStep();
    }

    private void EnsureOneStep()
    {
        if (!MauiProgram.BusinessLogic.Steps.Any())
        {
            MauiProgram.BusinessLogic.Steps.Add(new StepItem());
        }
    }

    // ---------- Helpers ----------

    private void ClearForm()
    {
        ActivityPicker.SelectedIndex = -1;
        NameEntry.Text = string.Empty;
        DescriptionEditor.Text = string.Empty;
        ImageEntry.Text = string.Empty;
        VideoEntry.Text = string.Empty;
        MauiProgram.BusinessLogic.Steps.Clear();
        EnsureOneStep();
        LockedSwitch.IsToggled = false;
        ValidationLabel.IsVisible = false;
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationLabel.IsVisible = true;
    }
}