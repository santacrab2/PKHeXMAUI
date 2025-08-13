using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class App : Application
{
	public App()
	{
        //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NGaF1cXGFCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWXZedXVTRmFfWEZxXkE=");
        InitializeComponent();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var Version = Preferences.Default.Get("SaveFile", 50);
        Window window = new(PSettings.RememberLastSave
            ? new AppShell(BlankSaveFile.Get((GameVersion)Version, "PKHeX"))
            : (Page)new AppShell(BlankSaveFile.Get(GameVersion.SL, "PKHeX")));
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
