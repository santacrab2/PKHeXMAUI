using PKHeX.Core;
using System.Runtime.CompilerServices;
using static PKHeXMAUI.MainPage;


namespace PKHeXMAUI;

public partial class EncounterDB : ContentPage
{
  
    public static PKHeX.Core.Searching.SearchSettings encSettings;
	public EncounterDB()
	{
		InitializeComponent();
        EncounterCollection.ItemTemplate = new DataTemplate(() =>
        {
            
            Grid grid = new Grid { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            Image image = new Image() { WidthRequest = 50, HeightRequest = 50 };
            Image shinysp = new Image() { Source = "rare_icon.png", WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Start };
            shinysp.TranslateTo(shinysp.TranslationX + 15, shinysp.TranslationY);
            image.SetBinding(Image.SourceProperty, "url");
            shinysp.SetBinding(Image.IsVisibleProperty, "EncounterInfo.IsShiny");
            Image EggSprite = new() { Source = "a_egg.png", HeightRequest=40, WidthRequest=40, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End };
            EggSprite.SetBinding(Image.IsVisibleProperty, "EncounterInfo.EggEncounter");
            grid.Add(image);
            grid.Add(shinysp);
            grid.Add(EggSprite);
            SwipeView swipe = new();
            SwipeItem view = new()
            {
                Text = "View",
                BackgroundColor = Colors.DeepSkyBlue,
                IconImageSource = "load.png"
            };
            view.Invoked += applyencpk;
            swipe.BottomItems.Add(view);
            swipe.Content = grid;
            return swipe;
        });
        EncounterCollection.ItemsLayout = new GridItemsLayout(6, ItemsLayoutOrientation.Vertical);
    }

    private void SetSearchSettings(object sender, EventArgs e)
    {
		Navigation.PushModalAsync(new SearchSettings());
    }

    public void applyencpk(object sender, EventArgs e)
    {
        IEncounterInfo enc = ((EncounterSprite)EncounterCollection.SelectedItem).EncounterInfo;
        pk = enc.ConvertToPKM(sav,EncounterCriteria.Unrestricted);
    }
    private void SearchEncountersClick(object sender, EventArgs e)
    {
        List<EncounterSprite> sprites = new();
        var Encounters = GetEncounters();
       foreach(var enc in Encounters)
        {
            sprites.Add(new(enc));
        }
        EncounterCollection.ItemsSource = sprites;
    }
    public List<IEncounterInfo> GetEncounters()
    {
        if (encSettings is { Species: 0, Moves.Count: 0 })
            return new List<IEncounterInfo>();
        var pk = sav.BlankPKM;

        var moves = encSettings.Moves.ToArray();
        var versions = encSettings.GetVersions(sav);
        var species = new[] { encSettings.Species };
        var results = GetAllSpeciesFormEncounters(species, sav.Personal, versions, moves, pk);
        if (encSettings.SearchEgg != null)
            results = results.Where(z => z.EggEncounter == encSettings.SearchEgg);
        if (encSettings.SearchShiny != null)
            results = results.Where(z => z.IsShiny == encSettings.SearchShiny);
        var comparer = new ReferenceComparer<IEncounterInfo>();
        results = results.Distinct(comparer);
        static bool IsPresentInGameSV(ISpeciesForm pk) => PersonalTable.SV.IsPresentInGame(pk.Species, pk.Form);
        static bool IsPresentInGameSWSH(ISpeciesForm pk) => PersonalTable.SWSH.IsPresentInGame(pk.Species, pk.Form);
        static bool IsPresentInGameBDSP(ISpeciesForm pk) => PersonalTable.BDSP.IsPresentInGame(pk.Species, pk.Form);
        static bool IsPresentInGameLA(ISpeciesForm pk) => PersonalTable.LA.IsPresentInGame(pk.Species, pk.Form);
        results = sav switch
        {
            SAV9SV => results.Where(IsPresentInGameSV),
            SAV8SWSH => results.Where(IsPresentInGameSWSH),
            SAV8BS => results.Where(IsPresentInGameBDSP),
            SAV8LA => results.Where(IsPresentInGameLA),
            _ => results.Where(z => z.Generation <= 7),
        };
        return results.ToList();
    }
    public IEnumerable<IEncounterInfo> GetAllSpeciesFormEncounters(IEnumerable<ushort> species, IPersonalTable pt, IReadOnlyList<GameVersion> versions, ushort[] moves, PKM pk)
    {
        var returnlist = new List<IEncounterInfo>();
        foreach (var s in species)
        {
            

            var pi = pt.GetFormEntry(s, 0);
            var fc = pi.FormCount;
           
            for (byte f = 0; f < fc; f++)
            {
                if (FormInfo.IsBattleOnlyForm(s, f, pk.Format))
                    continue;
                var encs = GetEncounters(s, f, moves, pk, versions);
                foreach (var enc in encs)
                    returnlist.Add(enc);
            }
            
        }
        return returnlist;
    }
    private IEnumerable<IEncounterInfo> GetEncounters(ushort species, byte form, ushort[] moves, PKM pk, IReadOnlyList<GameVersion> vers)
    {
        pk.Species = species;
        pk.Form = form;
        pk.SetGender(pk.GetSaneGender());
        EncounterMovesetGenerator.OptimizeCriteria(pk, sav);
        return EncounterMovesetGenerator.GenerateEncounters(pk, moves, vers);
    }
}
public class ReferenceComparer<T> : IEqualityComparer<T> where T : class
{
    public bool Equals(T? x, T? y)
    {
        if (x == null)
            return false;
        if (y == null)
            return false;
        return RuntimeHelpers.GetHashCode(x).Equals(RuntimeHelpers.GetHashCode(y));
    }

    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}
public class EncounterSprite 
{
   public IEncounterInfo EncounterInfo { get; set; }
    public string url { get; set; }

    public EncounterSprite(IEncounterInfo info)
    {
        
        EncounterInfo = info;
        if (info.Species == 0)
            url = $"";
        else
            url = $"a_{info.Species}{(info.Form > 0 ? $"_{info.Form}" : "")}.png";
    }

 
}