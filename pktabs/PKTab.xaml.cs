
using PKHeX.Core;
using System.Net.Sockets;
using PKHeX.Core.AutoMod;
using Octokit;
using System.Windows.Input;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;
using PKHeX.Core.Injection;

namespace PKHeXMAUI;

public partial class MainPage : ContentPage
{
    public static string Version = "v23.11.12";
    public bool SkipTextChange = false;
    public static int[] NoFormSpriteSpecies = new[] { 664, 665, 744, 982, 855, 854, 869,892,1012,1013 };
    public bool FirstLoad = true;
    public static PokeSysBotMini Remote;
    public static bool ReadonChangeBox = Preferences.Get("ReadonChangeBox", true);
    public static bool InjectinSlot = Preferences.Get("InjectinSlot", true);
    public MainPage()
	{
        sav = AppShell.AppSaveFile;
        GameInfo.FilteredSources = new FilteredGameDataSource(sav, GameInfo.Sources);
        datasourcefiltered = GameInfo.FilteredSources;
        pk = EntityBlank.GetBlank(sav.Generation,(GameVersion)sav.Version);
        pk.Species = sav.MaxSpeciesID;
        var validvers = RamOffsets.GetValidVersions(sav);
        ICommunicator com = RamOffsets.IsSwitchTitle(sav) ? new SysBotMini() : new NTRClient();
        Remote = new PokeSysBotMini(validvers[validvers.Length - 1], com, false);
        InitializeComponent();
        
        ICommand refreshCommand = new Command(async () =>
        {

            await applymainpkinfo(pk);
            PKRefresh.IsRefreshing = false;
        });
        PKRefresh.Command = refreshCommand;
        Permissions.RequestAsync<Permissions.StorageWrite>();
        SetSettings();
        specieslabel.ItemsSource = datasourcefiltered.Species;
        naturepicker.ItemsSource = Enum.GetNames(typeof(Nature));
        statnaturepicker.ItemsSource = Enum.GetNames(typeof(Nature));
        helditempicker.ItemsSource = datasourcefiltered.Items;
        helditempicker.DisplayMemberPath= "Text";
        if (datasourcefiltered.Items.Count() > 0)
        {
            helditempicker.IsVisible = true;
            helditemlabel.IsVisible = true;
        }
        languagepicker.ItemsSource = Enum.GetNames(typeof(LanguageID));
        checklegality();
        CheckForUpdate();
        FirstLoad = false;
    }
    public static LegalityAnalysis la;

