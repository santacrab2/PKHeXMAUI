using PKHeX.Core;

namespace PKHeXMAUI;

public partial class HomePage : ContentPage
{
    public bool SkipRefresh = false;
	public HomePage()
	{
		InitializeComponent();
        var noSelectVersions = new[] { GameVersion.GO, (GameVersion)0 };
        SaveVersionPicker.ItemsSource = GameInfo.VersionDataSource.Where(z => !noSelectVersions.Contains((GameVersion)z.Value)).ToList();
        SaveVersionPicker.ItemDisplayBinding = new Binding("Text");
        SkipRefresh = true;
        SaveVersionPicker.SelectedItem = GameInfo.VersionDataSource.Where(z =>(GameVersion) z.Value == MainPage.sav.Version).FirstOrDefault();
        SkipRefresh = false;

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
        if (!SkipRefresh)
        {
            var selected = (ComboItem)SaveVersionPicker.SelectedItem;
            Preferences.Set("SaveFile", selected.Value);
            App.Current.MainPage = new AppShell(SaveUtil.GetBlankSAV((GameVersion)selected.Value, "PKHeX"));
        }
    }
}