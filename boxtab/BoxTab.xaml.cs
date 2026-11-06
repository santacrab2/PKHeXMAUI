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
        boxnum.ItemsSource = Enumerable.Range(1, sav.BoxCount).ToArray();
        ICommand refreshCommand = new Command(() =>
        {
            try
            {
                if (Remote.Connected && ReadonChangeBox)
                {
                    var box = CurrentBox;
                    var len = sav.BoxSlotCount * (RamOffsets.GetSlotSize(Remote.Version) + RamOffsets.GetGapSize(Remote.Version));
                    var data = Remote.ReadBox(box, len);
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
    public static IList<boxsprite> boxsprites = [];
    public static int CurrentBox = 0;
	public void fillbox()
	{
		boxsprites = [];
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
            shinysp.TranslateToAsync(shinysp.TranslationX + 20, shinysp.TranslationY);
            Image Egg = new() { Source = SpeciesSpriteUtil.EggFileName(sav), HeightRequest = 50, WidthRequest = 50, VerticalOptions = LayoutOptions.End };
            Egg.SetBinding(Image.IsVisibleProperty, "pkm.IsEgg");
            Image ItemSprite = new() {  WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.End };
            ItemSprite.SetBinding(Image.SourceProperty, "ItemResource");
            ItemSprite.TranslateToAsync(ItemSprite.TranslationX + 18, ItemSprite.TranslationY);
            Image LegalSprite = new() { WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Start, Source = "warn.png" };
            LegalSprite.TranslateToAsync(LegalSprite.TranslationX - 6, ItemSprite.TranslationY);
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
    private async void Tapety(object? sender, TappedEventArgs? e)
    {
        boxview.SelectedItem = e?.Parameter;

        var result = await DisplayActionSheetAsync(L10n.T("code_slot_e_parameter_slotnumber", ((boxsprite?)e?.Parameter)?.SlotNumber), L10n.T("box_cancel"), L10n.T("box_delete"), [L10n.T("box_view"), L10n.T("code_write")]);
        switch (result)
        {
            case "Delete": del(sender,e); break;
            case "View": applypkfrombox(sender, e); break;
            case "Write": inject(sender, e); break;
            case "Cancel": boxview.SelectedItem = null; break;
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
    private async void DragStop(object? sender, DropEventArgs? e)
    {
        Grid theG = (sender as Element)?.Parent as Grid??[];
        Label Replace = (Label?)theG.Children.FirstOrDefault(x => x is Label)??new();
        var toreplace = boxsprites.FirstOrDefault(x=>x.SlotNumber == Replace.Text)??new boxsprite(EntityBlank.GetBlank(sav.Generation),0);
        var toreplaceindex = boxsprites.IndexOf((boxsprite)toreplace);
        if (boxview.SelectedItem is not null)
        {
            try
            {
                sav.SetBoxSlotAtIndex(((boxsprite)boxview.SelectedItem).pkm, boxnum.SelectedIndex, toreplaceindex);
                sav.SetBoxSlotAtIndex(((boxsprite)toreplace).pkm, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
                if (Remote.Connected && InjectinSlot)
                {
                    Span<byte> selecteddata = stackalloc byte[MainPage.sav.SIZE_PARTY];
                    ((boxsprite)boxview.SelectedItem).pkm.WriteEncryptedDataParty(selecteddata);
                    Remote.SendSlot(selecteddata, boxnum.SelectedIndex, toreplaceindex);
                    Span<byte> replacedata = stackalloc byte[MainPage.sav.SIZE_PARTY];
                    ((boxsprite)toreplace).pkm.WriteEncryptedDataParty(replacedata);
                    Remote.SendSlot(replacedata, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
                }
            }
            catch (Exception)
            {
                sav.SetBoxSlotAtIndex((PKM?)e?.Data?.Properties["PKM"]??EntityBlank.GetBlank(sav.Generation), boxnum.SelectedIndex, toreplaceindex);
                if (Remote.Connected && InjectinSlot)
                {
                    Span<byte> data =  new(new byte[MainPage.sav.SIZE_PARTY]);
                    ((PKM?)e?.Data?.Properties["PKM"]??EntityBlank.GetBlank(sav.Generation)).WriteEncryptedDataParty(data);
                    Remote.SendSlot(data, boxnum.SelectedIndex, toreplaceindex);
                }
            }
        }
        else
        {
            sav.SetBoxSlotAtIndex((PKM?)e?.Data?.Properties["PKM"]??EntityBlank.GetBlank(sav.Generation), boxnum.SelectedIndex, toreplaceindex);
            if(Remote.Connected && InjectinSlot)
            {
                Span<byte> data = stackalloc byte[MainPage.sav.SIZE_PARTY];
                ((PKM?)e?.Data?.Properties["PKM"]??EntityBlank.GetBlank(sav.Generation)).WriteEncryptedDataParty(data);
                Remote.SendSlot(data, boxnum.SelectedIndex, toreplaceindex);
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
    /// <summary>
    /// 通知编辑器刷新为指定的宝可梦。
    /// 从箱子「查看」时必须调用，否则编辑器界面与合法性判定仍停留在上一只宝可梦，
    /// 会出现「箱子里看着合法，进编辑器就报非法」的不一致。
    /// </summary>
    private static void RefreshEditorFor(PKM target)
    {
        try
        {
            var page = MainPage.Instance;
            if (page is null)
                return;
            page.applymainpkinfo(target);
            page.checklegality(target);
        }
        catch (Exception ex)
        {
            CrashLog.Write("箱子查看刷新编辑器", ex);
        }
    }

    private async void applypkfrombox(object? sender, EventArgs? e)
    {
            if (((boxsprite)boxview.SelectedItem).pkm.Species != 0)
            {
                pk = ((boxsprite)boxview.SelectedItem).pkm;
                // 从箱子查看时必须刷新编辑器界面与合法性判定，
                // 否则编辑器仍显示上一只宝可梦的合法/非法状态，与箱子里的显示不一致。
                RefreshEditorFor(pk);
            }
    }
    private async void applypkfromboxDrop(object? sender, DropEventArgs? e)
    {
        if (boxview.SelectedItem is not null)
        {
            if (((boxsprite)boxview.SelectedItem).pkm.Species != 0)
            {
                pk = ((boxsprite)boxview.SelectedItem).pkm;
                RefreshEditorFor(pk);
            }
        }
            deleter.IsVisible = false;
            viewer.IsVisible = false;
            LB_view.IsVisible = false;
            LB_delete.IsVisible = false;
            Sharer.IsVisible = false;
            LB_Share.IsVisible = false;
    }
    private async void inject(object? sender, EventArgs? e)
    {
        try
        {
            if (Remote.Connected && InjectinSlot)
            {
                pk.ResetPartyStats();
                Span<byte> data = stackalloc byte[MainPage.sav.SIZE_PARTY];
                pk.WriteEncryptedDataParty(data);
                Remote.SendSlot(data, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
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
    private async void del(object? sender, EventArgs? e)
    {
        try
        {
            if (Remote.Connected && InjectinSlot)
            {
                Span<byte> data = stackalloc byte[MainPage.sav.SIZE_PARTY];
                EntityBlank.GetBlank(sav).WriteEncryptedDataParty(data);
                Remote.SendSlot(data, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            }
            sav.SetBoxSlotAtIndex(EntityBlank.GetBlank(sav), boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            fillbox();
        }
        catch (Exception)
        {
            sav.SetBoxSlotAtIndex(EntityBlank.GetBlank(sav), boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
            fillbox();
        }
    }
    private async void delDrop(object? sender, DropEventArgs? e)
    {
        if (boxview.SelectedItem is not null)
        {
            try
            {
                if (Remote.Connected && InjectinSlot)
                {
                    Span<byte> data = stackalloc byte[MainPage.sav.SIZE_PARTY];
                    EntityBlank.GetBlank(sav).WriteEncryptedDataParty(data);
                    Remote.SendSlot(data, boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
                }
                sav.SetBoxSlotAtIndex(EntityBlank.GetBlank(sav), boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
                fillbox();
            }
            catch (Exception)
            {
                sav.SetBoxSlotAtIndex(EntityBlank.GetBlank(sav), boxnum.SelectedIndex, boxsprites.IndexOf((boxsprite)boxview.SelectedItem));
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
    private async void ShareDrop(object? sender, DropEventArgs? e)
    {
        PKM pkm = boxview.SelectedItem is not null ? ((boxsprite)boxview.SelectedItem).pkm : (PKM?)e?.Data?.Properties["PKM"]??EntityBlank.GetBlank(sav.Generation);
        var TempPath = Path.Combine(FileSystem.CacheDirectory, pkm.FileName);
        File.WriteAllBytes(TempPath, pkm.Data);
        await Share.RequestAsync(new ShareFileRequest()
        {
            Title = L10n.T("code_share_pkm"),
            File = new ShareFile(TempPath)
        });
        deleter.IsVisible = false;
        viewer.IsVisible = false;
        LB_view.IsVisible = false;
        LB_delete.IsVisible = false;
        Sharer.IsVisible = false;
        LB_Share.IsVisible = false;
    }
    private async void GenerateRandomTeam(object sender, EventArgs e)
    {
        RandomTeamButton.Text = L10n.T("box_loading");
        await Task.Delay(100);
        ModLogic.SetAlpha = PluginSettings.LivingDexSetAlpha;
        ModLogic.IncludeForms = PluginSettings.LivingDexAllForms;
        ModLogic.SetShiny = PluginSettings.LivingDexSetShiny;

        await Task.Run(copyboxdata);
        fillbox();
        RandomTeamButton.Text = L10n.T("box_generate_random_team");
    }
    public void copyboxdata()
    {
        // GetSixRandomMons 是 AutoMod 26.7.8 的扩展方法，与 Core 26.7.7 不配套时可能抛
        // MissingMethodException，这里改用 Core 自带的遭遇生成器实现同样的效果。
        PKM[] pkms = GetSixRandomMonsCore();
        sav.BoxData ??= [];
        var empties = AlmService.FindAllEmptySlots(sav.BoxData);
        for (int i = 0; i < 6 && i < pkms.Length; i++)
            sav.SetBoxSlotAtIndex(pkms[i], empties.Count > i ? empties[i] : i);
    }

    /// <summary>用 Core 自带的遭遇生成器取 6 只随机宝可梦（替代 AutoMod 的 GetSixRandomMons）。</summary>
    private static PKM[] GetSixRandomMonsCore()
    {
        var result = new List<PKM>();
        var generation = AlmService.GetGeneration(sav.Context);
        try
        {
            var blank = EntityBlank.GetBlank(generation);
            var generator = EncounterGenerator.GetGenerator((GameVersion)sav.Version, generation);
            var info = new LegalInfo(blank, new List<CheckResult>());
            var rng = new Random();
            var encounters = generator.GetEncounters(blank, Array.Empty<EvoCriteria>(), info).ToList();
            for (int i = 0; i < 6 && encounters.Count > 0; i++)
            {
                var enc = encounters[rng.Next(encounters.Count)];
                if (enc is IEncounterConvertible conv)
                {
                    var pkm = conv.ConvertToPKM(sav);
                    if (pkm is not null)
                        result.Add(pkm);
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("生成随机队伍", ex);
        }
        while (result.Count < 6)
            result.Add(EntityBlank.GetBlank(generation));
        return result.ToArray();
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
                var data = Remote.ReadBox(box, len);
                sav.SetBoxBinary(data, box);
            }

            fillbox();
        }
        catch (Exception) { fillbox(); }
    }
    private void displayOpts(object sender, DragEventArgs e) => DisplayOptions();

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        fillbox();
    }
}
public class boxsprite
{
    public boxsprite(PKM pk9,int slot)
    {
        SlotNumber = slot.ToString();
        pkm = pk9;
        species = $"{(Species)pk9.Species}";
        // 按存档世代选图集：Gen9 用 a_（朱紫造型），Gen1~8 用 b_（历代造型）
        // 闪光个体用对应世代的闪光造型，星标（rare_icon.png）另外叠在旁边
        url = pk9.Species == 0 ? "" : SpeciesSpriteUtil.FileName(sav, pkm.Species, pkm.Form, pk9.IsShiny);
        shiny = (pk9.IsShiny && pk9.Species != 0);
        ItemResource = ItemSpriteUtil.FileName(pkm.HeldItem, sav.Context, sav.Generation >= 9);
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