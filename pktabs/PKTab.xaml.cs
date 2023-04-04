
using PKHeX.Core;
using System.Net.Sockets;
using PKHeX.Core.AutoMod;

using System.Windows.Input;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;

namespace PKHeXMAUI;

public partial class MainPage : ContentPage
{

    public bool SkipTextChange = false;
	public MainPage()
	{
        sav = AppShell.AppSaveFile;
        datasourcefiltered = new(sav, new GameDataSource(GameInfo.Strings));
        pk = EntityBlank.GetBlank(sav.Generation,(GameVersion)sav.Version);
        InitializeComponent();
        Permissions.RequestAsync<Permissions.StorageWrite>();
        APILegality.SetAllLegalRibbons = false;
        APILegality.UseTrainerData = true;
        APILegality.AllowTrainerOverride = true;
        APILegality.UseTrainerData = true;
        APILegality.SetMatchingBalls = true;
        Legalizer.EnableEasterEggs = false;
        
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
        languagepicker.ItemsSource = Enum.GetValues(typeof(LanguageID));
  
        checklegality();




    }
    public static LegalityAnalysis la;

    public static PKM pk;
    public static SaveFile sav;
    public static FilteredGameDataSource datasourcefiltered;
    public static Socket SwitchConnection = new Socket(SocketType.Stream, ProtocolType.Tcp);
    public static string spriteurl = "iconp.png";
    public static string ipaddy = "";
    public static string itemspriteurl = "";
    public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        
        var pkfile = await FilePicker.PickAsync();
        var bytes= File.ReadAllBytes(pkfile.FullPath);
        pk = EntityFormat.GetFromBytes(bytes);
        applymainpkinfo(pk);
        checklegality();
    }
    public void checklegality()
    {
        la = new(pk,sav.Personal);
        legality.Source = la.Valid ? "valid.png" : "warn.png";
        
        
    }
    public void applymainpkinfo(PKM pkm)
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
      
        abilitypicker.SelectedIndex =pkm.AbilityNumber == 4? 2: pkm.AbilityNumber-1;
        Friendshipdisplay.Text = $"{pkm.CurrentFriendship}";
      
        genderdisplay.Source = $"gender_{pkm.Gender}.png";
        helditempicker.SelectedItem = datasourcefiltered.Items.Where(z=>z.Text== (GameInfo.Strings.Item[pkm.HeldItem])).FirstOrDefault();
        if (pkm.HeldItem > 0)
        {
            itemsprite.IsVisible = true;
            if (sav is SAV9SV)
            {
                itemspriteurl = $"aitem_{pkm.HeldItem}.png"; 
                itemsprite.Source = $"aitem_{pkm.HeldItem}.png";
            }
            else
            {
                itemspriteurl = $"bitem_{pkm.HeldItem}.png";
                itemsprite.Source = $"bitem_{pkm.HeldItem}.png";
            }
        }
        formpicker.SelectedIndex = pkm.Form;
      
            if (pkm.Species == 0)
                spriteurl = $"a_egg.png";
            else 
                spriteurl = $"a_{pkm.Species}{(pkm.Form >0?$"_{pkm.Form}":"")}.png";
            if (pkm.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible= false;
        
        
      
        pic.Source = spriteurl;
    
        languagepicker.SelectedIndex = pkm.Language;
        nicknamecheck.IsChecked = pkm.IsNicknamed;
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
            formargstepper.IsVisible = false;
            formlabel.IsVisible = false;
            formpicker.IsVisible = false;
            ComboItem test = (ComboItem)specieslabel.SelectedItem ?? new ComboItem("None", 0);
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
                spriteurl = $"a_{pk.Species}{(pk.Form > 0 ? $"_{pk.Form}" : "")}.png";
            if (pk.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible = false;



            pic.Source = spriteurl;
            checklegality();
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
                spriteurl = $"a_{pk.Species}{(pk.Form > 0 ? $"_{pk.Form}" : "")}.png";
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

    private async void legalize(object sender, EventArgs e)
    {
        try
        {
            pk = pk.Legalize();
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
        var makelegal = await DisplayAlert("Legality Report", la.Report(), "legalize","ok");
        if (makelegal)
        {
            pk = pk.Legalize();
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
}


