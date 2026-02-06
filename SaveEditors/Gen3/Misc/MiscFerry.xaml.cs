using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscFerry : ContentPage
{
    public SAV3E SAV;
    private const ushort ItemIDOldSeaMap = 0x178;
    private static ReadOnlySpan<ushort> TicketItemIDs => [0x109, 0x113, 0x172, 0x173, ItemIDOldSeaMap]; // item IDs
    public MiscFerry(SAV3E sav)
	{
		InitializeComponent();
        SAV = sav;
        cangetrideCheck.IsChecked = SAV.GetEventFlag(0x864);
        RsouthernCheck.IsChecked = SAV.GetEventFlag(0x8B3);
        RbirthislandCheck.IsChecked = SAV.GetEventFlag(0x8D5);
        RfarawayislandCheck.IsChecked = SAV.GetEventFlag(0x8D6);
        RnavelrockCheck.IsChecked = SAV.GetEventFlag(0x8E0);
        RbirthislandCheck.IsChecked = SAV.GetEventFlag(0x1D0);
        IsoutherCheck.IsChecked = SAV.GetEventFlag(0x1AE);
        IbirthislandCheck.IsChecked = SAV.GetEventFlag(0x1AF);
        IfarawayislandCheck.IsChecked = SAV.GetEventFlag(0x1B0);
        InavelrockCheck.IsChecked = SAV.GetEventFlag(0x1DB);
    }
    public void SaveFerry()
    {
        SAV.SetEventFlag(0x864, cangetrideCheck.IsChecked);
        SAV.SetEventFlag(0x8B3, RsouthernCheck.IsChecked);
        SAV.SetEventFlag(0x8D5, RbirthislandCheck.IsChecked);
        SAV.SetEventFlag(0x8D6, RfarawayislandCheck.IsChecked);
        SAV.SetEventFlag(0x8E0, RnavelrockCheck.IsChecked);
        SAV.SetEventFlag(0x1D0, RbattlefrontierCheck.IsChecked);
        SAV.SetEventFlag(0x1AE, IsoutherCheck.IsChecked);
        SAV.SetEventFlag(0x1AF, IbirthislandCheck.IsChecked);
        SAV.SetEventFlag(0x1B0, IfarawayislandCheck.IsChecked);
        SAV.SetEventFlag(0x1DB, InavelrockCheck.IsChecked);
    }
    private async void UnlockTickets(object sender, EventArgs e)
    {
        var Pouches = SAV.Inventory;
        var itemlist = GameInfo.Strings.GetItemStrings(SAV.Context, SAV.Version);

        var tickets = TicketItemIDs;
        var p = Pouches.Pouches.FirstOrDefault(z => z.Type == InventoryType.KeyItems)??Pouches.Pouches[0];
        bool hasOldSea = Array.Exists(p.Items, static z => z.Index == ItemIDOldSeaMap);
        if (!hasOldSea && !SAV.Japanese && DisplayAlertAsync("Non Japanese Save", $"Non Japanese save file. Add {itemlist[ItemIDOldSeaMap]} (unreleased)?","Yes","No").Result)
            tickets = tickets[..^1]; // remove old sea map

        // check for missing tickets
        Span<ushort> have = stackalloc ushort[tickets.Length]; int h = 0;
        Span<ushort> missing = stackalloc ushort[tickets.Length]; int m = 0;
        foreach (var item in tickets)
        {
            bool has = Array.Exists(p.Items, z => z.Index == item);
            if (has)
                have[h++] = item;
            else
                missing[m++] = item;
        }
        have = have[..h];
        missing = missing[..m];

        if (missing.Length == 0)
        {
            DisplayAlertAsync("Tickets","Already have all tickets.","cancel");
            getticketsButton.IsEnabled = false;
            return;
        }

        // check for space
        int end = Array.FindIndex(p.Items, static z => z.Index == 0);
        if (end == -1 || end + missing.Length >= p.Items.Length)
        {
            DisplayAlertAsync("Not enough space in pouch.", "Please use the InventoryEditor.","canel");
            getticketsButton.IsEnabled = false;
            return;
        }

        static string Format(ReadOnlySpan<ushort> items, ReadOnlySpan<string> names)
        {
            var sbAdd = string.Empty;
            foreach (var item in items)
            {
                if (sbAdd.Length != 0)
                    sbAdd+=", ";
                sbAdd+=(names[item]);
            }
            return sbAdd;
        }
        var added = Format(missing, itemlist);
        var addmsg = $"Add the following items?{Environment.NewLine}{added}";
        if (have.Length != 0)
        {
            string had = Format(have, itemlist);
            var havemsg = $"Already have:{Environment.NewLine}{had}";
            addmsg += Environment.NewLine + Environment.NewLine + havemsg;
        }
        if (DisplayAlertAsync("", addmsg, "Yes", "No").Result)
            return;

        // insert items at the end
        for (int i = 0; i < missing.Length; i++)
        {
            var item = p.Items[end + i];
            item.Index = missing[i];
            item.Count = 1;
        }

        string alert = $"Inserted the following items to the Key Items Pouch:{Environment.NewLine}{added}";
        DisplayAlertAsync("",alert,"cancel");
        Pouches.CopyTo(SAV);

        getticketsButton.IsEnabled = false;
    }
}