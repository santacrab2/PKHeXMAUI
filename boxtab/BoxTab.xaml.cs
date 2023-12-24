
using System.Windows.Input;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeXMAUI.MainPage;
using PKHeX.Core.Injection;

namespace PKHeXMAUI;

public partial class BoxTab : ContentPage
{
	public BoxTab()
	{
		InitializeComponent();
        var viewdrop = new DropGestureRecognizer() { AllowDrop = true };
        viewdrop.Drop += applypkfromboxDrop;
        viewer.GestureRecognizers.Add(viewdrop);
        var deldrop = new DropGestureRecognizer() { AllowDrop=true };
        deldrop.Drop += delDrop;
        deleter.GestureRecognizers.Add(deldrop);
        var sharedrop = new DropGestureRecognizer() { AllowDrop = true };
        sharedrop.Drop += ShareDrop;
        Sharer.GestureRecognizers.Add(sharedrop);
        boxnum.ItemsSource = Enumerable.Range(1, 32).ToArray();
        ICommand refreshCommand = new Command(() =>
        {
            try
            {
                if (Remote.Connected && ReadonChangeBox)
                {
                    var box = CurrentBox;
                    var len = sav.BoxSlotCount * (RamOffsets.GetSlotSize(Remote.Version) + RamOffsets.GetGapSize(Remote.Version));
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
        int i = 0;
        foreach (var boxpk in sav.GetBoxData(boxnum.SelectedIndex))
		{
			var boxinfo = new boxsprite(boxpk,i);
			boxsprites.Add(boxinfo);
            i++;
		}

        boxview.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            Border border = new() { Stroke = Colors.Black, BackgroundColor = Colors.Transparent };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            Label SlotNumber = new() {FontSize=24, IsVisible = false};
            SlotNumber.SetBinding(Label.TextProperty, "SlotNumber");
            Image image = new() { WidthRequest = 45, HeightRequest = 45 };
            Image shinysp = new() { Source = "rare_icon.png", WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Start };
            shinysp.TranslateTo(shinysp.TranslationX + 20, shinysp.TranslationY);
            Image Egg = new() { Source = "a_egg.png", HeightRequest = 50, WidthRequest = 50, VerticalOptions = LayoutOptions.End };
            Egg.SetBinding(Image.IsVisibleProperty, "pkm.IsEgg");
            Image ItemSprite = new() {  WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.End };
            ItemSprite.SetBinding(Image.SourceProperty, "ItemResource");
            ItemSprite.TranslateTo(ItemSprite.TranslationX + 18, ItemSprite.TranslationY);
            Image LegalSprite = new() { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Start, Source = "warn.png" };
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
            grid.Add(SlotNumber);
            grid.GestureRecognizers.Add(gesture);
            grid.GestureRecognizers.Add(tap);
            grid.GestureRecognizers.Add(drop);
            border.Content = grid;
            return border;
        });

        boxview.ItemsLayout = new GridItemsLayout(6, ItemsLayoutOrientation.Vertical);
        boxview.ItemsSource = boxsprites;
    }
    private async void Tapety(object sender, TappedEventArgs e)
    {
        boxview.SelectedItem = e.Parameter;

        var result = await DisplayActionSheet($"Slot {((boxsprite)e.Parameter).SlotNumber}", "cancel", "Delete", ["View", "Set"]);
        switch (result)
        {
            case "Delete": del(sender,e); break;
            case "View": applypkfrombox(sender, e); break;
            case "Set": inject(sender, e); break;
        }
    }
    public void DisplayOptions()
    {
        LB_view.IsVisible = true;
        LB_delete.IsVisible = true;
        deleter.Source = "delete.png";
        deleter.IsVisible = true;
        viewer.Source = "load.png";
        viewer.IsVisible = true;
        Sharer.Source = "export.png";
        Sharer.IsVisible = true;
        LB_Share.IsVisible = true;
    }
    private void DragStart(boxsprite boxslot)
    {
        boxview.SelectedItem = boxslot;
        LB_view.IsVisible = true;
        LB_delete.IsVisible = true;
        deleter.Source = "delete.png";
        deleter.IsVisible = true;
        viewer.Source = "load.png";
        viewer.IsVisible = true;
        Sharer.Source = "export.png";
        Sharer.IsVisible = true;
        LB_Share.IsVisible = true;
    }
    private async void DragStop(object sender, DropEventArgs e)
    {
        Grid theG = (sender as Element)?.Parent as Grid;
        Label Replace = (Label)theG.Children.FirstOrDefault(x => x is Label);
        var toreplace = boxsprites.FirstOrDefault(x=>x.SlotNumber == Replace.Text);
        var toreplaceindex = boxsprites.IndexOf((boxsprite)toreplace);
        if (boxview.SelectedItem is not null)
        {
            try
            {
                sav.SetBoxSlotAtIndex(((boxsprite)boxview.SelectedItem).pkm, boxnum.SelectedIndex, toreplaceindex);
                sav.SetBoxSlotAtIndex(((boxsprite)toreplace).pkm, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
                if (Remote.Connected && InjectinSlot)
                {
                    Remote.SendSlot(((boxsprite)boxview.SelectedItem).pkm.EncryptedPartyData, boxnum.SelectedIndex, toreplaceindex);
                    Remote.SendSlot(((boxsprite)toreplace).pkm.EncryptedPartyData, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
                }
            }
            catch (Exception)
            {
                sav.SetBoxSlotAtIndex((PKM)e.Data.Properties["PKM"], boxnum.SelectedIndex, toreplaceindex);
                if (Remote.Connected && InjectinSlot)
                {
                    Remote.SendSlot(((PKM)e.Data.Properties["PKM"]).EncryptedPartyData, boxnum.SelectedIndex, toreplaceindex);
                }
            }
        }
        else
        {
            sav.SetBoxSlotAtIndex((PKM)e.Data.Properties["PKM"], boxnum.SelectedIndex, toreplaceindex);
            if(Remote.Connected && InjectinSlot)
            {
                Remote.SendSlot(((PKM)e.Data.Properties["PKM"]).EncryptedPartyData, boxnum.SelectedIndex, toreplaceindex);
            }
        }
        deleter.IsVisible = false;
        viewer.IsVisible = false;
        LB_view.IsVisible = false;
        LB_delete.IsVisible = false;
        Sharer.IsVisible = false;
        LB_Share.IsVisible = false;
        fillbox();
    }
    private async void applypkfrombox(object sender, EventArgs e)
    {
            if (((boxsprite)boxview.SelectedItem).pkm.Species != 0)
            {
                pk = ((boxsprite)boxview.SelectedItem).pkm;
            }
    }
    private async void applypkfromboxDrop(object sender, DropEventArgs e)
    {
        if (boxview.SelectedItem is not null)
        {
            if (((boxsprite)boxview.SelectedItem).pkm.Species != 0)
            {
                pk = ((boxsprite)boxview.SelectedItem).pkm;
            }
        }
            deleter.IsVisible = false;
            viewer.IsVisible = false;
            LB_view.IsVisible = false;
            LB_delete.IsVisible = false;
            Sharer.IsVisible = false;
            LB_Share.IsVisible = false;
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
    private async void delDrop(object sender, DropEventArgs e)
    {
        if (boxview.SelectedItem is not null)
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
            deleter.IsVisible = false;
            viewer.IsVisible = false;
            LB_view.IsVisible = false;
            LB_delete.IsVisible = false;
            Sharer.IsVisible = false;
            LB_Share.IsVisible = false;
    }
    private async void ShareDrop(object sender, DropEventArgs e)
    {
        PKM pkm = boxview.SelectedItem is not null ? ((boxsprite)boxview.SelectedItem).pkm : (PKM)e.Data.Properties["PKM"];
        var TempPath = Path.Combine(FileSystem.CacheDirectory, pkm.FileName);
        File.WriteAllBytes(TempPath, pkm.Data);
        await Share.RequestAsync(new ShareFileRequest()
        {
            Title = "Share PKM",
            File = new ShareFile(TempPath)
        });
        deleter.IsVisible = false;
        viewer.IsVisible = false;
        LB_view.IsVisible = false;
        LB_delete.IsVisible = false;
        Sharer.IsVisible = false;
        LB_Share.IsVisible = false;
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
    public boxsprite(PKM pk9,int slot)
    {
        SlotNumber = slot.ToString();
        pkm = pk9;
        species = $"{(Species)pk9.Species}";
        url = pk9.Species == 0 ? "" : $"a_{pkm.Species}{((pkm.Form > 0 && !NoFormSpriteSpecies.Contains(pkm.Species)) ? $"_{pkm.Form}" : "")}.png";
        shiny = (pk9.IsShiny && pk9.Species != 0);
        ItemResource = sav is SAV9SV ? $"aitem_{pkm.SpriteItem}.png" : $"bitem_{pkm.SpriteItem}.png";
        legal = !new LegalityAnalysis(pk9).Valid;
        if (pk9.Species == 0)
            legal = false;
        boxie = this;
    }
    public int[] NoFormSpriteSpecies = [664, 665, 744, 982, 855, 854, 869, 892, 1012, 1013];
    public PKM pkm { get; set; }
    public string url { get; set; }
    public string species { get; set; }
    public bool shiny { get; set; }
    public string ItemResource { get; set; }
    public bool legal { get; set; }
    public string SlotNumber { get; set; }
    public boxsprite boxie {get;set;}
}