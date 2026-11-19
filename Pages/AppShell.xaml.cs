using AdaptedPE;

namespace Client_App;

public partial class AppShell : Shell
{
	public AppShell(SupabaseService supabase)
	{
		InitializeComponent();
        TeacherSignInShellContent.ContentTemplate = new DataTemplate(() => new TeacherSignInPage(supabase));
        SchoolSelectionShellContent.ContentTemplate = new DataTemplate(() => new SchoolLevelPage(supabase));
		Routing.RegisterRoute(nameof(DestinationPage), typeof(DestinationPage));
		Routing.RegisterRoute(nameof(LessonPlans), typeof(LessonPlans));
		Routing.RegisterRoute(nameof(ActivitiesPage), typeof(ActivitiesPage));
		Routing.RegisterRoute(nameof(EquipmentModificationsPage), typeof(EquipmentModificationsPage));
		Routing.RegisterRoute(nameof(DrillsPage), typeof(DrillsPage));
	}
}
