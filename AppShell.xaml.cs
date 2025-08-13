
using CommunityToolkit.Maui.Storage;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System.Windows.Input;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class AppShell : Shell
{
	public AppShell(SaveFile sav)
	{
        AppSaveFile = sav;
        InitializeComponent();
        Shelltest = TheShell;
        TheShell.ItemTemplate = new FlyoutCollectionSelector();
        TheShell.MenuItemTemplate = new FlyoutCollectionSelector();
    }
	public static SaveFile? AppSaveFile { get; set; }
    public BoxManipulator manip = new BoxManipulatorMAUI();
    public static bool boxexpanded = false;
    public static bool pkexpanded = false;
    public static bool fileexpanded = false;
    public static bool dataexpanded = false;
    public static Shell? Shelltest;
    public async void DropdownExpansion(object? sender, EventArgs? e)
    {
        if (((string?)((ImageButton?)sender)?.CommandParameter) == "Box/Party")
        {
            if (!boxexpanded)
            {
                boxexpanded = true;
                SetFlyoutItemIsVisible(DeleteBoxes, true);
                SetFlyoutItemIsVisible(SortBoxes, true);
                SetFlyoutItemIsVisible(SortBoxesAdvanced, true);
                SetFlyoutItemIsVisible(ModifyBoxes, true);
            }
            else
            {
                SetFlyoutItemIsVisible(DeleteBoxes, false);
                SetFlyoutItemIsVisible(SortBoxes, false);
                SetFlyoutItemIsVisible(SortBoxesAdvanced, false);
                SetFlyoutItemIsVisible(ModifyBoxes, false);
                DeleteExpanded = true;
                DeleteClicked(sender, e);
                SortExpanded = true;
                SortClick(sender, e);
                SortAdvancedExpanded = true;
                SortBoxesAdvancedClicked(sender, e);
                ModifyExpanded = true;
                ModifyBoxesClicked(sender, e);
                boxexpanded = false;
            }
            return;
        }
        if (((string?)((ImageButton?)sender)?.CommandParameter) == "pk editor")
        {
            if (!pkexpanded)
            {
                SetFlyoutItemIsVisible(thelegalizer, true);
                SetFlyoutItemIsVisible(impshow, true);
                SetFlyoutItemIsVisible(expshow, true);
                pkexpanded = true;
            }
            else
            {
                SetFlyoutItemIsVisible(thelegalizer, false);
                SetFlyoutItemIsVisible(impshow, false);
                SetFlyoutItemIsVisible(expshow, false);
                pkexpanded = false;
            }
            return;
        }
        if (((string?)((ImageButton?)sender)?.CommandParameter) == "File")
        {
            if (!fileexpanded)
            {
                SetFlyoutItemIsVisible(OpenFile,true);
                SetFlyoutItemIsVisible(SavePKM, true);
                SetFlyoutItemIsVisible(ExportSave, true);
                fileexpanded = true;
            }
            else
            {
                SetFlyoutItemIsVisible(OpenFile, false);
                SetFlyoutItemIsVisible(SavePKM, false);
                SetFlyoutItemIsVisible(ExportSave, false);
                fileexpanded = false;
            }
        }
        if (((string?)((ImageButton?)sender)?.CommandParameter) == "Data")
        {
            if (!dataexpanded)
            {
                SetFlyoutItemIsVisible(LoadBoxes,true);
                SetFlyoutItemIsVisible(DumpBox,true);
                SetFlyoutItemIsVisible(SaveBoxData, true);
                SetFlyoutItemIsVisible(MenuBatchEditor, true);
                dataexpanded = true;
            }
            else
            {
                SetFlyoutItemIsVisible(LoadBoxes, false);
                SetFlyoutItemIsVisible(DumpBox, false);
                SetFlyoutItemIsVisible(SaveBoxData, false);
                SetFlyoutItemIsVisible(MenuBatchEditor, false);
                dataexpanded = false;
            }
        }
    }
    public async void checkbox(object sender, EventArgs e)
    {
        switch (TheShell.CurrentPage)
        {
            case MainPage:
                if (!((MainPage)TheShell.CurrentPage).FirstLoad)
                    ((MainPage)TheShell.CurrentPage).applymainpkinfo(pk);
                break;
            case MetTab:
                if (!((MetTab)TheShell.CurrentPage).FirstLoad)
                    ((MetTab)TheShell.CurrentPage).applymetinfo(pk);
                break;
            case AttacksTab:
                if (!((AttacksTab)TheShell.CurrentPage).FirstLoad)
                    ((AttacksTab)TheShell.CurrentPage).applyattackinfo(pk);
                break;
            case Cosmeticstab:
                if (!((Cosmeticstab)TheShell.CurrentPage).FirstLoad)
                    ((Cosmeticstab)TheShell.CurrentPage).applycomsetics(pk);
                break;
            case OTTab:
                if (!((OTTab)TheShell.CurrentPage).FirstLoad)
                    ((OTTab)TheShell.CurrentPage).applyotinfo(pk);
                break;
            case StatsTab:
                if (!((StatsTab)TheShell.CurrentPage).FirstLoad)
                    ((StatsTab)TheShell.CurrentPage).applystatsinfo(pk);
                break;
            case PartyTab:
                ((PartyTab)TheShell.CurrentPage).fillParty();
                break;

        }
        if (TheShell.CurrentPage.GetType() != typeof(BoxTab))
        {
            try
            {
                ((BoxTab)TheShell.CurrentPage).boxview.SelectedItem = null;
            }
            catch(Exception) { }
        }
    }
    public bool SortExpanded = false;
    public bool DeleteExpanded = false;
    public bool SortAdvancedExpanded = false;
    public bool ModifyExpanded = false;
    private void DeleteClicked(object? sender, EventArgs? e)
    {
        if (DeleteExpanded)
        {
            SetFlyoutItemIsVisible(ClearBox, false);
            SetFlyoutItemIsVisible(ClearEggs, false);
            SetFlyoutItemIsVisible(ClearPast, false);
            SetFlyoutItemIsVisible(ClearForeign, false);
            SetFlyoutItemIsVisible(ClearUntrained, false);
            SetFlyoutItemIsVisible(ClearIllegal, false);
            SetFlyoutItemIsVisible(ClearClones, false);
            DeleteExpanded = false;
        }
        else
        {
            SetFlyoutItemIsVisible(ClearBox, true);
            SetFlyoutItemIsVisible(ClearEggs, true);
            SetFlyoutItemIsVisible(ClearPast, true);
            SetFlyoutItemIsVisible(ClearForeign, true);
            SetFlyoutItemIsVisible(ClearUntrained, true);
            SetFlyoutItemIsVisible(ClearIllegal, true);
            SetFlyoutItemIsVisible(ClearClones, true);
            DeleteExpanded = true;
        }
    }

    private async void ClearBoxClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Clear", "Clear All boxes", "Clear the Current box", BoxManipType.DeleteAll);
    }

    private async void ClearEggsClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Clear", "Clear Eggs in All boxes", "Clear Eggs in the Current box", BoxManipType.DeleteEggs);
    }

    private async void ClearPastClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Clear", "Clear Past Generation Pokemon in All boxes", "Clear Past Generation Pokemon in the Current box", BoxManipType.DeletePastGen);
    }

    private async void ClearForeignClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Clear", "Clear Foreign Pokemon in All boxes", "Clear Foreign Pokemon in the Current box", BoxManipType.DeleteForeign);
    }

    private async void ClearUntrainedClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Clear", "Clear Untrained Pokemon in All boxes", "Clear Untrained Pokemon in the Current box", BoxManipType.DeleteUntrained);
    }

    private async Task ManipulateBoxes(string title, string allBoxesMessage, string currentBoxMessage, BoxManipType manipType)
    {
        if (await DisplayAlert(title, allBoxesMessage, "Yes", "No"))
        {
            manip.Execute(manipType, 0, true);
        }
        else if (await DisplayAlert(title, currentBoxMessage, "Yes", "No"))
        {
            manip.Execute(manipType, BoxTab.CurrentBox, false);
        }
        manipType.TryGetManipCategoryName(out var cat);
        HideAllFlyoutItems(cat);
    }

    private void HideAllFlyoutItems(string? Menu)
    {
        if (Menu == "Delete")
        {
            SetFlyoutItemIsVisible(ClearBox, false);
            SetFlyoutItemIsVisible(ClearEggs, false);
            SetFlyoutItemIsVisible(ClearPast, false);
            SetFlyoutItemIsVisible(ClearForeign, false);
            SetFlyoutItemIsVisible(ClearUntrained, false);
            SetFlyoutItemIsVisible(ClearIllegal, false);
            SetFlyoutItemIsVisible(ClearClones, false);
            DeleteExpanded = false;
        }
        if (Menu == "Sort")
        {
            SetFlyoutItemIsVisible(SortSpecies, false);
            SetFlyoutItemIsVisible(SortSpeciesReverse, false);
            SetFlyoutItemIsVisible(SortLevellohi, false);
            SetFlyoutItemIsVisible(SortLevelhilo, false);
            SetFlyoutItemIsVisible(SortMetDate, false);
            SetFlyoutItemIsVisible(SortSpeciesName, false);
            SetFlyoutItemIsVisible(SortShiny, false);
            SetFlyoutItemIsVisible(SortRandom, false);
            SortExpanded = false;
        }
        if (Menu == "SortAdvanced")
        {
            SetFlyoutItemIsVisible(SortUsage, false);
            SetFlyoutItemIsVisible(SortIV, false);
            SetFlyoutItemIsVisible(SortEV, false);
            SetFlyoutItemIsVisible(SortOwnership, false);
            SetFlyoutItemIsVisible(SortType, false);
            if (sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                SetFlyoutItemIsVisible(SortTera, false);
            SetFlyoutItemIsVisible(SortVersion, false);
            SetFlyoutItemIsVisible(SortBaseStat, false);
            if (sav.Version == GameVersion.PLA || sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                SetFlyoutItemIsVisible(SortScale, false);
            SetFlyoutItemIsVisible(SortRibbonCount, false);
            SetFlyoutItemIsVisible(SortMarkCount, false);
            SetFlyoutItemIsVisible(SortLegal, false);
            SetFlyoutItemIsVisible(SortEncounter, false);
            SortAdvancedExpanded = false;
        }
        if (Menu == "Modify")
        {
            SetFlyoutItemIsVisible(HatchEggs, false);
            SetFlyoutItemIsVisible(MaxFriendship, false);
            SetFlyoutItemIsVisible(MaxLevel, false);
            SetFlyoutItemIsVisible(ResetMoves, false);
            SetFlyoutItemIsVisible(RandomizeMoves, false);
            SetFlyoutItemIsVisible(HyperTrain, false);
            SetFlyoutItemIsVisible(RemoveNicknames, false);
            SetFlyoutItemIsVisible(DeleteHeldItem, false);
            SetFlyoutItemIsVisible(Heal, false);
            ModifyExpanded = false;
        }
        if(Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = false;
    }
    private void SortClick(object? sender, EventArgs? e)
    {
        if (SortExpanded)
        {
            SetFlyoutItemIsVisible(SortSpecies, false);
            SetFlyoutItemIsVisible(SortSpeciesReverse, false);
            SetFlyoutItemIsVisible(SortLevellohi, false);
            SetFlyoutItemIsVisible(SortLevelhilo, false);
            SetFlyoutItemIsVisible(SortMetDate, false);
            SetFlyoutItemIsVisible(SortSpeciesName, false);
            SetFlyoutItemIsVisible(SortShiny, false);
            SetFlyoutItemIsVisible(SortRandom, false);
            SortExpanded = false;
        }
        else
        {
            SetFlyoutItemIsVisible(SortSpecies, true);
            SetFlyoutItemIsVisible(SortSpeciesReverse, true);
            SetFlyoutItemIsVisible(SortLevellohi, true);
            SetFlyoutItemIsVisible(SortLevelhilo, true);
            SetFlyoutItemIsVisible(SortMetDate, true);
            SetFlyoutItemIsVisible(SortSpeciesName, true);
            SetFlyoutItemIsVisible(SortShiny, true);
            SetFlyoutItemIsVisible(SortRandom, true);
            SortExpanded = true;
        }
    }
    private async void SortBySpecies(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Pokedex No.", "Would you like to Sort the Current box by Pokedex No.", BoxManipType.SortSpecies);
    }
    private async void SortSpeciesReverseClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Reverse Pokedex No.", "Would you like to Sort the Current box by Reverse Pokedex No.", BoxManipType.SortSpeciesReverse);
    }

    private async void SortLevellohiClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Level (low to high)", "Would you like to Sort the Current box by Level (low to high)", BoxManipType.SortLevel);
    }

    private async void SortLevelhiloClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Level (high to low)", "Would you like to Sort the Current box by Level (high to low)", BoxManipType.SortLevelReverse);
    }

    private async void SortMetDateClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Met Date", "Would you like to Sort the Current box by Met Date", BoxManipType.SortDate);
    }

    private async void SortSpeciesNameClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Name", "Would you like to Sort the Current box by Name", BoxManipType.SortName);
    }

    private async void SortShinyClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Shiny", "Would you like to Sort the Current box by Shiny", BoxManipType.SortShiny);
    }

    private async void SortRandomClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes("Sort", "Would you like to Sort All boxes by Random", "Would you like to Sort the Current box by Random", BoxManipType.SortRandom);
    }

    private void SortBoxesAdvancedClicked(object? sender, EventArgs? e)
    {
        if (SortAdvancedExpanded)
        {
            SetFlyoutItemIsVisible(SortUsage, false);
            SetFlyoutItemIsVisible(SortIV, false);
            SetFlyoutItemIsVisible(SortEV, false);
            SetFlyoutItemIsVisible(SortOwnership, false);
            SetFlyoutItemIsVisible(SortType, false);
            if (sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                SetFlyoutItemIsVisible(SortTera, false);
            SetFlyoutItemIsVisible(SortVersion, false);
            SetFlyoutItemIsVisible(SortBaseStat, false);
            if (sav.Version == GameVersion.PLA || sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                SetFlyoutItemIsVisible(SortScale, false);
            SetFlyoutItemIsVisible(SortRibbonCount, false);
            SetFlyoutItemIsVisible(SortMarkCount, false);
            SetFlyoutItemIsVisible(SortLegal, false);
            SetFlyoutItemIsVisible(SortEncounter, false);
            SortAdvancedExpanded = false;
        }
        else
        {
            SetFlyoutItemIsVisible(SortUsage, true);
            SetFlyoutItemIsVisible(SortIV, true);
            SetFlyoutItemIsVisible(SortEV, true);
            SetFlyoutItemIsVisible(SortOwnership, true);
            SetFlyoutItemIsVisible(SortType, true);
            SetFlyoutItemIsVisible(SortVersion, true);
            SetFlyoutItemIsVisible(SortBaseStat, true);
            if (sav.Version == GameVersion.PLA || sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                SetFlyoutItemIsVisible(SortScale, true);
            if (sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                SetFlyoutItemIsVisible(SortTera, true);
            SetFlyoutItemIsVisible(SortRibbonCount, true);
            SetFlyoutItemIsVisible(SortMarkCount, true);
            SetFlyoutItemIsVisible(SortLegal, true);
            SetFlyoutItemIsVisible(SortEncounter, true);
            SortAdvancedExpanded = true;
        }
    }

    private void SortUsageClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Usage", "Would you like to Sort the Current box by Usage", BoxManipType.SortUsage);
    }

    private void SortIVClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by IV Potential", "Would you like to Sort the Current box by IV Potential", BoxManipType.SortPotential);
    }

    private void SortEVClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by EV Training", "Would you like to Sort the Current box by EV Training", BoxManipType.SortTraining);
    }

    private void SortOwnershipClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Ownership", "Would you like to Sort the Current box by Ownership", BoxManipType.SortOwner);
    }

    private void SortTypeClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Type", "Would you like to Sort the Current box by Type", BoxManipType.SortType);
    }

    private void SortVersionClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Version", "Would you like to Sort the Current box by Version", BoxManipType.SortVersion);
    }

    private void SortBaseStatClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by BST", "Would you like to Sort the Current box by BST", BoxManipType.SortBST);
    }

    private void SortRibbonClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Ribbon Count", "Would you like to Sort the Current box by Ribbon Count", BoxManipType.SortRibbons);
    }

    private void SortMarkClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Mark Count", "Would you like to Sort the Current box by Mark Count", BoxManipType.SortMarks);
    }

    private void SortLegalClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Legal", "Would you like to Sort the Current box by Legal", BoxManipType.SortLegal);
    }

    private void SortEncounterClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Encounter Type", "Would you like to Sort the Current box by Encounter Type", BoxManipType.SortEncounterType);
    }

    private void SortScaleClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Scale", "Would you like to Sort the Current box by Scale", BoxManipType.SortScale);
    }

    private void SortTeraClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Sort Advanced", "Would you like to Sort All boxes by Tera Type", "Would you like to Sort the Current box by Tera Type", BoxManipType.SortTypeTera);
    }

    private void ModifyBoxesClicked(object? sender, EventArgs? e)
    {
        if (ModifyExpanded)
        {
            SetFlyoutItemIsVisible(HatchEggs, false);
            SetFlyoutItemIsVisible(MaxFriendship, false);
            SetFlyoutItemIsVisible(MaxLevel, false);
            SetFlyoutItemIsVisible(ResetMoves, false);
            SetFlyoutItemIsVisible(RandomizeMoves, false);
            SetFlyoutItemIsVisible(HyperTrain, false);
            SetFlyoutItemIsVisible(RemoveNicknames, false);
            SetFlyoutItemIsVisible(DeleteHeldItem, false);
            SetFlyoutItemIsVisible(Heal, false);
            ModifyExpanded = false;
        }
        else
        {
            SetFlyoutItemIsVisible(HatchEggs, true);
            SetFlyoutItemIsVisible(MaxFriendship, true);
            SetFlyoutItemIsVisible(MaxLevel, true);
            SetFlyoutItemIsVisible(ResetMoves, true);
            SetFlyoutItemIsVisible(RandomizeMoves, true);
            SetFlyoutItemIsVisible(HyperTrain, true);
            SetFlyoutItemIsVisible(RemoveNicknames, true);
            SetFlyoutItemIsVisible(DeleteHeldItem, true);
            SetFlyoutItemIsVisible(Heal, true);
            ModifyExpanded = true;
        }
    }

    private void HatchEggsClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to hatch eggs in All boxes?", "Do you want to hatch eggs in the current box only?", BoxManipType.ModifyHatchEggs);
    }

    private void MaxFriendshipClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to maximize friendship in All boxes?", "Do you want to maximize friendship in the current box only?", BoxManipType.ModifyMaxFriendship);
    }

    private void MaxLevelClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to maximize level in All boxes?", "Do you want to maximize level in the current box only?", BoxManipType.ModifyMaxLevel);
    }

    private void ResetMovesClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to reset moves in All boxes?", "Do you want to reset moves in the current box only?", BoxManipType.ModifyResetMoves);
    }

    private void RandomizeMovesClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to randomize moves in All boxes?", "Do you want to randomize moves in the current box only?", BoxManipType.ModifyRandomMoves);
    }

    private void HyperTrainClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to hyper train in All boxes?", "Do you want to hyper train in the current box only?", BoxManipType.ModifyHyperTrain);
    }

    private void RemoveNicknamesClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to remove nicknames in All boxes?", "Do you want to remove nicknames in the current box only?", BoxManipType.ModifyRemoveNicknames);
    }

    private void DeleteHeldItemClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to remove held items in All boxes?", "Do you want to remove held items in the current box only?", BoxManipType.ModifyRemoveItem);
    }

    private void HealClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Modify", "Do you want to heal stats in All boxes?", "Do you want to heal stats in the current box only?", BoxManipType.ModifyHeal);
    }

    private void ClearNoHeldItemClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Clear", "Do you want to Delete Pokemon with No Held Item in All Boxes?", "Do you want to delete Pokemon with no Held Item in the current box only?", BoxManipType.DeleteItemless);
    }

    private void ClearIllegalClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Clear", "Clear Illegal Pokemon in All boxes", "Clear Illegal Pokemon in the Current box", BoxManipType.DeleteIllegal);
    }

    private void ClearClonesClicked(object sender, EventArgs e)
    {
        ManipulateBoxes("Clear", "Clear clones in All boxes", "Clear clones in the Current box", BoxManipType.DeleteClones);
    }

    private async void OpenPKMClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)PKPage).pk9picker_Clicked(sender, e);
    }
    private async void ExportSaveClicked(object sender, EventArgs e)
    {
        // Set box now that we're saving
        if (sav.HasBox)
            sav.CurrentBox = BoxTab.CurrentBox;
        var ext = sav.Metadata.GetSuggestedExtension();
        var flags = sav.Metadata.GetSuggestedFlags(ext);
        await using var LiveStream = new MemoryStream(sav.Write(flags).ToArray());
        var result = await FileSaver.Default.SaveAsync(sav.Metadata.FileName??"", LiveStream, CancellationToken.None);
        if (result.IsSuccessful)
            await DisplayAlert("Success", $"Save file was exported to {result.FilePath}", "cancel");
        else
            await DisplayAlert("Failure", $"Save file did not export due to {result.Exception.Message}", "cancel");
    }
    private async void SavePKMClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)PKPage).pk9saver_Clicked(sender, e);
    }

    private async void LegalizePKM(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)PKPage).legalize(sender, e);
    }

    private async void ImpShowClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)PKPage).ImportShowdown(sender, e);
    }

    private async void ExpShowClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)PKPage).ExportShowdown(sender, e);
    }

    private async void LoadBoxesClicked(object sender, EventArgs e)
    {
        var folder = await FolderPicker.PickAsync(CancellationToken.None);
        if (folder.IsSuccessful)
        {
            foreach (var f in Directory.GetFiles(folder.Folder.Path))
            {
                PKM pkm = (PKM?)FileUtil.GetSupportedFile(f)??EntityBlank.GetBlank(sav.Generation);
                if (pkm.GetType() != sav.PKMType)
                {
                    var newpkm = EntityConverter.ConvertToType(pkm, sav.PKMType, out var result)??EntityBlank.GetBlank(sav.Generation);
                    if (result.IsSuccess() || PSettings.AllowIncompatibleConversion)
                    {
                        sav.AdaptToSaveFile(newpkm);
                        sav.SetBoxSlotAtIndex(newpkm, sav.NextOpenBoxSlot());
                        return;
                    }
                }
                sav.AdaptToSaveFile(pkm);
                sav.SetBoxSlotAtIndex(pkm, sav.NextOpenBoxSlot());
            }
            if (TheShell.CurrentPage is BoxTab tab)
                tab.fillbox();
        }
    }

    private async void DumpBoxClicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Dump","Dump All Boxes?", "yes", "cancel"))
        {
            var result = await FolderPicker.PickAsync(CancellationToken.None);
            if(result.IsSuccessful)
                BoxExport.Export(sav, result.Folder.Path, BoxExportSettings.Default);
            return;
        }
        else if(await DisplayAlert("Dump","Dump Current Box?", "yes", "cancel"))
        {
            var result = await FolderPicker.PickAsync(CancellationToken.None);
            if (result.IsSuccessful)
                BoxExport.Export(sav,result.Folder.Path, BoxExportSettings.Default with { Scope = BoxExportScope.Current });
            return;
        }
    }

    private async void SaveBoxDataClicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Dump", "Dump ALL Boxes?", "yes", "no"))
        {
            await using MemoryStream boxstream = new(sav.GetPCBinary());
            await FileSaver.SaveAsync("pcdata.bin", boxstream);
            return;
        }
        if (await DisplayAlert("Dump", "Dump Current Box?", "yes", "cancel"))
        {
            await using MemoryStream Cboxstream = new(sav.GetBoxBinary(sav.CurrentBox));
            await FileSaver.SaveAsync($"boxdata {sav.CurrentBox}.bin", Cboxstream);
            return;
        }
    }

    private void OpenBatchEditorClick(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new BatchEditor());
    }
}
public class BoxManipulatorMAUI : BoxManipulator
{
    protected override SaveFile SAV => sav;
    protected override void FinishBoxManipulation(string message, bool all, int count) => Shell.Current.DisplayAlert("Finished", message + $" ({count})", "cancel");

