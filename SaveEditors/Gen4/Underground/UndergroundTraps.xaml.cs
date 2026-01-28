using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class UndergroundTraps : ContentPage
{
    private readonly SAV4Sinnoh SAV;
	private readonly string[] ugTraps;
    private readonly string[] ugTrapsSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    private ObservableCollection<TrapsEntry> trapsList = [];
    private class TrapsEntry
    {
        public string ItemName { get; set; } = "";
    }
    public UndergroundTraps(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = sav;

        ugTraps = GameInfo.Strings.ugtraps;
        ugTrapsSorted = SanitizeList(ugTraps);
        CV_UndergroundTraps.ItemTemplate = new DataTemplate(() =>
		{
			Grid grid = [];
            var combo = new comboBox() { ItemSource = ugTrapsSorted };
            combo.SetBinding(comboBox.SelectedItemProperty, ".", BindingMode.TwoWay);
            grid.Add(combo);
            return grid;
		});
        ReadUGData();
    }
    private void ReadUGData()
    {
        var originlist = SAV.GetUGI_Traps();
        for(int i = 0; i < originlist.Length; i++)
        {
            var itemID = originlist[i];
            var itemName = itemID <= 0 ? ugTraps[0] : ugTraps[itemID];
            trapsList.Add(new TrapsEntry { ItemName = itemName });
        }
        while (trapsList.Count < MAX_SIZE)
            trapsList.Add(new TrapsEntry() { ItemName = ugTraps[0] });
        CV_UndergroundTraps.ItemsSource = trapsList;
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
        foreach (var item in trapsList)
        {
            if (item.ItemName == ugTraps[0]) continue; // ignore empty slot
            SAV.GetUGI_Traps()[ctr] = (byte)Array.IndexOf(ugTraps,item.ItemName);
            ctr++;
        }
    }
}