namespace Client_App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AdaptedPE.TeacherSignInPage), typeof(AdaptedPE.TeacherSignInPage));
	}
}
