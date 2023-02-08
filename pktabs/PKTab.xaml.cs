using static System.Buffers.Binary.BinaryPrimitives;
using PKHeX.Core;
using System.Net.Sockets;
using PKHeX.Core.AutoMod;
using static pk9reader.MetTab;
using System.Windows.Input;
using System.Collections.Generic;
using System.Numerics;

namespace pk9reader;

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
        Teratypepicker.ItemsSource= Enum.GetValues(typeof(MoveType));
        MainTeratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        helditempicker.ItemsSource = (System.Collections.IList)datasourcefiltered.Items;
        helditempicker.ItemDisplayBinding= new Binding("Text");
        languagepicker.ItemsSource = Enum.GetValues(typeof(LanguageID));
        ICommand refreshCommand = new Command(() =>
        {
         
            checklegality();
            mainrefresh.IsRefreshing = false;
            
        });
        mainrefresh.Command = refreshCommand;
        checklegality();




    }
    public static LegalityAnalysis la;

    public static PKM pk;
    public static SaveFile sav;
    public static FilteredGameDataSource datasourcefiltered;
    public static Socket SwitchConnection = new Socket(SocketType.Stream, ProtocolType.Tcp);
    public static string spriteurl = "iconp.png";
    public static string ipaddy = "";
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
        legality.Text = la.Valid ? "✔" : "⚠";
        legality.BackgroundColor = la.Valid ? Colors.Green : Colors.Red;
        
    }
    public void applymainpkinfo(PKM pkm)
    {
        if (pkm.IsShiny)
            shinybutton.Text = "★";
        
        specieslabel.SelectedIndex = specieslabel.Items.IndexOf(SpeciesName.GetSpeciesName(pkm.Species,2));
        displaypid.Text = $"{pkm.PID:X}";
        nickname.Text = pkm.Nickname;
        exp.Text = $"{pkm.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pkm.EXP, pkm.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedIndex = pkm.Nature;
        if (pkm is PK9 pk9)
        {
            OvTeralabel.IsVisible = true;
            OrTeralabel.IsVisible = true;
            Teratypepicker.IsVisible = true;
            MainTeratypepicker.IsVisible = true;
            Teratypepicker.SelectedIndex = (int)pk9.TeraTypeOverride == 0x13 ? (int)pk9.TeraTypeOriginal : (int)pk9.TeraTypeOverride;
            MainTeratypepicker.SelectedIndex = (int)pk9.TeraTypeOriginal;
        }
        
      
        abilitypicker.SelectedIndex =pkm.AbilityNumber == 4? 2: pkm.AbilityNumber-1;
        Friendshipdisplay.Text = $"{pkm.CurrentFriendship}";
      
        genderdisplay.Source = $"gender_{pkm.Gender}.png";
        helditempicker.SelectedIndex = helditempicker.Items.IndexOf(GameInfo.Strings.Item[pkm.HeldItem]);
        formpicker.SelectedIndex = pkm.Form;
        if (sav is SAV9SV)
        {
            if (pkm.Species == 0)
                spriteurl = $"a_egg.png";
            else 
                spriteurl = $"a_{pkm.Species}{(pkm.Form >0?$"_{pkm.Form}":"")}.png";
            if (pkm.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible= false;
        
        }
        else
        {

            if (pkm.IsShiny)
            {
                shinysparklessprite.IsVisible = true;
                spriteurl = $"b_{pkm.Species}{(pkm.Form != 0 ? $"_{pkm.Form}" : "")}s.png";
            }
            else
            {
                shinysparklessprite.IsVisible = false;
                spriteurl = $"b_{pkm.Species}{(pkm.Form > 0 ? $"_{pkm.Form}" : "")}.png";
            }

        }
        pic.Source = spriteurl;
    
        languagepicker.SelectedIndex = pkm.Language;
        nicknamecheck.IsChecked = pkm.IsNicknamed;
        checklegality();



    }
    public async void pk9saver_Clicked(object sender, EventArgs e)
    {
        pk.ResetPartyStats();
        await File.WriteAllBytesAsync($"/storage/emulated/0/Documents/{pk.FileName}", pk.DecryptedPartyData);
        
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
        for (int i = 0; i < 3; i++)
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
        if (sav is SAV9SV)
        {
            if (pk.Species == 0)
                spriteurl = $"a_egg.png";
            else
                spriteurl = $"a_{pk.Species}{(pk.Form > 0 ? $"_{pk.Form}" : "")}.png";
            if (pk.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible = false;

        }
        else
        {

            if (pk.IsShiny)
            {
                shinysparklessprite.IsVisible = true;
                spriteurl = $"b_{pk.Species}{(pk.Form != 0 ? $"_{pk.Form}" : "")}s.png";
            }
            else
            {
                shinysparklessprite.IsVisible = false;
                spriteurl = $"b_{pk.Species}{(pk.Form > 0 ? $"_{pk.Form}" : "")}.png";
            }

        }
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

        private void applytera(object sender, EventArgs e) { if (pk is PK9 pk9) { pk9.TeraTypeOverride = (MoveType)Teratypepicker.SelectedIndex; checklegality(); } }

        private void applymaintera(object sender, EventArgs e) { if(pk is PK9 pk9) pk9.TeraTypeOriginal = (MoveType)MainTeratypepicker.SelectedIndex; checklegality(); }

    private void applyform(object sender, EventArgs e) 
    {
        pk.Form = (byte)(formpicker.SelectedIndex >= 0 ? formpicker.SelectedIndex : pk.Form);
        if (sav is SAV9SV)
        {
            if (pk.Species == 0)
                spriteurl = $"a_egg.png";
            else
                spriteurl = $"a_{pk.Species}{(pk.Form > 0 ? $"_{pk.Form}" : "")}.png";
            if (pk.IsShiny)
                shinysparklessprite.IsVisible = true;
            else
                shinysparklessprite.IsVisible = false;

        }
        else
        {

            if (pk.IsShiny)
            {
                shinysparklessprite.IsVisible = true;
                spriteurl = $"b_{pk.Species}{(pk.Form != 0 ? $"_{pk.Form}" : "")}s.png";
            }
            else
            {
                shinysparklessprite.IsVisible = false;
                spriteurl = $"b_{pk.Species}{(pk.Form > 0 ? $"_{pk.Form}" : "")}.png";
            }

        }
        pic.Source = spriteurl;
        checklegality();
    }

    private void applyhelditem(object sender, EventArgs e) 
    {
        ComboItem helditemtoapply = (ComboItem)helditempicker.SelectedItem;
        pk.ApplyHeldItem(helditemtoapply.Value, EntityContext.Gen8);
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
}

