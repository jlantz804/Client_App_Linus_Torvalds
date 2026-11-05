using Microsoft.Extensions.Logging;

namespace Client_App;
using Client_App.Models;

public static class MauiProgram
{

    public static BusinessLogic BusinessLogic { get; } = new();
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<IDatabase, Database>();

        // Rishi: Activities now use BusinessLogic -> IDatabase -> Database.
        builder.Services.AddSingleton<BusinessLogic>(MauiProgram.BusinessLogic);
		builder.Services.AddTransient<ActivitiesPage>();
		builder.Services.AddTransient<EquipmentModificationsPage>();
		builder.Services.AddTransient<DrillsPage>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
