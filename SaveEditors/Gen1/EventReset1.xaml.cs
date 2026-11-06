
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
                // 按钮始终保持可点：早期版本点一次就 IsEnabled=false，
                // 结果每个事件重置按钮只能按一次，想再点就没反应了。
                IsEnabled = true,
            };
            b.Clicked += (s, e) =>
            {
                pair.Reset();
                UpdateButtonState(b, pair);
            };
            UpdateButtonState(b, pair);
            eventstack.Children.Add(b);
        }
        var cl = new Button
        {
            Text = L10n.T("common_close")
        };
        cl.Clicked += (s, e) => Navigation.PopModalAsync();
        eventstack.Children.Add(cl);
    }

    /// <summary>用颜色区分「事件标志仍是隐藏的（可重置）」与「已重置过」，但按钮不禁用。</summary>
    private static void UpdateButtonState(Button b, FlagPairG1Detail pair)
    {
        var pending = pair.IsHidden;
        b.TextColor = pending ? Colors.Black : Colors.Gray;
        b.Opacity = pending ? 1.0 : 0.55;
    }
}