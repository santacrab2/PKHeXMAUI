namespace PKHeXMAUI;

using PKHeX.Core;
using PKHeX.Core.Injection;
using static MainPage;

public partial class LiveHex : ContentPage
{
    public static bool SkipTextChanges = false;
    public static bool Reconnect = false;
    public LiveHex()
	{
		InitializeComponent();
		Port.Text = sav.Generation > 7 ? "6000" : "8000";
		var validvers = RamOffsets.GetValidVersions(sav);
        ICommunicator com = RamOffsets.IsSwitchTitle(sav) ? new SysBotMini() : new NTRClient();
		Remote = new PokeSysBotMini(validvers[^1], com, false);
        var ip = Preferences.Default.Get("IP", "192.168.1.1");
        SkipTextChanges = true;
        IP.Text = ip;
        CB_InjectinSlot.IsChecked = InjectinSlot;
        CB_ReadChangeBox.IsChecked = ReadonChangeBox;
        SkipTextChanges = false;
    }

    private async void botbaseconnect(object sender, EventArgs e)
    {
        IEnumerable<ConnectionProfile> profiles = Connectivity.Current.ConnectionProfiles;
		if (!profiles.Contains(ConnectionProfile.WiFi))
		{
			await DisplayAlert("WiFi", "Please Connect to WiFi", "ok");
			return;
		}
        if (Remote.Connected)
        {
            Remote.com.Disconnect();
            connect.Text = "Connect";
            return;
        }
        Remote.com.IP = IP.Text;
		Remote.com.Port = int.Parse(Port.Text);
		Remote.com.Connect();
        if (Remote.com is ICommunicatorNX nx)
        {
            var titleid = nx.GetTitleID();
            var gameVer = nx.GetGameInfo("version").Trim();
            var compatible = InjectionBase.SaveCompatibleWithTitle(sav, titleid);
            if (!compatible)
            {
                await DisplayAlert("Error", "Invalid Version Detected. Could not connect to device", "disconnect");
                Remote.com.Disconnect();
                return;
            }
            var lv = InjectionBase.GetVersionFromTitle(titleid, gameVer);
            Remote = new PokeSysBotMini(lv, Remote.com, false);
            if (!IsPKMDataValid(sav.GetDecryptedPKM(Remote.ReadSlot(0, 0))))
            {
                if (InjectionBase.CheckRAMShift(Remote, out var errorMessage))
                {
                    await DisplayAlert("Error", errorMessage, "disconnect");
                }
                else
                {
                    await DisplayAlert("Error", "Error Connecting", "disconnect");
                }
                await DisplayAlert("Error", "Invalid Version Detected. Could not connect to device", "disconnect");
                Remote.com.Disconnect();
                return;
            }
        }
        else
        {
            bool valid = false;
            foreach(var version in RamOffsets.GetValidVersions(sav).Reverse().ToArray())
            {
                try
                {
                    Remote = new PokeSysBotMini(version, Remote.com, false);
                    var data = sav.GetDecryptedPKM(Remote.ReadSlot(0, 0));
                    valid = IsPKMDataValid(data);
                    if (valid)
                    {
                        Remote = new PokeSysBotMini(version, Remote.com, false);
                        break;
                    }
                }
                catch(Exception) { }
                   

            }
            if (!valid)
            {
                await DisplayAlert("Error", "Invalid Version Detected. Could not connect to device", "disconnect");
                Remote.com.Disconnect();
                return;
            }
        }
        SetTrainerData(sav);
        RecentTrainerCache.SetRecentTrainer(sav);
        Reconnect = true;
        connect.Text = "Disconnect";
    }
    private bool IsPKMDataValid(PKM data)
    {
        if(data.Species > data.MaxSpeciesID)
            return false;
        if (!data.ChecksumValid)
            return false;
        if (data.Species == 0 && data.EncryptionConstant != 0)
            return false;
        if (data.Species > 0 && (LanguageID)data.Language is LanguageID.Hacked or LanguageID.UNUSED_6)
            return false;
        return true;
    }
    private void SaveUserIP(object sender, TextChangedEventArgs e)
    {
        if (!SkipTextChanges && IP.Text.Length > 0)
            Preferences.Set("IP", IP.Text);
    }

