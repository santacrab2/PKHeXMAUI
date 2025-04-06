using PKHeX.Core;
using System.Globalization;
using static System.Buffers.Binary.BinaryPrimitives;
namespace PKHeXMAUI;

public partial class MiscBattleFrontier4 : ContentPage
{
    private readonly SAV4 SAV;
    private readonly Hall4? Hall;
    private readonly RadioButton[] StatRBA;
    private readonly NumericUpDown[] StatNUDA;
    private readonly Label[] StatLabelA;
    private readonly NumericUpDown[] HallNUDA;
    private readonly Button[] PrintButtonA;
    private readonly int[][] BFF;
    private readonly int PrintIndexStart;

    private bool editing;
    private string[][] BFT = null!;
    private int[][] BFV = null!;
    public MiscBattleFrontier4(SAV4 sav)
	{
		InitializeComponent();
        SAV = (SAV4)(sav).Clone();
        StatNUDA = [NUD_Stat0, NUD_Stat1, NUD_Stat2, NUD_Stat3];
        StatLabelA = [L_Stat0, L_Stat1, L_Stat2, L_Stat3]; // Current, Trade, Record, Trade
        StatRBA = [RB_Stats3_01, RB_Stats3_02];
        HallNUDA =
        [
            NUD_HallType01, NUD_HallType02, NUD_HallType03, NUD_HallType04, NUD_HallType05, NUD_HallType06,
            NUD_HallType07, NUD_HallType08, NUD_HallType09, NUD_HallType10, NUD_HallType11, NUD_HallType12,
            NUD_HallType13, NUD_HallType14, NUD_HallType15, NUD_HallType16, NUD_HallType17,
        ];
        PrintButtonA = [BTN_PrintTower, BTN_PrintFactory, BTN_PrintHall, BTN_PrintCastle, BTN_PrintArcade];
        switch (sav)
        {
            case SAV4DP:
                GB_Prints.IsVisible = GB_Prints.IsEnabled = GB_Hall.IsVisible = GB_Hall.IsEnabled = GB_Castle.IsVisible = GB_Castle.IsEnabled = false;
                BFF = [
                    [0, 1, 0x5FCA, 0x04, 0x6601],
                ];
                break;
            case SAV4Pt:
                PrintIndexStart = 79;
                BFF = [
                    [0, 1, 0x68E0, 0x04, 0x723D],
                    [1, 0, 0x68F4, 0x10, 0x7EF8],
                    [0, 0, 0x6924, 0x18, 0x7EFC],
                    [2, 0, 0x696C, 0x10, 0x7F00],
                    [0, 0, 0x699C, 0x04, 0x7F04],
                ];
                Hall = SAV.GetHall();
                break;
            case SAV4HGSS:
                PrintIndexStart = 77;
                BFF = [
                    // { BFV, BFT, addr, 1BFTlen, checkBit
                    [0, 1, 0x5264, 0x04, 0x5BC1],
                    [1, 0, 0x5278, 0x10, 0x687C],
                    [0, 0, 0x52A8, 0x18, 0x6880],
                    [2, 0, 0x52F0, 0x10, 0x6884],
                    [0, 0, 0x5320, 0x04, 0x6888],
                ];
                Hall = SAV.GetHall();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sav), sav, null);
        }
        ReadBattleFrontier();
    }
    private void ReadBattleFrontier()
    {
        BFV = [
            [2, 0], // Max, Current
            [2, 0, 3, 1], // Max, Current, Max(Trade), Current(Trade)
            [2, 0, 1, -1, 3], // Max, Current, Current(CP), (UsedCP), Max(CP)
        ];
        BFT = [
            ["Singles", "Doubles", "Multi"],
            ["Singles", "Doubles", "Multi (Trainer)", "Multi (Friend)", "Wi-Fi"],
        ];

        if (SAV is not SAV4DP)
        {
            SetPrintColors(PrintButtonA);
            
        }
        /*if (Hall is null)
            NUD_HallStreaks.Visible = NUD_HallStreaks.Enabled = false; I need a save that displays this so i know wtf it is*/

        editing = true;
       List<string> facilityList = [];
        for (BattleFrontierFacility4 i = 0; i <= SAV.MaxFacility; i++)
            facilityList.Add(i.ToString());
        CB_Stats1.ItemSource = facilityList;
        StatRBA[0].IsChecked = true;
        var speciesList = GameInfo.FilteredSources.Species.Skip(1).ToList();
        CB_Species.ItemSource = speciesList;
        CB_Species.DisplayMemberPath = "Text";
        editing = false;
        CB_Stats1.SelectedIndex = 0;
    }
    private void SetPrintColors(ReadOnlySpan<Button> controls)
    {
        for (int i = 0; i < controls.Length; i++)
        {
            var pb = controls[i];
            var workIndex = PrintIndexStart + i;
            var value = SAV.GetWork(workIndex);
            SetPrintColor(pb, (BattleFrontierPrintStatus4)value);
        }
    }
    private static void SetPrintColor(Button pb, BattleFrontierPrintStatus4 value)
    {
        bool ready = value is BattleFrontierPrintStatus4.FirstReady or BattleFrontierPrintStatus4.SecondReady;
        if (ready)
            pb.BorderColor = Colors.Red;
        else if (value != 0)
            pb.BorderColor = Colors.Green;
        else
            pb.BorderColor = Colors.Black;

        if (value is BattleFrontierPrintStatus4.FirstReady or BattleFrontierPrintStatus4.FirstReceived)
            pb.BackgroundColor = Colors.Silver;
        else if (value is BattleFrontierPrintStatus4.SecondReady or BattleFrontierPrintStatus4.SecondReceived)
            pb.BackgroundColor = Colors.Gold;
        else
            pb.BackgroundColor = Colors.Transparent;
    }

    private void BTN_Print_Click(object sender, EventArgs e)
    {
        if (sender is not Button b)
            return;
        int index = Array.IndexOf(PrintButtonA, b);
        if (index < 0)
            return;
        index += PrintIndexStart;
        var current = SAV.GetWork(index);
        current++;
        if (current > (int)BattleFrontierPrintStatus4.SecondReceived)
            current = 0;
        SAV.SetWork(index, current);

        SetPrintColor(b, (BattleFrontierPrintStatus4)current);
    }

    private void ChangeStat1(object sender, EventArgs e)
    {
        if (editing)
            return;
        int facility = CB_Stats1.SelectedIndex;
        if (facility < 0)
            return;

        editing = true;
        CB_Stats2.ItemSource=BFT[BFF[facility][1]];

        StatRBA[0].IsChecked = true;
        foreach (RadioButton rb in StatRBA)
            rb.IsVisible = rb.IsEnabled = facility == 1;

        for (int i = 0; i < StatLabelA.Length; i++)
            StatLabelA[i].IsVisible = StatLabelA[i].IsEnabled = StatNUDA[i].IsVisible = StatNUDA[i].IsEnabled = Array.IndexOf(BFV[BFF[facility][0]], i) >= 0;
        if (facility == 0)
        {
            StatLabelA[1].IsVisible = StatLabelA[1].IsEnabled = StatNUDA[1].IsVisible = StatNUDA[1].IsEnabled = true;
            StatLabelA[1].Text = "Continue";
            StatNUDA[1].MaxValue = 65535;
        }
        else
        {
            if (StatNUDA[1].Number > 9999)
                StatNUDA[1].Number = 9999;
            StatNUDA[1].MaxValue = 9999;
        }

        if (facility == 1)
            StatLabelA[1].Text = StatLabelA[3].Text = "Trade";
        else if (facility == 3)
            StatLabelA[1].Text = StatLabelA[3].Text = "CP";

        GB_Hall.IsVisible = facility == 2;
        GB_Castle.IsVisible = facility == 3;

        editing = false;
        CB_Stats2.SelectedIndex = 0;
    }

    private void ChangeStat(object sender, EventArgs e)
    {
        if (editing)
            return;
        if (sender is RadioButton { IsChecked: false })
            return;
        StatAddrControl(SetValToSav: -2, SetSavToVal: true);
        if (GB_Hall.IsVisible && CB_Stats2.SelectedItem is string sH)
        {
            L_Hall.Text = $"Battle Hall ({sH})";
            editing = true;
            GetHallStat();
            editing = false;
        }
        else if (GB_Castle.IsVisible && CB_Stats2.SelectedItem is string sC)
        {
            L_Castle.Text = $"Battle Castle ({sC})";
            editing = true;
            GetCastleStat();
            editing = false;
        }
    }

    private void StatAddrControl(int SetValToSav = -2, bool SetSavToVal = false)
    {
        int Facility = CB_Stats1.SelectedIndex;
        int BattleType = CB_Stats2.SelectedIndex;
        int RBi = StatRBA[1].IsChecked ? 1 : 0;
        int addrVal = BFF[Facility][2] + (BFF[Facility][3] * BattleType) + (RBi << 3);
        int addrFlag = BFF[Facility][4];
        byte maskFlag = (byte)(1 << (BattleType + (RBi << 2)));
        int TowerContinueCountOfs = SAV is SAV4DP ? 3 : 1;

        var general = SAV.General;
        if (SetSavToVal)
        {
            editing = true;
            for (int i = 0; i < BFV[BFF[Facility][0]].Length; i++)
            {
                if (BFV[BFF[Facility][0]][i] < 0)
                    continue;
                int vali = ReadUInt16LittleEndian(general[(addrVal + (i << 1))..]);
                StatNUDA[BFV[BFF[Facility][0]][i]].Number = vali > 9999 ? 9999 : vali;
            }
            CHK_Continue.IsChecked = (SAV.General[addrFlag] & maskFlag) != 0;

            if (Facility == 0) // tower continue count
                StatNUDA[1].Number = ReadUInt16LittleEndian(general[(addrFlag + TowerContinueCountOfs + (BattleType << 1))..]);

            editing = false;
            return;
        }
        if (SetValToSav >= 0)
        {
            ushort val = (ushort)StatNUDA[SetValToSav].Number;

            if (Facility == 0 && SetValToSav == 1) // tower continue count
            {
                var offset = addrFlag + TowerContinueCountOfs + (BattleType << 1);
                WriteUInt16LittleEndian(general[offset..], val);
            }

            SetValToSav = Array.IndexOf(BFV[BFF[Facility][0]], SetValToSav);
            if (SetValToSav < 0)
                return;
            var clamp = Math.Min((ushort)9999, val);
            WriteUInt16LittleEndian(general[(addrVal + (SetValToSav << 1))..], clamp);
            return;
        }
        if (SetValToSav == -1)
        {
            if (CHK_Continue.IsChecked)
            {
                general[addrFlag] |= maskFlag;
                if (Facility == 3)
                    general[addrFlag + 1] |= 0x01; // not found what this flag means
            }
            else
            {
                general[addrFlag] &= (byte)~maskFlag;
            }
        }
    }

    private void ChangeStatVal(object sender, EventArgs e)
    {
        if (editing)
            return;

        int n = Array.IndexOf(StatNUDA, sender);
        if (n < 0)
            return;

        StatAddrControl(SetValToSav: n, SetSavToVal: false);

        if (CB_Stats1.SelectedIndex != 0)
            return;

        const int bias = 7;
        var n0 = StatNUDA[0];
        var n1 = StatNUDA[1];
        if (Math.Floor(n0.Number / bias) == n1.Number)
            return;

        if (n == 0)
        {
            n1.Number = Math.Floor(n0.Number / bias);
        }
        else if (n == 1)
        {
            if (n0.MaxValue > n1.Number * bias)
                n0.Number = n1.Number * bias;
            else if (n0.Number < n0.MaxValue)
                n0.Number = n0.MaxValue;
        }
    }

    private void CHK_Continue_CheckedChanged(object sender, EventArgs e)
    {
        if (editing)
            return;
        StatAddrControl(SetValToSav: -1, SetSavToVal: false);
    }

    private ushort species = ushort.MaxValue;

    private void ChangeSpecies(object sender, EventArgs e)
    {
        species = (ushort)CB_Species.SelectedIndex;
        if (editing)
            return;

        editing = true;
        GetHallStat();
        editing = false;
    }

    private void GetCastleStat()
    {
        int ofs = BFF[3][2] + (BFF[3][3] * CB_Stats2.SelectedIndex) + 0x0A;
        NumericUpDown[] na = [NUD_CastleRankRcv, NUD_CastleRankItem, NUD_CastleRankInfo];
        for (int i = 0; i < na.Length; i++)
        {
            int val = ReadInt16LittleEndian(SAV.General[(ofs + (i << 1))..]);
            na[i].Number = val > na[i].MaxValue ? na[i].MaxValue : val < na[i].MinValue ? na[i].MinValue : val;
        }
    }

    private void NUD_CastleRank_ValueChanged(object sender, EventArgs e)
    {
        if (editing)
            return;
        NumericUpDown[] na = [NUD_CastleRankRcv, NUD_CastleRankItem, NUD_CastleRankInfo];
        int i = Array.IndexOf(na, sender);
        if (i < 0)
            return;
        var offset = BFF[3][2] + (BFF[3][3] * CB_Stats2.SelectedIndex) + 0x0A + (i << 1);
        WriteInt32LittleEndian(SAV.General[offset..], (int)na[i].Number);
    }

    private void GetHallStat()
    {
        int ofscur = BFF[2][2] + (BFF[2][3] * CB_Stats2.SelectedIndex);
        var curspe = ReadUInt16LittleEndian(SAV.General[(ofscur + 4)..]);
        bool c = curspe == species;
        CHK_HallCurrent.IsChecked = c;
        L_HallCurrent.Text = curspe > 0 && curspe <= SAV.MaxSpeciesID
            ? $"Current: {SpeciesName.GetSpeciesNameGeneration(curspe, GameLanguage.GetLanguageIndex("en"), 4)}"
            : "Current: (None)";

        int s = 0;
        for (int i = 0; i < HallNUDA.Length; i++)
        {
            var d = c ? Math.Min(10, (SAV.General[ofscur + 6 + ((i >> 1) << 1)] >> ((i & 1) << 2)) & 0x0F) : 0;
            HallNUDA[i].Number = d;
            HallNUDA[i].IsEnabled = c;
            s += d;
        }
        L_SumHall.Text = s.ToString();
    }

    private void CHK_HallCurrent_CheckedChanged(object sender, EventArgs e)
    {
        if (editing)
            return;
        var offset = BFF[2][2] + (BFF[2][3] * CB_Stats2.SelectedIndex) + 4;
        ushort value = (ushort)(CHK_HallCurrent.IsChecked ? species : 0);
        WriteUInt16LittleEndian(SAV.General[offset..], value);
        editing = true;
        GetHallStat();
        editing = false;
    }

    private void NUD_HallType_ValueChanged(object sender, EventArgs e)
    {
        if (editing)
            return;
        int i = Array.IndexOf(HallNUDA, sender);
        if (i < 0)
            return;

        int ofs = BFF[2][2] + (BFF[2][3] * CB_Stats2.SelectedIndex) + 6 + ((i >> 1) << 1);
        SAV.General[ofs] = (byte)((SAV.General[ofs] & ~(0xF << ((i & 1) << 2))) | ((int)HallNUDA[i].Number << ((i & 1) << 2)));
        L_SumHall.Text = HallNUDA.Sum(x => x.Number).ToString();
    }

    private void NUD_HallStreaks_ValueChanged(object sender, EventArgs e)
    {
        if (editing || Hall is null)
            return;
       // Hall.SetCount(CB_Stats2.SelectedIndex, species, (ushort)NUD_HallStreaks.Value);
    }
}