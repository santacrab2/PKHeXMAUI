
using PKHeX.Core;
using System.Net.Sockets;
using CommunityToolkit.Maui.Core;

namespace pk9reader;

public partial class MainPage : ContentPage
{

    
	public MainPage()
	{
		InitializeComponent();
      
   
        specieslabel.ItemsSource = (System.Collections.IList)datasourcefiltered.Species;
        specieslabel.ItemDisplayBinding = new Binding("Text");
        naturepicker.ItemsSource = Enum.GetValues(typeof(Nature));
        Teratypepicker.ItemsSource= Enum.GetValues(typeof(MoveType));
        MainTeratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        helditempicker.ItemsSource = (System.Collections.IList)datasourcefiltered.Items;
        helditempicker.ItemDisplayBinding= new Binding("Text");

        





    }
    MetTab met = new();
    BotBaseRoutines botBase = new();
    public static PK9 pk = new();
    public static SaveFile sav = (SAV9SV) new();
    FilteredGameDataSource datasourcefiltered = new(sav, new GameDataSource(GameInfo.Strings));
    public static Socket SwitchConnection = new Socket(SocketType.Stream, ProtocolType.Tcp);
    public static string spriteurl = "iconp.png";
    public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        
        var pkfile = await FilePicker.PickAsync();
        var bytes= File.ReadAllBytes(pkfile.FullPath);
        pk = new(bytes);
        applymainpkinfo(pk);
        met.applymetpkinfo(pk);





    }
    public void applymainpkinfo(PK9 pkm)
    {
        if (pkm.IsShiny)
            shinybutton.Text = "★";
        specieslabel.SelectedIndex = specieslabel.Items.IndexOf($"{(Species)pkm.Species}");
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
        spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pkm.Species:0000}{(pkm.Form != 0 ? $"-{pkm.Form:00}" : "")}.png";
        pic.Source = spriteurl;
       
        


    }
    private static int GetSafeIndex(Picker cb, int index) => Math.Max(0, Math.Min(cb.Items.Count - 1, index));
    public async void pk9saver_Clicked(object sender, EventArgs e)
    {
        
            await File.WriteAllBytesAsync($"/storage/emulated/0/Documents/{(Species)pk.Species}.pk9", pk.DecryptedPartyData);
        
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
        spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk.Species:0000}{(pk.Form != 0 ? $"-{pk.Form:00}" : "")}.png";
        pic.Source = spriteurl;
    }

    private void rollpid(object sender, EventArgs e) 
    { 
        pk.SetPIDGender(pk.Gender);
        displaypid.Text = $"{pk.PID:X}";
    }

    private void applynickname(object sender, TextChangedEventArgs e)
    {
        pk.SetNickname(pk.Nickname);
        
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
    }

    private void applynature(object sender, EventArgs e) => pk.Nature = naturepicker.SelectedIndex;

    private void applytera(object sender, EventArgs e) => pk.TeraTypeOverride = (MoveType)Teratypepicker.SelectedIndex;

    private void applymaintera(object sender, EventArgs e) => pk.TeraTypeOriginal = (MoveType)MainTeratypepicker.SelectedIndex;

    private void applyform(object sender, EventArgs e) 
    {
        pk.Form = (byte)(formpicker.SelectedIndex >= 0 ? formpicker.SelectedIndex : pk.Form);
        spriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk.Species:0000}{(pk.Form != 0 ? $"-{pk.Form:00}" : "")}.png";
        pic.Source = spriteurl;
    }

    private void applyhelditem(object sender, EventArgs e) 
    {
        ComboItem helditemtoapply = (ComboItem)helditempicker.SelectedItem;
        pk.ApplyHeldItem(helditemtoapply.Value, EntityContext.Gen8); 
    }

    private void applyability(object sender, EventArgs e) => pk.SetAbility(abilitypicker.SelectedIndex);

    private void botbaseconnect(object sender, EventArgs e)
    {
        if (!SwitchConnection.Connected)
        {
            SwitchConnection.Connect(IP.Text,6000);
            connect.Text = "disconnect";
        }
        else
        {
            SwitchConnection.Disconnect(true);
            connect.Text = "connect";
        }
    }

    private async void inject(object sender, EventArgs e)
    {
        IEnumerable<long> jumps = new long[] { 0x42FD510, 0xA90, 0x9B0, 0x0 };
        var off = await botBase.PointerRelative(jumps);
        await botBase.WriteBytesAsync(pk.EncryptedPartyData, (uint)off);
    }

    private void changelevel(object sender, TextChangedEventArgs e)
    {
        if (leveldisplay.Text.Length > 0)
        {
            pk.CurrentLevel = int.Parse(leveldisplay.Text);
            exp.Text = $"{Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth)}";
            pk.EXP = Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth);
        }
    }

    private void applyfriendship(object sender, TextChangedEventArgs e) => pk.CurrentFriendship = int.Parse(Friendshipdisplay.Text);

    private void applyheight(object sender, TextChangedEventArgs e) => pk.HeightScalar = (byte)int.Parse(Heightdisplay.Text);

    private void applyweight(object sender, TextChangedEventArgs e) => pk.WeightScalar = (byte)int.Parse(Weightdisplay.Text);

    private void applyscale(object sender, TextChangedEventArgs e) => pk.Scale = (byte)int.Parse(scaledisplay.Text);

    private void applygender(object sender, EventArgs e) => pk.SetGender(genderdisplay.SelectedIndex);

    private async void read(object sender, EventArgs e)
    {
        IEnumerable<long> jumps = new long[] { 0x42FD510, 0xA90, 0x9B0, 0x0 };
        var off = await botBase.PointerRelative(jumps);
        var bytes = await botBase.ReadBytesAsync((uint)off, 344);
        pk = new(bytes);

        applymainpkinfo(pk);

    }
}

