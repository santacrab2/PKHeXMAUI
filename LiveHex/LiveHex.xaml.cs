namespace PKHeXMAUI;
using PKHeX.Core;
using PKHeX.Core.Injection;
using static MainPage;

public partial class LiveHex : ContentPage
{
    public static bool SkipTextChanges = false;
    public LiveHex()
	{
		InitializeComponent();
		Port.Text = sav.Generation > 7 ? "6000" : "8000";
		var validvers = RamOffsets.GetValidVersions(sav);
        ICommunicator com = RamOffsets.IsSwitchTitle(sav) ? new SysBotMini() : new NTRClient();
		Remote = new PokeSysBotMini(validvers[validvers.Length-1], com, false);
        var ip = Preferences.Default.Get("IP", "192.168.1.1");
        SkipTextChanges = true;
        IP.Text = ip;
        CB_InjectinSlot.IsChecked = InjectinSlot;
        CB_ReadChangeBox.IsChecked = ReadonChangeBox;
        SkipTextChanges = false;
    }
	public static PokeSysBotMini Remote;
    public static bool ReadonChangeBox = Preferences.Get("ReadonChangeBox", true);
    public static bool InjectinSlot = Preferences.Get("InjectinSlot", true);
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
        connect.Text = "Disconnect";
    }

 
    private void SaveUserIP(object sender, TextChangedEventArgs e)
    {
        if (!SkipTextChanges && IP.Text.Length > 0)
            Preferences.Set("IP", IP.Text);
    }

    private async void inject(object sender, EventArgs e)
    {
        if (!int.TryParse(boxnum.Text, out var box))
        {
            await DisplayAlert("Invalid", "Invalid Box number", "cancel");
            return;
        }
        if(!int.TryParse(slotnum.Text,out var slot))
        {
            await DisplayAlert("Invalid", "Invalid Slot number", "cancel");
            return;
        }
        Remote.SendSlot(pk.EncryptedBoxData, box-1, slot-1);
    }

    private async void read(object sender, EventArgs e)
    {
        if (!int.TryParse(boxnum.Text, out var box))
        {
            await DisplayAlert("Invalid", "Invalid Box number", "cancel");
            return;
        }
        if (!int.TryParse(slotnum.Text, out var slot))
        {
            await DisplayAlert("Invalid", "Invalid Slot number", "cancel");
            return;
        }
        pk = EntityFormat.GetFromBytes(Remote.ReadSlot(box-1, slot-1));
    }

    private void B_ReadFromOff_Click(object sender, EventArgs e)
    {
        
    }

    private void CB_ReadChangeBox_Check(object sender, CheckedChangedEventArgs e)
    {
        if(!SkipTextChanges)
            Preferences.Set("ReadonChangeBox", CB_ReadChangeBox.IsChecked);
    }

    private void CB_InjectinSlot_check(object sender, CheckedChangedEventArgs e)
    {
        if(!SkipTextChanges)
            Preferences.Set("InjectinSlot", CB_InjectinSlot.IsChecked);
    }

    private void B_ReadCurrentBox_Click(object sender, EventArgs e)
    {
        var box = BoxTab.CurrentBox;
        var len =
               sav.BoxSlotCount
               * (RamOffsets.GetSlotSize(Remote.Version) + RamOffsets.GetGapSize(Remote.Version));
        var data = Remote.ReadBox(box, len).AsSpan();
        sav.SetBoxBinary(data, box);
        
    }

    private void B_WriteCurrentBox_Click(object sender, EventArgs e)
    {
        var boxData = sav.GetBoxBinary(BoxTab.CurrentBox);
        Remote.SendBox(boxData, BoxTab.CurrentBox);
    }
}