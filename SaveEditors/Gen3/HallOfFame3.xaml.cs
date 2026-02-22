using PKHeX.Core;

namespace PKHeXMAUI;

public partial class HallOfFame3 : ContentPage
{
    private readonly SAV3 SAV;
    private readonly HallFame3Entry[] Fame;
    private int prevEntry;
    private int prevMember;
    private bool Loading;
    public HallOfFame3(SAV3 sav)
	{
		InitializeComponent();
        SAV = sav;
        Fame = HallFame3Entry.GetEntries(SAV);
        HoFCV.ItemsSource = Enumerable.Range(0, 50).ToList();
        var filtered = GameInfo.FilteredSources;
        CB_Species.ItemSource = filtered.Species.ToList();
        CB_Species.DisplayMemberPath = "Text";
        HoFCV.SelectedItem = 0;
        NUD_Members.Number = 0;
        var pk = Fame[(int)HoFCV.SelectedItem].Team[(int)NUD_Members.Number];
        LoadEntry(pk);
        TB_TID.TextChanged += (_, _) => ValidateIDs();
        TB_SID.TextChanged += (_, _) => ValidateIDs();
        TB_PID.TextChanged += (_, _) => ValidateIDs();
        NUD_Members.ValueChanged += (_, _) =>
        {
            SaveEntry(Fame[prevEntry].Team[prevMember]);
            var pkm = Fame[(int)HoFCV.SelectedItem].Team[(int)NUD_Members.Number];
            LoadEntry(pkm);
            prevMember = (int)NUD_Members.Number;
            prevEntry = (int)HoFCV.SelectedItem;
        };

        HoFCV.SelectionChanged += (_, _) =>
        {
            SaveEntry(Fame[prevEntry].Team[prevMember]);
            NUD_Members.Number = 0;
            var pkm = Fame[(int)HoFCV.SelectedItem].Team[0];
            LoadEntry(pkm);
            prevMember = (int)NUD_Members.Number;
            prevEntry = (int)HoFCV.SelectedItem;
        };
    }

    private void LoadEntry(HallFame3PKM pk)
    {
        Loading = true;
        TB_TID.Text = pk.TID16.ToString("00000");
        TB_SID.Text = pk.SID16.ToString("00000");
        TB_PID.Text = pk.PID.ToString("X8");
        TB_Nickname.Text = pk.Nickname;
        NUD_Level.Number = pk.Level;
        CB_Species.SelectedItem = new ComboItem(((Species)pk.Species).ToString(), (int)pk.Species);
        Loading = false;
    }
    private void ValidateIDs()
    {
        var pid = Util.GetHexValue(TB_PID.Text);
        if (pid.ToString("X") != TB_PID.Text && pid.ToString("X8") != TB_PID.Text)
            TB_PID.Text = pid.ToString("X8");

        var tid = Util.ToUInt32(TB_TID.Text);
        if (tid > ushort.MaxValue)
            tid = ushort.MaxValue;
        if (tid.ToString() != TB_TID.Text)
            TB_TID.Text = tid.ToString();

        var sid = Util.ToUInt32(TB_SID.Text);
        if (sid > ushort.MaxValue)
            sid = ushort.MaxValue;
        if (sid.ToString() != TB_SID.Text)
            TB_SID.Text = sid.ToString();

        CHK_Shiny.IsChecked = ShinyUtil.GetIsShiny3((sid << 16) | tid, pid);
    }
    private void SaveEntry(HallFame3PKM pk)
    {
        pk.TID16 = Convert.ToUInt16(TB_TID.Text);
        pk.SID16 = Convert.ToUInt16(TB_SID.Text);
        pk.PID = Util.GetHexValue(TB_PID.Text);
        if (pk.Nickname != TB_Nickname.Text) // preserve trash
            pk.Nickname = TB_Nickname.Text;
        pk.Level = (int)NUD_Level.Number;
        pk.Species = (ushort)((ComboItem)CB_Species.SelectedItem).Value;
    }
    private void ClearFields()
    {
        TB_TID.Text = TB_SID.Text = "0";
        TB_PID.Text = "0";
        TB_Nickname.Text = string.Empty;
        NUD_Level.Number = 0;
        CB_Species.SelectedIndex = 0;

    }
    private void B_Cancel_Click(object sender, EventArgs e) => Navigation.PopModalAsync();

    private void B_Save_Click(object sender, EventArgs e)
    {
        var pkm = Fame[(int)HoFCV.SelectedItem].Team[(int)NUD_Members.Number];
        SaveEntry(pkm);
        HallFame3Entry.SetEntries(SAV, Fame);
        Navigation.PopModalAsync();
    }
    private void B_Clear_Click(object sender, EventArgs e) => ClearFields();

}