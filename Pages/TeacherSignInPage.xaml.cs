using Client_App;

namespace AdaptedPE;

public partial class TeacherSignInPage : ContentPage
{
    private readonly SupabaseService _supabase;

    public TeacherSignInPage(SupabaseService supabase)
    {
        InitializeComponent();
        _supabase = supabase;
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        SignInMessageLabel.IsVisible = false;
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowMessage(SignInMessageLabel, "Enter your email and password.");
            return;
        }

        SignInButton.IsEnabled = false;
        try
        {
            if (!await _supabase.SignInAsync(email, password))
            {
                ShowMessage(SignInMessageLabel, "Sign-in did not return a user session.");
                return;
            }

            await Shell.Current.GoToAsync($"//{nameof(SchoolLevelPage)}");
        }
        catch (Exception exception)
        {
            ShowMessage(SignInMessageLabel, exception.Message);
        }
        finally
        {
            SignInButton.IsEnabled = true;
        }
    }

    private void OnCreateAccountClicked(object? sender, EventArgs e)
    {
        CreateNameEntry.Text = string.Empty;
        CreateEmailEntry.Text = EmailEntry.Text;
        CreatePasswordEntry.Text = string.Empty;
        CreateAccountMessageLabel.IsVisible = false;
        CreateAccountOverlay.IsVisible = true;
    }

    private void OnCancelCreateAccountClicked(object? sender, EventArgs e)
    {
        CreateAccountOverlay.IsVisible = false;
    }

    private async void OnSubmitCreateAccountClicked(object? sender, EventArgs e)
    {
        CreateAccountMessageLabel.IsVisible = false;
        var name = CreateNameEntry.Text?.Trim() ?? string.Empty;
        var email = CreateEmailEntry.Text?.Trim() ?? string.Empty;
        var password = CreatePasswordEntry.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowMessage(CreateAccountMessageLabel, "Enter your name, email, and password.");
            return;
        }

        if (password.Length < 6)
        {
            ShowMessage(CreateAccountMessageLabel, "Password must be at least 6 characters.");
            return;
        }

        CreateAccountSubmitButton.IsEnabled = false;
        try
        {
            var signedIn = await _supabase.CreateAccountAsync(email, name, password);
            CreateAccountOverlay.IsVisible = false;
            if (signedIn)
            {
                await Shell.Current.GoToAsync($"//{nameof(SchoolLevelPage)}");
            }
            else
            {
                await DisplayAlertAsync(
                    "Account created",
                    "Check your email to confirm your account, then sign in.",
                    "OK");
                EmailEntry.Text = email;
            }
        }
        catch (Exception exception)
        {
            ShowMessage(CreateAccountMessageLabel, exception.Message);
        }
        finally
        {
            CreateAccountSubmitButton.IsEnabled = true;
        }
    }

    private static void ShowMessage(Label label, string message)
    {
        label.Text = message;
        label.IsVisible = true;
    }
}
