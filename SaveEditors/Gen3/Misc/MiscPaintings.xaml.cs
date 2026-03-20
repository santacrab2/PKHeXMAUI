using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscPaintings : ContentPage
{
    private SAV3 SAV;
    private List<ComboItem> filteredspecies = [.. GameInfo.FilteredSources.Species];

    public MiscPaintings(SAV3 sav)
	{
		InitializeComponent();
        SAV = sav;
        Speciesbox.ItemSource = filteredspecies;
        Speciesbox.DisplayMemberPath = "Text";
        CurrentPaintingEntry.Number = 0;
        CurrentPaintingView.IsVisible = PaintingEnabledCheck.IsChecked;
    }
    private int PaintingIndex = -1;
    private void CurrentPaintingChanged(object sender, EventArgs e)
    {
        var index = (int)CurrentPaintingEntry.Number;
        if (index == PaintingIndex) return;
        SavePainting(PaintingIndex);
        LoadPainting(index);
    }
    private void LoadPainting(int index)
    {
        if ((uint)index >= 5)
            return;
        if (SAV.LargeBlock is not ISaveBlock3LargeHoenn gallery)
            return;
        var painting = gallery.GetPainting(index, SAV.Japanese);

        CurrentPaintingView.IsVisible = PaintingEnabledCheck.IsChecked = SAV.GetEventFlag(Paintings3.GetFlagIndexContestStat(index));

        Speciesbox.SelectedItem = filteredspecies.Find(z=>z.Value== (int)painting.Species)??new ComboItem("",0);
        Captionentry.Number = (ulong)painting.GetCaptionRelative(index);
        TIDEntry.Text = painting.TID.ToString();
        SIDEntry.Text = painting.SID.ToString();
        PIDEntry.Text = painting.PID.ToString("X8");
        Nicknameentry.Text = painting.Nickname;
        OTEntry.Text = painting.OT;

        PaintingIndex = index;

        CurrentPaintingEntry.BackgroundColor = index switch
        {
            0 => Color.FromRgb(248, 152, 096),
            1 => Color.FromRgb(128, 152, 248),
            2 => Color.FromRgb(248, 168, 208),
            3 => Color.FromRgb(112, 224, 112),
            _ => Color.FromRgb(248, 240, 056),
        };
    }
    private void SavePainting(int index)
    {
        if ((uint)index >= 5)
            return;
        if (SAV.LargeBlock is not ISaveBlock3LargeHoenn gallery)
            return;
        var painting = gallery.GetPainting(index, SAV.Japanese);

        var enabled = PaintingEnabledCheck.IsChecked;
        SAV.SetEventFlag(Paintings3.GetFlagIndexContestStat(index), enabled);
        if (!enabled)
        {
            painting.Clear();
            gallery.SetPainting(index, painting);
            return;
        }

        painting.Species = (ushort)((ComboItem)Speciesbox.SelectedItem).Value;
        painting.SetCaptionRelative(index, Math.Min((byte)Captionentry.Number,(byte)2));
        painting.TID = (ushort)Util.ToUInt32(TIDEntry.Text);
        painting.SID = (ushort)Util.ToUInt32(SIDEntry.Text);
        painting.PID = Util.GetHexValue(PIDEntry.Text);
        painting.Nickname = Nicknameentry.Text;
        painting.OT = OTEntry.Text;

        gallery.SetPainting(index, painting);
    }

    private void enablePainting(object sender, CheckedChangedEventArgs e)
    {
        CurrentPaintingView.IsVisible = PaintingEnabledCheck.IsChecked;
    }
}