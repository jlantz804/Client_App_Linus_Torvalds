namespace AdaptedPE;

public partial class TeacherSignInPage : ContentPage
{
    public TeacherSignInPage()
    {
        InitializeComponent();
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SchoolLevelPage));
        //await Shell.Current.GoToAsync("//SchoolLevelPage");
    }
}
