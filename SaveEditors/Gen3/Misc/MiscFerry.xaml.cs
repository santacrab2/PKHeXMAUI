using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscFerry : ContentPage
{
    public SAV3E SAV;
    private const ushort ItemIDOldSeaMap = 0x178;
    private static readonly ushort[] TicketItemIDs = [0x109, 0x113, 0x172, 0x173, ItemIDOldSeaMap]; // item IDs
    public MiscFerry(SAV3E sav)
	{
		InitializeComponent();
        SAV = sav;
        cangetrideCheck.IsChecked = SAV.GetEventFlag(0x864);
        RsouthernCheck.IsChecked = SAV.GetEventFlag(0x8B3);
        RbirthislandCheck.IsChecked = SAV.GetEventFlag(0x8D5);
        RfarawayislandCheck.IsChecked = SAV.GetEventFlag(0x8D6);
        RnavelrockCheck.IsChecked = SAV.GetEventFlag(0x8E0);
        // 0x1D0 是「对战开拓区」标志，此前误写到了诞生之岛，导致对战开拓区永远读不到、诞生之岛读错
        RbattlefrontierCheck.IsChecked = SAV.GetEventFlag(0x1D0);
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
        if (!hasOldSea && !SAV.Japanese && await DisplayAlertAsync(L10n.T("code_non_japanese_save"), L10n.T("code_non_japanese_save_add", itemlist[ItemIDOldSeaMap]),L10n.T("pktab_ok"), L10n.T("box_cancel")))
            tickets = tickets[..^1]; // remove old sea map

        // check for missing tickets
        ushort[] have = new ushort[tickets.Length]; int h = 0;
        ushort[] missing = new ushort[tickets.Length]; int m = 0;
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
            DisplayAlertAsync(L10n.T("gen3_tickets"),L10n.T("gen3_you_already_have"),L10n.T("pktab_ok"));
            // 不要把按钮永久禁用：道具被删掉/换档之后还要能再点
            return;
        }

        // check for space
        int end = Array.FindIndex(p.Items, static z => z.Index == 0);
        // end == -1 表示没有空位；end + missing.Length > 容量才是真的放不下（刚好放满是可用的）
        if (end == -1 || end + missing.Length > p.Items.Length)
        {
            DisplayAlertAsync(L10n.T("gen3_not_enough_bag"), L10n.T("gen3_please_use_the"),L10n.T("pktab_ok"));
            // 不要把按钮永久禁用：清出空位之后还要能再点
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
        var addmsg = L10n.T("code_add_the_following_items", Environment.NewLine, added);
        if (have.Length != 0)
        {
            string had = Format(have, itemlist);
            var havemsg = L10n.T("code_already_own_environment_newline", Environment.NewLine, had);
            addmsg += Environment.NewLine + Environment.NewLine + havemsg;
        }
        // 提示里「是」表示确认添加，「否」表示放弃
        if (!await DisplayAlertAsync("", addmsg, L10n.T("pktab_ok"), L10n.T("box_cancel")))
            return;

        // insert items at the end
        for (int i = 0; i < missing.Length; i++)
        {
            var item = p.Items[end + i];
            item.Index = missing[i];
            item.Count = 1;
        }

        string alert = L10n.T("code_added_to_the_key", Environment.NewLine, added);
        DisplayAlertAsync("",alert,L10n.T("pktab_ok"));
        Pouches.CopyTo(SAV);

        // 按钮保持可点：随时可以再点一次补齐缺的船票
    }
}