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

   public async void pk9picker_Clicked(object sender, EventArgs e)
    {
        var pkfile = await FilePicker.PickAsync();
        var pkbytes = await File.ReadAllBytesAsync(pkfile.FullPath);
        PK9 pk = new(pkbytes);
       
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
}

