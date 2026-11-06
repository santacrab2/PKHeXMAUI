using CommunityToolkit.Maui.Storage;
using Octokit;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using PKHeX.Core.Injection;
using System.Collections;
using System.Globalization;
using System.Net.Sockets;
using System.Windows.Input;
namespace PKHeXMAUI;

public partial class MainPage : ContentPage
{
    /// <summary>
    /// 当前编辑器页面实例。箱子页等其它页面需要通知编辑器刷新时使用
    /// （AppShell.xaml 里的 x:Name 字段是 private，跨类无法直接访问）。
    /// </summary>
    public static MainPage? Instance { get; private set; }

    public static PKM pk = EntityBlank.GetBlank(9);
    public static LegalityAnalysis la = new(pk);
    public static SaveFile sav = AppShell.AppSaveFile??BlankSaveFile.Get(EntityContext.Gen9,"");
    public static FilteredGameDataSource datasourcefiltered = new(sav, GameInfo.Sources);
    public static Socket SwitchConnection = new(SocketType.Stream, ProtocolType.Tcp);
    public static string spriteurl = "iconp.png";
    public static string ipaddy = "";
    public static string itemspriteurl = "";
    public bool SkipTextChange = false;
    /// <summary>
    /// 「形态数 &gt; 1 但外观看起来一样」的物种（朱紫里的多形态但同一造型的几个 dex）。
    /// 精灵图的形态后缀现在改由 <see cref="SpeciesSpriteUtil"/> 按资源存在性自动回退，
    /// 这个列表不再参与文件名拼接，保留仅作参考。
    /// </summary>
    public static int[] NoFormSpriteSpecies = [664, 665, 744, 982, 855, 854, 869,892,1012,1013];
    public bool FirstLoad = true;
    public static PokeSysBotMini Remote = new(LiveHeXVersion.SV_v301, new SysBotMini());
    public static bool ReadonChangeBox = Preferences.Get("ReadonChangeBox", true);
    public static bool InjectinSlot = Preferences.Get("InjectinSlot", true);
    public static TextEditor TrashWindow = new("", [], BlankSaveFile.Get(EntityContext.Gen9,""), 9);
    public static bool EditingTrash = false;
    public MainPage()
	{
        Instance = this;
        sav = AppShell.AppSaveFile ?? BlankSaveFile.Get(EntityContext.Gen9, "");
        GameInfo.FilteredSources = new FilteredGameDataSource(sav, GameInfo.Sources);
        datasourcefiltered = GameInfo.FilteredSources;
        pk = EntityBlank.GetBlank(sav);
        pk.Species = sav.MaxSpeciesID;
        pk.Language = sav.Language;
        var validvers = RamOffsets.GetValidVersions(sav);
        ICommunicator com = RamOffsets.IsSwitchTitle(sav) ? new SysBotMini() : new NTRClient();
        Remote = new PokeSysBotMini(validvers[^1], com);

        InitializeComponent();
        // 蛋图：b_ 集里没有蛋素材（b_0.png 是空图），统一用唯一那张 a_egg.png
        eggsprite.Source = SpeciesSpriteUtil.EggFileName(sav);
        var OpenTrash = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };
        OpenTrash.Tapped += OpenTrashEditor;
        NicknameLabel.GestureRecognizers.Add(OpenTrash);
        ICommand refreshCommand = new Command(async () =>
        {
            await applymainpkinfo(pk);
            PKRefresh.IsRefreshing = false;
        });
        PKRefresh.Command = refreshCommand;
        Permissions.RequestAsync<Permissions.StorageWrite>();
        SetSettings();
        specieslabel.DisplayMemberPath = "Text";
        specieslabel.ItemSource = (IList)datasourcefiltered.Species;
        naturepicker.DisplayMemberPath = "Text";
        naturepicker.ItemSource = (IList)datasourcefiltered.Natures;
        abilitypicker.ItemsSource = new List<ComboItem>();
        if (pk.Format > 7)
        {
            statnaturepicker.IsVisible = true;
            Lab_StatNature.IsVisible = true;
            statnaturepicker.DisplayMemberPath = "Text";
            statnaturepicker.ItemSource = (IList)datasourcefiltered.Natures;
        }
        helditempicker.DisplayMemberPath = "Text";
        helditempicker.ItemSource = (IList)datasourcefiltered.Items;
        if (datasourcefiltered.Items.Count > 0)
        {
            helditempicker.IsVisible = true;
            helditemlabel.IsVisible = true;
        }
        // 语言下拉框：只列出「当前存档版本实际发行过的语言」
        // （日版绿宝石只能日文、日/英/法/德/意/西…各版本不同），
        // 显示名走本地化资源，而不是 Core 的英文枚举名。
        languagepicker.ItemDisplayBinding = new Binding("Text");
        languagepicker.ItemsSource = BuildLanguageSource();
        SyncLanguageSelection();
        checklegality();
        CheckForUpdate();
        FirstLoad = false;
        applymainpkinfo(pk);
        if(PSettings.StartupPage != 0)
        {
            switch (PSettings.StartupPage)
            {
                case StartPage.Box: AppShell.Current.GoToAsync("///BoxPage"); break;
                case StartPage.Encounters: AppShell.Current.GoToAsync("///EncounterPage"); break;
                case StartPage.LiveHex: AppShell.Current.GoToAsync("///livehextab"); break;
                case StartPage.SaveEditors: AppShell.Current.GoToAsync("///SaveEditorsPage"); break;
            }
        }
    }
    protected override void OnAppearing()
    {
        if (!FirstLoad)
        {
            applymainpkinfo(pk);
            checklegality();
        }
    }
    public static void SetSettings()
    {
        // AutoMod 26.7.8 与 Core 26.7.7 不配套，它的静态成员/初始化可能触发 MissingMethodException。
        // 这里整段兜底：失败只记日志，不影响 App 启动与其它设置。
        try
        {
        APILegality.SetAllLegalRibbons = PluginSettings.SetAllLegalRibbons;
        APILegality.UseTrainerData = PluginSettings.UseTrainerData;
        var trainerfolder = PluginSettings.TrainerFolderPath;
        APILegality.AllowTrainerOverride = true;
        APILegality.SetMatchingBalls = PluginSettings.SetBallByColor;
        Legalizer.EnableEasterEggs = PluginSettings.EnableMemesForIllegalSets;
        APILegality.SetBattleVersion = PluginSettings.SetBattleVersion;
        APILegality.ForceSpecifiedBall = true;
        APILegality.Timeout = 45;
        EncounterMovesetGenerator.PriorityList = [EncounterTypeGroup.Slot, EncounterTypeGroup.Trade, EncounterTypeGroup.Static, EncounterTypeGroup.Mystery, EncounterTypeGroup.Egg];
        //TrainerSettings.DefaultOT = PluginSettings.DefaultOT;
        EncounterEvent.RefreshMGDB();
        var IsSIDdigits = ushort.TryParse(PluginSettings.DefaultSID, out var SID);
        if (IsSIDdigits)
            TrainerSettings.DefaultSID16 = SID;
        var IsTIDdigits = ushort.TryParse(PluginSettings.DefaultTID, out var TID);
        if (IsTIDdigits)
            TrainerSettings.DefaultTID16 = TID;
        TrainerSettings.Clear();
        if (!APILegality.UseTrainerData)
        {
            TrainerSettings.Register(TrainerSettings.DefaultFallback((GameVersion)sav.Version, (LanguageID)sav.Language));
        }
        else
        {
            if (Directory.Exists(trainerfolder))
            {
                var files = Directory.GetFiles(trainerfolder);
                foreach (var file in files)
                {
                    if (!EntityDetection.IsSizePlausible(new FileInfo(file).Length))
                    {
                        break;
                    }

                    byte[] data = File.ReadAllBytes(file);
                    EntityContext contextFromExtension = EntityFileExtension.GetContextFromExtension(file);
                    PKM fromBytes = EntityFormat.GetFromBytes(data, contextFromExtension)??EntityBlank.GetBlank(contextFromExtension.Generation);
                    if (fromBytes != null)
                    {
                        TrainerSettings.Register(new PokeTrainerDetails(fromBytes.Clone()));
                    }
                }
            }
        }
        }
        catch (Exception ex)
        {
            CrashLog.Write("自动合规设置", ex);
        }
        var startup = new LegalSettings();
        SaveFile.SetUpdatePKM = PSettings.SetUpdatePKM ? EntityImportOption.Enable : EntityImportOption.Disable;
        ParseSettings.InitFromSaveFileData(sav);
        ParseSettings.Settings.WordFilter.CheckWordFilter = startup.CheckWordFilter;
        ParseSettings.Settings.Tradeback.AllowGen1Tradeback= startup.AllowGen1Tradeback;
        ParseSettings.Settings.Nickname.Nickname12 = ParseSettings.Settings.Nickname.Nickname3 = ParseSettings.Settings.Nickname.Nickname4 = ParseSettings.Settings.Nickname.Nickname5 = ParseSettings.Settings.Nickname.Nickname6 = ParseSettings.Settings.Nickname.Nickname7 = ParseSettings.Settings.Nickname.Nickname7b = ParseSettings.Settings.Nickname.Nickname8 = ParseSettings.Settings.Nickname.Nickname8a = ParseSettings.Settings.Nickname.Nickname8b = ParseSettings.Settings.Nickname.Nickname9 = new NicknameRestriction() { NicknamedTrade = startup.NicknamedTrade, NicknamedMysteryGift = startup.NicknamedMysteryGift };
        ParseSettings.Settings.FramePattern.RNGFrameNotFound3=startup.RNGFrameNotFound;
        ParseSettings.Settings.Game.Gen7.Gen7TransferStarPID=startup.Gen7TransferStarPID;
        ParseSettings.Settings.HOMETransfer.HOMETransferTrackerNotPresent=startup.Gen8TransferTrackerNotPresent;
        ParseSettings.Settings.Game.Gen8.Gen8MemoryMissingHT=startup.Gen8MemoryMissingHT;
        ParseSettings.Settings.Nickname.NicknamedAnotherSpecies=startup.NicknamedAnotherSpecies;
        ParseSettings.Settings.HOMETransfer.ZeroHeightWeight=startup.ZeroHeightWeight;
        ParseSettings.Settings.Handler.CurrentHandlerMismatch= startup.CurrentHandlerMismatch;
        ParseSettings.Settings.Handler.CheckActiveHandler = startup.CheckActiveHandler;
    }
    public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        // async void 里未捕获的异常会直接终止进程，这里兜住并提示，避免整个窗口消失
        try
        {
            EntityConverter.AllowIncompatibleConversion = PSettings.AllowIncompatibleConversion ? EntityCompatibilitySetting.AllowIncompatibleAll : EntityCompatibilitySetting.DisallowIncompatible;
            var pkfile = await FilePicker.PickAsync();
            if (pkfile is null)
                return;
            var input = File.ReadAllBytes(pkfile.FullPath);
            var obj = FileUtil.GetSupportedFile(input, Path.GetExtension(pkfile.FullPath), sav);
            if (obj is null) return;
            switch (obj)
            {
                case PKM pkm: OpenPKMFile(pkm); return;
                case SaveFile s: opensavefile(s, pkfile.FullPath); return;
                case IPokeGroup b: OpenGroup(b); return;
                case MysteryGift g: OpenMG(g); return;
                case ConcatenatedEntitySet pkms: OpenPCBoxBin(pkms); return;
                case IEncounterConvertible enc: OpenPKMFile(enc.ConvertToPKM(sav)); return;
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("打开文件", ex);
            await DisplayAlertAsync(L10n.T("common_open_failed"), ex.Message, L10n.T("pktab_ok"));
        }
    }
    private async void opensavefile(SaveFile savefile, string path)
    {
        savefile.Metadata.SetExtraInfo(path);
        App.Current?.Windows[0].Page = new AppShell(savefile);
    }
    public async void OpenPCBoxBin(ConcatenatedEntitySet pkms)
    {
        var data = pkms.Data.Span;
        if (sav.GetPCBinary().Length == data.Length)
        {
            if (sav.IsAnySlotLockedInBox(0, sav.BoxCount - 1))
            {
                await DisplayAlertAsync(L10n.T("pktab_failed"), L10n.T("pktab_battle_box_slots"), L10n.T("pktab_ok"));
                return;
            }
            if(!sav.SetPCBinary(data))
            {
                await DisplayAlertAsync(L10n.T("pktab_failed"), L10n.T("pktab_import_failed"), L10n.T("pktab_ok"));
                return;
            }
            await DisplayAlertAsync(L10n.T("pktab_success"), L10n.T("pktab_pc_binary_data"), L10n.T("pktab_ok"));
        }
        else if(sav.GetBoxBinary(sav.CurrentBox).Length == data.Length)
        {
            if (sav.IsAnySlotLockedInBox(0, sav.BoxCount - 1))
            {
                await DisplayAlertAsync(L10n.T("pktab_failed"), L10n.T("pktab_battle_box_slots_2"), L10n.T("pktab_ok"));
                return;
            }
            if (!sav.SetBoxBinary(data,sav.CurrentBox))
            {
                await DisplayAlertAsync(L10n.T("pktab_failed"), L10n.T("pktab_import_failed"), L10n.T("pktab_ok"));
                return;
            }
            await DisplayAlertAsync(L10n.T("pktab_success"), L10n.T("pktab_box_binary_data"), L10n.T("pktab_ok"));
        }
        else
        {
            await DisplayAlertAsync(L10n.T("pktab_failed"), L10n.T("pktab_import_failed"), L10n.T("pktab_ok"));
        }
    }
    public async void OpenMG(MysteryGift g)
    {
        if(!g.IsEntity) return;
        var pkm = g.ConvertToPKM(sav);
        OpenPKMFile(pkm);
    }
    public async void OpenGroup(IPokeGroup b)
    {
        var type = sav.PKMType;
        int slotSkipped = 0;
        int index = 0;
        foreach (var x in b.Contents)
        {
            var i = index++;
            if (sav.IsBoxSlotOverwriteProtected(sav.CurrentBox, i))
            {
                slotSkipped++;
                continue;
            }

            var convert = EntityConverter.ConvertToType(x, type, out _);
            if (convert?.GetType() != type)
            {
                slotSkipped++;
                continue;
            }
            sav.SetBoxSlotAtIndex(x, sav.CurrentBox, i);
        }
    }
    public async void OpenPKMFile(PKM pkm)
    {
        if (pkm.GetType() != sav.PKMType)
        {
            var newpkm = EntityConverter.ConvertToType(pkm, sav.PKMType, out var result);
            if ((result.IsSuccess && newpkm is not null) || (PSettings.AllowIncompatibleConversion && newpkm is not null))
            {
                sav.AdaptToSaveFile(newpkm);
                pk = newpkm;              // 先同步静态字段，界面刷新的判定才针对这只宝可梦
                applymainpkinfo(newpkm);
                checklegality(newpkm);
                return;
            }
            else
            {
                await DisplayAlertAsync(L10n.T("pktab_incompatible"), L10n.T("pktab_this_file_is"), L10n.T("pktab_ok"));
                return;
            }
        }
        sav.AdaptToSaveFile(pkm);
        pk = pkm;                  // 同上：先赋值再刷新
        applymainpkinfo(pkm);
        checklegality(pkm);
    }
    public void checklegality() => checklegality(pk);

    /// <summary>
    /// 刷新合法性判定。
    /// 注意：必须传入「正在查看的那只宝可梦」，不能直接用静态字段 pk。
    /// 打开新宝可梦时调用顺序是 applymainpkinfo(pkm) → pk = pkm，
    /// 若此处用旧的 pk，编辑器会显示上一只宝可梦的合法性报告。
    /// </summary>
    public void checklegality(PKM target)
    {
        la = new(target, sav.Personal);
        legality.Source = la.Valid ? "valid.png" : "warn.png";
    }
    public List<ComboItem> abilitySource = [];
    public async Task applymainpkinfo(PKM pkm)
    {
        SkipTextChange = true;
        itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinybutton.Text = "★";
        else
            shinybutton.Text = "☆";
        // 闪光按钮始终可点：用于在「闪光/非闪光」之间来回切换。
        // 早期版本在设为闪光后 IsEnabled=false，导致点一次就永久失效、无法再取消。
        shinybutton.IsEnabled = true;

        specieslabel.SelectedItem = datasourcefiltered.Species.FirstOrDefault(z => z.Text== SpeciesName.GetSpeciesName(pkm.Species,2))??new ComboItem(SpeciesName.GetSpeciesName(sav.MaxSpeciesID,2),sav.MaxSpeciesID);
        displaypid.Text = $"{pkm.PID:X}";
        nickname.Text = pkm.IsNicknamed ? pkm.Nickname : SpeciesName.GetSpeciesName(pkm.Species, pk.Language);
        exp.Text = $"{pkm.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pkm.EXP, pkm.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedItem = datasourcefiltered.Natures.FirstOrDefault(z => z.Value == (int)pkm.Nature)??new ComboItem("Hardy",0);
        statnaturepicker.SelectedItem = datasourcefiltered.Natures.FirstOrDefault(z => z.Value == (int)pkm.StatAlignment) ?? new ComboItem("Hardy", 0);
        iseggcheck.IsChecked = pkm.IsEgg;
        infectedcheck.IsChecked = pkm.IsPokerusInfected;
        curedcheck.IsChecked = pkm.IsPokerusCured;
        abilitySource = [.. GameInfo.FilteredSources.GetAbilityList(pkm.PersonalInfo)];
        abilitypicker.ItemDisplayBinding = new Binding("Text");
        abilitypicker.ItemsSource = abilitySource;
        abilitypicker.SelectedIndex = pkm.AbilityNumber == 4? 2: pkm.AbilityNumber-1;
        Friendshipdisplay.Text = $"{pkm.CurrentFriendship}";
        genderdisplay.Source = $"gender_{pkm.Gender}.png";
        helditempicker.SelectedItem = datasourcefiltered.Items.FirstOrDefault(z=>z.Value == pkm.HeldItem)??new ComboItem("(None)",0);
        if (pkm.HeldItem > 0)
        {
            itemsprite.IsVisible = true;
            // 统一交给 ItemSpriteUtil 处理：精灵图编号换算（Gen3 要换算）、
            // TM/HM 特判（Gen3 的 TM 是 289~346，比 Gen4 更靠前）、以及缺图回退占位图。
            itemspriteurl = ItemSpriteUtil.FileName(pkm.HeldItem, sav.Context, sav.Generation >= 9);
            itemsprite.Source = itemspriteurl;
        }
        formpicker.Items.Clear();
        var str = GameInfo.Strings;
        var forms = FormConverter.GetFormList(pkm.Species, str.types, str.forms, GameInfo.GenderSymbolUnicode, pkm.Context);
        if (pkm.PersonalInfo.FormCount != 1)
        {
            formlabel.IsVisible = true;
            formpicker.IsVisible = true;

            foreach (var form in forms)
            {
                formpicker.Items.Add(form);
            }
            formpicker.SelectedIndex = pkm.Form;
            if (pkm is IFormArgument fa)
            {
                formargstepper.IsVisible = true;
                formargstepper.Text = fa.FormArgument.ToString();
            }
        }

            spriteurl = SpeciesSpriteUtil.FileName(sav, pkm.Species, pkm.Form, pkm.IsShiny);
        shinysparklessprite.IsVisible = pkm.IsShiny;

        pic.Source = spriteurl;
        type1sprite.Source = $"type_icon_{pkm.PersonalInfo.Type1:00}";
        type2sprite.Source = $"type_icon_{pkm.PersonalInfo.Type2:00}";
        type2sprite.IsVisible = (pkm.PersonalInfo.Type1 != pkm.PersonalInfo.Type2);
        // 下标不再等于语言字节值（下拉框已按版本过滤），改为按值匹配
        SyncLanguageSelection();
        if (pkm.Species == (ushort)Species.Manaphy && pk.IsEgg)
        {
            pkm.IsNicknamed = false;
            pkm.IsNicknamed = false;
        }
        nicknamecheck.IsChecked =  pkm.IsNicknamed;
        if(pkm is PK5 pk5)
        {
            NSparkleLabel.IsVisible = true;
            NSparkleCheckbox.IsVisible = true;
            NSparkleActiveLabel.IsVisible = true;
            NSparkleCheckbox.IsChecked = pk5.NSparkle;
        }
        checklegality(pkm);
        SkipTextChange = false;
    }
    public async void pk9saver_Clicked(object sender, EventArgs e)
    {
        try
        {
            pk.ResetPartyStats();
            Span<byte> data = stackalloc byte[MainPage.sav.SIZE_PARTY];
            pk.WriteDecryptedDataParty(data);
            await using var CrossedStreams = new MemoryStream([..data]);
            var result = await FileSaver.Default.SaveAsync(pk.FileName, CrossedStreams, CancellationToken.None);
            if (result.IsSuccessful)
                await DisplayAlertAsync(L10n.T("pktab_success"), L10n.T("code_pk_file_saved_to", result.FilePath), L10n.T("pktab_ok"));
            else
                await DisplayAlertAsync(L10n.T("pktab_failed"), L10n.T("code_pk_file_save_failed", result.Exception?.Message ?? L10n.T("common_unknown_reason")), L10n.T("pktab_ok"));
        }
        catch (Exception ex)
        {
            CrashLog.Write("保存 PKM", ex);
            await DisplayAlertAsync(L10n.T("common_save_failed"), ex.Message, L10n.T("pktab_ok"));
        }
    }

    private void specieschanger(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            if (specieslabel.SelectedItem is null) return;
            ComboItem test = (ComboItem)specieslabel.SelectedItem;
            var tree = EvolutionTree.GetEvolutionTree(sav.Context);
            var evos = tree.GetEvolutionsAndPreEvolutions(pk.Species, pk.Form);
            if(!evos.Contains(((ushort)test.Value,pk.Form)))
                pk = EntityBlank.GetBlank(sav);
            pk.Language = sav.Language;
            formargstepper.IsVisible = false;
            formlabel.IsVisible = false;
            formpicker.IsVisible = false;

            pk.Species = (ushort)test.Value;
            abilitySource = [.. GameInfo.FilteredSources.GetAbilityList(pk.PersonalInfo)];
            abilitypicker.ItemDisplayBinding =new Binding("Text");
            abilitypicker.ItemsSource = abilitySource;
            abilitypicker.SelectedIndex = pk.AbilityNumber == 4 ? 2 : pk.AbilityNumber-1;
            if (pk.PersonalInfo.Genderless && genderdisplay.Source != (ImageSource)"gender_2.png")
            {
                pk.Gender = 2;
                genderdisplay.Source = "gender_2.png";
            }
            if (pk.PersonalInfo.IsDualGender && genderdisplay.Source == (ImageSource)"gender_2.png")
            {
                pk.Gender = 0;
                genderdisplay.Source = "gender_0.png";
            }
            if (!pk.IsNicknamed)
                pk.ClearNickname();
            if (formpicker.Items.Count != 0)
                formpicker.Items.Clear();
            if (!evos.Contains(((ushort)test.Value, pk.Form)))
                pk.Form = 0;
            var str = GameInfo.Strings;
            var forms = FormConverter.GetFormList(pk.Species, str.types, str.forms, GameInfo.GenderSymbolUnicode, pk.Context);
            if (pk.PersonalInfo.FormCount != 1)
            {
                formlabel.IsVisible = true;
                formpicker.IsVisible = true;

                foreach (var form in forms)
                {
                    formpicker.Items.Add(form);
                }
                formpicker.SelectedIndex = pk.Form;
                if (pk is IFormArgument fa)
                {
                    formargstepper.IsVisible = true;
                    formargstepper.Text = fa.FormArgument.ToString();
                }
            }

            spriteurl = SpeciesSpriteUtil.FileName(sav, pk.Species, pk.Form, pk.IsShiny);
            shinysparklessprite.IsVisible = pk.IsShiny;

            pic.Source = spriteurl;
            checklegality();
            applymainpkinfo(pk);
        }
    }

    private void rollpid(object sender, EventArgs e)
    {
        if (pk.Format == 3)
        {
            // Gen3：PID 与 IV 同源（Method 1 随机算法），只刷 PID 会让合法性报
            // 「遭遇类型与PID不一致」。这里改成「随机一颗种子」，由种子同时派生
            // PID 与 IV，两者天然配套。
            //
            // 约束：优先级顺序里排在「个体值」之前的项必须保住 ——
            // 当前顺序「性格 → 闪光 → 个体值 → 性别」即在闪光与性格的条件下随机 PID，
            // 个体值与性别随之改变。
            Gen3PidFixer.TryRandomizeGen3(pk, trainer: sav);
        }
        else
        {
            pk.SetPIDGender(pk.Gender);
            pk.SetRandomEC();
        }

        displaypid.Text = $"{pk.PID:X}";
        // 重掷后闪光状态可能变化，星标按钮要同步
        // （之前只在非闪光时更新，重掷出闪光时按钮会失同步）
        shinybutton.IsEnabled = true;
        shinybutton.Text = pk.IsShiny ? "★" : "☆";
        checklegality(pk);
    }

    private void applynickname(object sender, TextChangedEventArgs e)
    {
        if (nickname.Text != SpeciesName.GetSpeciesNameGeneration(pk.Species, pk.Language, pk.Format) && !SkipTextChange)
        {
            pk.SetNickname(nickname.Text);
            checklegality();
        }
        else if (!SkipTextChange)
        {
            nicknamecheck.IsChecked = false;
        }
    }

    private void turnshiny(object sender, EventArgs e)
    {
        // 闪光按钮做成可来回切换：点一次变闪光，再点一次恢复非闪光。
        // 早期版本设为闪光后 IsEnabled=false 且没有取消分支，导致点一次就永久失效。
        var wantShiny = !pk.IsShiny;

        if (wantShiny)
            pk.SetIsShiny(true);
        else
            pk.SetIsShiny(false);

        // Gen3 的 PID/IV/性格/性别必须构成 Method 1 关系，而 Core 的 SetShiny 只改 PID 不动 IV，
        // 直接改会让「遭遇类型与PID不一致」、PID 类型变成 None。这里补一次配套修复。
        if (pk.Format == 3)
        {
            var fixResult = Gen3PidFixer.TryFixDetailed(pk, wantShiny, trainer: sav);
            if (fixResult == Gen3PidFixer.FixResult.IvReplaced)
            {
                // 原 IV 与闪光在 Method 1 下无解，只能换一组 IV —— 必须明确告知用户
                DisplayAlertAsync(L10n.T("pktab_notice"),
                    L10n.T("pktab_this_pkm_s"),
                    L10n.T("pktab_ok"));
            }
        }

        shinybutton.Text = wantShiny ? "★" : "☆";
        displaypid.Text = $"{pk.PID:X}";
        shinysparklesprite();
        checklegality(pk);
    }

    private void shinysparklesprite()
    {
        shinysparklessprite.IsVisible = pk.IsShiny;
    }

    private void applyexp(object sender, TextChangedEventArgs e)
    {
        if (!SkipTextChange)
        {
            if (exp.Text.Length > 0)
            {
                if (!uint.TryParse(exp.Text, out var result))
                    return;
                pk.EXP = result;
                var newlevel = Experience.GetLevel(pk.EXP, pk.PersonalInfo.EXPGrowth);
                if (pk.CurrentLevel != newlevel)
                    pk.CurrentLevel = newlevel;
                SkipTextChange = true;
                leveldisplay.Text = $"{pk.CurrentLevel}";
                SkipTextChange = false;
            }
            checklegality();
        }
    }

    private void applynature(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            if (naturepicker.SelectedItem is null)
                return;
            int selectedNature = ((ComboItem)naturepicker.SelectedItem).Value;
            if (pk.Format <= 4 && (int)pk.Nature != selectedNature)
            {
                pk.SetPIDNature((Nature)selectedNature);
                // Gen3：SetPIDNature 只改 PID 不动 IV，会打破 Method 1 关系 → 报「遭遇类型与PID不一致」。
                // 这里补一次配套修复，让 PID 与 IV 重新配对（IV 尽量保持原值）。
                if (pk.Format == 3)
                {
                    var fixResult = Gen3PidFixer.TryFixDetailed(pk, pk.IsShiny, trainer: sav);
                    if (fixResult == Gen3PidFixer.FixResult.IvReplaced)
                    {
                        DisplayAlertAsync(L10n.T("pktab_notice"),
                        L10n.T("pktab_this_pkm_s_2"),
                        L10n.T("pktab_ok"));
                    }
                }
                displaypid.Text = $"{pk.PID:X}";
            }
            pk.Nature = (Nature)selectedNature;
            applymainpkinfo(pk);
        }
    }

    private void applyform(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            pk.Form = (byte)(formpicker.SelectedIndex >= 0 ? formpicker.SelectedIndex : pk.Form);

            spriteurl = SpeciesSpriteUtil.FileName(sav, pk.Species, pk.Form, pk.IsShiny);
            shinysparklessprite.IsVisible = pk.IsShiny;
            pic.Source = spriteurl;
            checklegality();
        }
    }

    private void applyhelditem(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            if (helditempicker.SelectedItem is null)
                return;
            itemsprite.IsVisible = false;
            ComboItem helditemtoapply = (ComboItem)helditempicker.SelectedItem;
            pk.ApplyHeldItem(helditemtoapply.Value, sav.Context);
            if (pk.HeldItem > 0)
            {
                itemsprite.IsVisible = true;
                itemspriteurl = ItemSpriteUtil.FileName(pk.HeldItem, sav.Context, sav.Generation >= 9);
                itemsprite.Source = itemspriteurl;
            }

            checklegality();
        }
    }

    private void applyability(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            pk.SetAbilityIndex(abilitypicker.SelectedIndex);
        }
    }

    public static bool reconnect = false;

    private void changelevel(object sender, TextChangedEventArgs e)
    {
        if (leveldisplay.Text.Length > 0 && !SkipTextChange)
        {
            if (!byte.TryParse(leveldisplay.Text, out var result))
                return;
            pk.CurrentLevel = result;
            exp.Text = $"{Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth)}";
            pk.EXP = Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth);

            checklegality();
        }
        
    }

        private void applyfriendship(object sender, TextChangedEventArgs e)
    {
        if (!SkipTextChange)
        {
            if (!byte.TryParse(Friendshipdisplay.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Friendshipdisplay.Text = $"{result}";
            }
            if (Friendshipdisplay.Text.Length > 0)
            {
                pk.CurrentFriendship = result;
                checklegality();
            }
        }
    }

    private void swapgender(object sender, EventArgs e)
    {
        if (!pk.PersonalInfo.Genderless)
        {
            if (pk.Gender == 0)
            {
                pk.SetGender(1);
                genderdisplay.Source = "gender_1.png";
            }
            else
            {
                pk.SetGender(0);
                genderdisplay.Source = "gender_0.png";
            }
        }
    }

    public async void legalize(object sender, EventArgs e)
    {
        try
        {
            var result = await Task.Run(() => AlmService.Legalize(sav, pk));
            if (result is null)
            {
                await DisplayAlertAsync(L10n.T("pktab_auto_legalize_failed"), L10n.T("pktab_failed_to_generate"), L10n.T("pktab_ok"));
                return;
            }
            pk = result;
            checklegality();
            applymainpkinfo(pk);
        }
        catch(Exception j)
        {
            CrashLog.Write("自动合规", j);
            await DisplayAlertAsync(L10n.T("pktab_auto_legalize_failed"), Loc.AutoModError(j.Message), L10n.T("pktab_ok"));
        }
    }

    private async void displaylegalitymessage(object sender, EventArgs e)
    {
        applymainpkinfo(pk);
        checklegality();
        if (la.Valid && PSettings.IgnoreLegalPopup)
            return;
        // 报告正文用 PKHeX.Core 自带的中文资源生成
        var makelegal = await DisplayAlertAsync(L10n.T("common_legality_report"), Loc.LegalityReport(la), L10n.T("common_legalize"), L10n.T("pktab_ok"));
        if (!makelegal)
            return;
        try
        {
            var result = await Task.Run(() => AlmService.Legalize(sav, pk));
            if (result is null)
            {
                await DisplayAlertAsync(L10n.T("pktab_auto_legalize_failed"), L10n.T("pktab_failed_to_generate"), L10n.T("pktab_ok"));
                return;
            }
            pk = result;
            checklegality();
            applymainpkinfo(pk);
        }
        catch (Exception j)
        {
            CrashLog.Write("自动合规", j);
            await DisplayAlertAsync(L10n.T("pktab_auto_legalize_failed"), Loc.AutoModError(j.Message), L10n.T("pktab_ok"));
        }
    }

    private void applylang(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            // 语言下拉框现在按「该版本实际支持的语言」过滤，
            // 下标不再等于语言字节值，必须取 ComboItem.Value。
            if (languagepicker.SelectedItem is ComboItem sel)
                pk.Language = (byte)sel.Value;
            if (!pk.IsNicknamed)
            {
                nickname.Text = SpeciesName.GetSpeciesName(pk.Species, pk.Language);
                pk.Nickname = nickname.Text;
            }
            checklegality();
        }
    }

    /// <summary>
    /// 构建语言下拉框的数据源：只含当前存档版本实际发行过的语言。
    /// 第一、二世代的语言是推导值（不可设置），此时隐藏该下拉框。
    /// </summary>
    private List<ComboItem> BuildLanguageSource()
    {
        var list = new List<ComboItem>();
        if (!GameLanguageCatalog.IsSettable(sav))
            return list;
        foreach (var opt in GameLanguageCatalog.ForSave(sav))
            list.Add(new ComboItem(opt.Display, opt.Value));
        return list;
    }

    /// <summary>把下拉框选中项同步到当前宝可梦的语言。</summary>
    private void SyncLanguageSelection()
    {
        // 第一、二世代没有可设置的语言 → 隐藏该行
        var settable = GameLanguageCatalog.IsSettable(sav);
        languagepicker.IsVisible = settable;
        languagelabel.IsVisible = settable;
        if (!settable)
            return;

        if (languagepicker.ItemsSource is not IList<ComboItem> items || items.Count == 0)
            return;
        var want = (int)pk.Language;
        foreach (var item in items)
        {
            if (item.Value == want)
            {
                languagepicker.SelectedItem = item;
                return;
            }
        }
        // 当前语言不在该版本的合法列表里 → 保持列表首项，但不改 pk.Language
        languagepicker.SelectedItem = items[0];
    }

    private void refreshmain(object sender, EventArgs e)
    {
        applymainpkinfo(pk);
    }

    private void nicknamechecker(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipTextChange)
        {
            pk.IsNicknamed = nicknamecheck.IsChecked;
            if (!nicknamecheck.IsChecked)
            {
                pk.ClearNickname();
            }
        }
    }

    private void applystatnature(object sender, EventArgs e)
    {
        if(!SkipTextChange)
            pk.StatAlignment = (Nature?)((ComboItem?)statnaturepicker.SelectedItem)?.Value??Nature.Hardy;
        checklegality();
    }

    private void applyformarg(object sender, TextChangedEventArgs e)
    {
        if (!SkipTextChange)
        {
            if (!uint.TryParse(formargstepper.Text, out var formargu))
                return;
            if (pk is IFormArgument fa)
            {
                if (fa.FormArgumentMaximum > 0 && formargu > fa.FormArgumentMaximum)
                {
                    formargu = fa.FormArgumentMaximum;
                    formargstepper.Text = $"{formargu}";
                }
                fa.FormArgument = formargu;
            }
        }
    }

    private void applyisegg(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipTextChange)
            pk.IsEgg = iseggcheck.IsChecked;
        if (pk.IsEgg)
        {
            SkipTextChange = true;
            eggsprite.IsVisible= true;
            FriendshipLabel.Text = L10n.T("pktab_hatch_counter");
            pk.CurrentFriendship = (byte)EggStateLegality.GetMinimumEggHatchCycles(pk);

            pk.SetNickname(SpeciesName.GetEggName(pk.Language, pk.Format));
            if (pk is PK9)
                pk.Version = 0;
            pk.MetLocation = LocationEdits.GetNoneLocation(pk);
            pk.Move1_PPUps= 0;
            pk.Move2_PPUps = 0;
            pk.Move3_PPUps = 0;
            pk.Move4_PPUps = 0;
            pk.Move1_PP = pk.GetMovePP(pk.Move1, 0);
            pk.Move2_PP = pk.GetMovePP(pk.Move2, 0);
            pk.Move3_PP = pk.GetMovePP(pk.Move3, 0);
            pk.Move4_PP = pk.GetMovePP(pk.Move4, 0);
            if (pk is ITeraType tera)
                tera.TeraTypeOverride = (MoveType)0x13;
            if (pk.Format >= 6)
                pk.ClearMemories();
            if(pk.CurrentHandler == 1)
            {
                pk.CurrentHandler = 0;
                if (pk is IHandlerLanguage hl)
                    hl.HandlingTrainerLanguage = 0;
                pk.HandlingTrainerName = string.Empty;
                pk.HandlingTrainerFriendship = 0;
            }

            SkipTextChange = false;
        }
        else
        {
            SkipTextChange = true;
            FriendshipLabel.Text = L10n.T("common_friendship");
            eggsprite.IsVisible = false;
            Friendshipdisplay.Text = pk.CurrentFriendship.ToString();
            pk.ClearNickname();
            SkipTextChange = false;
        }
    }

    private void applyinfection(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipTextChange)
            pk.IsPokerusInfected = infectedcheck.IsChecked;
    }

    private void applycure(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipTextChange)
            pk.IsPokerusCured = curedcheck.IsChecked;
    }

    private void applySparkle(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipTextChange)
        {
            if (pk is PK5 pk5)
                pk5.NSparkle = NSparkleCheckbox.IsChecked;
        }
    }
    public async void ImportShowdown(object sender, EventArgs e)
    {
        if (!Clipboard.HasText)
        {
            await DisplayAlertAsync(L10n.T("pktab_showdown"), L10n.T("pktab_no_showdown_text"), L10n.T("pktab_ok"));
            return;
        }
        var doit = await DisplayAlertAsync(L10n.T("pktab_showdown"), L10n.T("code_apply_this_set", await Clipboard.GetTextAsync()), L10n.T("pktab_ok"), L10n.T("box_cancel"));
        if (!doit)
            return;
        var set = new ShowdownSet(await Clipboard.GetTextAsync());
        var result = sav.GetLegalFromSet(set);
        var pkm = result.Created;
        if(!new LegalityAnalysis(pkm).Valid)
        {
            await DisplayAlertAsync(L10n.T("pktab_showdown"), L10n.T("pktab_unable_to_legalize"), L10n.T("pktab_ok"));
        }
        pk = pkm;
        applymainpkinfo(pk);
    }

    public async void ExportShowdown(object sender, EventArgs e)
    {
        if (!await DisplayAlertAsync(L10n.T("pktab_showdown"), L10n.T("shell_export_the_current"), L10n.T("pktab_ok"), L10n.T("box_cancel")))
            Clipboard.SetTextAsync(ShowdownParsing.GetShowdownText(pk));
        else
            Clipboard.SetTextAsync(ShowdownParsing.GetShowdownSets(sav.GetBoxData(sav.CurrentBox), "\n"));
    }
    private async Task<bool> IsUpdateAvailable()
    {
        
        var latest = await GetLatest();
        if (latest == "0")
        {
            return false;
        }
        if (latest.StartsWith('v'))
            latest = latest[1..];
        Version.TryParse(latest, out var v);
        var currentVersion = AppInfo.Current.Version;
        if (v is null || v > currentVersion)
            return true;
        return false;
    }

    private static async Task<string> GetLatest()
    {
        var connection = Connectivity.Current.NetworkAccess;
        if (connection is NetworkAccess.Internet)
        {
            var client = new GitHubClient(new Octokit.ProductHeaderValue("PKHeXMAUI"));
            var release = await client.Repository.Release.GetLatest("santacrab2", "PKHeXMAUI");
            return release.TagName;
        }
        else
        {
            return "0";
        }

    }
    public async void CheckForUpdate()
    {
        if (await IsUpdateAvailable())
        {
            var Update = await AppShell.Shelltest.DisplayAlertAsync(L10n.T("pktab_update"), L10n.T("pktab_update_available"), L10n.T("pktab_update"), L10n.T("box_cancel"));
            if (Update)
            {
                await Browser.OpenAsync("https://github.com/santacrab2/PKHeXMAUI/releases/latest");
            }
        }
       
    }

    private void applyPID(object sender, TextChangedEventArgs e)
    {
        if(displaypid.Text.Length > 0 && !SkipTextChange)
        {
            if (uint.TryParse(displaypid.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
            {
                pk.PID = result;
                // Gen3：手动填 PID 会与现有 IV 脱钩 → 「遭遇类型与PID不一致」。
                // 这里把 IV 重新配对到该 PID 上（PID 是用户指定的，必须保留）。
                if (pk.Format == 3)
                    SyncIVsToPid(pk);
            }
        }
    }

    /// <summary>
    /// Gen3 专用：让 IV 与指定的 PID 重新构成合法的 Method 1 关系。
    /// PID 是用户显式指定的，必须保留；IV 若无法与该 PID 同源，则重新生成一组。
    /// </summary>
    private static void SyncIVsToPid(PKM pk)
    {
        try
        {
            uint iv32 = pk.GetIVs();
            uint iv1 = iv32 & 0x7FFF;
            uint iv2 = iv32 >> 15;
            Span<uint> seeds = stackalloc uint[LCRNG.MaxCountSeedsIV];
            var count = LCRNGReversal.GetSeedsIVs(seeds, iv1 << 16, iv2 << 16);
            for (int i = 0; i < count; i++)
            {
                var seed = LCRNG.Prev2(seeds[i]);
                if (ClassicEraRNG.GetSequentialPID(seed) == pk.PID)
                {
                    // 现有 IV 与该 PID 同源，无需改动
                    return;
                }
            }

            // 该 PID 对应的 IV 组合 → 由种子正向取出
            // Method 1：seed → Next16×2 → PID → Next15×2 → IV
            // 这里反向求解：已知 PID，取其对应的 seed。
            // Core 未直接提供「PID → IV」接口，采用等价的正向枚举：
            // 遍历候选 seed 直到 PID 命中（命中概率约 1/2^32，用受限枚举避免过慢）。
            for (uint attempt = 0; attempt < 1_000_000; attempt++)
            {
                var probe = (uint)Util.Rand.Next(1, int.MaxValue);
                var s2 = probe;
                var pid = ClassicEraRNG.GetSequentialPID(ref s2);
                if (pid != pk.PID)
                    continue;
                pk.SetIVs(ClassicEraRNG.GetSequentialIVs(ref s2));
                pk.EncryptionConstant = pk.PID;
                pk.RefreshAbility((int)(pk.PID & 1));
                pk.Gender = EntityGender.GetFromPIDAndRatio(pk.PID, pk.PersonalInfo.Gender);
                return;
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("Gen3 手动填 PID 同步 IV", ex);
        }
    }

    private void MainPKDrag(object sender, DragStartingEventArgs e)
    {
        e.Data.Properties.Add("PKM", pk);
        Shell.Current.GoToAsync("//BoxShell/boxtab/BoxPage");
    }
    public async void OpenTrashEditor(object? sender, TappedEventArgs? e)
    {
        TrashWindow = new TextEditor(nickname.Text, pk.NicknameTrash, MainPage.sav, MainPage.sav.Generation);
        await Navigation.PushModalAsync(TrashWindow);
        Task.Run(()=>WaitForTrashToClose());
    }
    private void WaitForTrashToClose()
    {
        EditingTrash = true;
        while (EditingTrash) { Task.Delay(1); }
        TrashWindow.FinalBytes.CopyTo(pk.NicknameTrash);
        if (TrashWindow.FinalString != SpeciesName.GetSpeciesNameGeneration(pk.Species, pk.Language, pk.Format))
            pk.SetNickname(TrashWindow.FinalString);
        else
            pk.ClearNickname();
        applymainpkinfo(pk);
    }

}
