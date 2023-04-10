using static PKHeXMAUI.MainPage;
using static PKHeXMAUI.EncounterDB;
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class SearchSettings : ContentPage
{
	public SearchSettings()
	{
		InitializeComponent();
        var Any = new ComboItem("Any", 0);
        var EncSpeciesList = datasourcefiltered.Species.ToList();
        EncSpeciesList.Insert(0, Any);
        EncSpecies.ItemsSource = EncSpeciesList;
        var EncMoveList = datasourcefiltered.Moves.ToList();
        EncMoveList.RemoveAt(0); EncMoveList.Insert(0, Any);
        EncMove1.ItemsSource = EncMoveList;
        EncMove2.ItemsSource = EncMoveList;
        EncMove3.ItemsSource = EncMoveList;
        EncMove4.ItemsSource = EncMoveList;
        var EncVersionList = datasourcefiltered.Games.ToList();
        EncVersionList.RemoveAt(EncVersionList.Count - 1); EncVersionList.Insert(0, Any);
        EncVersion.ItemsSource = EncVersionList;


    }



    private void ChangeFontColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
		if (EncSpecies.IsDropDownOpen)
			EncSpecies.TextColor = Colors.Black;
		else
			EncSpecies.TextColor = Colors.White;
    }

    private void CloseSearchSettings(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void SaveSearchSettings(object sender, EventArgs e)
    {
        encSettings = new() 
        { 
            Species = (ushort)((ComboItem)EncSpecies.SelectedItem).Value,
            Format = sav.Generation,
            Generation = sav.Generation,
            Version = ((ComboItem)EncVersion.SelectedItem).Value,
        };
        encSettings.AddMove((ushort)((ComboItem)EncMove1.SelectedItem).Value);
        encSettings.AddMove((ushort)((ComboItem)EncMove2.SelectedItem).Value);
        encSettings.AddMove((ushort)((ComboItem)EncMove3.SelectedItem).Value);
        encSettings.AddMove((ushort)((ComboItem)EncMove4.SelectedItem).Value);
        encSettings.SearchShiny = ShinyCheck.IsChecked;
        encSettings.SearchEgg= EggCheck.IsChecked;
        Navigation.PopModalAsync();
    }
}