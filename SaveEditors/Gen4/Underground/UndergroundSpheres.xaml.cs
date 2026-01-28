using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class UndergroundSpheres : ContentPage
{
    private readonly SAV4Sinnoh SAV;
    private readonly string[] ugSpheres;
    private readonly string[] ugSpheresSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    
    private class SphereEntry
    {
        public string SphereName { get; set; } = "";
        public byte Count { get; set; }
    }
    
    private readonly ObservableCollection<SphereEntry> finallist = [];
    
    public UndergroundSpheres(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = sav;

        ugSpheres = GameInfo.Strings.ugspheres;
        ugSpheresSorted = SanitizeList(ugSpheres);
        CV_UndergroundSpheres.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            var combo = new comboBox() { ItemSource = ugSpheresSorted };
            combo.SetBinding(comboBox.SelectedItemProperty, "SphereName", BindingMode.TwoWay);
            grid.Add(combo);
            var entry = new Entry() { Keyboard = Keyboard.Numeric };
            entry.SetBinding(Entry.TextProperty, "Count", BindingMode.TwoWay);
            grid.Add(entry, 1);
            return grid;
        });
        ReadUGData();
    }
    private void ReadUGData()
    {
        var spheresList = SAV.GetUGI_Spheres();
        var sphereCount = spheresList[MAX_SIZE..];
        spheresList = spheresList[..MAX_SIZE];
        
        for (int i = 0; i < spheresList.Length; i++)
            finallist.Add(new SphereEntry { SphereName = ugSpheres[spheresList[i]], Count = sphereCount[i] });
        CV_UndergroundSpheres.ItemsSource = finallist;
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
        foreach (var item in finallist)
        {
            if (item.SphereName == ugSpheres[0]) continue; // ignore empty slot
            SAV.GetUGI_Spheres()[ctr] = (byte)Array.IndexOf(ugSpheres, item.SphereName);
            SAV.GetUGI_Spheres()[ctr + MAX_SIZE] = item.Count;
            ctr++;
        }
    }
}