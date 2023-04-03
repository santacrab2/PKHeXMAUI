using static PKHeXMAUI.MainPage;
using static PKHeXMAUI.EncounterDB;
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class SearchSettings : ContentPage
{
	public SearchSettings()
	{
		InitializeComponent();
		EncSpecies.ItemsSource = datasourcefiltered.Species;
		EncMove1.ItemsSource = datasourcefiltered.Moves;
        EncMove2.ItemsSource = datasourcefiltered.Moves;
        EncMove3.ItemsSource = datasourcefiltered.Moves;
        EncMove4.ItemsSource = datasourcefiltered.Moves;
        EncVersion.ItemsSource = datasourcefiltered.Games;
      
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