    protected override bool CanManipulateRegion(int start, int end, string prompt, string fail)
    {
        bool canModify = base.CanManipulateRegion(start, end, prompt, fail);
        if (!canModify && !string.IsNullOrEmpty(fail))
            Shell.Current.DisplayAlert("Box", fail, "cancel");
        return canModify;
    }
}
public class FlyoutCollectionSelector : DataTemplateSelector
{
    public DataTemplate FlyoutItemDataTemplate =  new(() =>
    {
        Grid grid = new() { Padding = 15 };
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        Label label = new() { TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
        label.SetBinding(Label.TextProperty, "Title");
        grid.Add(label);
        Image icon = new() { HorizontalOptions = LayoutOptions.Start, HeightRequest=25, WidthRequest=25 } ;
        icon.SetBinding(Image.SourceProperty, "Icon");
        grid.Add(icon);
        ImageButton button = new()
        {
            BackgroundColor = Colors.White,
            HeightRequest = 25,
            WidthRequest = 25,
            Source = "dump.png"
        };
        button.SetBinding(ImageButton.CommandParameterProperty,"Title");
        button.Clicked += ((AppShell)AppShell.Current).DropdownExpansion;
        grid.Add(button, 1);
        Border border = new()
        {
            Stroke = Colors.White,
            BackgroundColor = Colors.Transparent,
            Content = grid
        };
        return border;
    });
    public DataTemplate MenuItemDataTemplate = new(() =>
    {
        Grid grid = new() { Padding = 15};
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        Label label = new() { TextColor = Colors.White , HorizontalOptions = LayoutOptions.Center};
        label.SetBinding(Label.TextProperty, "Title");
        grid.Add(label);
        Image icon = new() { HorizontalOptions = LayoutOptions.Start, HeightRequest = 25, WidthRequest = 25 };
        icon.SetBinding(Image.SourceProperty, "Icon");
        grid.Add(icon);
        Border border = new()
        {
            Stroke = Colors.White,
            BackgroundColor = Colors.Transparent,
            Content = grid
        };
        return border;
    });
    public DataTemplate MenuItemDataTemplate2 = new(() =>
    {
        Grid grid = new() { Padding = 15 };
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        Label label = new() { TextColor = Colors.White};
        label.SetBinding(Label.TextProperty, "Title");
        grid.Add(label);
        Image icon = new() { HorizontalOptions = LayoutOptions.Start, HeightRequest = 25, WidthRequest = 25 };
        icon.SetBinding(Image.SourceProperty, "Icon");
        grid.Add(icon);
        Border border = new()
        {
            Stroke = Colors.White,
            BackgroundColor = Colors.Transparent,
            Margin = new Thickness(20, 0, 20, 0),
            Content = grid
        };
        return border;
    });
    public DataTemplate MenuItemDropdownDataTemplate = new(() =>
    {
        Grid grid = new() { Padding = 15 };
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        Label label = new() { TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
        label.SetBinding(Label.TextProperty, "Text");
        grid.Add(label);
        Image icon = new() { HorizontalOptions = LayoutOptions.Start, HeightRequest = 25, WidthRequest = 25 };
        icon.SetBinding(Image.SourceProperty, "Icon");
        grid.Add(icon);
        ImageButton button = new()
        {
            BackgroundColor = Colors.White,
            HeightRequest = 25,
            WidthRequest = 25,
            Source = "dump.png"
        };
        button.SetBinding(ImageButton.CommandParameterProperty, "Text");
        button.Clicked += ((AppShell)AppShell.Current).DropdownExpansion;
        grid.Add(button, 1);
        Border border = new()
        {
            Stroke = Colors.White,
            BackgroundColor = Colors.Transparent,
            Content = grid
        };
        return border;
    });
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is FlyoutItem e)
        {
            if (e.Title == "Box/Party" || e.Title == "pk editor" || e.Title == "File")
                return FlyoutItemDataTemplate;
            else
                return MenuItemDataTemplate;
        }
        if(item is Tab t)
        {
            if (t.Title == "Box/Party" || t.Title == "pk editor" || t.Title == "File")
                return FlyoutItemDataTemplate;
            else
                return MenuItemDataTemplate;
        }
        if (item.GetType().GetProperty("Title")?.GetValue(item) is string s)
        {
            if(s == "File"|| s == "Data")
            return MenuItemDropdownDataTemplate;
        }
        return MenuItemDataTemplate2;
    }
}
