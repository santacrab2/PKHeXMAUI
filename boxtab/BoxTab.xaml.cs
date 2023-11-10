using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using System.Linq;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeXMAUI.MainPage;
using PKHeX.Core.Injection;
using static PKHeXMAUI.LiveHex;

namespace PKHeXMAUI;

public partial class BoxTab : ContentPage
{
	public BoxTab()
	{
		InitializeComponent();
        boxnum.ItemsSource = Enumerable.Range(1, 32).ToArray();
        ICommand refreshCommand = new Command(() =>
        {
            try
            {
                if (Remote.Connected && ReadonChangeBox)
                {
                    var box = CurrentBox;
                    var len =
                           sav.BoxSlotCount
                           * (RamOffsets.GetSlotSize(Remote.Version) + RamOffsets.GetGapSize(Remote.Version));
                    var data = Remote.ReadBox(box, len).AsSpan();
                    sav.SetBoxBinary(data, box);
                }
                fillbox();
            }
            catch (Exception)
            {
                fillbox();
            }
            boxrefresh.IsRefreshing = false;
        });
        boxrefresh.Command = refreshCommand;
        boxnum.SelectedIndex = sav.CurrentBox;
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
            
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            Image image = new Image() { WidthRequest = 45, HeightRequest = 45 };
            Image shinysp = new Image() { Source = "rare_icon.png", WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Start };
            shinysp.TranslateTo(shinysp.TranslationX + 20, shinysp.TranslationY);
            Image Egg = new() { Source = "a_egg.png", HeightRequest = 50, WidthRequest = 50, VerticalOptions = LayoutOptions.End };
            Egg.SetBinding(Image.IsVisibleProperty, "pkm.IsEgg");
            Image ItemSprite = new Image() {  WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.End };
            ItemSprite.SetBinding(Image.SourceProperty, "ItemResource");
            ItemSprite.TranslateTo(ItemSprite.TranslationX + 18, ItemSprite.TranslationY);
            Image LegalSprite = new Image() { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Start, Source = "warn.png" };
            LegalSprite.TranslateTo(LegalSprite.TranslationX - 6, ItemSprite.TranslationY);
            LegalSprite.SetBinding(Image.IsVisibleProperty, "legal");
            image.SetBinding(Image.SourceProperty, "url");
            var gesture = new DragGestureRecognizer() { CanDrag = true };
            gesture.SetBinding(DragGestureRecognizer.DragStartingCommandParameterProperty, "boxie");
            gesture.DragStartingCommand = new Command<boxsprite>(DragStart);
            var drop = new DropGestureRecognizer() { AllowDrop = true };

            drop.Drop += DragStop;
            var tap = new TapGestureRecognizer();
            tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, "boxie");
            tap.Tapped += Tapety;
            shinysp.SetBinding(Image.IsVisibleProperty, "shiny");
            grid.Add(image);
            grid.Add(shinysp);
            grid.Add(ItemSprite);
            grid.Add(Egg);
            grid.Add(LegalSprite);
            grid.GestureRecognizers.Add(gesture);
            grid.GestureRecognizers.Add(tap);
            grid.GestureRecognizers.Add(drop);
            return grid;
        });
      
        boxview.ItemsLayout = new GridItemsLayout(6, ItemsLayoutOrientation.Vertical);
        boxview.ItemsSource = boxsprites;
        

    }
    private async void Tapety(object sender, TappedEventArgs e)
    {
        boxview.SelectedItem = e.Parameter;
        var result = await DisplayActionSheet("Slot", "cancel", "Delete", new string[] { "View", "Set" });
        switch (result)
        {
            case "Delete": del(sender,e); break;
            case "View": applypkfrombox(sender, e); break;
            case "Set": inject(sender, e); break;
        }
    }
    private void DragStart(boxsprite boxslot)
    {

        boxview.SelectedItem = boxslot;
        deleter.IsVisible = true;
        viewer.IsVisible = true;
    }
    private async void DragStop(object sender, DropEventArgs e)
    {
        Grid theG = (sender as Element)?.Parent as Grid;
        Image Replace = (Image)theG.Children.FirstOrDefault(x => x is Image);
        await DisplayAlert("test", $"{Replace.Source}", "cancel");
        var toreplace = boxsprites.FirstOrDefault(x=>x.url == Replace.Source.ToString().Replace("File: ",""));
        var toreplaceindex = boxsprites.IndexOf((boxsprite)toreplace);
        sav.SetBoxSlotAtIndex(((boxsprite)boxview.SelectedItem).pkm,boxnum.SelectedIndex,toreplaceindex);
        deleter.IsVisible = false;
        viewer.IsVisible = false;
        fillbox();
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
        try
        {
            if (Remote.Connected && InjectinSlot)
            {
                pk.ResetPartyStats();
                Remote.SendSlot(pk.EncryptedPartyData, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            }
            sav.SetBoxSlotAtIndex(pk, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            fillbox();
        }
        catch (Exception)
        {
            sav.SetBoxSlotAtIndex(pk, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            fillbox();
        }
    }
    private async void del(object sender, EventArgs e)
    {
        try
        {
            if (Remote.Connected && InjectinSlot)
            {
                Remote.SendSlot(EntityBlank.GetBlank(sav.Generation, sav.Version).EncryptedPartyData, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            }
            sav.SetBoxSlotAtIndex(EntityBlank.GetBlank(sav.Generation, sav.Version), boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            fillbox();
        }
        catch (Exception)
        {
            sav.SetBoxSlotAtIndex(EntityBlank.GetBlank(sav.Generation, sav.Version), boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            fillbox();
        }
    }
    private async void Generateliving(object sender, EventArgs e)
    {
        livingdexbutton.Text = "loading...";
        await Task.Delay(100);
        ModLogic.SetAlpha = PluginSettings.LivingDexSetAlpha;
        ModLogic.IncludeForms = PluginSettings.LivingDexAllForms;
        ModLogic.NativeOnly = PluginSettings.LivingDexNativeOnly;
        ModLogic.SetShiny = PluginSettings.LivingDexSetShiny;

        await Task.Run(copyboxdata);
        fillbox();
        livingdexbutton.Text = "Generate Living Dex";
    }
    public void copyboxdata()
    {
        Span<PKM> pkms = sav.GenerateLivingDex().ToArray();
        Span<PKM> bd = sav.BoxData.ToArray();
        pkms.CopyTo(bd);
        sav.BoxData = bd.ToArray();
        
    }
    private void changebox(object sender, EventArgs e)
    {
        CurrentBox = boxnum.SelectedIndex;
        sav.CurrentBox = CurrentBox;
        try
        {
            if (Remote.Connected && ReadonChangeBox)
            {
                var box = BoxTab.CurrentBox;
                var len =
                       sav.BoxSlotCount
                       * (RamOffsets.GetSlotSize(Remote.Version) + RamOffsets.GetGapSize(Remote.Version));
                var data = Remote.ReadBox(box, len).AsSpan();
                sav.SetBoxBinary(data, box);
            }

            fillbox();
        }
        catch (Exception) { fillbox(); }
        
        
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
        legal = !new LegalityAnalysis(pk9).Valid;
        if (pk9.Species == 0)
            legal = false;
        boxie = this;
    }
    public int[] NoFormSpriteSpecies = new[] { 664, 665, 744, 982, 855, 854, 869, 892, 1012, 1013 };
    public PKM pkm { get; set; }
    public string url { get; set; }
    public string species { get; set; }
    public bool shiny { get; set; }
    public string ItemResource { get; set; }
    public bool legal { get; set; }
    public boxsprite boxie {get;set;}
}