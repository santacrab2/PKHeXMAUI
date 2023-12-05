using System.Runtime.CompilerServices;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;
using System.Reflection;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
namespace PKHeXMAUI;

public partial class LegalitySettings : ContentPage
{
    public static bool skipevent = true;
    public static List<GenericCollection> props = [];
    public LegalitySettings()
	{
		InitializeComponent();
        foreach (var p in new LegalSettings().GetType().GetProperties())
            props.Add(new GenericCollection(p));
        LegalitySettingsCollection.ItemTemplate = new GenericCollectionSelector();
        LegalitySettingsCollection.ItemsSource = props;
	}
}

public class LegalSettings
{
public   bool CheckWordFilter { get => Preferences.Default.Get("CheckWordFilter", true); }
    public  bool AllowGen1Tradeback { get => Preferences.Default.Get("AllowGen1Tradeback", true); }
    public Severity NicknamedTrade { get => (Severity)Preferences.Default.Get("NicknamedTrade", 0); }
    public Severity NicknamedMysteryGift { get => (Severity)Preferences.Default.Get("NicknamedMysteryGift", 0); }
    public Severity RNGFrameNotFound { get => (Severity)Preferences.Default.Get("RNGFrameNotFound", 0); }
    public Severity Gen7TransferStarPID { get => (Severity)Preferences.Default.Get("Gen7TransferStarPID", 0); }
    public Severity Gen8MemoryMissingHT { get => (Severity)Preferences.Default.Get("Gen8MemoryMissingHT", 0); }
    public Severity Gen8TransferTrackerNotPresent { get => (Severity)Preferences.Default.Get("Gen8TransferTrackerNotPresent", -1); }
    public Severity NicknamedAnotherSpecies { get => (Severity)Preferences.Default.Get("NicknamedAnotherSpecies", 0); }
    public Severity ZeroHeightWeight { get => (Severity)Preferences.Default.Get("ZeroHeightWeight", 0); }
    public Severity CurrentHandlerMismatch { get => (Severity)Preferences.Default.Get("CurrentHandlerMismatch", 0); }
    public  bool CheckActiveHandler { get => Preferences.Default.Get("CheckActiveHandler", true); }
}