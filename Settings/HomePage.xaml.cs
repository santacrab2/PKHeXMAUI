using PKHeX.Core;
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

        var savefilebytes = File.ReadAllBytes(savefile.FullPath);
        var savefileobj = (SaveFile)FileUtil.GetSupportedFile(savefilebytes, "");
        savefileobj.Metadata.SetExtraInfo(savefile.FullPath);
        App.Current.MainPage = new AppShell(savefileobj);
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

    private void OpenItems(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new Items());
    }

    private async void ExportSave(object sender, EventArgs e)
    {
        // Set box now that we're saving
        if (sav.HasBox)
            sav.CurrentBox = BoxTab.CurrentBox;

#if ANDROID

        string path = "";
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            try
            {
                path = "/storage/emulated/0/Documents/";
                var ext = sav.Metadata.GetSuggestedExtension();
                var flags = sav.Metadata.GetSuggestedFlags(ext);
                await File.WriteAllBytesAsync($"{path}/{sav.Metadata.FileName}", sav.Write(flags));
                sav.State.Edited = false;
               // sav.Metadata.SetExtraInfo($"{path}/{OpenPath}{ext}");
            }
            catch(Exception)
            {
                path = "/storage/emulated/0/Documents/";
                var ext = sav.Metadata.GetSuggestedExtension();
                var flags = sav.Metadata.GetSuggestedFlags(ext);
                await File.WriteAllBytesAsync($"{path}/ChangeMe", sav.Write(flags));
                sav.State.Edited = false;
            }
        }
        else
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                try
                {
                    path = "/storage/emulated/0/Android/data/com.PKHeX.maui/";
                    var ext = sav.Metadata.GetSuggestedExtension();
                    var flags = sav.Metadata.GetSuggestedFlags(ext);
                    await File.WriteAllBytesAsync($"{path}/{sav.Metadata.FileName}", sav.Write(flags));
                    sav.State.Edited = false;
                }
                catch (Exception)
                {
                    path = "/storage/emulated/0/Android/data/com.PKHeX.maui/";
                    var ext = sav.Metadata.GetSuggestedExtension();
                    var flags = sav.Metadata.GetSuggestedFlags(ext);
                    await File.WriteAllBytesAsync($"{path}/ChangeMe", sav.Write(flags));
                    sav.State.Edited = false;
                }
            }
            else
            {
                try
                {
                    path = "/storage/emulated/0/";
                    var ext = sav.Metadata.GetSuggestedExtension();
                    var flags = sav.Metadata.GetSuggestedFlags(ext);
                    await File.WriteAllBytesAsync($"{path}/{sav.Metadata.FileName}", sav.Write(flags));
                    sav.State.Edited = false;
                }
                catch (Exception)
                {
                    path = "/storage/emulated/0/";
                    var ext = sav.Metadata.GetSuggestedExtension();
                    var flags = sav.Metadata.GetSuggestedFlags(ext);
                    await File.WriteAllBytesAsync($"{path}/ChangeMe", sav.Write(flags));
                    sav.State.Edited = false;
                }
            }
        }
        await DisplayAlert("Saved",$"Save File has been saved to {path}", "ok");

#endif
    }
}