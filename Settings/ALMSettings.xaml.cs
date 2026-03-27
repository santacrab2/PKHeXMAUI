
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
        var result = await DisplayActionSheetAsync("Add Type", "cancel", null, ["Add"]);
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
        var result = await DisplayActionSheetAsync("Remove Type", "cancel", null, ["Remove"]);
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
    public static string DefaultOT { get => Preferences.Get("DefaultOT", "ALM"); set => Preferences.Set("DefaultOT", value); }
    public static string DefaultTID { get => Preferences.Get("DefaultTID", "12345"); set => Preferences.Set("DefaultTID", value); }
    public static string DefaultSID { get => Preferences.Get("DefaultSID", "54321"); set => Preferences.Set("DefaultSID", value); }
    public static bool UseTrainerData { get => Preferences.Get("UseTrainerData", false); set => Preferences.Set("UseTrainerData", value); }
    public static string TrainerFolderPath { get => Preferences.Get("TrainerFolderPath", ""); set => Preferences.Set("TrainerFolderPath", value); }
    public static bool SetAllLegalRibbons { get => Preferences.Default.Get("SetAllLegalRibbons", false); set => Preferences.Set("SetAllLegalRibbons", value);  }
    public static bool SetBattleVersion { get => Preferences.Default.Get("SetBattleVersion", false); set => Preferences.Set("SetBattleVersion", value);  }
    public static bool SetBallByColor { get => Preferences.Default.Get("SetBallByColor", false); set => Preferences.Set("SetBallByColor", value);  }
    public static bool EnableMemesForIllegalSets { get => Preferences.Default.Get("EnableMemesForIllegalSets", false); set => Preferences.Set("EnableMemesForIllegalSets", value);  }
    public static bool ForceLevel100For50 { get => Preferences.Default.Get("ForceLevel100For50", false); set => Preferences.Set("ForceLevel100For50", value); }
    public static bool LivingDexAllForms { get => Preferences.Get("LivingDexAllForms", false); set => Preferences.Set("LivingDexAllForms", value); }
    public static bool LivingDexNativeOnly { get => Preferences.Get("LivingDexNativeOnly", false); set => Preferences.Set("LivingDexNativeOnly", value); }
    public static bool LivingDexSetAlpha { get => Preferences.Get("LivingDexSetAlpha", false); set => Preferences.Set("LivingDexSetAlpha", value); }
    public static bool LivingDexSetShiny { get => Preferences.Get("LivingDexSetShiny", false); set => Preferences.Set("LivingDexSetShiny", value); }
    
}