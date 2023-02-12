using PKHeX.Core;

namespace PKHeXMAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		MainPage = new AppShell(new SAV9SV());
	}
}