    public static PKM pk;
    public static SaveFile sav;
    public static FilteredGameDataSource datasourcefiltered;
    public static Socket SwitchConnection = new Socket(SocketType.Stream, ProtocolType.Tcp);
    public static string spriteurl = "iconp.png";
    public static string ipaddy = "";
    public static string itemspriteurl = "";
    public static void SetSettings()
    {
        
        APILegality.SetAllLegalRibbons = PluginSettings.SetAllLegalRibbons;
        APILegality.UseTrainerData = false;
        APILegality.AllowTrainerOverride = true;
        APILegality.SetMatchingBalls = PluginSettings.SetBallByColor;
        Legalizer.EnableEasterEggs = PluginSettings.EnableMemesForIllegalSets;
        APILegality.PrioritizeGame = PluginSettings.PrioritizeGame;
        APILegality.PrioritizeGameVersion = PluginSettings.PrioritizeGameVersion;
        APILegality.SetBattleVersion = PluginSettings.SetBattleVersion;
        APILegality.ForceSpecifiedBall = true;
        APILegality.Timeout = 45;
        APILegality.AllowHOMETransferGeneration = PluginSettings.AllowHomeless;
        EncounterMovesetGenerator.PriorityList = new List<EncounterTypeGroup>() { EncounterTypeGroup.Slot, EncounterTypeGroup.Trade, EncounterTypeGroup.Static, EncounterTypeGroup.Mystery, EncounterTypeGroup.Egg };
        TrainerSettings.DefaultOT = PluginSettings.DefaultOT;
        EncounterEvent.RefreshMGDB();
        var IsSIDdigits = ushort.TryParse(PluginSettings.DefaultSID, out var SID);
        if (IsSIDdigits)
            TrainerSettings.DefaultSID16 = SID;
        var IsTIDdigits = ushort.TryParse(PluginSettings.DefaultTID, out var TID);
        if (IsTIDdigits)
            TrainerSettings.DefaultTID16 = TID;
        TrainerSettings.Clear();
        TrainerSettings.Register(TrainerSettings.DefaultFallback((GameVersion)sav.Version, (LanguageID)sav.Language));
        var startup = new LegalSettings();
        SaveFile.SetUpdatePKM = PSettings.SetUpdatePKM ? PKMImportSetting.Update : PKMImportSetting.Skip;
        ParseSettings.InitFromSaveFileData(sav);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.CheckWordFilter))!.SetValue(null, startup.CheckWordFilter);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.AllowGen1Tradeback))!.SetValue(null, startup.AllowGen1Tradeback);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.NicknamedTrade))!.SetValue(null, startup.NicknamedTrade);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.NicknamedMysteryGift))!.SetValue(null, startup.NicknamedMysteryGift);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.RNGFrameNotFound))!.SetValue(null, startup.RNGFrameNotFound);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.Gen7TransferStarPID))!.SetValue(null, startup.Gen7TransferStarPID);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.Gen8TransferTrackerNotPresent))!.SetValue(null, startup.Gen8TransferTrackerNotPresent);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.Gen8MemoryMissingHT))!.SetValue(null, startup.Gen8MemoryMissingHT);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.NicknamedAnotherSpecies))!.SetValue(null, startup.NicknamedAnotherSpecies);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.ZeroHeightWeight))!.SetValue(null, startup.ZeroHeightWeight);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.CurrentHandlerMismatch))!.SetValue(null, startup.CurrentHandlerMismatch);
        typeof(ParseSettings).GetProperty(nameof(ParseSettings.CheckActiveHandler))!.SetValue(null, startup.CheckActiveHandler);
    }
    public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        EntityConverter.AllowIncompatibleConversion = PSettings.AllowIncompatibleConversion ? EntityCompatibilitySetting.AllowIncompatibleAll : EntityCompatibilitySetting.DisallowIncompatible;
        var pkfile = await FilePicker.PickAsync();
        if (pkfile is null)
            return;
        
        var obj = FileUtil.GetSupportedFile(pkfile.FullPath);
        if(obj is null) return;
        switch (obj)
        {
            case PKM pkm: OpenPKMFile(pkm);return;
            case IPokeGroup b: OpenGroup(b); return;
            case MysteryGift g: OpenMG(g); return;
            case IEnumerable<byte[]> pkms: OpenPCBoxBin(pkms); return;
            case IEncounterConvertible enc: OpenPKMFile(enc.ConvertToPKM(sav));return;
        }
      
       
    }
    public async void OpenPCBoxBin(IEnumerable<byte[]> pkms)
    {
        var data = pkms.SelectMany(z => z).ToArray();
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
            if (sav.IsSlotOverwriteProtected(sav.CurrentBox, i))
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
            if (result.IsSuccess() || PSettings.AllowIncompatibleConversion)
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
    public async Task applymainpkinfo(PKM pkm)
    {
        SkipTextChange = true;
        itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinybutton.Text = "★";
        
        specieslabel.SelectedItem = datasourcefiltered.Species.Where(z => z.Text== SpeciesName.GetSpeciesName(pkm.Species,2)).FirstOrDefault();
        displaypid.Text = $"{pkm.PID:X}";
        nickname.Text = pkm.Nickname;
        exp.Text = $"{pkm.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pkm.EXP, pkm.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedItem = (Nature)pkm.Nature;
        statnaturepicker.SelectedItem = (Nature)pkm.StatNature;
        iseggcheck.IsChecked = pk.IsEgg;
        infectedcheck.IsChecked = pk.PKRS_Infected;
        curedcheck.IsChecked = pk.PKRS_Cured;
        if (abilitypicker.Items.Count() != 0)
            abilitypicker.Items.Clear();
        for (int i = 0; i < pk.PersonalInfo.AbilityCount; i++)
        {
            var abili = pk.PersonalInfo.GetAbilityAtIndex(i);
            abilitypicker.Items.Add($"{(Ability)abili}");

        }
        abilitypicker.SelectedIndex =pkm.AbilityNumber == 4? 2: pkm.AbilityNumber-1;
        Friendshipdisplay.Text = $"{pkm.CurrentFriendship}";
      
        genderdisplay.Source = $"gender_{pkm.Gender}.png";
        helditempicker.SelectedItem = datasourcefiltered.Items.Where(z=>z.Text== (GameInfo.Strings.Item[pkm.HeldItem])).FirstOrDefault();
        if (pkm.HeldItem > 0)
        {
            itemsprite.IsVisible = true;
            if (sav is SAV9SV)
            {
                if (pkm.HeldItem >= 329 && pkm.HeldItem <= 420 || pkm.HeldItem >= 2161 && pkm.HeldItem <= 2232)
                {
                    itemspriteurl = $"aitem_tm.png";
                    itemsprite.Source = $"aitem_tm.png";
                }
                else
                {
                    itemspriteurl = $"aitem_{pkm.HeldItem}.png";
                    itemsprite.Source = $"aitem_{pkm.HeldItem}.png";
                }
            }
            else
            {
                if (pkm.HeldItem >= 328 && pkm.HeldItem <= 419 || pkm.HeldItem >= 2160 && pkm.HeldItem <= 2231)
                {
                    itemspriteurl = $"bitem_tm.png";
                    itemsprite.Source = $"bitem_tm.png";
                }
                else
                {
                    itemspriteurl = $"bitem_{pkm.HeldItem}.png";
                    itemsprite.Source = $"bitem_{pkm.HeldItem}.png";
                }
            }
        }
        formpicker.Items.Clear();
        var str = GameInfo.Strings;
        var forms = FormConverter.GetFormList(pkm.Species, str.types, str.forms, GameInfo.GenderSymbolUnicode, pkm.Context);
        if (forms[0] != "")
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

            if (pkm.Species == 0)
                spriteurl = $"a_egg.png";
            else 
                spriteurl = $"a_{pkm.Species}{((pkm.Form >0&&!NoFormSpriteSpecies.Contains(pkm.Species))?$"_{pkm.Form}":"")}.png";
            if (pkm.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible= false;
        
        
      
        pic.Source = spriteurl;
        type1sprite.Source = $"type_icon_{pk.PersonalInfo.Type1:00}";
        type2sprite.Source = $"type_icon_{pk.PersonalInfo.Type2:00}";
        type2sprite.IsVisible = (pk.PersonalInfo.Type1 != pk.PersonalInfo.Type2);
        languagepicker.SelectedIndex = pkm.Language;
        if (pkm.Species == (ushort)Species.Manaphy && pk.IsEgg) 
        { 
            pk.IsNicknamed = false;
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
#if ANDROID
        pk.ResetPartyStats();
        string path = "";
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {

            await File.WriteAllBytesAsync($"/storage/emulated/0/Documents/{pk.FileName}", pk.DecryptedPartyData);
            path = "/storage/emulated/0/Documents/";
        }
        else
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                
                await File.WriteAllBytesAsync($"/storage/emulated/0/Android/data/com.PKHeX.maui/{pk.FileName}", pk.DecryptedPartyData);
                path="/storage/emulated/0/Android/data/com.PKHeX.maui/";
            }
            else
            {
                await File.WriteAllBytesAsync($"/storage/emulated/0/{pk.FileName}", pk.DecryptedPartyData);
                path = "/storage/emulated/0/";
            }
        }
        await DisplayAlert("Saved",$"{pk.Nickname} has been saved to {path}", "ok");
#endif
    }

    private void specieschanger(object sender, EventArgs e) 
    {
        if (!SkipTextChange)
        {
            ComboItem test = (ComboItem)specieslabel.SelectedItem ?? new ComboItem("None", 0);
            var tree = EvolutionTree.GetEvolutionTree(sav.Context);
            var evos = tree.GetEvolutionsAndPreEvolutions(pk.Species, pk.Form);
            if(!evos.Contains(((ushort)test.Value,pk.Form)))
                pk = EntityBlank.GetBlank(sav.Generation, (GameVersion)sav.Version);
            pk.Language = sav.Language;
            formargstepper.IsVisible = false;
            formlabel.IsVisible = false;
            formpicker.IsVisible = false;
            
            pk.Species = (ushort)test.Value;
            if (abilitypicker.Items.Count() != 0)
                abilitypicker.Items.Clear();
            for (int i = 0; i < pk.PersonalInfo.AbilityCount; i++)
            {
                var abili = pk.PersonalInfo.GetAbilityAtIndex(i);
                abilitypicker.Items.Add($"{(Ability)abili}");

            }
            abilitypicker.SelectedIndex = 0;
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
            if (forms[0] != "")
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

            if (pk.Species == 0)
                spriteurl = $"a_egg.png";
            else
                spriteurl = $"a_{pk.Species}{((pk.Form > 0 && !NoFormSpriteSpecies.Contains(pk.Species)) ? $"_{pk.Form}" : "")}.png";
            if (pk.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible = false;



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

    private void applynature(object sender, EventArgs e) { if (!SkipTextChange) { pk.Nature = naturepicker.SelectedIndex; checklegality(); } }

        

    private void applyform(object sender, EventArgs e) 
    {
        if (!SkipTextChange)
        {
            pk.Form = (byte)(formpicker.SelectedIndex >= 0 ? formpicker.SelectedIndex : pk.Form);

            if (pk.Species == 0)
                spriteurl = $"a_egg.png";
            else
                spriteurl = $"a_{pk.Species}{((pk.Form > 0 && !NoFormSpriteSpecies.Contains(pk.Species)) ? $"_{pk.Form}" : "")}.png";
            if (pk.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible = false;


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
                    itemspriteurl = $"aitem_{pk.HeldItem}.png";
                    itemsprite.Source = $"aitem_{pk.HeldItem}.png";
                }
                else
                {
                    itemspriteurl = $"bitem_{pk.HeldItem}.png";
                    itemsprite.Source = $"bitem_{pk.HeldItem}.png";
                }
            }

            checklegality();
        }
    }

    private void applyability(object sender, EventArgs e)
    {
        if (!SkipTextChange)
        {
            if (abilitypicker.SelectedIndex != -1)
            {
                var abil = pk.PersonalInfo.GetAbilityAtIndex(abilitypicker.SelectedIndex);
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
                if (!int.TryParse(leveldisplay.Text, out var result))
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
            if (!int.TryParse(Friendshipdisplay.Text, out var result))
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
        if(!SkipTextChange)
            pk.Language = languagepicker.SelectedIndex; checklegality();
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
            pk.StatNature = statnaturepicker.SelectedIndex; checklegality();
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
            pk.CurrentFriendship = EggStateLegality.GetMinimumEggHatchCycles(pk);
            
            pk.SetNickname(SpeciesName.GetEggName(pk.Language, pk.Format));
            if (pk is PK9)
                pk.Version = 0;
            pk.Met_Location = LocationEdits.GetNoneLocation(pk);
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
                    hl.HT_Language = 0;
                pk.HT_Name = string.Empty;
                pk.HT_Friendship = 0;
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
            pk.PKRS_Infected = infectedcheck.IsChecked;
    }

    private void applycure(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipTextChange)
            pk.PKRS_Cured = curedcheck.IsChecked;
    }

    private void applySparkle(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipTextChange)
        {
            if (pk is PK5 pk5)
                pk5.NSparkle = NSparkleCheckbox.IsChecked;
        }
    }

    private void ChangeComboBoxFontColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        SfComboBox box = (SfComboBox)sender;
        if (box.IsDropDownOpen)
            box.TextColor = Colors.Black;
        else
            box.TextColor = Colors.White;
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

    public void ExportShowdown(object sender, EventArgs e)
    {
        Clipboard.SetTextAsync(ShowdownParsing.GetShowdownText(pk));
    }
    private static async Task<bool> IsUpdateAvailable()
    {
        try
        {
            var currentVersion = ParseVersion(Version);
            var latestVersion = ParseVersion(await GetLatestVersion());

            if (latestVersion[0] > currentVersion[0])
                return true;
            else if (latestVersion[0] == currentVersion[0])
            {
                if (latestVersion[1] > currentVersion[1])
                    return true;
                else if (latestVersion[1] == currentVersion[1])
                {
                    if (latestVersion[2] > currentVersion[2])
                        return true;
                }
            }
            return false;
        }
        catch { return false; };
    }
    private static async Task<string> GetLatestVersion()
    {
        try
        {
            return await GetLatest();
        }
        catch (Exception)
        {
            return "0.0.0";
        }
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
        BoxTab.DisplayOptions();
    }
}


