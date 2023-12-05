using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class App : Application
{
	public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NGaF1cXGFCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWXZedXVTRmFfWEZxXkE=");
        InitializeComponent();

        var Version = Preferences.Default.Get("SaveFile", 50);
        MainPage = PSettings.RememberLastSave
            ? new AppShell(SaveUtil.GetBlankSAV((GameVersion)Version, "PKHeX"))
            : (Page)new AppShell(SaveUtil.GetBlankSAV(GameVersion.SL, "PKHeX"));
    }
    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Resumed += (s, e) =>
        {
            if (LiveHex.Reconnect)
            {
                if (!Remote.Connected)
                {
                    Remote.com.Connect();
                }
                else
                {
                    Remote.com.Disconnect();
                    Remote.com.Connect();
                }
            }
        };
        return window;
    }
}
