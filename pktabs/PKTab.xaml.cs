
using PKHeX.Core;
using System.Net.Sockets;
using PKHeX.Core.AutoMod;

using System.Windows.Input;


namespace PKHeXMAUI;

public partial class MainPage : ContentPage
{

    
	public MainPage()
	{
        sav = AppShell.AppSaveFile;
        datasourcefiltered = new(sav, new GameDataSource(GameInfo.Strings));
        pk = EntityBlank.GetBlank(sav);
        InitializeComponent();
  
        APILegality.SetAllLegalRibbons = false;
        APILegality.UseTrainerData = true;
        APILegality.AllowTrainerOverride = true;
        APILegality.UseTrainerData = true;
        APILegality.SetMatchingBalls = true;
        Legalizer.EnableEasterEggs = false;
        
        specieslabel.ItemsSource = (System.Collections.IList)datasourcefiltered.Species;
        specieslabel.ItemDisplayBinding = new Binding("Text");
        naturepicker.ItemsSource = Enum.GetValues(typeof(Nature));
        statnaturepicker.ItemsSource = Enum.GetValues(typeof(Nature));
      
        helditempicker.ItemsSource = (System.Collections.IList)datasourcefiltered.Items;
        helditempicker.ItemDisplayBinding= new Binding("Text");
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
        itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinybutton.Text = "★";
        
        specieslabel.SelectedIndex = specieslabel.Items.IndexOf(SpeciesName.GetSpeciesName(pkm.Species,2));
        displaypid.Text = $"{pkm.PID:X}";
        nickname.Text = pkm.Nickname;
        exp.Text = $"{pkm.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pkm.EXP, pkm.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedIndex = pkm.Nature;
    
        
      
        abilitypicker.SelectedIndex =pkm.AbilityNumber == 4? 2: pkm.AbilityNumber-1;
        Friendshipdisplay.Text = $"{pkm.CurrentFriendship}";
      
        genderdisplay.Source = $"gender_{pkm.Gender}.png";
        helditempicker.SelectedIndex = helditempicker.Items.IndexOf(GameInfo.Strings.Item[pkm.HeldItem]);
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
        checklegality();



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
        formargstepper.IsVisible = false;
        formlabel.IsVisible = false;
        formpicker.IsVisible = false;
        ComboItem test = (ComboItem)specieslabel.SelectedItem;
        pk.Species = (ushort)test.Value;
        if (abilitypicker.Items.Count() != 0)
            abilitypicker.Items.Clear();
        for (int i = 0; i < pk.PersonalInfo.AbilityCount; i++)
        {
            var abili = pk.PersonalInfo.GetAbilityAtIndex(i);
            abilitypicker.Items.Add($"{(Ability)abili}");

        }
        abilitypicker.SelectedIndex = 0;
        if(pk.PersonalInfo.Genderless && genderdisplay.Source != (ImageSource)"gender_2.png" )
        {
            pk.Gender = 2;
            genderdisplay.Source = "gender_2.png";
        }
        if(pk.PersonalInfo.IsDualGender && genderdisplay.Source == (ImageSource)"gender_2.png")
        {
            pk.Gender = 0;
            genderdisplay.Source = "gender_0.png";
        }
        if(!pk.IsNicknamed)
            pk.ClearNickname();
        if (formpicker.Items.Count != 0)
            formpicker.Items.Clear();
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
           if(pk is IFormArgument fa)
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

    private void rollpid(object sender, EventArgs e) 
    { 
        
        pk.SetPIDGender(pk.Gender);
        pk.SetRandomEC();
        displaypid.Text = $"{pk.PID:X}";
        checklegality();
    }

    private void applynickname(object sender, TextChangedEventArgs e)
    {

        if (nickname.Text != ((Species)pk.Species).ToString())
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
        if(exp.Text.Length > 0)
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

    private void applynature(object sender, EventArgs e) { pk.Nature = naturepicker.SelectedIndex; checklegality(); }

        

    private void applyform(object sender, EventArgs e) 
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

    private void applyhelditem(object sender, EventArgs e) 
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

    private void applyability(object sender, EventArgs e)
    {
        if (abilitypicker.SelectedIndex != -1)
        {
            var abil = pk.PersonalInfo.GetAbilityAtIndex(abilitypicker.SelectedIndex);
            pk.SetAbility(abil);
        }
    }
    public static bool reconnect = false;
    

    private void changelevel(object sender, TextChangedEventArgs e)
    {
        if (leveldisplay.Text.Length > 0)
        {
            if (!int.TryParse(leveldisplay.Text, out var result))
                return;
            pk.CurrentLevel = result;
            exp.Text = $"{Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth)}";
            pk.EXP = Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth);
        }
        checklegality();
    }

    private void applyfriendship(object sender, TextChangedEventArgs e) 
    {
        if (!int.TryParse(Friendshipdisplay.Text, out var result))
            return;
        if(result > 255)
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
        pk.Language = languagepicker.SelectedIndex; checklegality();
    }

    private void refreshmain(object sender, EventArgs e)
    {
        applymainpkinfo(pk);
    }

    private void nicknamechecker(object sender, CheckedChangedEventArgs e)
    {
        pk.IsNicknamed = nicknamecheck.IsChecked;
        if(!nicknamecheck.IsChecked)
        {
            pk.ClearNickname();
        }
    }

    private void applystatnature(object sender, EventArgs e)
    {
        pk.StatNature = statnaturepicker.SelectedIndex; checklegality();
    }

    private void applyformarg(object sender, TextChangedEventArgs e)
    {
        if (!uint.TryParse(formargstepper.Text, out var formargu))
            return;
        if(pk is IFormArgument fa)
        {
            if (fa.FormArgumentMaximum > 0 && formargu > fa.FormArgumentMaximum)
            {
                formargu = fa.FormArgumentMaximum;
                formargstepper.Text = $"{formargu}";
            }
           fa.FormArgument= formargu;
        }
        
    }

    private void applyisegg(object sender, CheckedChangedEventArgs e)
    {
        pk.IsEgg = iseggcheck.IsChecked;
    }

    private void applyinfection(object sender, CheckedChangedEventArgs e)
    {
        pk.PKRS_Infected = infectedcheck.IsChecked;
    }

    private void applycure(object sender, CheckedChangedEventArgs e)
    {
        pk.PKRS_Cured = curedcheck.IsChecked;
    }
}

