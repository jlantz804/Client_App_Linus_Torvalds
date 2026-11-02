using Client_App;
using Microsoft.Extensions.DependencyInjection;

namespace AdaptedPE;

public partial class TeacherSignInPage : ContentPage
{
    private SupabaseService? _supabase;

    public TeacherSignInPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _supabase = Handler?.MauiContext?.Services.GetService<SupabaseService>();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Enter your email and password.");
            return;
        }

        if (_supabase is null)
        {
            ShowError("The Supabase service is unavailable. Restart the app and try again.");
            return;
        }

        SignInButton.IsEnabled = false;
        SignInButton.Text = "Signing in…";
        ErrorLabel.IsVisible = false;

        try
        {
            await _supabase.SignInAsync(email, password);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
        finally
        {
            SignInButton.IsEnabled = true;
            SignInButton.Text = "Sign in";
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
