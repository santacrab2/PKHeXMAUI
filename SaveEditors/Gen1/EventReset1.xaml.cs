
using PKHeX.Core;
namespace PKHeXMAUI;

public partial class EventReset1 : ContentPage
{
    private readonly G1OverworldSpawner Overworld;
    public EventReset1(SAV1 sav)
	{
		InitializeComponent();
        Overworld = new G1OverworldSpawner(sav);
        InitializeButtons();
    }
    private void InitializeButtons()
    {
        var pairs = Overworld.GetFlagPairs().OrderBy(z => z.Name);
        foreach (var pair in pairs)
        {
            var split = pair.Name.Split('_');
            var specName = split[0][G1OverworldSpawner.FlagPropertyPrefix.Length..];

            // convert species name to current localization language
            SpeciesName.TryGetSpecies(specName, (int)LanguageID.English, out var species);
            var pkmname = GameInfo.Strings.specieslist[species];

            if (split.Length != 1)
                pkmname += $" {split[1]}";
            var b = new Button
            {
                Text = pkmname,
                IsEnabled = pair.IsHidden,
            };
            b.Clicked += (s, e) =>
            {
                pair.Reset();
                b.IsEnabled = false;
            };
            eventstack.Children.Add(b);
        }
        var cl = new Button
        {
            Text = "Close"
        };
        cl.Clicked += (s, e) => Navigation.PopModalAsync();
        eventstack.Children.Add(cl);
    }
}