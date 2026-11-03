namespace Client_App;

[QueryProperty(nameof(SchoolLevel), "schoolLevel")]
public partial class DestinationPage : ContentPage
{
    private string _schoolLevel = string.Empty;

    public string SchoolLevel
    {
        get => _schoolLevel;
        set
        {
            _schoolLevel = value;
            SchoolLevelLabel.Text = string.IsNullOrWhiteSpace(value)
                ? "Choose a section"
                : $"{value} · Choose a section";
        }
    }

    public DestinationPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnLessonPlansClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LessonPlans));
    }


    private bool _openingActivities = false;

    // Rishi: Open Activities and show any navigation error.
    private async void OnActivitiesClicked(object? sender, EventArgs e)
    {
        if (_openingActivities)
        {
            return;
        }

        try
        {
            _openingActivities = true;

            string code = SchoolLevel switch
            {
                "Elementary" => "elementary",
                "Elementary School" => "elementary",
                "Middle School" => "middle",
                "High School" => "high",
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(code))
            {
                await DisplayAlertAsync(
                    "School Level",
                    "Please select a school level first.",
                    "OK");
                return;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(ActivitiesPage)}?schoolLevel={code}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());

            await DisplayAlertAsync(
                "Activities Error",
                ex.ToString(),
                "OK");
        }
        finally
        {
            _openingActivities = false;
        }
    }

    // For EquipmentModifications ALEX
    private async void OnEquipmentClicked(object? sender, EventArgs e)
{
    await Shell.Current.GoToAsync(nameof(EquipmentModificationsPage));
}
}
