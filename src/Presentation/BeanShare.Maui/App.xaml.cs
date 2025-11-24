namespace BeanShare.Maui;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		System.Diagnostics.Debug.WriteLine("[BeanShare] App constructor starting");
		InitializeComponent();
		System.Diagnostics.Debug.WriteLine("[BeanShare] App InitializeComponent completed");
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		System.Diagnostics.Debug.WriteLine("[BeanShare] Creating main window");
		var mainPage = new MainPage();
		var window = new Window(mainPage) { Title = "BeanShare" };
		System.Diagnostics.Debug.WriteLine("[BeanShare] Main window created");
		return window;
	}

	protected override void OnStart()
	{
		base.OnStart();
		System.Diagnostics.Debug.WriteLine("[BeanShare] App OnStart");
	}
}
