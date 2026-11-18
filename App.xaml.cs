using Microsoft.Extensions.DependencyInjection;

namespace Client_App;

public partial class App : Application
{
    private readonly SupabaseService _supabase;

    public App(SupabaseService supabase)
    {
        InitializeComponent();
        _supabase = supabase;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell(_supabase));
    }
}
