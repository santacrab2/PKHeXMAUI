using CommunityToolkit.Maui.Core.Extensions;
using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class MiscFashionCase : ContentPage
{
    private readonly string[] accessories, backdrops;
    public ObservableCollection<string> backdropsSource;
    public SAV4 SAV;
    private void B_ClearAcessoriesClick(object sender, EventArgs e) => ClearAccessories();
    public void ClearAccessories()
    {
        for (int i = 0; i < AccessoryInfo.Count; i++)
            AccessoryList[i].Item2 = 0;
    }
    private void OnBAllAccessoriesLegalOnClick(object sender, EventArgs e)
    {
        bool setUnreleasedIndexes = sender == B_AllAccessoriesIllegal;
        SetAllAccessories(setUnreleasedIndexes);
    }
    public void SetAllAccessories(bool unreleased = false)
    {
        for (int i = 0; i <= AccessoryInfo.MaxMulti; i++)
            AccessoryList[i].Item2 = AccessoryInfo.AccessoryMaxCount;

        var count = unreleased ? AccessoryInfo.Count : (AccessoryInfo.MaxLegal + 1);
        for (int i = AccessoryInfo.MaxMulti + 1; i < count; i++)
            AccessoryList[i].Item2 = 1;
    }
    public void SaveAccessories()
    {
        for (int i = 0; i < AccessoryInfo.Count; i++)
        {
            SAV.SetAccessoryOwnedCount((Accessory4)i, Math.Clamp(AccessoryList[i].Item2, (byte)0, byte.MaxValue));
        }
    }
    private void B_ClearBackdropsClick(object sender, EventArgs e) => ClearBackdrops();
    private void ClearBackdrops()
    {
        for (int i = 0; i < BackdropInfo.Count; i++)
            backdropsSource[i] = backdrops[(int)Backdrop4.Unset];
    }
    public void SetAllBackdrops(bool unreleased = false)
    {
        var count = unreleased ? BackdropInfo.Count : ((int)BackdropInfo.MaxLegal + 1);
        for (int i = 0; i < count; i++)
            backdropsSource[i] = backdrops[i];
    }
    private void OnBAllBackdropsLegalOnClick(object sender, EventArgs e)
    {
        bool setUnreleasedIndexes = sender == B_AllBackdropsIllegal;
        SetAllBackdrops(setUnreleasedIndexes);
    }
    public void SaveBackdrops()
    {
        for (int i = 0; i < BackdropInfo.Count; i++)
            SAV.RemoveBackdrop((Backdrop4)i); // clear all slots

        byte ctr = 0;
        for (int i = 0; i < BackdropInfo.Count; i++)
        {
            Backdrop4 bd = (Backdrop4)Array.IndexOf(backdrops, backdropsSource[i]);
            if (bd.IsUnset()) // skip empty slots
                continue;

            SAV.SetBackdropPosition(bd, ctr);
            ctr++;
        }
    }
    public static ObservableCollection<AccessoryItem> AccessoryList = [];
    public MiscFashionCase(SAV4 sav)
	{
		InitializeComponent();
        SAV = sav;
        accessories = GameInfo.Strings.accessories;
        backdrops = GameInfo.Strings.backdrops;
        backdropsSource = backdrops.ToObservableCollection();
        CV_Accessories.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = [];
            NumericUpDown cb = new();
            cb.SetBinding(NumericUpDown.NumberProperty, ".Item2", BindingMode.TwoWay);
            Label lab = new();
            lab.SetBinding(Label.TextProperty, ".Item1");
            grid.Children.Add(cb);
            grid.Children.Add(lab);
            return grid;
        });
        CV_Backdrops.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = [];
            Label lab = new();
            lab.SetBinding(Label.TextProperty, ".");
            grid.Children.Add(lab);
            return grid;
        });
        for (int i = 0; i < AccessoryInfo.Count; i++)
            AccessoryList.Add(new(accessories[i], sav.GetAccessoryOwnedCount((Accessory4)i)));
        for (int i = 0; i < BackdropInfo.Count; i++)
            backdropsSource[i] = (backdrops[(int)Backdrop4.Unset]);
        for (int i = 0; i < BackdropInfo.Count; i++)
        {
            var pos = sav.GetBackdropPosition((Backdrop4)i);
            if (pos < BackdropInfo.Count)
                backdropsSource[i] = backdrops[i];
        }
        CV_Accessories.ItemsSource = AccessoryList;
        CV_Backdrops.ItemsSource = backdropsSource;
    }
}

public class AccessoryItem (string accessory, byte count)
{
    public string Item1 { get; set; } = accessory;
    public byte Item2 { get; set; } = count;

}
