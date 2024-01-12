using PKHeX.Core;
using CommunityToolkit.Maui.Storage;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class HomePage : ContentPage
{
    public bool SkipRefresh = false;
    public static string OpenPath = "ChangeMe";
    public HomePage()
    {
        InitializeComponent();
        Permissions.RequestAsync<Permissions.StorageWrite>();
        var noSelectVersions = new[] { GameVersion.GO, GameVersion.Any };
        SaveVersionPicker.ItemsSource = GameInfo.VersionDataSource.Where(z => !noSelectVersions.Contains((GameVersion)z.Value)).ToList();
        SaveVersionPicker.ItemDisplayBinding = new Binding("Text");
        SkipRefresh = true;
        SaveVersionPicker.SelectedItem = GameInfo.VersionDataSource.FirstOrDefault(z =>(GameVersion) z.Value == MainPage.sav.Version);
        SkipRefresh = false;
    }

    private async void opensavefile(object sender, EventArgs e)
    {
        var savefile = await FilePicker.PickAsync();
        if (savefile != null)
        {
            var savefilebytes = File.ReadAllBytes(savefile.FullPath);
            var savefileobj = (SaveFile)FileUtil.GetSupportedFile(savefilebytes, "");
            savefileobj.Metadata.SetExtraInfo(savefile.FullPath);
            App.Current.MainPage = new AppShell(savefileobj);
        }

    }

    private void applynewsave(object sender, EventArgs e)
    {
        if (!SkipRefresh)
        {
            var selected = (ComboItem)SaveVersionPicker.SelectedItem;
            Preferences.Set("SaveFile", selected.Value);
            var blanksav = SaveUtil.GetBlankSAV((GameVersion)selected.Value, "PKHeX");

            App.Current.MainPage = new AppShell(blanksav);
        }
    }
    private async void ExportSave(object sender, EventArgs e)
    {
        // Set box now that we're saving
        if (sav.HasBox)
            sav.CurrentBox = BoxTab.CurrentBox;
        var ext = sav.Metadata.GetSuggestedExtension();
        var flags = sav.Metadata.GetSuggestedFlags(ext);
        using var LiveStream = new MemoryStream(sav.Write(flags));
        var result = await FileSaver.Default.SaveAsync(sav.Metadata.FileName, LiveStream, CancellationToken.None);
        if (result.IsSuccessful)
            await DisplayAlert("Success", $"Save file was exported to {result.FilePath}", "cancel");
        else
            await DisplayAlert("Failure", $"Save file did not export due to {result.Exception.Message}", "cancel");
    }
}