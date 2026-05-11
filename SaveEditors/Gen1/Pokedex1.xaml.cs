
using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class Pokedex1 : ContentPage
{
    private readonly SaveFile SAV;
    private readonly int MaxSpeciesID;
    private ObservableCollection<SimplePokedexInfo> pkdxInfo = [];
    public Pokedex1(SaveFile sav)
	{
		InitializeComponent();
        SAV = sav;
        MaxSpeciesID = SAV.MaxSpeciesID;
        var speciesNames = GameInfo.Strings.specieslist.AsSpan(1, MaxSpeciesID);
        foreach (var sp in speciesNames)
            pkdxInfo.Add(new SimplePokedexInfo(sp));
        SeenCollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            CheckBox isSeen = new();
            isSeen.SetBinding(CheckBox.IsCheckedProperty, new Binding("seen",BindingMode.TwoWay));
            Label specname = new() { VerticalOptions = LayoutOptions.Center };
            specname.SetBinding(Label.TextProperty, new Binding("Species"));
            grid.Add(isSeen);
            grid.Add(specname, 1);
            return grid;
        });
        CaughtCollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            CheckBox isSeen = new();
            isSeen.SetBinding(CheckBox.IsCheckedProperty, new Binding("caught", BindingMode.TwoWay));
            Label specname = new() { VerticalOptions = LayoutOptions.Center };
            specname.SetBinding(Label.TextProperty, new Binding("Species"));
            grid.Add(isSeen);
            grid.Add(specname, 1);
            return grid;
        });
        SeenCollection.ItemsSource = pkdxInfo;
        CaughtCollection.ItemsSource = pkdxInfo;
    }

    private void close(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void SetSeenAll(object sender, EventArgs e)
    {
        foreach (var info in pkdxInfo)
            info.seen = true;
    }
    private void SetCaughtAll(object sender, EventArgs e)
    {
        foreach (var info in pkdxInfo)
            info.caught = true;
    }
    private void SetSeenNone(object sender, EventArgs e)
    {
        foreach (var info in pkdxInfo)
            info.seen = false;
    }
    private void SetCaughtNone(object sender, EventArgs e)
    {
        foreach (var info in pkdxInfo)
            info.caught = false;
    }
    private void save(object sender, EventArgs e)
    {
        for(var i = 1; i < MaxSpeciesID; i++)
        {
            SAV.SetSeen((ushort)i, pkdxInfo[i].seen);
            SAV.SetCaught((ushort)i, pkdxInfo[i].caught);
        }
        Navigation.PopModalAsync();
    }
}
public class SimplePokedexInfo
{
    public bool seen { get; set; }
    public bool caught { get; set; }
    public ushort SpecieID { get; set; }
    public string Species { get; set; }
    public SimplePokedexInfo(string species)
    {
        Species = species;
        SpeciesName.TryGetSpecies(species, 2, out var specid);
        SpecieID = specid;
        seen = MainPage.sav.GetSeen(SpecieID);
        caught = MainPage.sav.GetCaught(SpecieID);
    }
}