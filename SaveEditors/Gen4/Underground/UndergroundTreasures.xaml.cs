using PKHeX.Core;

namespace PKHeXMAUI;

public partial class UndergroundTreasures : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SAV4Sinnoh SAV;
	private readonly string[] ugTreasures;
    private readonly string[] ugTreasuresSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    public UndergroundTreasures(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = (SAV4Sinnoh)(Origin = sav).Clone();

        ugTreasures = GameInfo.Strings.ugtreasures;
        ugTreasuresSorted = SanitizeList(ugTreasures);
        CV_UndergroundTreasures.ItemTemplate = new DataTemplate(() =>
		{
			Grid grid = [];
            var combo = new comboBox() { ItemSource = ugTreasuresSorted };
            combo.SetBinding(comboBox.SelectedIndexProperty, ".");
            grid.Add(combo);
            return grid;
		});
        ReadUGData();
    }
    private void ReadUGData()
    {
        var treasuresList = SAV.GetUGI_Treasures();
        while (treasuresList.Length < MAX_SIZE)
            treasuresList.ToArray().ToList().Add(0);
        CV_UndergroundTreasures.ItemsSource = treasuresList.ToArray();
    }
    private static string[] SanitizeList(string[] inputlist)
    {
        string[] listSorted = Array.FindAll(inputlist, x => !string.IsNullOrEmpty(x));
        Array.Sort(listSorted);

        return listSorted;
    }
    public void SaveUGData()
    {
        var treasuresList = SAV.GetUGI_Treasures();
        treasuresList.Clear();
        
        if (CV_UndergroundTreasures.ItemsSource is not byte[] items) return;
        
        int ctr = 0;
        foreach (var item in items)
        {
            if (item <= 0) continue; // ignore empty slot
            treasuresList[ctr] = item;
            ctr++;
        }
    }
    
}