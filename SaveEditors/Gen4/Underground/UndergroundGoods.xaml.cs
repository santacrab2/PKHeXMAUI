using PKHeX.Core;

namespace PKHeXMAUI;

public partial class UndergroundGoods : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SAV4Sinnoh SAV;
	private readonly string[] ugGoods;
    private readonly string[] ugGoodsSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    public UndergroundGoods(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = (SAV4Sinnoh)(Origin = sav).Clone();

        ugGoods = GameInfo.Strings.uggoods;
        ugGoodsSorted = SanitizeList(ugGoods);
        CV_UndergroundGoods.ItemTemplate = new DataTemplate(() =>
		{
			Grid grid = [];
            var combo = new comboBox() { ItemSource = ugGoodsSorted };
            combo.SetBinding(comboBox.SelectedIndexProperty, ".", BindingMode.TwoWay);
            grid.Add(combo);
            return grid;
		});
        ReadUGData();
    }
    private void ReadUGData()
    {
        var goodsList = SAV.GetUGI_Goods();
        while (goodsList.Length < MAX_SIZE)
            goodsList.ToArray().ToList().Add(0);
        CV_UndergroundGoods.ItemsSource = goodsList.ToArray();
    }
    private static string[] SanitizeList(string[] inputlist)
    {
        string[] listSorted = Array.FindAll(inputlist, x => !string.IsNullOrEmpty(x));
        Array.Sort(listSorted);

        return listSorted;
    }
}