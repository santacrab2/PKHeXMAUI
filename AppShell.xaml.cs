using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class AppShell : Shell
{
	public AppShell(SaveFile sav)
	{
        AppSaveFile = sav;
        InitializeComponent();
    }
	public static SaveFile AppSaveFile { get; set; }
    public BoxManipulator manip = new BoxManipulatorMAUI();
    private void checkbox(object sender, ShellNavigatedEventArgs e)
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
        }
        if (TheShell.CurrentPage.GetType() == typeof(BoxTab))
        {
            Shell.SetFlyoutItemIsVisible(DeleteBoxes, true);
            Shell.SetFlyoutItemIsVisible(SortBoxes, true);
            Shell.SetFlyoutItemIsVisible(SortBoxesAdvanced, true);
            Shell.SetFlyoutItemIsVisible(ModifyBoxes, true);
            ((BoxTab)TheShell.CurrentPage).fillbox();
            if (e.Previous.Location.ToString() == "//PKShell/pkeditortab/PKPage")
                ((BoxTab)TheShell.CurrentPage).DisplayOptions();
        }
        else
        {
            Shell.SetFlyoutItemIsVisible(DeleteBoxes, false);
            Shell.SetFlyoutItemIsVisible(SortBoxes, false);
            Shell.SetFlyoutItemIsVisible(SortBoxesAdvanced, false);
            Shell.SetFlyoutItemIsVisible(ModifyBoxes, false);
            DeleteExpanded = true;
            DeleteClicked(sender, e);
            SortExpanded = true;
            SortClick(sender, e);
            SortAdvancedExpanded = true;
            SortBoxesAdvancedClicked(sender, e);
            ModifyExpanded = true;
            ModifyBoxesClicked(sender, e);
        }
        if (TheShell.CurrentPage.GetType() == typeof(MainPage))
        {
            Shell.SetFlyoutItemIsVisible(OpenPKM, true);
            Shell.SetFlyoutItemIsVisible(SavePKM, true);
            Shell.SetFlyoutItemIsVisible(thelegalizer, true);
            Shell.SetFlyoutItemIsVisible(impshow, true);
            Shell.SetFlyoutItemIsVisible(expshow, true);
        }
        else
        {
            Shell.SetFlyoutItemIsVisible(OpenPKM, false);
            Shell.SetFlyoutItemIsVisible(SavePKM, false);
            Shell.SetFlyoutItemIsVisible(thelegalizer, false);
            Shell.SetFlyoutItemIsVisible(impshow, false);
            Shell.SetFlyoutItemIsVisible(expshow, false);
        }
    }
    public bool SortExpanded = false;
    public bool DeleteExpanded = false;
    public bool SortAdvancedExpanded = false;
    public bool ModifyExpanded = false;
    private void DeleteClicked(object sender, EventArgs e)
    {
        if (DeleteExpanded)
        {
            Shell.SetFlyoutItemIsVisible(ClearBox, false);
            Shell.SetFlyoutItemIsVisible(ClearEggs, false);
            Shell.SetFlyoutItemIsVisible(ClearPast, false);
            Shell.SetFlyoutItemIsVisible(ClearForeign, false);
            Shell.SetFlyoutItemIsVisible(ClearUntrained, false);
            Shell.SetFlyoutItemIsVisible(ClearIllegal, false);
            Shell.SetFlyoutItemIsVisible(ClearClones, false);
            DeleteExpanded = false;
        }
        else
        {
            Shell.SetFlyoutItemIsVisible(ClearBox, true);
            Shell.SetFlyoutItemIsVisible(ClearEggs, true);
            Shell.SetFlyoutItemIsVisible(ClearPast, true);
            Shell.SetFlyoutItemIsVisible(ClearForeign, true);
            Shell.SetFlyoutItemIsVisible(ClearUntrained, true);
            Shell.SetFlyoutItemIsVisible(ClearIllegal, true);
            Shell.SetFlyoutItemIsVisible(ClearClones, true);
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

        HideAllFlyoutItems(manipType.GetManipCategoryName());
    }

    private void HideAllFlyoutItems(string Menu)
    {
        if (Menu == "Delete")
        {
            Shell.SetFlyoutItemIsVisible(ClearBox, false);
            Shell.SetFlyoutItemIsVisible(ClearEggs, false);
            Shell.SetFlyoutItemIsVisible(ClearPast, false);
            Shell.SetFlyoutItemIsVisible(ClearForeign, false);
            Shell.SetFlyoutItemIsVisible(ClearUntrained, false);
            Shell.SetFlyoutItemIsVisible(ClearIllegal, false);
            Shell.SetFlyoutItemIsVisible(ClearClones, false);
            DeleteExpanded = false;
        }
        if (Menu == "Sort")
        {
            Shell.SetFlyoutItemIsVisible(SortSpecies, false);
            Shell.SetFlyoutItemIsVisible(SortSpeciesReverse, false);
            Shell.SetFlyoutItemIsVisible(SortLevellohi, false);
            Shell.SetFlyoutItemIsVisible(SortLevelhilo, false);
            Shell.SetFlyoutItemIsVisible(SortMetDate, false);
            Shell.SetFlyoutItemIsVisible(SortSpeciesName, false);
            Shell.SetFlyoutItemIsVisible(SortShiny, false);
            Shell.SetFlyoutItemIsVisible(SortRandom, false);
            SortExpanded = false;
        }
        if (Menu == "SortAdvanced")
        {
            Shell.SetFlyoutItemIsVisible(SortUsage, false);
            Shell.SetFlyoutItemIsVisible(SortIV, false);
            Shell.SetFlyoutItemIsVisible(SortEV, false);
            Shell.SetFlyoutItemIsVisible(SortOwnership, false);
            Shell.SetFlyoutItemIsVisible(SortType, false);
            if (sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                Shell.SetFlyoutItemIsVisible(SortTera, false);
            Shell.SetFlyoutItemIsVisible(SortVersion, false);
            Shell.SetFlyoutItemIsVisible(SortBaseStat, false);
            if (sav.Version == GameVersion.PLA || sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                Shell.SetFlyoutItemIsVisible(SortScale, false);
            Shell.SetFlyoutItemIsVisible(SortRibbonCount, false);
            Shell.SetFlyoutItemIsVisible(SortMarkCount, false);
            Shell.SetFlyoutItemIsVisible(SortLegal, false);
            Shell.SetFlyoutItemIsVisible(SortEncounter, false);
            SortAdvancedExpanded = false;
        }
        if (Menu == "Modify")
        {
            Shell.SetFlyoutItemIsVisible(HatchEggs, false);
            Shell.SetFlyoutItemIsVisible(MaxFriendship, false);
            Shell.SetFlyoutItemIsVisible(MaxLevel, false);
            Shell.SetFlyoutItemIsVisible(ResetMoves, false);
            Shell.SetFlyoutItemIsVisible(RandomizeMoves, false);
            Shell.SetFlyoutItemIsVisible(HyperTrain, false);
            Shell.SetFlyoutItemIsVisible(RemoveNicknames, false);
            Shell.SetFlyoutItemIsVisible(DeleteHeldItem, false);
            Shell.SetFlyoutItemIsVisible(Heal, false);
            ModifyExpanded = false;
        }
        Shell.Current.FlyoutIsPresented = false;
    }
    private void SortClick(object sender, EventArgs e)
    {
        if (SortExpanded)
        {
            Shell.SetFlyoutItemIsVisible(SortSpecies, false);
            Shell.SetFlyoutItemIsVisible(SortSpeciesReverse, false);
            Shell.SetFlyoutItemIsVisible(SortLevellohi, false);
            Shell.SetFlyoutItemIsVisible(SortLevelhilo, false);
            Shell.SetFlyoutItemIsVisible(SortMetDate, false);
            Shell.SetFlyoutItemIsVisible(SortSpeciesName, false);
            Shell.SetFlyoutItemIsVisible(SortShiny, false);
            Shell.SetFlyoutItemIsVisible(SortRandom, false);
            SortExpanded = false;
        }
        else
        {
            Shell.SetFlyoutItemIsVisible(SortSpecies, true);
            Shell.SetFlyoutItemIsVisible(SortSpeciesReverse, true);
            Shell.SetFlyoutItemIsVisible(SortLevellohi, true);
            Shell.SetFlyoutItemIsVisible(SortLevelhilo, true);
            Shell.SetFlyoutItemIsVisible(SortMetDate, true);
            Shell.SetFlyoutItemIsVisible(SortSpeciesName, true);
            Shell.SetFlyoutItemIsVisible(SortShiny, true);
            Shell.SetFlyoutItemIsVisible(SortRandom, true);
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

    private void SortBoxesAdvancedClicked(object sender, EventArgs e)
    {
        if (SortAdvancedExpanded)
        {
            Shell.SetFlyoutItemIsVisible(SortUsage, false);
            Shell.SetFlyoutItemIsVisible(SortIV, false);
            Shell.SetFlyoutItemIsVisible(SortEV, false);
            Shell.SetFlyoutItemIsVisible(SortOwnership, false);
            Shell.SetFlyoutItemIsVisible(SortType, false);
            if (sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                Shell.SetFlyoutItemIsVisible(SortTera, false);
            Shell.SetFlyoutItemIsVisible(SortVersion, false);
            Shell.SetFlyoutItemIsVisible(SortBaseStat, false);
            if (sav.Version == GameVersion.PLA || sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                Shell.SetFlyoutItemIsVisible(SortScale, false);
            Shell.SetFlyoutItemIsVisible(SortRibbonCount, false);
            Shell.SetFlyoutItemIsVisible(SortMarkCount, false);
            Shell.SetFlyoutItemIsVisible(SortLegal, false);
            Shell.SetFlyoutItemIsVisible(SortEncounter, false);
            SortAdvancedExpanded = false;
        }
        else
        {
            Shell.SetFlyoutItemIsVisible(SortUsage, true);
            Shell.SetFlyoutItemIsVisible(SortIV, true);
            Shell.SetFlyoutItemIsVisible(SortEV, true);
            Shell.SetFlyoutItemIsVisible(SortOwnership, true);
            Shell.SetFlyoutItemIsVisible(SortType, true);
            Shell.SetFlyoutItemIsVisible(SortVersion, true);
            Shell.SetFlyoutItemIsVisible(SortBaseStat, true);
            if (sav.Version == GameVersion.PLA || sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                Shell.SetFlyoutItemIsVisible(SortScale, true);
            if (sav.Version == GameVersion.VL || sav.Version == GameVersion.SL)
                Shell.SetFlyoutItemIsVisible(SortTera, true);
            Shell.SetFlyoutItemIsVisible(SortRibbonCount, true);
            Shell.SetFlyoutItemIsVisible(SortMarkCount, true);
            Shell.SetFlyoutItemIsVisible(SortLegal, true);
            Shell.SetFlyoutItemIsVisible(SortEncounter, true);
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

    private void ModifyBoxesClicked(object sender, EventArgs e)
    {
        if (ModifyExpanded)
        {
            Shell.SetFlyoutItemIsVisible(HatchEggs, false);
            Shell.SetFlyoutItemIsVisible(MaxFriendship, false);
            Shell.SetFlyoutItemIsVisible(MaxLevel, false);
            Shell.SetFlyoutItemIsVisible(ResetMoves, false);
            Shell.SetFlyoutItemIsVisible(RandomizeMoves, false);
            Shell.SetFlyoutItemIsVisible(HyperTrain, false);
            Shell.SetFlyoutItemIsVisible(RemoveNicknames, false);
            Shell.SetFlyoutItemIsVisible(DeleteHeldItem, false);
            Shell.SetFlyoutItemIsVisible(Heal, false);
            ModifyExpanded = false;
        }
        else
        {
            Shell.SetFlyoutItemIsVisible(HatchEggs, true);
            Shell.SetFlyoutItemIsVisible(MaxFriendship, true);
            Shell.SetFlyoutItemIsVisible(MaxLevel, true);
            Shell.SetFlyoutItemIsVisible(ResetMoves, true);
            Shell.SetFlyoutItemIsVisible(RandomizeMoves, true);
            Shell.SetFlyoutItemIsVisible(HyperTrain, true);
            Shell.SetFlyoutItemIsVisible(RemoveNicknames, true);
            Shell.SetFlyoutItemIsVisible(DeleteHeldItem, true);
            Shell.SetFlyoutItemIsVisible(Heal, true);
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

    private void OpenPKMClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)TheShell.CurrentPage).pk9picker_Clicked(sender, e);
    }

    private void SavePKMClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)TheShell.CurrentPage).pk9saver_Clicked(sender, e);
    }

    private void LegalizePKM(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)TheShell.CurrentPage).legalize(sender, e);
    }

    private void ImpShowClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)TheShell.CurrentPage).ImportShowdown(sender, e);
    }

    private void ExpShowClicked(object sender, EventArgs e)
    {
        TheShell.FlyoutIsPresented = false;
        ((MainPage)TheShell.CurrentPage).ExportShowdown(sender, e);
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