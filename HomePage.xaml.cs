using PKHeX.Core;

namespace pk9reader;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
        var noSelectVersions = new[] { GameVersion.GO, (GameVersion)0 };
        SaveVersionPicker.ItemsSource = GameInfo.VersionDataSource.Where(z => !noSelectVersions.Contains((GameVersion)z.Value)).ToList();
        SaveVersionPicker.ItemDisplayBinding = new Binding("Text");
    }

    private void openeditorclicked(object sender, EventArgs e)
    {
        var selected = (ComboItem)SaveVersionPicker.SelectedItem;
		App.Current.MainPage = new AppShell(SaveUtil.GetBlankSAV((GameVersion)selected.Value,"PKHeX"));
    }

    private async void opensavefile(object sender, EventArgs e)
    {
        var savefile = await FilePicker.PickAsync();
        var savefilebytes = File.ReadAllBytes(savefile.FullPath);
        var savefileobj = FileUtil.GetSupportedFile(savefilebytes, "");
        App.Current.MainPage = new AppShell((SaveFile)savefileobj);
    }
}