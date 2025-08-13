
using PKHeX.Core;
using System.Runtime.CompilerServices;
using static System.Buffers.Binary.BinaryPrimitives;
namespace PKHeXMAUI;

public partial class MiscBattleFrontier : ContentPage
{
    private Button[] SymbolButtonA = null!;
    private bool editingcont;
    private bool editingval;
    private RadioButton[] StatRBA = null!;
    private NumericUpDown[] StatNUDA = null!;
    private Label[] StatLabelA = null!;
    private bool loading;
    private int[][] BFF = null!;
    private string[]?[] BFT = null!;
    private int[][] BFV = null!;
    private string[] BFN = null!;
    public SAV3 SAV;
    public MiscBattleFrontier(SAV3 sav)
	{
		InitializeComponent();
        SAV = sav;
        loading = true;
        BFF = [
            // { BFV, BFT, addr(BFV.len), checkBitShift(BFT.len)
            [0, 2, 0xCE0, 0xCF0, 0x00, 0x0E, 0x10, 0x12],
            [1, 1, 0xD0C, 0xD14, 0xD1C, 0x02, 0x14],
            [0, 1, 0xDC8, 0xDD0, 0x04, 0x16],
            [0, 0, 0xDDA, 0xDDE, 0x06],
            [2, 1, 0xDE2, 0xDF2, 0xDEA, 0xDFA, 0x08, 0x18],
            [1, 0, 0xE04, 0xE08, 0xE0C, 0x0A],
            [0, 0, 0xE1A, 0xE1E, 0x0C],
        ];
        BFV =
        [
            [0, 2], // Current, Max
            [0, 2, 3], // Current, Max, Total
            [0, 1, 2, 3], // Current, Trade, Max, Trade
        ];
        BFT = [
            null,
            ["Singles", "Doubles"],
            ["Singles", "Doubles", "Multi", "Linked"],
        ];
        BFN =
        [
            "Tower","Dome","Palace","Arena","Factory","Pike","Pyramid",
        ];
        StatNUDA = [NUD_Stat0, NUD_Stat1, NUD_Stat2, NUD_Stat3];
        StatLabelA = [L_Stat0, L_Stat1, L_Stat2, L_Stat3];
        StatRBA = [RB_Stats3_01, RB_Stats3_02];
        SymbolButtonA = [BTN_SymbolA, BTN_SymbolT, BTN_SymbolS, BTN_SymbolG, BTN_SymbolK, BTN_SymbolL, BTN_SymbolB];
        CHK_ActivatePass.IsChecked = SAV.GetEventFlag(0x860 + 0x72);
        SetFrontierSymbols();

        CB_Stats1.ItemSource?.Cast<Object>().ToList().Clear();
        CB_Stats1.ItemSource=BFN;
        loading = false;
        CB_Stats1.SelectedIndex = 1;
        CB_Stats1.SelectedIndex = 0;
    }
    private void SetFrontierSymbols()
    {
        for (int i = 0; i < SymbolButtonA.Length; i++)
        {
            var flagIndex = 0x860 + 0x64 + (i * 2);
            var silver = SAV.GetEventFlag(flagIndex);
            var gold = SAV.GetEventFlag(flagIndex + 1);
            var value = silver ? gold ? Colors.Gold : Colors.Silver : Colors.Transparent;
            SymbolButtonA[i].BackgroundColor = value;
        }
    }
    private void BTN_Symbol_Click(object sender, EventArgs e)
    {
        var match = Array.Find(SymbolButtonA, z => z == sender);
        if (match == null)
            return;

        var color = match.BackgroundColor;
        color = color == Colors.Transparent ? Colors.Silver : color == Colors.Silver ? Colors.Gold : Colors.Transparent;
        match.BackgroundColor = color;
    }
    private void CHK_Continue_CheckedChanged(object sender, EventArgs e)
    {
        if (editingval)
            return;
        StatAddrControl(SetValToSav: -1, SetSavToVal: false);
    }
    private void ChangeStatVal(object sender, EventArgs e)
    {
        if (editingval || sender is not NumericUpDown nud)
            return;
        int n = Array.IndexOf(StatNUDA, nud);
        if (n < 0)
            return;
        StatAddrControl(SetValToSav: n, SetSavToVal: false);
    }
    private void StatAddrControl(int SetValToSav = -2, bool SetSavToVal = false)
    {
        int Facility = CB_Stats1.SelectedIndex;
        if (Facility < 0)
            return;

        int BattleType = CB_Stats2.SelectedIndex;
        var bft = BFT[BFF[Facility][1]];
        if (bft == null)
            BattleType = 0;
        else if (BattleType < 0)
            return;
        else if (BattleType >= bft.Length)
            return;

        int RBi = -1;
        for (int i = 0, j = 0; i < StatRBA.Length; i++)
        {
            if (!StatRBA[i].IsChecked)
                continue;
            if (++j > 1)
                return;
            RBi = i;
        }
        if (RBi < 0)
            return;

        if (SetValToSav >= 0)
        {
            ushort val = (ushort)StatNUDA[SetValToSav].Number;
            SetValToSav = Array.IndexOf(BFV[BFF[Facility][0]], SetValToSav);
            if (SetValToSav < 0)
                return;
            if (val > 9999)
                val = 9999;
            var offset = BFF[Facility][2 + SetValToSav] + (4 * BattleType) + (2 * RBi);
            WriteUInt32LittleEndian(SAV.Small[offset..], val);
            return;
        }
        if (SetValToSav == -1)
        {
            int p = BFF[Facility][2 + BFV[BFF[Facility][0]].Length + BattleType] + RBi;
            const int offset = 0xCDC;
            var current = ReadUInt32LittleEndian(SAV.Small[offset..]);
            var update = (current & ~(1u << p)) | (CHK_Continue.IsChecked ? 1u : 0) << p;
            WriteUInt32LittleEndian(SAV.Small[offset..], update);
            return;
        }
        if (!SetSavToVal)
            return;

        editingval = true;
        for (int i = 0; i < BFV[BFF[Facility][0]].Length; i++)
        {
            var offset = BFF[Facility][2 + i] + (4 * BattleType) + (2 * RBi);
            int vali = ReadUInt16LittleEndian(SAV.Small[offset..]);
            if (vali > 9999)
                vali = 9999;
            StatNUDA[BFV[BFF[Facility][0]][i]].Number = (ulong)vali;
        }

        var shift = (BFF[Facility][2 + BFV[BFF[Facility][0]].Length + BattleType] + RBi);
        CHK_Continue.IsChecked = (ReadUInt32LittleEndian(SAV.Small[0xCDC..]) & (1 << shift)) != 0;
        editingval = false;
    }
    private void ChangeStat(object sender, EventArgs e)
    {
        if (editingcont)
            return;
        StatAddrControl(SetValToSav: -2, SetSavToVal: true);
    }
    private void ChangeStat1(object sender, EventArgs e)
    {
        if (loading)
            return;
        int facility = CB_Stats1.SelectedIndex;
        if ((uint)facility >= BFN.Length)
            return;
        editingcont = true;
        CB_Stats2.ItemSource?.Cast<Object>().ToList().Clear();
        foreach (RadioButton rb in StatRBA)
            rb.IsChecked = false;

        var bft = BFT[BFF[facility][1]];
        if (bft == null)
        {
            CB_Stats2.IsVisible = false;
        }
        else
        {
            CB_Stats2.IsVisible = true;
            CB_Stats2.ItemSource = bft;
            CB_Stats2.SelectedIndex = 0;
        }

        for (int i = 0; i < StatLabelA.Length; i++)
            StatLabelA[i].IsVisible = StatLabelA[i].IsEnabled = StatNUDA[i].IsVisible = StatNUDA[i].IsEnabled = Array.IndexOf(BFV[BFF[facility][0]], i) >= 0;

        editingcont = false;
        StatRBA[0].IsChecked = true;
    }
}