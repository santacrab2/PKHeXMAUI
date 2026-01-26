using PKHeX.Core;

namespace PKHeXMAUI;

public partial class UndergroundTraps : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SAV4Sinnoh SAV;
	private readonly string[] ugTraps;
    private readonly string[] ugTrapsSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    public UndergroundTraps(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = (SAV4Sinnoh)(Origin = sav).Clone();

        ugTraps = GameInfo.Strings.ugtraps;
        ugTrapsSorted = SanitizeList(ugTraps);
        CV_UndergroundTraps.ItemTemplate = new DataTemplate(() =>
		{
			Grid grid = [];
            var combo = new comboBox() { ItemSource = ugTrapsSorted };
            combo.SetBinding(comboBox.SelectedIndexProperty, ".");
            grid.Add(combo);
            return grid;
		});
        ReadUGData();
    }
    private void ReadUGData()
    {
        var trapsList = SAV.GetUGI_Traps();
        while (trapsList.Length < MAX_SIZE)
            trapsList.ToArray().ToList().Add(0);
        CV_UndergroundTraps.ItemsSource = trapsList.ToArray();
    }
    private static string[] SanitizeList(string[] inputlist)
    {
        string[] listSorted = Array.FindAll(inputlist, x => !string.IsNullOrEmpty(x));
        Array.Sort(listSorted);

        return listSorted;
    }
    public void SaveUGData()
    {
        var trapsList = SAV.GetUGI_Traps();
        trapsList.Clear();
        
        if (CV_UndergroundTraps.ItemsSource is not byte[] items) return;
        
        int ctr = 0;
        foreach (var item in items)
        {
            if (item <= 0) continue; // ignore empty slot
            trapsList[ctr] = item;
            ctr++;
        }
    }
}