
using PKHeX.Core;
namespace PKHeXMAUI;

public partial class LegalitySettings : ContentPage
{
    public static bool skipevent = true;
    public static List<GenericCollection> props = [];
    public LegalitySettings()
	{
		InitializeComponent();
        var prop = new propertyGrid(new LegalSettings());
        Stack_LegalitySettings.Children.Add(prop);
    }
}

public class LegalSettings
{
public   bool CheckWordFilter { get => Preferences.Default.Get("CheckWordFilter", true); set => Preferences.Set("CheckWordFilter", value); }
    public  bool AllowGen1Tradeback { get => Preferences.Default.Get("AllowGen1Tradeback", true); set => Preferences.Set("AllowGen1Tradeback", value); }
    public Severity NicknamedTrade { get => (Severity)Preferences.Default.Get("NicknamedTrade", 0); set => Preferences.Set("NicknamedTrade", (sbyte)value); }
    public Severity NicknamedMysteryGift { get => (Severity)Preferences.Default.Get("NicknamedMysteryGift", 0); set => Preferences.Set("NicknamedMysteryGift", (sbyte)value); }
    public Severity RNGFrameNotFound { get => (Severity)Preferences.Default.Get("RNGFrameNotFound", 0); set => Preferences.Set("RNGFrameNotFound", (sbyte)value); }
    public Severity Gen7TransferStarPID { get => (Severity)Preferences.Default.Get("Gen7TransferStarPID", 0); set => Preferences.Set("Gen7TransferStarPID", (sbyte)value); }
    public Severity Gen8MemoryMissingHT { get => (Severity)Preferences.Default.Get("Gen8MemoryMissingHT", 0); set => Preferences.Set("Gen8MemoryMissingHT", (sbyte)value); }
    public Severity Gen8TransferTrackerNotPresent { get => (Severity)Preferences.Default.Get("Gen8TransferTrackerNotPresent", -1); set => Preferences.Set("Gen8TransferTrackerNotPresent", (sbyte)value); }
    public Severity NicknamedAnotherSpecies { get => (Severity)Preferences.Default.Get("NicknamedAnotherSpecies", 0); set => Preferences.Set("NicknamedAnotherSpecies", (sbyte)value); }
    public Severity ZeroHeightWeight { get => (Severity)Preferences.Default.Get("ZeroHeightWeight", 0); set => Preferences.Set("ZeroHeightWeight", (sbyte)value); }
    public Severity CurrentHandlerMismatch { get => (Severity)Preferences.Default.Get("CurrentHandlerMismatch", 0); set => Preferences.Set("CurrentHandlerMismatch", (sbyte)value); }
    public  bool CheckActiveHandler { get => Preferences.Default.Get("CheckActiveHandler", false); set => Preferences.Set("CheckActiveHandler", value); }
}