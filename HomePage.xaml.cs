using PKHeX.Core;

namespace PKHeXMAUI;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
        var noSelectVersions = new[] { GameVersion.GO, (GameVersion)0 };
        SaveVersionPicker.ItemsSource = GameInfo.VersionDataSource.Where(z => !noSelectVersions.Contains((GameVersion)z.Value)).ToList();
        SaveVersionPicker.ItemDisplayBinding = new Binding("Text");
        
    }


    private async void opensavefile(object sender, EventArgs e)
    {
        var savefile = await FilePicker.PickAsync();
        var savefilebytes = File.ReadAllBytes(savefile.FullPath);
        var savefileobj = FileUtil.GetSupportedFile(savefilebytes, "");
        App.Current.MainPage = new AppShell((SaveFile)savefileobj);
    }

    private void applynewsave(object sender, EventArgs e)
    {
        var selected = (ComboItem)SaveVersionPicker.SelectedItem;
        App.Current.MainPage = new AppShell(SaveUtil.GetBlankSAV((GameVersion)selected.Value, "PKHeX"));
    }
}