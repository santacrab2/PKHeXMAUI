using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
namespace PKHeXMAUI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        if (PluginSettings.UseTrainerData)
        {
            try
            {
                if (!Android.OS.Environment.IsExternalStorageManager)
                {
                    Intent intent = new();
                    intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
                    Android.Net.Uri uri = Android.Net.Uri.FromParts("package", this.PackageName, null)!;
                    intent.SetData(uri);
                    StartActivity(intent);
                }
            }
            catch (Exception) { }
        }
        base.OnCreate(savedInstanceState);
    }
}
