namespace PKHeXMAUI;
using PKHeX.Core;
using PKHeX.Core.Injection;
using static MainPage;

public partial class LiveHex : ContentPage
{
	public LiveHex()
	{
		InitializeComponent();
		Port.Text = sav.Generation > 7 ? "6000" : "8000";
		var validvers = RamOffsets.GetValidVersions(sav);
		var com = RamOffsets.GetCommunicator(sav, InjectorCommunicationType.SocketNetwork);
		Remote = new PokeSysBotMini(validvers[0], com, false);
	}
	public static PokeSysBotMini Remote;

    private async void botbaseconnect(object sender, EventArgs e)
    {
        IEnumerable<ConnectionProfile> profiles = Connectivity.Current.ConnectionProfiles;
		if (!profiles.Contains(ConnectionProfile.WiFi))
		{
			await DisplayAlert("WiFi", "Please Connect to WiFi", "ok");
			return;
		}
        Remote.com.IP = IP.Text;
		Remote.com.Port = int.Parse(Port.Text);
		Remote.com.Connect();
    }
}