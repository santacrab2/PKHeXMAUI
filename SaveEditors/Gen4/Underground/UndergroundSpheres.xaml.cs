using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class UndergroundSpheres : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SAV4Sinnoh SAV;
    private readonly string[] ugSpheres;
    private readonly string[] ugSpheresSorted;
    private const int MAX_SIZE = SAV4Sinnoh.UG_POUCH_SIZE;
    public UndergroundSpheres(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = (SAV4Sinnoh)(Origin = sav).Clone();

        ugSpheres = GameInfo.Strings.ugspheres;
        ugSpheresSorted = SanitizeList(ugSpheres);
        CV_UndergroundSpheres.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            var combo = new comboBox() { ItemSource = ugSpheres };
            combo.SetBinding(comboBox.SelectedIndexProperty, "Item1");
            grid.Add(combo);
            var entry = new Entry() { Keyboard = Keyboard.Numeric };
            entry.SetBinding(Entry.TextProperty, "Item2");
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
        ObservableCollection<Tuple<byte, byte>> finallist = [];
        for (int i = 0; i < spheresList.Length; i++)
            finallist.Add(new Tuple<byte, byte>(spheresList[i], sphereCount[i]));
        CV_UndergroundSpheres.ItemsSource = finallist;
    }
    private static string[] SanitizeList(string[] inputlist)
    {
        string[] listSorted = Array.FindAll(inputlist, x => !string.IsNullOrEmpty(x));
        Array.Sort(listSorted);

        return listSorted;
    }
}