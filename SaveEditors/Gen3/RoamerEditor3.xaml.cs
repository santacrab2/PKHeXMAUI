using PKHeX.Core;

namespace PKHeXMAUI;

public partial class RoamerEditor3 : ContentPage
{
    private readonly Roamer3 Reader;
    private readonly SAV3 SAV;
    public RoamerEditor3(SAV3 sav)
	{
		InitializeComponent();
		SAV = sav;
		Reader = new Roamer3(sav.LargeBlock);
		CB_Species.ItemSource = GameInfo.FilteredSources.Species.ToList();
		CB_Species.DisplayMemberPath = "Text";
        E_PID.Text = Reader.PID.ToString("X8");
        CHK_Shiny.IsChecked = Roamer3.IsShiny(Reader.PID, SAV);

        CB_Species.SelectedItem = new ComboItem(((Species)Reader.Species).ToString(),Reader.Species);
        var IVs = Reader.IVs;

        var iv = new[] { E_HPIV, E_ATKIV, E_DEFIV, E_SPEIV, E_SPAIV, E_SPDIV };
        for (int i = 0; i < iv.Length; i++)
            iv[i].Text = IVs[i].ToString();

        CHK_Active.IsChecked = Reader.Active;
        NUD_Level.Number = Math.Min(NUD_Level.MaxValue, Reader.CurrentLevel);
    }
    private void SaveData()
    {
        Reader.PID = Util.GetHexValue(E_PID.Text);
        Reader.Species = (ushort)((ComboItem)CB_Species.SelectedItem).Value;
        Reader.SetIVs(
        [
            Util.ToInt32(E_HPIV.Text),
            Util.ToInt32(E_ATKIV.Text),
            Util.ToInt32(E_DEFIV.Text),
            Util.ToInt32(E_SPEIV.Text),
            Util.ToInt32(E_SPAIV.Text),
            Util.ToInt32(E_SPDIV.Text),
        ]);
        Reader.Active = CHK_Active.IsChecked;
        Reader.CurrentLevel = (byte)NUD_Level.Number;
    }

    private void close(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void Save(object sender, EventArgs e)
    {
        SaveData();
        Navigation.PopModalAsync();
    }
    private void TB_PID_TextChanged(object sender, EventArgs e)
    {
        if (E_PID.Text.Length < 8)
            return;
        var pid = Util.GetHexValue(E_PID.Text);
        CHK_Shiny.IsChecked = Roamer3.IsShiny(pid, SAV);
    }
}