
using Microsoft.Maui.Graphics.Text;
using PKHeX.Core;
using System.Collections.ObjectModel;
namespace PKHeXMAUI;

public partial class MailBox : ContentPage
{
    private readonly MailDetail[] m = null!;
    private readonly int[] MailItemID = null!;
    private readonly Label[] PKMLabels, PKMHeldItems;
    private readonly NumericUpDown[] PKMNUDs, Miscs;
    private readonly NumericUpDown[][] Messages;
    private readonly comboBox[] AppearPKMs;
    private readonly int PartyBoxCount;
    private ObservableCollection<string> PartyBoxList = [];
    private ObservableCollection<string> PCBoxList = [];
    private List<string> MailItemIDs = [];
    private readonly byte Generation;
    private readonly byte ResetVer, ResetLang;
    private readonly IList<PKM> p;
    private int entry;
    private readonly SaveFile SAV;
    private readonly SaveFile Origin;
    private string loadedLBItemLabel = null!;
    private bool LabelValue_GenderF;
    private readonly string[] gendersymbols = ["gender_0.png", "gender_1.png"];
    public MailBox(SaveFile sav)
	{
		InitializeComponent();
        SAV = (Origin = sav).Clone();
        Generation = sav.Generation;
        p = sav.PartyData;
        Messages =
        [
            [NUD_Message00, NUD_Message01, NUD_Message02, NUD_Message03],
            [NUD_Message10, NUD_Message11, NUD_Message12, NUD_Message13],
            [NUD_Message20, NUD_Message21, NUD_Message22, NUD_Message23],
        ];
        Miscs = [NUD_Misc1, NUD_Misc2, NUD_Misc3];
        PKMLabels = [L_PKM1, L_PKM2, L_PKM3, L_PKM4, L_PKM5, L_PKM6];
        PKMHeldItems = [L_HeldItem1, L_HeldItem2, L_HeldItem3, L_HeldItem4, L_HeldItem5, L_HeldItem6];
        PKMNUDs = [NUD_MailID1, NUD_MailID2, NUD_MailID3, NUD_MailID4, NUD_MailID5, NUD_MailID6];
        AppearPKMs = [CB_AppearPKM1, CB_AppearPKM2, CB_AppearPKM3];
        CV_PartyHeld.ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid();
			Label lb = new();
			lb.SetBinding(Label.TextProperty, new Binding("."));
			grid.Add(lb);
			return grid;
		});
        CV_PCBox.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid();
            Label lb = new();
            lb.SetBinding(Label.TextProperty, new Binding("."));
            grid.Add(lb);
            return grid;
        });
        NUD_BoxSize.IsVisible = L_BoxSize.IsVisible = CHK_UserEntered.IsVisible = L_UserEntered.IsVisible = Generation == 2;
        GB_MessageTB.IsVisible = Generation == 2;
        GB_MessageNUD.IsVisible = Generation != 2;
        Messages[0][3].IsVisible = Messages[1][3].IsVisible = Messages[2][3].IsVisible = Generation is 4 or 5;
        NUD_AuthorSID.IsVisible = Generation != 2;
        L_OTGender.IsVisible = CB_AuthorVersion.IsVisible = Generation is 4 or 5;
        L_AppearPKM.IsVisible = AppearPKMs[0].IsVisible = Generation != 5;
        AppearPKMs[1].IsVisible = AppearPKMs[2].IsVisible = Generation == 4;
        NUD_MessageEnding.IsVisible = Generation == 5;
        L_MiscValue.IsVisible = NUD_Misc1.IsVisible = NUD_Misc2.IsVisible = NUD_Misc3.IsVisible = Generation == 5;
        GB_PKM.IsVisible = SAV is not SAV2Stadium;
        for (int i = p.Count; i < 6; i++)
            PKMNUDs[i].IsVisible = PKMLabels[i].IsVisible = PKMHeldItems[i].IsVisible = false;
        if (Generation != 3)
        {
            for (int i = 0; i < PKMNUDs.Length; i++)
            {
                PKMNUDs[i].Number = i;
                PKMNUDs[i].IsEnabled = true;
            }
        }
        switch (sav)
		{
			case SAV2 sav2:
                m = new Mail2[6 + 10];
                for (int i = 0; i < m.Length; i++)
                    m[i] = new Mail2(sav2, i);
                NUD_BoxSize.Number = sav.Data[Mail2.GetMailboxOffset(sav.Language)];
                MailItemID = [0x9E, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD];
                PartyBoxCount = 6;
                break;
            case SAV2Stadium sav2Stadium:
                m = new Mail2[SAV2Stadium.MailboxHeldMailCount + SAV2Stadium.MailboxMailCount];
                for (int i = 0; i < m.Length; i++)
                    m[i] = new Mail2(sav2Stadium, i);

                NUD_BoxSize.MaxValue = SAV2Stadium.MailboxMailCount;
                NUD_BoxSize.Number = Math.Min(NUD_BoxSize.MaxValue, SAV.Data[Mail2.GetMailboxOffsetStadium2(SAV.Language)]);
                MailItemID = [0x9E, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD];
                PartyBoxCount = SAV2Stadium.MailboxHeldMailCount;
                break;
            case SAV3 sav3:
                m = new Mail3[6 + 10];
                for (int i = 0; i < m.Length; i++)
                    m[i] = sav3.LargeBlock.GetMail(i);

                MailItemID = [121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132];
                PartyBoxCount = 6;
                break;
            case SAV4 sav4:
                m = new Mail4[p.Count + 20];
                for (int i = 0; i < p.Count; i++)
                    m[i] = new Mail4(((PK4)p[i]).HeldMail.ToArray());
                for (int i = p.Count, j = 0; i < m.Length; i++, j++)
                    m[i] = sav4.GetMail(j);
                var l4 = (Mail4)m[^1];
                ResetVer = l4.AuthorVersion;
                ResetLang = l4.AuthorLanguage;
                MailItemID = [137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148];
                PartyBoxCount = p.Count;
                break;
            case SAV5 sav5:
                m = new Mail5[p.Count + 20];
                for (int i = 0; i < p.Count; i++)
                    m[i] = new Mail5(((PK5)p[i]).HeldMail.ToArray());
                for (int i = p.Count, j = 0; i < m.Length; i++, j++)
                    m[i] = sav5.GetMail(j);
                var l5 = (Mail5)m[^1];
                ResetVer = l5.AuthorVersion;
                ResetLang = l5.AuthorLanguage;
                MailItemID = [137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148];
                PartyBoxCount = p.Count;
                break;
        }
        for (int i = 0; i < PartyBoxCount; i++)
            PartyBoxList.Add(GetLBLabel(i));
        if (Generation == 2)
        {
            for (int i = PartyBoxCount, j = 0, boxsize = (int)NUD_BoxSize.Number; i < m.Length; i++, j++)
            {
                if (j < boxsize)
                    PCBoxList.Add(GetLBLabel(i));
            }
        }
        else
        {
            for (int i = PartyBoxCount; i < m.Length; i++)
                PCBoxList.Add(GetLBLabel(i));
        }
        CV_PCBox.ItemsSource = PCBoxList;
        CV_PartyHeld.ItemsSource = PartyBoxList;
        for (int i = 0; i < p.Count; i++)
        {
            PKMLabels[i].Text = GetSpeciesNameFromCB(p[i].Species);
            int j = Array.IndexOf(MailItemID, p[i].HeldItem);
            PKMHeldItems[i].Text = j >= 0 ? GameInfo.Strings.GetItemStrings(sav.Context, sav.Version)[j + 1]! : "(not Mail)";
            if (Generation != 3)
                continue;
            int k = ((PK3)p[i]).HeldMailID;
            PKMNUDs[i].Number = k is >= -1 and <= 5 ? k : -1;
        }
        if (Generation is 2 or 3)
        {
            CB_AppearPKM1.ItemSource = GameInfo.FilteredSources.Species.ToList();
            CB_AppearPKM1.DisplayMemberPath = "Text";
        }
        else if (Generation is 4 or 5)
        {
            var species = GameInfo.FilteredSources.Species.ToList();
            foreach (comboBox a in AppearPKMs)
            {
                a.ItemSource = species;
            }

            var vers = GameInfo.Sources.VersionDataSource
                .Where(z => ((GameVersion)z.Value).Generation == Generation);
            CB_AuthorVersion.ItemSource = vers.ToList();
            CB_AuthorVersion.DisplayMemberPath = "Text";
        }
        AuthorLang.ItemsSource = GameInfo.LanguageDataSource(sav.Generation, sav.Context).ToList();
        AuthorLang.ItemDisplayBinding = new Binding("Text");
        var ItemList = GameInfo.Strings.GetItemStrings(sav.Context, sav.Version);
        MailItemIDs.Add(ItemList[0]);
        foreach(var item in MailItemID)
            MailItemIDs.Add(ItemList[item]);
        MailTypePicker.ItemsSource = MailItemIDs;
        entry = -1;
        if (PartyBoxList.Count > 0)
            CV_PartyHeld.SelectedItem = PartyBoxList[0];
    }

    private void EntryControl(object sender, SelectionChangedEventArgs e)
    {
        int partyindex = Array.IndexOf([.. PartyBoxList], CV_PartyHeld.SelectedItem);
        int pcboxindex = Array.IndexOf([.. PCBoxList], CV_PCBox.SelectedItem);
        if(entry >= 0)
        {
            TempSave();
            if (GetLBLabel(entry) != loadedLBItemLabel)
                LoadList();
        }
        if (sender == CV_PartyHeld && partyindex >= 0)
        {
            entry = partyindex;
            CV_PCBox.SelectedItem = null;
        }
        else if (sender == CV_PCBox && pcboxindex >= 0)
        {
            entry = PartyBoxCount + pcboxindex;
            CV_PartyHeld.SelectedItem = null;
        }
        else
        {
            entry = -1;
        }

        if (entry >= 0)
        {
            LoadMail();
            loadedLBItemLabel = GetLBLabel(entry);
        }
    }
    private void LoadMail()
    {
        MailDetail mail = m[entry];
        AuthorOT.Text = mail.AuthorName;
        NUD_AuthorTID.Number = mail.AuthorTID;
        AuthorLang.SelectedItem = GameInfo.LanguageDataSource(SAV.Generation, SAV.Context).FirstOrDefault(z=>z.Value == (int)mail.AuthorLanguage);
        MailTypePicker.SelectedIndex = MailTypeToCBIndex(mail);
        var species = mail.AppearPKM;
        if (Generation == 2)
        {
            CB_AppearPKM1.SelectedItem = GameInfo.FilteredSources.Species.FirstOrDefault(z=>z.Value == (int)species);
            Message1.Text = mail.GetMessage(false);
            Message2.Text = mail.GetMessage(true);
            AuthorLang.IsEnabled = AuthorLang.SelectedItem is not (int)LanguageID.Japanese and not (int)LanguageID.Korean;
            CHK_UserEntered.IsChecked = mail.UserEntered;
            return;
        }
        NUD_AuthorSID.Number = mail.AuthorSID;
        for (int y = 0, xc = Generation == 3 ? 3 : 4; y < 3; y++)
        {
            for (int x = 0; x < xc; x++)
                Messages[y][x].Number = mail.GetMessage(y, x);
        }
        if (Generation == 3)
        {
            AppearPKMs[0].SelectedIndex = SpeciesConverter.GetNational3(species);
            return;
        }
        CB_AuthorVersion.SelectedItem = CB_AuthorVersion.ItemSource.Cast<ComboItem>().FirstOrDefault(z=>z.Value==(int)mail.AuthorVersion);
        LabelValue_GenderF = (mail.AuthorGender & 1) != 0;
        LoadOTlabel();
        switch (mail)
        {
            case Mail4 m4:
                for (int i = 0; i < AppearPKMs.Length; i++)
                    AppearPKMs[i].SelectedItem = AppearPKMs[i].ItemSource.Cast<ComboItem>().FirstOrDefault(z=>z.Value== Math.Max(0, m4.GetAppearSpecies(i) - 7));
                break;
            case Mail5 m5:
                for (int i = 0; i < Miscs.Length; i++)
                    Miscs[i].Number = m5.GetMisc(i);
                NUD_MessageEnding.Number = m5.MessageEnding;
                break;
        }
    }
    private string GetLBLabel(int index) => m[index].IsEmpty != true ? $"{index}: From {m[index].AuthorName}" : $"{index}:  (empty)";
    private string GetSpeciesNameFromCB(int index)
    {
        var result = GameInfo.FilteredSources.Species.FirstOrDefault(z => z.Value == index);
        return result != null ? result.Text : "PKM";
    }
    private int MailTypeToCBIndex(MailDetail mail) => Generation <= 3 ? 1 + Array.IndexOf(MailItemID, mail.MailType) : (mail.IsEmpty == false ? 1 + mail.MailType : 0);
    private int CBIndexToMailType(int cbindex) => Generation <= 3 ? (cbindex > 0 ? MailItemID[cbindex - 1] : 0) : (cbindex > 0 ? cbindex - 1 : 0xFF);
    private void TempSave()
    {
        MailDetail mail = m[entry];
        mail.AuthorName = AuthorOT.Text;
        mail.AuthorTID = (ushort)NUD_AuthorTID.Number;
        mail.AuthorLanguage = (byte)((int?)AuthorLang.SelectedIndex ?? (int)LanguageID.English);
        mail.MailType = CBIndexToMailType(MailTypePicker.SelectedIndex);
        var species = (ushort?)((ComboItem?)CB_AppearPKM1.SelectedItem)?.Value;
        if (Generation == 2)
        {
            mail.AppearPKM = species??0;
            mail.SetMessage(Message1.Text, Message2.Text, CHK_UserEntered.IsChecked);
            return;
        }
        mail.AuthorSID = (ushort)NUD_AuthorSID.Number;
        for (int y = 0, xc = Generation == 3 ? 3 : 4; y < 3; y++)
        {
            for (int x = 0; x < xc; x++)
                mail.SetMessage(y, x, (ushort)Messages[y][x].Number);
        }
        if (Generation == 3)
        {
            mail.AppearPKM = SpeciesConverter.GetInternal3(species??0);
            return;
        }

        mail.AuthorVersion = (byte)((int?)CB_AuthorVersion.SelectedIndex ?? 0);

        mail.AuthorGender = (byte)((mail.AuthorGender & 0xFE) | (LabelValue_GenderF ? 1 : 0));
        switch (mail)
        {
            case Mail4 m4:
                for (int i = 0; i < AppearPKMs.Length; i++)
                {
                    var index = AppearPKMs[i].SelectedIndex;
                    if (index == -1)
                        index = 0;
                    else
                        index += 7;
                    m4.SetAppearSpecies(i, (ushort)index);
                }

                break;
            case Mail5 m5:
               for (int i = 0; i < Miscs.Length; i++)
                  m5.SetMisc(i, (ushort)Miscs[i].Number);
               m5.MessageEnding = (ushort)NUD_MessageEnding.Number;
               break;
        }
    }
    private void LoadList()
    {
        if (entry < PartyBoxCount) MakePartyList();
        else MakePCList();
    }
    private void LoadOTlabel()
    {
        L_OTGender.Source = gendersymbols[LabelValue_GenderF ? 1 : 0];
        L_OTGender.BorderColor = GetGenderColor((byte)(LabelValue_GenderF ? 1 : 0));
    }
    public Color GetGenderColor(byte gender) => gender switch
    {
        0 => Colors.Blue,
        1 => Colors.Red,
        _ => Colors.White,
    };
    private void L_OTGender_Click(object sender, EventArgs e)
    {
        LabelValue_GenderF ^= true;
        LoadOTlabel();
    }
    private void MBServedValueChanged(object sender, EventArgs e) => MakePCList();

    private void MakePartyList()
    {
        PartyBoxList.Clear();
        for (int i = 0; i < PartyBoxCount; i++)
            PartyBoxList.Add(GetLBLabel(i));
    }

    private void close(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void MakePCList()
    {
        PCBoxList.Clear();
        if (Generation == 2)
        {
          
            for (int i = PartyBoxCount, j = 0, boxsize = (int)NUD_BoxSize.Number; i < m.Length; i++, j++)
            {
                if (j < boxsize)
                    PCBoxList.Add(GetLBLabel(i));
            }
            
        }
        else
        {
            for (int i = PartyBoxCount; i < m.Length; i++)
                PCBoxList.Add(GetLBLabel(i));
        }
    }
    private void Save()
    {
        switch (Generation)
        {
            case 2:
                foreach (var n in m) n.CopyTo(SAV);
                if (SAV is SAV2)
                {
                    // duplicate
                    int ofs = 0x600;
                    int len = Mail2.GetMailSize(SAV.Language) * 6;
                    Array.Copy(SAV.Data.ToArray(), ofs, SAV.Data.ToArray(), ofs + len, len);
                    ofs += len << 1;
                    SAV.Data[ofs] = (byte)NUD_BoxSize.Number;
                    len = (Mail2.GetMailSize(SAV.Language) * 10) + 1;
                    Array.Copy(SAV.Data.ToArray(), ofs, SAV.Data.ToArray(), ofs + len, len);
                }
                else if (SAV is SAV2Stadium)
                {
                    int ofs = Mail2.GetMailboxOffsetStadium2(SAV.Language);
                    SAV.Data[ofs] = (byte)NUD_BoxSize.Number;
                }
                break;
            case 3:
                foreach (var n in m) n.CopyTo(SAV);
                break;
            case 4:
                for (int i = 0; i < p.Count; i++)
                    m[i].CopyTo((PK4)p[i]);
                for (int i = p.Count; i < m.Length; i++)
                    m[i].CopyTo(SAV);
                break;
            case 5:
                for (int i = 0; i < p.Count; i++)
                    m[i].CopyTo((PK5)p[i]);
                for (int i = p.Count; i < m.Length; i++)
                    m[i].CopyTo(SAV);
                break;
        }
        if (p.Count > 0)
            SAV.PartyData = p;
    }
    private async void B_Save_Clicked(object sender, EventArgs e)
    {
        if (entry >= 0) TempSave();
        Save();
        var Err = CheckValid();
        if (Err.Count != 0 && await DisplayAlertAsync("Invalid", $"{Err.Aggregate($"Validation Error. Save?{Environment.NewLine}", (tmp, v) => $"{tmp}{Environment.NewLine}{v}")}","yes","no"))
            return;
        Origin.CopyChangesFrom(SAV);
        Navigation.PopModalAsync();
    }
    private List<string> CheckValid()
    {
        var ret = new List<string>();
        // Gen3
        // A: held item is mail, but heldMailID is not 0 to 5. it should be 0 to 5, or held not mail.
        // B: held item is mail, but mail is empty(mail type is 0). it should be not empty, or held not mail and heldMailId -1.
        // C: held item is not mail, but heldMailID is not -1. it should be -1, or held mail and mail not empty.
        // D: other pk have same heldMailID. it should be different.
        // E: mail is not empty, but no pk refer to the mail. it should be empty, or someone refer to the mail.
        if (Generation == 3)
        {
            Span<int> heldMailIDs = stackalloc int[p.Count];
            for (int i = 0; i < p.Count; i++)
            {
                int h = ((PK3)p[i]).HeldMailID;
                heldMailIDs[i] = h;
                if (ItemIsMail(p[i].HeldItem))
                {
                    if (h is < 0 or > 5) //A
                        ret.Add($"Party#{i + 1} MailID mismatch");
                    else if (m[h].IsEmpty == true) //B
                        ret.Add($"Party#{i + 1} MailID mismatch");
                }
                else if (h != -1) //C
                {
                    ret.Add($"Party#{i + 1} MailID mismatch");
                }
            }
            for (int i = 0; i < 6; i++)
            {
                var count = heldMailIDs.Count(i);
                if (count > 1) //D
                    ret.Add($"MailID{i} duplicated");
                if (m[i].IsEmpty == false && count == 0) //E
                    ret.Add($"MailID{i} not referred");
            }
        }
        // Gen2, Gen4
        // P: held item is mail, but mail is empty(invalid mail type. g2:not 181 to 189, g4:12 to 255). it should be not empty or held not mail.
        // Q: held item is not mail, but mail is not empty. it should be empty or held mail.
        else if (Generation is 2 or 4)
        {
            for (int i = 0; i < p.Count; i++)
            {
                if (ItemIsMail(p[i].HeldItem))
                {
                    if (m[i].IsEmpty == true) //P
                        ret.Add($"MailID{i} MailType mismatch");
                }
                else if (m[i].IsEmpty == false) //Q
                {
                    ret.Add($"MailID{i} MailType mismatch");
                }
            }
        }
        // Gen5
        // P
        // Gen5, move mail to pc will not erase mail data, still remains, duplicates.
        else if (Generation == 5)
        {
            for (int i = 0; i < p.Count; i++)
            {
                if (ItemIsMail(p[i].HeldItem))
                {
                    if (m[i].IsEmpty == true) //P
                        ret.Add($"MailID{i} MailType mismatch");
                }
            }
        }
        // Gen*
        // Z: mail type is illegal
        for (int i = 0; i < m.Length; i++)
        {
            if (m[i].IsEmpty is null) // Z
                ret.Add($"MailID{i} MailType mismatch");
        }

        return ret;
    }
    private bool ItemIsMail(int itemID) => Array.IndexOf(MailItemID, itemID) >= 0;
}