    private async void inject(object sender, EventArgs e)
    {
        if (Remote.Connected)
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
            Remote.SendSlot(pk.EncryptedBoxData, box - 1, slot - 1);
        }
    }

    private async void read(object sender, EventArgs e)
    {
        if (Remote.Connected)
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
            pk = EntityFormat.GetFromBytes(Remote.ReadSlot(box - 1, slot - 1));
        }
    }

    private async void B_ReadFromOff_Click(object sender, EventArgs e)
    {
        if (Remote.Connected)
        {
            if (offset.Text.StartsWith('['))
            {
                if (Remote.com is not ICommunicatorNX sb)
                {
                    await DisplayAlert("Error", "Error", "cancel");
                    return;
                }

                ulong address = GetPointerAddress(sb);
                if (address == 0)
                {
                    await DisplayAlert("Error", "No pointer address.", "cancel");
                    return;
                }

                var size = Remote.SlotSize;
                var data = sb.ReadBytesAbsolute(address, size);
                var pkm = sav.GetDecryptedPKM(data);

                // Since data might not actually exist at the user-specified offset, double check that the pkm data is valid.
                if (pkm.ChecksumValid)
                    pk = pkm;
            }
            else
            {
                var txt = offset.Text;
                var off = Util.GetHexValue64(txt);
                if (off.ToString("X16") != txt.ToUpper().PadLeft(16, '0'))
                {
                    await DisplayAlert("Error", "Specified offset is not a valid hex string.", "cancel");
                    return;
                }
                try
                {
                    const RWMethod method = RWMethod.Heap;

                    var result = ReadOffset(off, method);
                    if (!result)
                        await DisplayAlert("Error", "No valid data is located at the specified offset.", "cancel");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Unable to load data from the specified offset. {ex.Message}", "cancel"); 
                }
            }
        }
    }
    public bool ReadOffset(ulong offset, RWMethod method = RWMethod.Heap)
    {
        var data = ReadData(offset, method);
        var pkm = sav.GetDecryptedPKM(data);

        // Since data might not actually exist at the user-specified offset, double check that the pkm data is valid.
        if (!pkm.ChecksumValid)
            return false;

        pk = pkm;
        return true;
    }
    private byte[] ReadData(ulong offset, RWMethod method)
    {
        if (Remote.com is not ICommunicatorNX nx)
            return Remote.ReadOffset(offset);
        return method switch
        {
            RWMethod.Heap => Remote.ReadOffset(offset),
            RWMethod.Main => nx.ReadBytesMain(offset, Remote.SlotSize),
            RWMethod.Absolute => nx.ReadBytesAbsolute(offset, Remote.SlotSize),
            _ => Remote.ReadOffset(offset),
        };
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
        if (Remote.Connected)
        {
            var box = BoxTab.CurrentBox;
            var len =
                   sav.BoxSlotCount
                   * (RamOffsets.GetSlotSize(Remote.Version) + RamOffsets.GetGapSize(Remote.Version));
            var data = Remote.ReadBox(box, len).AsSpan();
            sav.SetBoxBinary(data, box);
        }
    }

    private void B_WriteCurrentBox_Click(object sender, EventArgs e)
    {
        if (Remote.Connected)
        {
            var boxData = sav.GetBoxBinary(BoxTab.CurrentBox);
            Remote.SendBox(boxData, BoxTab.CurrentBox);
        }
    }
    public ulong GetPointerAddress(ICommunicatorNX sb)
    {
        var ptr = offset.Text.Contains("[key]")
            ? offset.Text.Replace("[key]", "").Trim()
            : offset.Text.Trim();
        var address = Remote.GetCachedPointer(sb, ptr, false);
        return address;
    }
    private void SetTrainerData(SaveFile sav)
    {
        // Check and set trainerdata based on ISaveBlock interfaces
        byte[] dest;
        int startofs = 0;

        Func<PokeSysBotMini, byte[]?> tdata;
        switch (sav)
        {
            case ISaveBlock8SWSH s8:
                dest = s8.MyStatus.Data;
                startofs = s8.MyStatus.Offset;
                tdata = LPBasic.GetTrainerData;
                break;

            case ISaveBlock7Main s7:
                dest = s7.MyStatus.Data;
                startofs = s7.MyStatus.Offset;
                tdata = LPBasic.GetTrainerData;
                break;

            case ISaveBlock6Main s6:
                dest = s6.Status.Data;
                startofs = s6.Status.Offset;
                tdata = LPBasic.GetTrainerData;
                break;

            case SAV7b slgpe:
                dest = slgpe.Blocks.Status.Data;
                startofs = slgpe.Blocks.Status.Offset;
                tdata = LPLGPE.GetTrainerData;
                break;

            case SAV8BS sbdsp:
                dest = sbdsp.MyStatus.Data;
                startofs = sbdsp.MyStatus.Offset;
                tdata = LPBDSP.GetTrainerData;
                break;

            case SAV8LA sbla:
                dest = sbla.MyStatus.Data;
                startofs = sbla.MyStatus.Offset;
                tdata = LPPointer.GetTrainerDataLA;
                break;

            case SAV9SV s9sv:
                dest = s9sv.MyStatus.Data;
                startofs = s9sv.MyStatus.Offset;
                tdata = LPPointer.GetTrainerDataSV;
                break;

            default:
                dest = [];
                tdata = LPBasic.GetTrainerData;
                break;
        }

        if (dest.Length == 0)
        {
            return;
        }

        var data = tdata(Remote);
        if (data is null)
        {
            return;
        }

        data.CopyTo(dest, startofs);
    }
}