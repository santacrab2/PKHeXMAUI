using PKHeX.Core;
using System;
using System.Collections.ObjectModel;
namespace PKHeXMAUI;

public partial class MiscSeals : ContentPage
{
	private readonly string[] seals;
    public ObservableCollection<Seels> SealList = [];
    private readonly SAV4 SAV;
    public MiscSeals(SAV4 sav)
	{
		InitializeComponent();
        SAV = sav;
        seals = GameInfo.Strings.seals;
        CV_Seals.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = [];
            NumericUpDown cb = new();
            cb.SetBinding(NumericUpDown.NumberProperty, ".Item1", BindingMode.TwoWay);
            Label lab = new();
            lab.SetBinding(Label.TextProperty, ".Item2");
            grid.Children.Add(cb);
            grid.Children.Add(lab);
            return grid;
        });
        const int count = (int)Seal4.MAX;
        for (int i = 0; i < count; i++)
            SealList.Add(new(sav.GetSealCount((Seal4)i), seals[i]));
        CV_Seals.ItemsSource = SealList;
    }

    public void ClearSeals()
    {
        for (int i = 0; i < (int)Seal4.MAX; i++)
            SealList[i].Item1 = 0;
    }

    public void SetAllSeals(bool unreleased = false)
    {
        var sealIndexCount = (int)(unreleased ? Seal4.MAX : Seal4.MAXLEGAL);
        for (int i = 0; i < sealIndexCount; i++)
            SealList[i].Item1 = SAV4.SealMaxCount;
    }

    private void B_ClearSeals_Click(object sender, EventArgs e) => ClearSeals();

    private void OnBAllSealsLegalOnClick(object sender, EventArgs e)
    {
        bool setUnreleasedIndexes = sender == B_AllSealsIllegal;
        SetAllSeals(setUnreleasedIndexes);
    }

    public void SaveSeals()
    {
        for (int i = 0; i < (int)Seal4.MAX; i++)
        {

            SAV.SetSealCount((Seal4)i, Math.Clamp(SealList[i].Item1, (byte)0, byte.MaxValue));
        }
    }
}
public class Seels(byte item1, string item2)
{
    public byte Item1 { get; set; } = item1;
    public string Item2 { get; set; } = item2;
}