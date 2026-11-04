namespace Client_App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AdaptedPE.SchoolLevelPage), typeof(AdaptedPE.SchoolLevelPage));
		Routing.RegisterRoute(nameof(DestinationPage), typeof(DestinationPage));
		Routing.RegisterRoute(nameof(LessonPlans), typeof(LessonPlans));
		Routing.RegisterRoute(nameof(ActivitiesPage), typeof(ActivitiesPage));
		Routing.RegisterRoute(nameof(EquipmentModificationsPage), typeof(EquipmentModificationsPage));
		Routing.RegisterRoute(nameof(DrillsPage), typeof(DrillsPage));
	}
}
