using static PKHeXMAUI.MainPage;
using static PKHeXMAUI.EncounterDB;
using PKHeX.Core;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;

namespace PKHeXMAUI;

public partial class SearchSettings : ContentPage
{
    public static ComboItem Any = new ComboItem("Any", 0);
    public SearchSettings()
	{
		InitializeComponent();
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
        if(encSettings != null)
        {
            EncSpecies.SelectedItem = datasourcefiltered.Species.Where(z => (ushort)z.Value == encSettings.Species).First();
            if (encSettings.Moves.Count >0 && encSettings.Moves[0]!=0)
                EncMove1.SelectedItem = EncMoveList.Where(z => z.Value == encSettings.Moves[0]).First();
            if (encSettings.Moves[1] != 0)
                EncMove2.SelectedItem = EncMoveList.Where(z => z.Value == encSettings.Moves[1]).First();
            if (encSettings.Moves[2] != 0)
                EncMove3.SelectedItem = EncMoveList.Where(z => z.Value == encSettings.Moves[2]).First();
            if (encSettings.Moves[3] != 0)
                EncMove4.SelectedItem = EncMoveList.Where(z => z.Value == encSettings.Moves[3]).First();
            
            EncVersion.SelectedItem = EncVersionList.Where(z=>z.Value == encSettings.Version).First();
            ShinyCheck.IsChecked = (bool)encSettings.SearchShiny;
            EggCheck.IsChecked = (bool)encSettings.SearchEgg;
        }

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
            Nature = (EncounterSettings.UsePkEditorAsCriteria ? pk.Nature : 0),
            Ability = (EncounterSettings.UsePkEditorAsCriteria ? pk.Ability : 0),
            Level = (EncounterSettings.UsePkEditorAsCriteria ? pk.CurrentLevel : 0),
            Item = (EncounterSettings.UsePkEditorAsCriteria ? pk.HeldItem : 0)
        };
        encSettings.AddMove((ushort)((ComboItem)EncMove1.SelectedItem??Any).Value);
        encSettings.AddMove((ushort)((ComboItem)EncMove2.SelectedItem??Any).Value);
        encSettings.AddMove((ushort)((ComboItem)EncMove3.SelectedItem??Any).Value);
        encSettings.AddMove((ushort)((ComboItem)EncMove4.SelectedItem??Any).Value);
        encSettings.SearchShiny = ShinyCheck.IsChecked;
        encSettings.SearchEgg= EggCheck.IsChecked;
        EncounterMovesetGenerator.PriorityList = GetTypes();
        Navigation.PopModalAsync();
    }
    private EncounterTypeGroup[] GetTypes()
    {
        return SearchSettingsPage.Children.OfType<CheckBox>().Where(z => z.IsChecked && SearchSettingsPage.Children.IndexOf(z) >3).Select(z => z.StyleId)
            .Select(z => (EncounterTypeGroup)Enum.Parse(typeof(EncounterTypeGroup), z)).ToArray();
    }
}