
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PKHeXMAUI;

public partial class ALMSettings : ContentPage
{
    public ALMSettings()
	{
		InitializeComponent();
        var props = new propertyGrid(new PluginSettings());
        Stack_AlmSettings.Children.Add(props);
    }
    public async void TapTapTap(object? sender, TappedEventArgs? e)
    {
        PKHeXSettings.skipevent = true;
        GenericCollectionSelector.Options.SelectedItem = e?.Parameter;
        var result = await DisplayActionSheet("Add Type", "cancel", null, ["Add"]);
        switch (result)
        {
            case "cancel": break;
            case "Add": GenericCollectionSelector.SelectedSource.Add((MoveType?)e?.Parameter??0);
                Preferences.Set("RandomTypes", JsonSerializer.Serialize(GenericCollectionSelector.SelectedSource));
                APILegality.RandTypes = [.. GenericCollectionSelector.SelectedSource];
                GenericCollectionSelector.MoveTypeOptionsSource.Remove((MoveType?)e?.Parameter??0);
                break;

        }
        PKHeXSettings.skipevent = false;
    }
    public async void RemoveTap(object? sender, TappedEventArgs? e)
    {
        GenericCollectionSelector.Selected.SelectedItem = e?.Parameter;
        var result = await DisplayActionSheet("Remove Type", "cancel", null, ["Remove"]);
        switch (result)
        {
            case "cancel": break;
            case "Remove": GenericCollectionSelector.SelectedSource.Remove((MoveType?)e?.Parameter ?? 0);
                Preferences.Set("RandomTypes", JsonSerializer.Serialize(GenericCollectionSelector.SelectedSource));
                APILegality.RandTypes = [.. GenericCollectionSelector.SelectedSource];
                GenericCollectionSelector.MoveTypeOptionsSource.Add((MoveType?)e?.Parameter ?? 0); break;
        }
    }
}

public class PluginSettings
{
    public static string DefaultOT { get => Preferences.Get("DefaultOT", "ALM"); }
    public static string DefaultTID { get => Preferences.Get("DefaultTID", "12345"); }
    public static string DefaultSID { get => Preferences.Get("DefaultSID", "54321"); }
    public static bool UseTrainerData { get => Preferences.Get("UseTrainerData", false); }
    public static string TrainerFolderPath { get => Preferences.Get("TrainerFolderPath", ""); }
    public static bool SetAllLegalRibbons { get => Preferences.Default.Get("SetAllLegalRibbons", false);  }
    public static bool SetBattleVersion { get => Preferences.Default.Get("SetBattleVersion", false);  }
    public static bool SetBallByColor { get => Preferences.Default.Get("SetBallByColor", false);  }
    public static bool EnableMemesForIllegalSets { get => Preferences.Default.Get("EnableMemesForIllegalSets", false);  }
    public static bool ForceLevel100For50 { get => Preferences.Default.Get("ForceLevel100For50", false); }
    public static bool LivingDexAllForms { get => Preferences.Get("LivingDexAllForms", false); }
    public static bool LivingDexNativeOnly { get => Preferences.Get("LivingDexNativeOnly", false); }
    public static bool LivingDexSetAlpha { get => Preferences.Get("LivingDexSetAlpha", false); }
    public static bool LivingDexSetShiny { get => Preferences.Get("LivingDexSetShiny", false); }
    
}