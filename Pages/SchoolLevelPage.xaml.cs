using Client_App;

namespace AdaptedPE;

/*
Author: Joey Lantz
Date: October 4th
Description: This is the school level page logic
Bugs: None found
*/

public partial class SchoolLevelPage : ContentPage
{
    public SchoolLevelPage()
    {
        InitializeComponent();
    }

    private async void OnElementarySelected(object? sender, TappedEventArgs e) => await NavigateToDestinationsAsync("Elementary");

    private async void OnMiddleSchoolSelected(object? sender, TappedEventArgs e) => await NavigateToDestinationsAsync("Middle School");

    private async void OnHighSchoolSelected(object? sender, TappedEventArgs e) => await NavigateToDestinationsAsync("High School");

    private Task NavigateToDestinationsAsync(string schoolLevel) =>
        Shell.Current.GoToAsync($"{nameof(DestinationPage)}?schoolLevel={Uri.EscapeDataString(schoolLevel)}");

}
