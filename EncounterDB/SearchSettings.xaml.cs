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
        EncSpecies.DisplayMemberPath = "Text";
        EncSpecies.ItemSource = EncSpeciesList;
        var EncMoveList = datasourcefiltered.Moves.ToList();
        EncMoveList.RemoveAt(0); EncMoveList.Insert(0, Any);
        EncMove1.DisplayMemberPath = "Text";
        EncMove1.ItemSource = EncMoveList;
        EncMove2.DisplayMemberPath = "Text";
        EncMove2.ItemSource = EncMoveList;
        EncMove3.DisplayMemberPath = "Text";
        EncMove3.ItemSource = EncMoveList;
        EncMove4.DisplayMemberPath = "Text";
        EncMove4.ItemSource = EncMoveList;
        var EncVersionList = datasourcefiltered.Games.ToList();
        EncVersionList.RemoveAt(EncVersionList.Count - 1); EncVersionList.Insert(0, Any);
        EncVersion.DisplayMemberPath= "Text";
        EncVersion.ItemSource = EncVersionList;
        if (encSettings != null)
        {
            EncSpecies.SelectedItem = datasourcefiltered.Species.FirstOrDefault(z => (ushort)z.Value == encSettings.Species)??new ComboItem("(None)",0);
            if (encSettings.Moves.Count >0)
                EncMove1.SelectedItem = EncMoveList.FirstOrDefault(z => z.Value == encSettings.Moves[0])??new("(None)",0);
            if (encSettings.Moves.Count >1)
                EncMove2.SelectedItem = EncMoveList.FirstOrDefault(z => z.Value == encSettings.Moves[1])?? new("(None)", 0);
            if (encSettings.Moves.Count>2)
                EncMove3.SelectedItem = EncMoveList.FirstOrDefault(z => z.Value == encSettings.Moves[2]) ?? new("(None)", 0);
            if (encSettings.Moves.Count>3)
                EncMove4.SelectedItem = EncMoveList.FirstOrDefault(z => z.Value == encSettings.Moves[3]) ?? new("(None)", 0);

            EncVersion.SelectedItem = EncVersionList.FirstOrDefault(z => z.Value == (int)encSettings.Version) ?? new ComboItem("(None)", 0);
            ShinyCheck.IsChecked = encSettings.SearchShiny??false;
            EggCheck.IsChecked = encSettings.SearchEgg??false;
        }
    }

    private void CloseSearchSettings(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void SaveSearchSettings(object sender, EventArgs e)
    {
        encSettings = new()
        {
            Species = (ushort)((ComboItem?)EncSpecies.SelectedItem??Any).Value,
            Context = sav.Context,
            Generation = sav.Generation,
            Version = (GameVersion)((ComboItem?)EncVersion.SelectedItem??Any).Value,
            Nature = (EncounterSettings.UsePkEditorAsCriteria ? pk.Nature : 0),
            Ability = (EncounterSettings.UsePkEditorAsCriteria ? pk.Ability : 0),
            Level = (EncounterSettings.UsePkEditorAsCriteria ? pk.CurrentLevel : (byte)0),
            Item = (EncounterSettings.UsePkEditorAsCriteria ? pk.HeldItem : 0)
        };
        encSettings.AddMove((ushort)((ComboItem?)EncMove1.SelectedItem??Any).Value);
        encSettings.AddMove((ushort)((ComboItem?)EncMove2.SelectedItem??Any).Value);
        encSettings.AddMove((ushort)((ComboItem?)EncMove3.SelectedItem??Any).Value);
        encSettings.AddMove((ushort)((ComboItem?)EncMove4.SelectedItem??Any).Value);
        encSettings.SearchShiny = ShinyCheck.IsChecked;
        encSettings.SearchEgg= EggCheck.IsChecked;
        EncounterMovesetGenerator.PriorityList = GetTypes();
        Navigation.PopModalAsync();
    }
    private EncounterTypeGroup[] GetTypes()
    {
        return SearchSettingsPage.Children.OfType<CheckBox>().Where(z => z.IsChecked && SearchSettingsPage.Children.IndexOf(z) >3).Select(z => z.StyleId)
            .Select(z => Enum.Parse<EncounterTypeGroup>(z)).ToArray();
    }
}