using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class UndergroundTreasures : ContentPage
{
    private readonly SAV4Sinnoh SAV;
	private readonly string[] ugTreasures;
    private readonly string[] ugTreasuresSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    private ObservableCollection<TreasuresEntry> treasuresList = [];
    private class TreasuresEntry
    {
        public string ItemName { get; set; } = "";
    }
    public UndergroundTreasures(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = sav;

        ugTreasures = GameInfo.Strings.ugtreasures;
        ugTreasuresSorted = SanitizeList(ugTreasures);
        CV_UndergroundTreasures.ItemTemplate = new DataTemplate(() =>
		{
			Grid grid = [];
            var combo = new comboBox() { ItemSource = ugTreasuresSorted };
            combo.SetBinding(comboBox.SelectedItemProperty, ".", BindingMode.TwoWay);
            grid.Add(combo);
            return grid;
		});
        ReadUGData();
    }
    private void ReadUGData()
    {
        var originlist = SAV.GetUGI_Treasures();
        for(int i = 0; i < originlist.Length; i++)
        {
            var itemID = originlist[i];
            var itemName = itemID <= 0 ? ugTreasures[0] : ugTreasures[itemID];
            treasuresList.Add(new TreasuresEntry { ItemName = itemName });
        }
        while (treasuresList.Count < MAX_SIZE)
            treasuresList.Add(new TreasuresEntry() { ItemName = ugTreasures[0] });
        CV_UndergroundTreasures.ItemsSource = treasuresList;
    }
    private static string[] SanitizeList(string[] inputlist)
    {
        string[] listSorted = Array.FindAll(inputlist, x => !string.IsNullOrEmpty(x));
        Array.Sort(listSorted);

        return listSorted;
    }
    public void SaveUGData()
    {
        int ctr = 0;
        foreach (var item in treasuresList)
        {
            if (item.ItemName == ugTreasures[0]) continue; // ignore empty slot
            SAV.GetUGI_Treasures()[ctr] = (byte)Array.IndexOf(ugTreasures,item.ItemName);
            ctr++;
        }
    }
    
}