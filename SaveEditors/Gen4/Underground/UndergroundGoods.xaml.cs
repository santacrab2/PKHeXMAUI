using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class UndergroundGoods : ContentPage
{
    private readonly SAV4Sinnoh SAV;
	private readonly string[] ugGoods;
    private readonly string[] ugGoodsSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    
    private class GoodsEntry
    {
        public string ItemName { get; set; } = "";
    }
    
    private readonly ObservableCollection<GoodsEntry> goodsList = [];
    
    public UndergroundGoods(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = sav;

        ugGoods = GameInfo.Strings.uggoods;
        ugGoodsSorted = SanitizeList(ugGoods);
        CV_UndergroundGoods.ItemTemplate = new DataTemplate(() =>
		{
			Grid grid = [];
            var combo = new comboBox() { ItemSource = ugGoodsSorted };
            combo.SetBinding(comboBox.SelectedItemProperty, "ItemName", BindingMode.TwoWay);
            grid.Add(combo);
            return grid;
		});
        ReadUGData();
    }
    private void ReadUGData()
    {
        var originlist = SAV.GetUGI_Goods();
        for(int i = 0; i < originlist.Length; i++)
        {
            var itemID = originlist[i];
            var itemName = itemID <= 0 ? ugGoods[0] : ugGoods[itemID];
            goodsList.Add(new GoodsEntry { ItemName = itemName });
        }
        while (goodsList.Count < MAX_SIZE)
            goodsList.Add(new GoodsEntry { ItemName = ugGoods[0] });
        CV_UndergroundGoods.ItemsSource = goodsList;
    }
    private static string[] SanitizeList(string[] inputlist)
    {
        string[] listSorted = Array.FindAll(inputlist, x => !string.IsNullOrEmpty(x));
        //Array.Sort(listSorted);

        return listSorted;
    }
    public void SaveUGData()
    {
        var origingoods = SAV.GetUGI_Goods();
        int ctr = 0;
        foreach (var item in goodsList)
        {
            if (item.ItemName == ugGoods[0]) continue; // ignore empty slot
            origingoods[ctr] = (byte)Array.IndexOf(ugGoods, item.ItemName);
            ctr++;
        }
    }
}