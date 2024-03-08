using static PKHeXMAUI.MainPage;
using static PKHeXMAUI.EncounterDB;
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class SearchSettings : ContentPage
{
    public static ComboItem Any = new("Any", 0);
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
            EncSpecies.SelectedItem = datasourcefiltered.Species.First(z => (ushort)z.Value == encSettings.Species);
            if (encSettings.Moves.Count >0)
                EncMove1.SelectedItem = EncMoveList.First(z => z.Value == encSettings.Moves[0]);
            if (encSettings.Moves.Count >1)
                EncMove2.SelectedItem = EncMoveList.First(z => z.Value == encSettings.Moves[1]);
            if (encSettings.Moves.Count>2)
                EncMove3.SelectedItem = EncMoveList.First(z => z.Value == encSettings.Moves[2]);
            if (encSettings.Moves.Count>3)
                EncMove4.SelectedItem = EncMoveList.First(z => z.Value == encSettings.Moves[3]);

            EncVersion.SelectedItem = EncVersionList.First(z=>z.Value == (int)encSettings.Version);
            ShinyCheck.IsChecked = (bool)encSettings.SearchShiny;
            EggCheck.IsChecked = (bool)encSettings.SearchEgg;
        }
    }

    private void ChangeFontColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
		EncSpecies.TextColor = EncSpecies.IsDropDownOpen ? Colors.Black : Colors.White;
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
            Version = (GameVersion)((ComboItem)EncVersion.SelectedItem).Value,
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