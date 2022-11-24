using PKHeX.Core;


namespace pk9reader;

public partial class MainPage : ContentPage
{
	

	public MainPage()
	{
		InitializeComponent();
        specieslabel.ItemsSource = Enum.GetValues(typeof(Species));
        naturepicker.ItemsSource = Enum.GetValues(typeof(Nature));
        Teratypepicker.ItemsSource= Enum.GetValues(typeof(MoveType));
       
        

    }
    public PK9 pk;
    public FileResult pkfile;
   public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        pkfile = await FilePicker.PickAsync();
        var pkbytes = await File.ReadAllBytesAsync(pkfile.FullPath);
        pk = new(pkbytes);
       
        specieslabel.SelectedIndex = pk.Species;
        displaypid.Text = $"{pk.PID:X}";
        nickname.Text = pk.Nickname;
        exp.Text = $"{pk.EXP}";
        naturepicker.SelectedIndex = pk.Nature;
        Teratypepicker.SelectedIndex = pk.TeraType == 0x13 ? pk.MainType : pk.TeraType;
        var str = GameInfo.Strings;
        var forms = FormConverter.GetFormList(pk.Species, str.types, str.forms, GameInfo.GenderSymbolUnicode, EntityContext.Gen8);
        formpicker.ItemsSource = forms;
        formpicker.SelectedIndex = pk.Form;

    }

    public async void pk9saver_Clicked(object sender, EventArgs e)
    {
        
        await File.WriteAllBytesAsync($"/storage/emulated/0/Documents/{(Species)pk.Species}.pk9", pk.pkdata);
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
        nickname.Text = pk.Nickname;
    }
}

