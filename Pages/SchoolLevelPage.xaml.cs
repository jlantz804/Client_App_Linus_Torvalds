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
    private readonly SupabaseService _supabase;

    public SchoolLevelPage(SupabaseService supabase)
    {
        InitializeComponent();
        _supabase = supabase;
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        var signOutButton = sender as Button;
        if (signOutButton is not null)
        {
            signOutButton.IsEnabled = false;
        }

        try
        {
            await _supabase.SignOutAsync();
            await Shell.Current.GoToAsync($"//{nameof(TeacherSignInPage)}");
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Sign out failed", exception.Message, "OK");
        }
        finally
        {
            if (signOutButton is not null)
            {
                signOutButton.IsEnabled = true;
            }
        }
    }

    private async void OnElementarySelected(object? sender, TappedEventArgs e) => await NavigateToDestinationsAsync("Elementary");

    private async void OnMiddleSchoolSelected(object? sender, TappedEventArgs e) => await NavigateToDestinationsAsync("Middle School");

    private async void OnHighSchoolSelected(object? sender, TappedEventArgs e) => await NavigateToDestinationsAsync("High School");

    private Task NavigateToDestinationsAsync(string schoolLevel) =>
        Shell.Current.GoToAsync($"{nameof(DestinationPage)}?schoolLevel={Uri.EscapeDataString(schoolLevel)}");

}
