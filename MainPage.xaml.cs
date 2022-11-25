
using PKHeX.Core;
using System.Net.Sockets;



namespace pk9reader;

public partial class MainPage : ContentPage
{
	

	public MainPage()
	{
		InitializeComponent();
        specieslabel.ItemsSource = Enum.GetValues(typeof(Species));
        naturepicker.ItemsSource = Enum.GetValues(typeof(Nature));
        Teratypepicker.ItemsSource= Enum.GetValues(typeof(MoveType));
        MainTeratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        naturepicker.ItemsSource = Enum.GetValues(typeof(Nature));
        helditempicker.ItemsSource = Util.GetItemsList("en");
       





    }
    BotBaseRoutines botBase = new();
    PK9 pk = new();
    public static Socket SwitchConnection = new Socket(SocketType.Stream, ProtocolType.Tcp);
    public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        
        var pkfile = await FilePicker.PickAsync();
        var bytes= File.ReadAllBytes(pkfile.FullPath);
        pk = new(bytes);

        if (pk.IsShiny)
            shinybutton.Text = "★";
        specieslabel.SelectedIndex = pk.Species;
        displaypid.Text = $"{pk.PID:X}";
        nickname.Text = pk.Nickname;
        exp.Text = $"{pk.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pk.EXP, pk.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedIndex = pk.Nature;
        Teratypepicker.SelectedIndex = (int) pk.TeraTypeOverride == 0x13 ? (int)pk.TeraTypeOriginal : (int)pk.TeraTypeOverride;
        MainTeratypepicker.SelectedIndex = (int)pk.TeraTypeOriginal;
        var str = GameInfo.Strings;
        var forms = FormConverter.GetFormList(pk.Species, str.types, str.forms, GameInfo.GenderSymbolUnicode, EntityContext.Gen9);
        if(forms.Count() != 0) 
        { 
            formpicker.ItemsSource = forms;
        formpicker.SelectedIndex = pk.Form;
        }
        if(abilitypicker.Items.Count() != 0)
            abilitypicker.Items.Clear();
        for(int i = 0; i<3; i++)
        {
            var abili = pk.PersonalInfo.GetAbilityAtIndex(i);
            abilitypicker.Items.Add($"{(Ability)abili}");
            
        }



       
        Friendshipdisplay.Text = $"{pk.CurrentFriendship}";
        Heightdisplay.Text = $"{pk.HeightScalar}";
        Weightdisplay.Text =$"{pk.WeightScalar}";
        scaledisplay.Text = $"{pk.Scale}";
        genderdisplay.SelectedIndex = pk.Gender;
        





    }
    private static int GetSafeIndex(Picker cb, int index) => Math.Max(0, Math.Min(cb.Items.Count - 1, index));
    public async void pk9saver_Clicked(object sender, EventArgs e)
    {
        
            await File.WriteAllBytesAsync($"/storage/emulated/0/Documents/{(Species)pk.Species}.pk9", pk.DecryptedPartyData);
        
    }

    private void specieschanger(object sender, EventArgs e) => pk.Species = (ushort)specieslabel.SelectedIndex;

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

    private void applyexp(object sender, TextChangedEventArgs e) => pk.EXP = uint.Parse(exp.Text);

    private void applynature(object sender, EventArgs e) => pk.Nature = naturepicker.SelectedIndex;

    private void applytera(object sender, EventArgs e) => pk.TeraTypeOverride = (MoveType)Teratypepicker.SelectedIndex;

    private void applymaintera(object sender, EventArgs e) => pk.TeraTypeOriginal = (MoveType)MainTeratypepicker.SelectedIndex;

    private void applyform(object sender, EventArgs e) => pk.Form = (byte)formpicker.SelectedIndex;

    private void applyhelditem(object sender, EventArgs e) => pk.ApplyHeldItem(helditempicker.SelectedIndex, EntityContext.Gen8);

    private void applyability(object sender, EventArgs e) => pk.SetAbility((int)(Ability)abilitypicker.SelectedItem);

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
        pk.CurrentLevel = int.Parse(leveldisplay.Text);
        exp.Text = $"{Experience.GetEXP(pk.CurrentLevel, pk.PersonalInfo.EXPGrowth)}";
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

        if (pk.IsShiny)
            shinybutton.Text = "★";
        specieslabel.SelectedIndex = pk.Species;
        displaypid.Text = $"{pk.PID:X}";
        nickname.Text = pk.Nickname;
        exp.Text = $"{pk.EXP}";
        leveldisplay.Text = $"{Experience.GetLevel(pk.EXP, pk.PersonalInfo.EXPGrowth)}";
        naturepicker.SelectedIndex = pk.Nature;
        Teratypepicker.SelectedIndex = (int)pk.TeraTypeOverride == 0x13 ? (int)pk.TeraTypeOriginal : (int)pk.TeraTypeOverride;
        MainTeratypepicker.SelectedIndex = (int)pk.TeraTypeOriginal;
        var str = GameInfo.Strings;
        var forms = FormConverter.GetFormList(pk.Species, str.types, str.forms, GameInfo.GenderSymbolUnicode, EntityContext.Gen9);
        if (forms.Count() != 0)
        {
            formpicker.ItemsSource = forms;
            formpicker.SelectedIndex = pk.Form;
        }
        if (abilitypicker.Items.Count() != 0)
            abilitypicker.Items.Clear();
        for (int i = 0; i < 3; i++)
        {
            var abili = pk.PersonalInfo.GetAbilityAtIndex(i);
            abilitypicker.Items.Add($"{(Ability)abili}");

        }



        
        Friendshipdisplay.Text = $"{pk.CurrentFriendship}";
        Heightdisplay.Text = $"{pk.HeightScalar}";
        Weightdisplay.Text = $"{pk.WeightScalar}";
        scaledisplay.Text = $"{pk.Scale}";
        genderdisplay.SelectedIndex = pk.Gender;

    }
}

