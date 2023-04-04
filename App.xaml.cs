using PKHeX.Core;

namespace PKHeXMAUI;

public partial class App : Application
{
	public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTYxNjIyNEAzMjMxMmUzMTJlMzMzOG55Zm9Jc1VTUjRNM24xV3lJa1VXMFRjQXo3TlVOY014STYxd2FYNWRGSEE9");
        InitializeComponent();
		
		MainPage = new AppShell(SaveUtil.GetBlankSAV(GameVersion.SL, "PKHeX"));
	}
}
