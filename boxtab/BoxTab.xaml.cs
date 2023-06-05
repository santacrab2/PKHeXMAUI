using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using System.Linq;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeXMAUI.MainPage;


namespace PKHeXMAUI;

public partial class BoxTab : ContentPage
{
	public BoxTab()
	{
		InitializeComponent();
        boxnum.ItemsSource = Enumerable.Range(1, 32).ToArray();
        ICommand refreshCommand = new Command(() =>
        {
			
			fillbox();
            boxrefresh.IsRefreshing = false;
        });
        boxrefresh.Command = refreshCommand;
        boxnum.SelectedIndex = 0;
    }
    public static IList<boxsprite> boxsprites = new List<boxsprite>();
    public static int CurrentBox = 0;
	public void fillbox()
	{
		
		boxsprites = new List<boxsprite>();
       //if(sav.GetBoxData(boxnum.SelectedIndex).Count() == 0) { sav.SetBoxBinary(BitConverter.GetBytes(0),boxnum.SelectedIndex); }
		foreach (var boxpk in sav.GetBoxData(boxnum.SelectedIndex))
		{
			var boxinfo = new boxsprite(boxpk);
			boxsprites.Add(boxinfo);
		}
		
        boxview.ItemTemplate = new DataTemplate(() =>
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
            shinysp.TranslateTo(shinysp.TranslationX + 20, shinysp.TranslationY);
            Image Egg = new() { Source = "a_egg.png", HeightRequest = 50, WidthRequest = 50, VerticalOptions = LayoutOptions.End };
            Egg.SetBinding(Image.IsVisibleProperty, "pkm.IsEgg");
            Image ItemSprite = new Image() {  WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.End };
            ItemSprite.SetBinding(Image.SourceProperty, "ItemResource");
            ItemSprite.TranslateTo(ItemSprite.TranslationX + 20, ItemSprite.TranslationY);
            image.SetBinding(Image.SourceProperty, "url");
            shinysp.SetBinding(Image.IsVisibleProperty, "shiny");
            grid.Add(image);
            grid.Add(shinysp);
            grid.Add(ItemSprite);
            grid.Add(Egg);
            SwipeView swipe = new();
            SwipeItem view = new()
            {
                Text = "View",
                BackgroundColor = Colors.DeepSkyBlue,
                IconImageSource = "load.png"
            };
            view.Invoked += applypkfrombox;
            swipe.BottomItems.Add(view);
            SwipeItem Set = new()
            {
                Text = "Set",
                BackgroundColor = Colors.Green,
                IconImageSource = "dump.png"
            };
            Set.Invoked += inject;
            swipe.TopItems.Add(Set);
            SwipeItem Delete = new()
            {
                Text = "Delete",
                BackgroundColor = Colors.Red,
                IconImageSource = "delete.png"
            };
            Delete.Invoked += del;
            swipe.LeftItems.Add(Delete);
            swipe.Content = grid;
            return swipe;
        });
      
        boxview.ItemsLayout = new GridItemsLayout(6, ItemsLayoutOrientation.Vertical);
        boxview.ItemsSource = boxsprites;
        

    }
    private async void applypkfrombox(object sender, EventArgs e)
    {
        
            if (((boxsprite)boxview.SelectedItem).pkm.Species != 0)
            {
                pk = ((boxsprite)boxview.SelectedItem).pkm;
            }
        
        
        
    }
    private async void inject(object sender, EventArgs e)
    {
        sav.SetBoxSlotAtIndex(pk, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
        fillbox();
    }
    private async void del(object sender, EventArgs e)
    {
       
        sav.SetBoxSlotAtIndex(EntityBlank.GetBlank(sav.Generation, sav.Version), boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
        fillbox();
    }
    private async void Generateliving(object sender, EventArgs e)
    {
        livingdexbutton.Text = "loading...";
        await Task.Delay(100);
        ModLogic.SetAlpha = PluginSettings.LivingDexSetAlpha;
        ModLogic.IncludeForms = PluginSettings.LivingDexAllForms;
        ModLogic.NativeOnly = PluginSettings.LivingDexNativeOnly;
        ModLogic.SetShiny = PluginSettings.LivingDexSetShiny;

        copyboxdata();
        fillbox();
        livingdexbutton.Text = "Generate Living Dex";
    }
    private void copyboxdata()
    {
        Span<PKM> pkms = sav.GenerateLivingDex().ToArray();
        Span<PKM> bd = sav.BoxData.ToArray();
        pkms.CopyTo(bd);
        sav.BoxData = bd.ToArray();
    }
    private void changebox(object sender, EventArgs e)
    {
        CurrentBox = boxnum.SelectedIndex;
        fillbox();
        
    }

    private void openBatchEditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new BatchEditor());
    }
}
public class boxsprite
{
	public boxsprite(PKM pk9)
	{
		pkm = pk9;
		species = $"{(Species)pk9.Species}";
        if (pk9.Species == 0)
            url = $"";
        else
            url = $"a_{pkm.Species}{((pkm.Form > 0 && !NoFormSpriteSpecies.Contains(pkm.Species)) ? $"_{pkm.Form}" : "")}.png";
        shiny = (pk9.IsShiny && pk9.Species != 0);
        if (sav is SAV9SV)
        {
            ItemResource = $"aitem_{pkm.HeldItem}.png";
            
        }
        else
        {
            ItemResource = $"bitem_{pkm.HeldItem}.png";
          
        }
    }
    public int[] NoFormSpriteSpecies = new[] { 664, 665, 744, 982, 855, 854, 869,892 };
    public PKM pkm { get; set; }
	public string url { get; set; }
	public string species { get; set; }
    public bool shiny { get; set; }
    public string ItemResource { get; set; }
}