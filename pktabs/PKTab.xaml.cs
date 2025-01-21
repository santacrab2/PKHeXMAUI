using System.Collections;
using PKHeX.Core;
using System.Net.Sockets;
using PKHeX.Core.AutoMod;
using Octokit;
using System.Windows.Input;
using PKHeX.Core.Injection;
using CommunityToolkit.Maui.Storage;
namespace PKHeXMAUI;

public partial class MainPage : ContentPage
{
    public static string Version = "v25.01.12";
    public static PKM pk = EntityBlank.GetBlank(9);
    public static LegalityAnalysis la = new(pk);
    public static SaveFile sav = AppShell.AppSaveFile??SaveUtil.GetBlankSAV(EntityContext.Gen9,"");
    public static FilteredGameDataSource datasourcefiltered = new(sav, GameInfo.Sources);
    public static Socket SwitchConnection = new(SocketType.Stream, ProtocolType.Tcp);
    public static string spriteurl = "iconp.png";
    public static string ipaddy = "";
    public static string itemspriteurl = "";
    public bool SkipTextChange = false;
    public static int[] NoFormSpriteSpecies = [664, 665, 744, 982, 855, 854, 869,892,1012,1013];
    public bool FirstLoad = true;
    public static PokeSysBotMini Remote = new(LiveHeXVersion.SV_v301, new SysBotMini(), false);
    public static bool ReadonChangeBox = Preferences.Get("ReadonChangeBox", true);
    public static bool InjectinSlot = Preferences.Get("InjectinSlot", true);
    public static TextEditor TrashWindow = new("", [], SaveUtil.GetBlankSAV(EntityContext.Gen9,""), 9);
    public static bool EditingTrash = false;
    public MainPage()
	{
        sav = AppShell.AppSaveFile;
        GameInfo.FilteredSources = new FilteredGameDataSource(sav, GameInfo.Sources);
        datasourcefiltered = GameInfo.FilteredSources;
        pk = EntityBlank.GetBlank(sav.Generation,(GameVersion)sav.Version);
        pk.Species = sav.MaxSpeciesID;
        pk.Language = sav.Language;
        var validvers = RamOffsets.GetValidVersions(sav);
        ICommunicator com = RamOffsets.IsSwitchTitle(sav) ? new SysBotMini() : new NTRClient();
        Remote = new PokeSysBotMini(validvers[^1], com, false);

        InitializeComponent();
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
        List<ComboItem> languageSource = [];
        foreach(var item in Enum.GetValues<LanguageID>())
        {
            languageSource.Add(new ComboItem($"{(LanguageID)item}", (int)item));
        }
        languagepicker.ItemDisplayBinding = new Binding("Text");
        languagepicker.ItemsSource = languageSource;
        checklegality();
        CheckForUpdate();
        FirstLoad = false;
        applymainpkinfo(pk);
        if(PSettings.StartupPage != 0)
        {
            switch (PSettings.StartupPage)
            {
                case 1: AppShell.Current.GoToAsync("///BoxPage"); break;
                case 2: AppShell.Current.GoToAsync("///EncounterPage"); break;
                case 3: AppShell.Current.GoToAsync("///livehextab"); break;
                case 4: AppShell.Current.GoToAsync("///SaveEditorsPage"); break;
            }
        }
    }
    public static void SetSettings()
    {
        APILegality.SetAllLegalRibbons = PluginSettings.SetAllLegalRibbons;
        APILegality.UseTrainerData = PluginSettings.UseTrainerData;
        var trainerfolder = PluginSettings.TrainerFolderPath;
        APILegality.AllowTrainerOverride = true;
        APILegality.SetMatchingBalls = PluginSettings.SetBallByColor;
        Legalizer.EnableEasterEggs = PluginSettings.EnableMemesForIllegalSets;
        APILegality.PrioritizeGame = PluginSettings.PrioritizeGame;
        APILegality.PrioritizeGameVersion = PluginSettings.PrioritizeGameVersion;
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
                    PKM fromBytes = EntityFormat.GetFromBytes(data, contextFromExtension)??EntityBlank.GetBlank(contextFromExtension.Generation());
                    if (fromBytes != null)
                    {
                        TrainerSettings.Register(new PokeTrainerDetails(fromBytes.Clone()));
                    }
                }
            }
        }
        var startup = new LegalSettings();
        SaveFile.SetUpdatePKM = PSettings.SetUpdatePKM ? PKMImportSetting.Update : PKMImportSetting.Skip;
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
        EntityConverter.AllowIncompatibleConversion = PSettings.AllowIncompatibleConversion ? EntityCompatibilitySetting.AllowIncompatibleAll : EntityCompatibilitySetting.DisallowIncompatible;
        var pkfile = await FilePicker.PickAsync();
        if (pkfile is null)
            return;
        var input = File.ReadAllBytes(pkfile.FullPath);
        var obj = FileUtil.GetSupportedFile(input,pkfile.ContentType,sav);
        if(obj is null) return;
        switch (obj)
        {
            case PKM pkm: OpenPKMFile(pkm);return;
            case SaveFile s: opensavefile(s, pkfile.FullPath); return;
            case IPokeGroup b: OpenGroup(b); return;
            case MysteryGift g: OpenMG(g); return;
            case ConcatenatedEntitySet pkms: OpenPCBoxBin(pkms); return;
            case IEncounterConvertible enc: OpenPKMFile(enc.ConvertToPKM(sav));return;
        }
    }
    private async void opensavefile(SaveFile savefile, string path)
    {
        savefile.Metadata.SetExtraInfo(path);
        if(App.Current is not null)
            App.Current.Windows[0].Page = new AppShell(savefile);
    }
    public async void OpenPCBoxBin(ConcatenatedEntitySet pkms)
    {
        var data = pkms.Data.Span;
        if (sav.GetPCBinary().Length == data.Length)
        {
            if (sav.IsAnySlotLockedInBox(0, sav.BoxCount - 1))
            {
                await DisplayAlert("Fail", "Battle Box slots prevent loading of PC data.", "cancel");
                return;
            }
            if(!sav.SetPCBinary(data))
            {
                await DisplayAlert("Fail", "Failed to import", "cancel");
                return;
            }
            await DisplayAlert("Success", "PC Binary imported.", "okay");
        }
        else if(sav.GetBoxBinary(sav.CurrentBox).Length == data.Length)
        {
            if (sav.IsAnySlotLockedInBox(0, sav.BoxCount - 1))
            {
                await DisplayAlert("Fail", "Battle Box slots prevent loading of Box data.", "cancel");
                return;
            }
            if (!sav.SetBoxBinary(data,sav.CurrentBox))
            {
                await DisplayAlert("Fail", "Failed to import", "cancel");
                return;
            }
            await DisplayAlert("Success", "Box Binary imported.", "okay");
        }
        else
        {
            await DisplayAlert("Fail", "Failed to import", "cancel");
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
            if ((result.IsSuccess() && newpkm is not null) || (PSettings.AllowIncompatibleConversion && newpkm is not null))
            {
                sav.AdaptPKM(newpkm);
                applymainpkinfo(newpkm);
                checklegality();
                pk = newpkm;
                return;
            }
            else
            {
                await DisplayAlert("Incompatible", "This file is incompatible with the current save file", "cancel");
                return;
            }
        }
        sav.AdaptPKM(pkm);
        applymainpkinfo(pkm);
        checklegality();
        pk = pkm;
    }
    public void checklegality()
    {
        la = new(pk,sav.Personal);
        legality.Source = la.Valid ? "valid.png" : "warn.png";
    }
    public List<ComboItem> abilitySource = [];
    public async Task applymainpkinfo(PKM pkm)
    {
        SkipTextChange = true;
        itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinybutton.Text = "★";

        specieslabel.SelectedItem = datasourcefiltered.Species.FirstOrDefault(z => z.Text== SpeciesName.GetSpeciesName(pkm.Species,2))??new ComboItem(SpeciesName.GetSpeciesName(sav.MaxSpeciesID,2),sav.MaxSpeciesID);
        displaypid.Text = $"{pkm.PID:X}";
        nickname.Text = pkm.IsNicknamed ? pkm.Nickname : SpeciesName.GetSpeciesName(pkm.Species, pk.Language);
        exp.Text = $"{pkm.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pkm.EXP, pkm.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedItem = datasourcefiltered.Natures.FirstOrDefault(z => z.Value == (int)pkm.Nature)??new ComboItem("Hardy",0);
        statnaturepicker.SelectedItem = datasourcefiltered.Natures.FirstOrDefault(z => z.Value == (int)pkm.StatNature) ?? new ComboItem("Hardy", 0);
        iseggcheck.IsChecked = pkm.IsEgg;
        infectedcheck.IsChecked = pkm.IsPokerusInfected;
        curedcheck.IsChecked = pkm.IsPokerusCured;
        if (abilitySource.Count != 0)
            abilitySource.Clear();
        for (int i = 0; i < pkm.PersonalInfo.AbilityCount; i++)
        {
            var abili = pkm.PersonalInfo.GetAbilityAtIndex(i);

            abilitySource.Add(new ComboItem($"{(Ability)abili}", i));
        }
        abilitypicker.ItemDisplayBinding = new Binding("Text");
        abilitypicker.ItemsSource = abilitySource;
        abilitypicker.SelectedIndex = -1;
        abilitypicker.SelectedIndex =pkm.AbilityNumber == 4? 2: pkm.AbilityNumber;
        Friendshipdisplay.Text = $"{pkm.CurrentFriendship}";
        genderdisplay.Source = $"gender_{pkm.Gender}.png";
        helditempicker.SelectedItem = datasourcefiltered.Items.FirstOrDefault(z=>z.Value == pkm.HeldItem)??new ComboItem("(None)",0);
        if (pkm.HeldItem > 0)
        {
            itemsprite.IsVisible = true;
            if (sav is SAV9SV)
            {
                if ((pkm.HeldItem >= 329 && pkm.HeldItem <= 420) || (pkm.HeldItem >= 2161 && pkm.HeldItem <= 2232))
                {
                    itemspriteurl = "aitem_tm.png";
                    itemsprite.Source = "aitem_tm.png";
                }
                else
                {
                    itemspriteurl = $"aitem_{pkm.SpriteItem}.png";
                    itemsprite.Source = $"aitem_{pkm.SpriteItem}.png";
                }
            }
            else
            {
                if ((pkm.HeldItem >= 328 && pkm.HeldItem <= 419) || (pkm.HeldItem >= 2160 && pkm.HeldItem <= 2231))
                {
                    itemspriteurl = "bitem_tm.png";
                    itemsprite.Source = "bitem_tm.png";
                }
                else
                {
                    itemspriteurl = $"bitem_{pkm.SpriteItem}.png";
                    itemsprite.Source = $"bitem_{pkm.SpriteItem}.png";
                }
            }
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

            spriteurl = pkm.Species == 0
                ? "a_egg.png"
                : $"a_{pkm.Species}{((pkm.Form >0&&!NoFormSpriteSpecies.Contains(pkm.Species))?$"_{pkm.Form}":"")}.png";
        shinysparklessprite.IsVisible = pkm.IsShiny;

        pic.Source = spriteurl;
        type1sprite.Source = $"type_icon_{pkm.PersonalInfo.Type1:00}";
        type2sprite.Source = $"type_icon_{pkm.PersonalInfo.Type2:00}";
        type2sprite.IsVisible = (pkm.PersonalInfo.Type1 != pkm.PersonalInfo.Type2);
        languagepicker.SelectedIndex = pkm.Language;
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
        checklegality();
        SkipTextChange = false;
    }
    public async void pk9saver_Clicked(object sender, EventArgs e)
    {
        pk.ResetPartyStats();
        await using var CrossedStreams = new MemoryStream(pk.DecryptedPartyData);
        var result = await FileSaver.Default.SaveAsync(pk.FileName, CrossedStreams, CancellationToken.None);
        if (result.IsSuccessful)
            await DisplayAlert("Success", $"PK File saved at {result.FilePath}", "cancel");
        else
            await DisplayAlert("Failure", $"PK File did not save due to {result.Exception.Message}", "cancel");
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
                pk = EntityBlank.GetBlank(sav.Generation, (GameVersion)sav.Version);
            pk.Language = sav.Language;
            formargstepper.IsVisible = false;
            formlabel.IsVisible = false;
            formpicker.IsVisible = false;

            pk.Species = (ushort)test.Value;
            if (abilitySource.Count != 0)
                abilitySource.Clear();
            for (int i = 0; i < pk.PersonalInfo.AbilityCount; i++)
            {
                var abili = pk.PersonalInfo.GetAbilityAtIndex(i);
                abilitySource.Add(new ComboItem($"{(Ability)abili}",i));
            }
            abilitypicker.ItemDisplayBinding =new Binding("Text");
            abilitypicker.ItemsSource = abilitySource;
            abilitypicker.SelectedIndex = -1;
            abilitypicker.SelectedIndex = pk.AbilityNumber == 4 ? 2 : pk.AbilityNumber;
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

            spriteurl = pk.Species == 0 ? "a_egg.png" : $"a_{pk.Species}{((pk.Form > 0 && !NoFormSpriteSpecies.Contains(pk.Species)) ? $"_{pk.Form}" : "")}.png";
            shinysparklessprite.IsVisible = pk.IsShiny;

            pic.Source = spriteurl;
            checklegality();
            applymainpkinfo(pk);
        }
    }

    private void rollpid(object sender, EventArgs e)
    {
        pk.SetPIDGender(pk.Gender);
        pk.SetRandomEC();
        displaypid.Text = $"{pk.PID:X}";
        checklegality();
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
        if (!pk.IsShiny)
        {
            pk.SetIsShiny(true);
            shinybutton.Text = "★";
        }
        else
        {
            pk.SetIsShiny(false);
            shinybutton.Text = "☆";
        }

        displaypid.Text = $"{pk.PID:X}";
        checklegality();
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
                pk.CurrentLevel = newlevel;
                leveldisplay.Text = $"{pk.CurrentLevel}";
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

            spriteurl = pk.Species == 0 ? "a_egg.png" : $"a_{pk.Species}{((pk.Form > 0 && !NoFormSpriteSpecies.Contains(pk.Species)) ? $"_{pk.Form}" : "")}.png";
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
                if (sav is SAV9SV)
                {
                    itemspriteurl = $"aitem_{pk.SpriteItem}.png";
                    itemsprite.Source = $"aitem_{pk.SpriteItem}.png";
                }
                else
                {
                    itemspriteurl = $"bitem_{pk.SpriteItem}.png";
                    itemsprite.Source = $"bitem_{pk.SpriteItem}.png";
                }
            }

            checklegality();
        }
    }

    private void applyability(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            if (abilitypicker.SelectedItem != null)
            {
                var abil = pk.PersonalInfo.GetAbilityAtIndex(((ComboItem)abilitypicker.SelectedItem).Value);
                pk.SetAbility(abil);
            }
        }
    }

    public static bool reconnect = false;

    private void changelevel(object sender, TextChangedEventArgs e)
    {
        if (!SkipTextChange)
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
            pk = await Task.Run(() => sav.Legalize(pk));
            checklegality();
            applymainpkinfo(pk);
        }
        catch(Exception j)
        {
            await DisplayAlert("error", j.Message, "ok");
        }
    }

    private async void displaylegalitymessage(object sender, EventArgs e)
    {
        applymainpkinfo(pk);
        checklegality();
        if (la.Valid && PSettings.IgnoreLegalPopup)
            return;
        var makelegal = await DisplayAlert("Legality Report", la.Report(), "legalize","ok");
        if (makelegal)
        {
            pk = await Task.Run(()=>sav.Legalize(pk));
            checklegality();
            applymainpkinfo(pk);
        }
    }

    private void applylang(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            pk.Language = languagepicker.SelectedIndex;
            if (!pk.IsNicknamed)
            {
                nickname.Text = SpeciesName.GetSpeciesName(pk.Species, pk.Language);
                pk.Nickname = nickname.Text;
            }
            checklegality();
        }
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
            pk.StatNature = (Nature?)((ComboItem?)statnaturepicker.SelectedItem)?.Value??Nature.Hardy;
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
            FriendshipLabel.Text = "Hatch Counter:";
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
            FriendshipLabel.Text = "FriendShip:";
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
            await DisplayAlert("Showdown", "No showdown text found on clipboard", "cancel");
            return;
        }
        var doit = await DisplayAlert("Showdown", $"Apply this set?\n{await Clipboard.GetTextAsync()}", "Yes", "cancel");
        if (!doit)
            return;
        var set = new ShowdownSet(await Clipboard.GetTextAsync());
        var result = sav.GetLegalFromSet(set);
        var pkm = result.Created;
        if(new LegalityAnalysis(pkm).Valid)
        {
            pk = pkm;
            applymainpkinfo(pk);
        }
        else
        {
            if (PluginSettings.EnableMemesForIllegalSets)
                applymainpkinfo(pkm);
            await DisplayAlert("Showdown", "I could not legalize the provided Showdown Set","cancel");
        }
    }

    public async void ExportShowdown(object sender, EventArgs e)
    {
        if (!await DisplayAlert("Showdown", "Export the current box?", "yes", "cancel"))
            Clipboard.SetTextAsync(ShowdownParsing.GetShowdownText(pk));
        else
            Clipboard.SetTextAsync(ShowdownParsing.GetShowdownSets(sav.GetBoxData(sav.CurrentBox), "\n"));
    }
    private static async Task<bool> IsUpdateAvailable()
    {
            var currentVersion = ParseVersion(Version);
            var latestVersion = ParseVersion(await GetLatest());

            if (latestVersion[0] > currentVersion[0])
            {
                return true;
            }
            else if (latestVersion[0] == currentVersion[0])
            {
                if (latestVersion[1] > currentVersion[1])
                {
                    return true;
                }
                else if (latestVersion[1] == currentVersion[1])
                {
                    if (latestVersion[2] > currentVersion[2])
                        return true;
                }
            }
            return false;
    }

    private static async Task<string> GetLatest()
    {
        var client = new GitHubClient(new Octokit.ProductHeaderValue("PKHeXMAUI"));
        var release = await client.Repository.Release.GetLatest("santacrab2", "PKHeXMAUI");
        return release.Name;
    }

    private static int[] ParseVersion(string version)
    {
        var v = new int[3];
        v[0] = int.Parse($"{version[1] + version[2]}");
        v[1] = int.Parse($"{version[4] + version[5]}");
        v[2] = int.Parse($"{version[7..]}");
        return v;
    }
    public async void CheckForUpdate()
    {
        if (await IsUpdateAvailable())
        {
            var Update = await DisplayAlert("Update", "Update is available", "Update", "Cancel");
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
            if (uint.TryParse(displaypid.Text, out var result))
            {
                pk.PID = result;
            }
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
