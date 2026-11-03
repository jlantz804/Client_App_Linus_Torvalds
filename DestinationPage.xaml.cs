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
}
