using PKHeX.Core;
using Syncfusion.Maui.Inputs;

namespace PKHeXMAUI;

public partial class ALMSettings : ContentPage
{
    public static List<GenericCollection> props = new();
    public ALMSettings()
	{
		InitializeComponent();
        foreach (var p in new PluginSettings().GetType().GetProperties())
            props.Add(new GenericCollection(p));
        ALMSettingsCollection.ItemTemplate = new GenericCollectionSelector();
        ALMSettingsCollection.ItemsSource = props;
    }
}

public class PluginSettings
{
    public static string DefaultOT { get => Preferences.Get("DefaultOT", "ALM"); }
    public static string DefaultTID { get => Preferences.Get("DefaultTID", "12345"); }
    public static string DefaultSID { get => Preferences.Get("DefaultSID", "54321"); }
    public static bool PrioritizeGame { get => Preferences.Default.Get("PrioritizeGame", false);  }
    public static GameVersion PrioritizeGameVersion { get => (GameVersion)Preferences.Default.Get("PrioritizeGameVersion", 50);  }
    public static bool SetAllLegalRibbons { get => Preferences.Default.Get("SetAllLegalRibbons", false);  }
    public static bool SetBattleVersion { get => Preferences.Default.Get("SetBattleVersion", false);  }
    public static bool SetBallByColor { get => Preferences.Default.Get("SetBallByColor", false);  }
    public static bool EnableMemesForIllegalSets { get => Preferences.Default.Get("EnableMemesForIllegalSets", false);  }
    public static bool ForceLevel100For50 { get => Preferences.Default.Get("ForceLevel100For50", false); }
    public static bool AllowHomeless { get => Preferences.Default.Get("AllowHomeless", false); }
    public static bool LivingDexAllForms { get => Preferences.Get("LivingDexAllForms", false); }
    public static bool LivingDexNativeOnly { get => Preferences.Get("LivingDexNativeOnly", false); }
    public static bool LivingDexSetAlpha { get => Preferences.Get("LivingDexSetAlpha", false); }
    public static bool LivingDexSetShiny { get => Preferences.Get("LivingDexSetShiny", false); }

}