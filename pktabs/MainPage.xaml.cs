
using PKHeX.Core;
using System.Net.Sockets;
using PKHeX.Core.AutoMod;
using static pk9reader.MetTab;
using System.Windows.Input;


namespace pk9reader;

public partial class MainPage : ContentPage
{

    
	public MainPage()
	{
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
       




    }
    public static LegalityAnalysis la;
    public static BotBaseRoutines botBase = new();
    public static PK9 pk = new();
    public static SaveFile sav = (SAV9SV) new();
    public static FilteredGameDataSource datasourcefiltered = new(sav, new GameDataSource(GameInfo.Strings));
    public static Socket SwitchConnection = new Socket(SocketType.Stream, ProtocolType.Tcp);
    public static string spriteurl = "iconp.png";

    public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        
        var pkfile = await FilePicker.PickAsync();
        var bytes= File.ReadAllBytes(pkfile.FullPath);
        pk = new(bytes);
        applymainpkinfo(pk);
      


        checklegality();
    }
    public void checklegality()
    {
        la = new(pk);
        legality.Text = la.Valid ? "✔" : "⚠";
        legality.BackgroundColor = la.Valid ? Colors.Green : Colors.Red;
    }
    public void applymainpkinfo(PK9 pkm)
    {
        if (pkm.IsShiny)
            shinybutton.Text = "★";
        
        specieslabel.SelectedIndex = specieslabel.Items.IndexOf(SpeciesName.GetSpeciesName(pkm.Species,2));
        displaypid.Text = $"{pkm.PID:X}";
        nickname.Text = pkm.Nickname;
        exp.Text = $"{pkm.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pkm.EXP, pkm.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedIndex = pkm.Nature;
        Teratypepicker.SelectedIndex = (int)pkm.TeraTypeOverride == 0x13 ? (int)pkm.TeraTypeOriginal : (int)pkm.TeraTypeOverride;
        MainTeratypepicker.SelectedIndex = (int)pkm.TeraTypeOriginal;
        
        if (abilitypicker.Items.Count() != 0)
            abilitypicker.Items.Clear();
        for (int i = 0; i < 3; i++)
        {
            var abili = pkm.PersonalInfo.GetAbilityAtIndex(i);
            abilitypicker.Items.Add($"{(Ability)abili}");

        }
        abilitypicker.SelectedIndex = pkm.Ability;
        Friendshipdisplay.Text = $"{pkm.CurrentFriendship}";
        Heightdisplay.Text = $"{pkm.HeightScalar}";
        Weightdisplay.Text = $"{pkm.WeightScalar}";
        scaledisplay.Text = $"{pkm.Scale}";
        genderdisplay.SelectedIndex = pkm.Gender;
        helditempicker.SelectedIndex = helditempicker.Items.IndexOf(GameInfo.Strings.Item[pkm.HeldItem]);
        formpicker.SelectedIndex = pkm.Form;
        if (pkm.Species == 0)
            spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pkm.Species:0000}{(pkm.Form != 0 ? $"-{pkm.Form:00}" : "")}.png";
        else if (pkm.IsShiny)
            spriteurl = $"https://www.serebii.net/Shiny/SV/{pkm.Species}.png";
        else if (pkm.Form != 0)
            spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pkm.Species:0000}{(pkm.Form != 0 ? $"-{pkm.Form:00}" : "")}.png";
        else
            spriteurl = $"https://www.serebii.net/scarletviolet/pokemon/{pkm.Species}.png";
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
        ComboItem test = (ComboItem)specieslabel.SelectedItem;
        pk.Species = (ushort)test.Value;
        if (abilitypicker.Items.Count() != 0)
            abilitypicker.Items.Clear();
        for (int i = 0; i < 3; i++)
        {
            var abili = pk.PersonalInfo.GetAbilityAtIndex(i);
            abilitypicker.Items.Add($"{(Ability)abili}");

        }
        abilitypicker.SelectedIndex = pk.Ability;
        if(pk.PersonalInfo.Genderless && genderdisplay.SelectedIndex != 2)
        {
            pk.Gender = 2;
            genderdisplay.SelectedIndex = 2;
        }
        if(pk.PersonalInfo.IsDualGender && genderdisplay.SelectedIndex == 2)
        {
            pk.Gender = 0;
            genderdisplay.SelectedIndex = 0;
        }
        if(!pk.IsNicknamed)
            pk.ClearNickname();
        if (formpicker.Items.Count != 0)
            formpicker.Items.Clear();
        var str = GameInfo.Strings;
        var forms = FormConverter.GetFormList(pk.Species, str.types, str.forms, GameInfo.GenderSymbolUnicode, pk.Context);
        foreach (var form in forms)
        {
            formpicker.Items.Add(form);
        }
        formpicker.SelectedIndex = pk.Form;
        if (pk.Species == 0)
            spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk.Species:0000}{(pk.Form != 0 ? $"-{pk.Form:00}" : "")}.png";
        else if (pk.IsShiny)
            spriteurl = $"https://www.serebii.net/Shiny/SV/{pk.Species}.png";
        else if (pk.Form != 0)
            spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk.Species:0000}{(pk.Form != 0 ? $"-{pk.Form:00}" : "")}.png";
        else
            spriteurl = $"https://www.serebii.net/scarletviolet/pokemon/{pk.Species}.png";
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
        if (pk.IsNicknamed)
        {
            pk.SetNickname(pk.Nickname);
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
            pk.EXP = uint.Parse(exp.Text);
            var newlevel = Experience.GetLevel(pk.EXP, pk.PersonalInfo.EXPGrowth);
            pk.CurrentLevel = newlevel;
            leveldisplay.Text = $"{pk.CurrentLevel}";
        }
        checklegality();
    }

    private void applynature(object sender, EventArgs e) { pk.Nature = naturepicker.SelectedIndex; checklegality(); }

        private void applytera(object sender, EventArgs e) { pk.TeraTypeOverride = (MoveType)Teratypepicker.SelectedIndex; checklegality(); }

        private void applymaintera(object sender, EventArgs e) { pk.TeraTypeOriginal = (MoveType)MainTeratypepicker.SelectedIndex; checklegality(); }

    private void applyform(object sender, EventArgs e) 
    {
        pk.Form = (byte)(formpicker.SelectedIndex >= 0 ? formpicker.SelectedIndex : pk.Form);
        spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk.Species:0000}{(pk.Form != 0 ? $"-{pk.Form:00}" : "")}.png";
        pic.Source = spriteurl;
        checklegality();
    }

    private void applyhelditem(object sender, EventArgs e) 
    {
        ComboItem helditemtoapply = (ComboItem)helditempicker.SelectedItem;
        pk.ApplyHeldItem(helditemtoapply.Value, EntityContext.Gen8);
        checklegality();
    }

    private void applyability(object sender, EventArgs e) { pk.SetAbility(abilitypicker.SelectedIndex); checklegality(); }

        private async void botbaseconnect(object sender, EventArgs e)
    {
        
        if (!SwitchConnection.Connected)
        {
            try
            {
                SwitchConnection.Connect(IP.Text, 6000);
                connect.Text = "loading";
            }
            catch (Exception)
            {
                await DisplayAlert("Connection Error", "Ensure you are on WiFi and that your IP is correct", "cancel");
            }
        
          
        }
        else
        {
            SwitchConnection.Disconnect(true);
            connect.Text = "connect";
           SwitchConnection = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }
        if (SwitchConnection.Connected)
        {
            var temp = (SAV9SV)sav;
            var info = temp.MyStatus;
            var off = await botBase.PointerRelative(new long[] { 0x4384B18, 0x148, 0x40 });
            var read = await botBase.ReadBytesAsync((uint)off, info.Data.Length);
            read.CopyTo(info.Data, 0);
            
            sav = temp;
            connect.Text = "disconnect";
        }
    }

    private async void inject(object sender, EventArgs e)
    {
        pk.ResetPartyStats();
        IEnumerable<long> jumps = new long[] { 0x4384B18, 0x128, 0x9B0,0x0};
        var off = await botBase.PointerRelative(jumps);
        await botBase.WriteBytesAsync(pk.EncryptedPartyData, (uint)off + (344 * 30 * uint.Parse(boxnum.Text) + (344 * uint.Parse(slotnum.Text))));
    }

    private void changelevel(object sender, TextChangedEventArgs e)
    {
        if (leveldisplay.Text.Length > 0)
        {
            pk.CurrentLevel = int.Parse(leveldisplay.Text);
            exp.Text = $"{Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth)}";
            pk.EXP = Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth);
        }
        checklegality();
    }

    private void applyfriendship(object sender, TextChangedEventArgs e) { pk.CurrentFriendship = int.Parse(Friendshipdisplay.Text); checklegality(); }

        private void applyheight(object sender, TextChangedEventArgs e) { pk.HeightScalar = (byte)int.Parse(Heightdisplay.Text); checklegality(); }

        private void applyweight(object sender, TextChangedEventArgs e) { pk.WeightScalar = (byte)int.Parse(Weightdisplay.Text); checklegality(); }

        private void applyscale(object sender, TextChangedEventArgs e) { pk.Scale = (byte)int.Parse(scaledisplay.Text); checklegality(); }

        private void applygender(object sender, EventArgs e) { pk.SetGender(genderdisplay.SelectedIndex); checklegality(); }

    private async void read(object sender, EventArgs e)
    {
        IEnumerable<long> jumps = new long[] { 0x4384B18, 0x128, 0x9B0,0x0};
        var off = await botBase.PointerRelative(jumps);
        var bytes = await botBase.ReadBytesAsync((uint)off+(344*30*uint.Parse(boxnum.Text)+(344*uint.Parse(slotnum.Text))),344);
        pk = new(bytes);

        applymainpkinfo(pk);
        checklegality();
    }

    private void legalize(object sender, EventArgs e)
    {
        pk = (PK9)pk.Legalize();
        checklegality();
        applymainpkinfo(pk);
    }

    private async void displaylegalitymessage(object sender, EventArgs e)
    {
        applymainpkinfo(pk);
        checklegality();
        var makelegal = await DisplayAlert("Legality Report", la.Report(), "legalize","ok");
        if (makelegal)
        {
            pk = (PK9)pk.Legalize();
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
    private static byte[] DecryptBlock(uint key, byte[] block)
    {
        var rng = new SCXorShift32(key);
        for (int i = 0; i < block.Length; i++)
            block[i] = (byte)(block[i] ^ rng.Next());
        return block;
    }
}

