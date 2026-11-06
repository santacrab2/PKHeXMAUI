using CommunityToolkit.Maui.Storage;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System.Windows.Input;
using static PKHeXMAUI.MainPage;
using System;
using System.Diagnostics;

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

    /// <summary>
    /// 页面呈现后再补做一次 RTL 排版。
    /// <para>
    /// <see cref="App.CreateWindow"/> 里也调了一次 <c>L10nLayout.Apply(window)</c>，
    /// 但那时 Shell 刚 new 出来、视觉树尚未构建完，遍历不到子元素，
    /// 对齐与换行不会生效。这里等视觉树就绪后补做。
    /// </para>
    /// <para>
    /// 当前只影响阿拉伯语/希伯来语（RTL）；这两种语言官方未发行、暂未启用，
    /// 但机制已就绪，等补齐译文即可直接生效。
    /// </para>
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (L10nManager.Instance.IsRightToLeft)
            L10nLayout.Apply(this);
    }

	public static SaveFile? AppSaveFile { get; set; }
    public BoxManipulator manip = new BoxManipulatorMAUI();
    public static bool boxexpanded = false;
    public static bool pkexpanded = false;
    public static bool fileexpanded = false;
    public static bool dataexpanded = false;
    public static Shell? Shelltest;

    /// <summary>
    /// 菜单展开用的内部键。
    /// <para>
    /// 侧边栏标题会随界面语言变化，所以**不能**用标题文本做匹配 ——
    /// 这里统一改用稳定标识：FlyoutItem 用 <c>Route</c>，MenuItem 用 <c>AutomationId</c>。
    /// </para>
    /// </summary>
    public static string MenuKey(string? id) => id switch
    {
        "BoxShell" => "Box/Party",
        "PKShell" => "pk editor",
        "File" => "File",
        "Data" => "Data",
        // 兼容旧路径：历史自检脚本按标题文本调用
        "Box/Party" or "Boxes / Party" or "Box/Party" => "Box/Party",
        "pk editor" or "Pokémon Editor" => "pk editor",
        _ => id ?? string.Empty,
    };

    public async void DropdownExpansion(object? sender, EventArgs? e)
    {
        var key = MenuKey((string?)((ImageButton?)sender)?.CommandParameter);
        if (key == "Box/Party")
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
        if (key == "pk editor")
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
        if (key == "File")
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
        if (key == "Data")
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
        await ManipulateBoxes(L10n.T("code_clear"), L10n.T("code_clear_all_boxes"), L10n.T("code_clear_the_current_box"), BoxManipType.DeleteAll);
    }

    private async void ClearEggsClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_clear"), L10n.T("code_clear_the_eggs_in_2"), L10n.T("code_clear_the_eggs_in"), BoxManipType.DeleteEggs);
    }

    private async void ClearPastClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_clear"), L10n.T("code_clear_the_cross_generation_2"), L10n.T("code_clear_the_cross_generation"), BoxManipType.DeletePastGen);
    }

    private async void ClearForeignClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_clear"), L10n.T("code_clear_the_foreign_pkm_2"), L10n.T("code_clear_the_foreign_pkm"), BoxManipType.DeleteForeign);
    }

    private async void ClearUntrainedClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_clear"), L10n.T("code_clear_the_untrained_pkm_2"), L10n.T("code_clear_the_untrained_pkm"), BoxManipType.DeleteUntrained);
    }

    private async Task ManipulateBoxes(string title, string allBoxesMessage, string currentBoxMessage, BoxManipType manipType)
    {
        if (await DisplayAlertAsync(title, allBoxesMessage, L10n.T("pktab_ok"), L10n.T("box_cancel")))
        {
            manip.Execute(manipType, 0, true);
        }
        else if (await DisplayAlertAsync(title, currentBoxMessage, L10n.T("pktab_ok"), L10n.T("box_cancel")))
        {
            manip.Execute(manipType, BoxTab.CurrentBox, false);
        }
        // 执行完之后只收起侧边栏浮层，不要收起子菜单。
        // 早期版本在这里把整组子菜单（删除/整理/修改…）全部隐藏，
        // 结果「重置招式」这类按钮点一次就从菜单里消失，再也点不了第二次。
        Shell.Current?.FlyoutIsPresented = false;
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
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("code_sort_all_boxes_by"), L10n.T("code_sort_the_current_box"), BoxManipType.SortSpecies);
    }
    private async void SortSpeciesReverseClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("code_sort_all_boxes_by_2"), L10n.T("code_sort_the_current_box_2"), BoxManipType.SortSpeciesReverse);
    }

    private async void SortLevellohiClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("code_sort_all_boxes_by_3"), L10n.T("code_sort_the_current_box_3"), BoxManipType.SortLevel);
    }

    private async void SortLevelhiloClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("code_sort_all_boxes_by_4"), L10n.T("code_sort_the_current_box_4"), BoxManipType.SortLevelReverse);
    }

    private async void SortMetDateClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("code_sort_all_boxes_by_5"), L10n.T("code_sort_the_current_box_5"), BoxManipType.SortDate);
    }

    private async void SortSpeciesNameClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("code_sort_all_boxes_by_6"), L10n.T("code_sort_the_current_box_6"), BoxManipType.SortName);
    }

    private async void SortShinyClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("code_sort_all_boxes_by_7"), L10n.T("code_sort_the_current_box_7"), BoxManipType.SortShiny);
    }

    private async void SortRandomClicked(object sender, EventArgs e)
    {
        await ManipulateBoxes(L10n.T("code_sort_boxes_2"), L10n.T("shell_sort_random_all"), L10n.T("shell_sort_random_current"), BoxManipType.SortRandom);
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
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_16"), L10n.T("code_sort_the_current_box_16"), BoxManipType.SortUsage);
    }

    private void SortIVClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_9"), L10n.T("code_sort_the_current_box_9"), BoxManipType.SortPotential);
    }

    private void SortEVClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_10"), L10n.T("code_sort_the_current_box_10"), BoxManipType.SortTraining);
    }

    private void SortOwnershipClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_18"), L10n.T("code_sort_the_current_box_18"), BoxManipType.SortOwner);
    }

    private void SortTypeClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_17"), L10n.T("code_sort_the_current_box_17"), BoxManipType.SortType);
    }

    private void SortVersionClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_19"), L10n.T("code_sort_the_current_box_19"), BoxManipType.SortVersion);
    }

    private void SortBaseStatClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_11"), L10n.T("code_sort_the_current_box_11"), BoxManipType.SortBST);
    }

    private void SortRibbonClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_13"), L10n.T("code_sort_the_current_box_13"), BoxManipType.SortRibbons);
    }

    private void SortMarkClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_14"), L10n.T("code_sort_the_current_box_14"), BoxManipType.SortMarks);
    }

    private void SortLegalClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_12"), L10n.T("code_sort_the_current_box_12"), BoxManipType.SortLegal);
    }

    private void SortEncounterClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_15"), L10n.T("code_sort_the_current_box_15"), BoxManipType.SortEncounterType);
    }

    private void SortScaleClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_20"), L10n.T("code_sort_the_current_box_20"), BoxManipType.SortScale);
    }

    private void SortTeraClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_sort_boxes"), L10n.T("code_sort_all_boxes_by_21"), L10n.T("code_sort_the_current_box_21"), BoxManipType.SortTypeTera);
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
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_hatch_eggs_all"), L10n.T("shell_hatch_eggs_current"), BoxManipType.ModifyHatchEggs);
    }

    private void MaxFriendshipClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_max_friendship_all"), L10n.T("shell_max_friendship_current"), BoxManipType.ModifyMaxFriendship);
    }

    private void MaxLevelClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_max_level_all"), L10n.T("shell_max_level_current"), BoxManipType.ModifyMaxLevel);
    }

    private void ResetMovesClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_reset_moves_all"), L10n.T("shell_reset_moves_current"), BoxManipType.ModifyResetMoves);
    }

    private void RandomizeMovesClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_random_moves_all"), L10n.T("shell_random_moves_current"), BoxManipType.ModifyRandomMoves);
    }

    private void HyperTrainClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_hyper_train_all"), L10n.T("shell_hyper_train_current"), BoxManipType.ModifyHyperTrain);
    }

    private void RemoveNicknamesClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_remove_nicknames_all"), L10n.T("shell_remove_nicknames_current"), BoxManipType.ModifyRemoveNicknames);
    }

    private void DeleteHeldItemClicked(object sender, EventArgs e)
    {
        ManipulateBoxes(L10n.T("code_modify_boxes"), L10n.T("shell_remove_items_all"), L10n.T("shell_remove_items_current"), BoxManipType.ModifyRemoveItem);
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
        try
        {
            // Set box now that we're saving
            if (sav.HasBox)
                sav.CurrentBox = BoxTab.CurrentBox;
            var ext = sav.Metadata.GetSuggestedExtension();
            var flags = sav.Metadata.GetSuggestedFlags(ext);
            await using var LiveStream = new MemoryStream(sav.Write(flags).ToArray());
            var result = await FileSaver.Default.SaveAsync(sav.Metadata.FileName ?? "", LiveStream, CancellationToken.None);
            if (result.IsSuccessful)
                await DisplayAlertAsync(L10n.T("pktab_success"), L10n.T("code_save_exported_to_result", result.FilePath), L10n.T("pktab_ok"));
            else
                await DisplayAlertAsync(L10n.T("pktab_failed"), L10n.T("code_save_export_failed_result", result.Exception?.Message ?? L10n.T("common_unknown_reason")), L10n.T("pktab_ok"));
        }
        catch (Exception ex)
        {
            CrashLog.Write("导出存档", ex);
            await DisplayAlertAsync(L10n.T("common_export_failed"), ex.Message, L10n.T("pktab_ok"));
        }
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
        try
        {
            var folder = await FolderPicker.PickAsync(CancellationToken.None);
            if (folder.IsSuccessful)
            {
                foreach (var f in Directory.GetFiles(folder.Folder.Path))
                {
                    PKM pkm = (PKM?)FileUtil.GetSupportedFile(f) ?? EntityBlank.GetBlank(sav.Generation);
                    if (pkm.GetType() != sav.PKMType)
                    {
                        var newpkm = EntityConverter.ConvertToType(pkm, sav.PKMType, out var result) ?? EntityBlank.GetBlank(sav.Generation);
                        if (result.IsSuccess || PSettings.AllowIncompatibleConversion)
                        {
                            sav.AdaptToSaveFile(newpkm);
                            sav.SetBoxSlotAtIndex(newpkm, sav.NextOpenBoxSlot());
                            continue;
                        }
                    }
                    sav.AdaptToSaveFile(pkm);
                    sav.SetBoxSlotAtIndex(pkm, sav.NextOpenBoxSlot());
                }
                if (TheShell.CurrentPage is BoxTab tab)
                    tab.fillbox();
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("载入箱子", ex);
            await DisplayAlertAsync(L10n.T("common_load_failed"), ex.Message, L10n.T("pktab_ok"));
        }
    }

    private async void DumpBoxClicked(object sender, EventArgs e)
    {
        try
        {
            if (await DisplayAlertAsync(L10n.T("shell_export"), L10n.T("shell_export_all_boxes"), L10n.T("pktab_ok"), L10n.T("box_cancel")))
            {
                var result = await FolderPicker.PickAsync(CancellationToken.None);
                if (result.IsSuccessful)
                    BoxExport.Export(sav, result.Folder.Path, BoxExportSettings.Default);
                return;
            }
            else if (await DisplayAlertAsync(L10n.T("shell_export"), L10n.T("shell_export_the_current"), L10n.T("pktab_ok"), L10n.T("box_cancel")))
            {
                var result = await FolderPicker.PickAsync(CancellationToken.None);
                if (result.IsSuccessful)
                    BoxExport.Export(sav, result.Folder.Path, BoxExportSettings.Default with { Scope = BoxExportScope.Current });
                return;
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("导出箱子", ex);
            await DisplayAlertAsync("导出失败", ex.Message, L10n.T("pktab_ok"));
        }
    }

    private async void SaveBoxDataClicked(object sender, EventArgs e)
    {
        try
        {
            if (await DisplayAlertAsync(L10n.T("shell_export"), L10n.T("shell_export_all_boxes"), L10n.T("pktab_ok"), L10n.T("box_cancel")))
            {
                await using MemoryStream boxstream = new(sav.GetPCBinary());
                await FileSaver.SaveAsync("pcdata.bin", boxstream);
                return;
            }
            if (await DisplayAlertAsync(L10n.T("shell_export"), L10n.T("shell_export_the_current"), L10n.T("pktab_ok"), L10n.T("box_cancel")))
            {
                await using MemoryStream Cboxstream = new(sav.GetBoxBinary(sav.CurrentBox));
                await FileSaver.SaveAsync($"boxdata {sav.CurrentBox}.bin", Cboxstream);
                return;
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("保存箱子数据", ex);
            await DisplayAlertAsync("导出失败", ex.Message, L10n.T("pktab_ok"));
        }
    }

    private void OpenBatchEditorClick(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new BatchEditor());
    }

    /// <summary>
    /// 运行时自检：把侧边栏下拉展开、模板选择、设置页构建都真跑一遍，结果写进 debug.log。
    /// 由环境变量 PKHEXMAUI_SELFTEST=1 触发，正常使用不受影响。
    /// </summary>
    internal async Task<string> SelfTest()
    {
        var sb = new System.Text.StringBuilder();
        var selector = new FlyoutCollectionSelector();

        // FlyoutItems 里 AsMultipleItems 的分组是包了一层的集合，这里递归拆开
        static IEnumerable<object> Flatten(System.Collections.IEnumerable source)
        {
            foreach (var it in source)
            {
                if (it is null)
                    continue;
                if (it is System.Collections.IEnumerable inner and not string)
                {
                    foreach (var sub in Flatten(inner))
                        yield return sub;
                    continue;
                }
                yield return it;
            }
        }

        try
        {
            foreach (var item in Flatten(FlyoutItems))
            {
                var t = item.GetType();
                string title = item switch
                {
                    MenuItem m => m.Text ?? "",
                    BaseShellItem b => b.Title ?? "",
                    _ => (t.GetProperty("Title")?.GetValue(item) as string)
                         ?? (t.GetProperty("Text")?.GetValue(item) as string)
                         ?? "",
                };
                string tpl;
                try { tpl = selector.Probe(item); }
                catch (Exception ex) { tpl = L10n.T("code_exception_detail", ex.GetType().Name, ex.Message); }
                // 可见性：菜单项没这个属性，只有 ShellItem（Tab/FlyoutItem）能查
                string vis = "-";
                if (item is BaseShellItem)
                {
                    try { vis = Shell.GetFlyoutItemIsVisible((BaseShellItem)item).ToString(); }
                    catch (Exception ex) { vis = L10n.T("code_query_failed", ex.GetType().Name); }
                }
                var line = $"[模板] {t.Name} '{title}' -> {tpl}  可见={vis}";
                sb.AppendLine(line);
                CrashLog.Write("Flyout", null, line);
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine("[模板] 遍历失败 " + ex);
        }

        void Check(string key, Func<bool> childVisible)
        {
            try
            {
                var btn = new ImageButton { CommandParameter = key };
                CrashLog.Write("SelfTest", null, $"进入 Check({key})");
                DropdownExpansion(btn, EventArgs.Empty);
                CrashLog.Write("SelfTest", null, $"  展开完成 -> {childVisible()}");
                bool expanded = childVisible();
                DropdownExpansion(btn, EventArgs.Empty);
                bool collapsed = childVisible();
                CrashLog.Write("SelfTest", null, $"  再点完成 -> {collapsed}");
                sb.AppendLine($"[展开] {key}: 展开后子项可见={expanded}，再点一次后={collapsed}");
            }
            catch (Exception ex)
            {
                CrashLog.Write("SelfTest", ex, $"Check({key}) 抛异常");
                sb.AppendLine($"[展开] {key}: 异常 {ex.GetType().Name}: {ex.Message}");
            }
        }

        Check(L10n.T("shell_file"), () => Shell.GetFlyoutItemIsVisible(OpenFile));
        Check(L10n.T("shell_data"), () => Shell.GetFlyoutItemIsVisible(LoadBoxes));
        Check(L10n.T("shell_boxes_party"), () => Shell.GetFlyoutItemIsVisible(DeleteBoxes));
        Check(L10n.T("shell_pkm_editor"), () => Shell.GetFlyoutItemIsVisible(thelegalizer));

        var settingsCases = new (string Name, Func<object> Create)[]
        {
            (L10n.T("loc_typenames_psettings"), () => new PSettings()),
            (L10n.T("loc_typenames_encountersettings"), () => new PKHeXMAUI.EncounterSettings()),
            (L10n.T("shell_alm_settings"), () => new PluginSettings()),
            (L10n.T("shell_legality_settings"), () => new LegalSettings()),
        };
        foreach (var (name, create) in settingsCases)
        {
            try
            {
                var grid = new propertyGrid(create());
                sb.AppendLine($"[设置] {name}: 构建成功");
                // 审计：把实际渲染出来的文字全扫一遍，列出还含英文字母的显示项
                var texts = new List<string>();
                CollectDisplayTexts(grid, texts);
                var english = texts
                    .Where(t => HasLatin(StripAllowed(t)))
                    .Select(t => t.Trim())
                    .Distinct()
                    .ToList();
                sb.AppendLine(english.Count == 0
                    ? $"[英文] {name}: 无残留"
                    : $"[英文] {name}: {string.Join(" / ", english)}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[设置] {name}: 异常 {ex.GetType().Name}: {ex.Message}");
            }
        }

        // 自动合规：确认合法性报告在真实进程里走的是中文资源
        try
        {
            var probe = EntityBlank.GetBlank(sav.Generation);
            probe.Species = 25;
            probe.CurrentLevel = 50;
            var report = Loc.LegalityReport(new LegalityAnalysis(probe, sav.Personal));
            var lines = report.Split('\n');
            sb.AppendLine("[合规] 报告首行: " + lines[0].Trim());
            var left = lines.Where(l => HasLatin(StripAllowed(l))).Select(l => l.Trim()).Take(8).ToList();
            sb.AppendLine(left.Count == 0
                ? "[合规] 报告无英文残留"
                : "[合规] 报告仍含英文的行: " + string.Join(" / ", left));
        }
        catch (Exception ex)
        {
            sb.AppendLine($"[合规] 报告生成异常 {ex.GetType().Name}: {ex.Message}");
        }

        // 导航自检：菜单栏里每个入口都要能跳到对应页面
        var routes = new (string Name, string Route)[]
        {
            ("编辑器", "//PKShell/pkeditortab/PKPage"),
            (L10n.T("stats_stats"), "//PKShell/pkeditortab/StatsPage"),
            (L10n.T("shell_moves"), "//PKShell/pkeditortab/MovesPage"),
            (L10n.T("shell_cosmetic"), "//PKShell/pkeditortab/CosmeticPage"),
            (L10n.T("shell_encounter"), "//PKShell/pkeditortab/MetPage"),
            (L10n.T("gen6_trainer"), "//PKShell/pkeditortab/OTPage"),
            (L10n.T("shell_boxes"), "//BoxShell/boxtab/BoxPage"),
            (L10n.T("box_party"), "//BoxShell/boxtab/PartyPage"),
            (L10n.T("shell_settings"), "//SettingsFlyout/SettingsTab/PKHeXSettingsPage"),
            ("ALM", "//SettingsFlyout/SettingsTab/ALMSettingsPage"),
            (L10n.T("loc_categories_legality"), "//SettingsFlyout/SettingsTab/LegalitySettingsPage"),
            ("Gen3优先级", "//SettingsFlyout/SettingsTab/Gen3PrioritySettingsPage"),
            ("相遇库", "//EncounterFlyout/EncounterPage"),
            ("LiveHex", "//LiveHexFlyout/livehextab/LiveHexPage"),
            ("存档", "//LiveHexFlyout/SaveEditorsPage"),
        };
        foreach (var (name, route) in routes)
        {
            try
            {
                await TheShell.GoToAsync(route);
                await Task.Delay(150);
                sb.AppendLine($"[导航] {name} -> {TheShell.CurrentPage?.Title ?? "(无标题)"}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[导航] {name}: 失败 {ex.GetType().Name}: {ex.Message}");
            }
        }

        return sb.ToString();
    }

    /// <summary>自检用：专有名词不算英文残留（品牌名、缩写、默认初训家等）。</summary>
    private static string StripAllowed(string text)
    {
        foreach (var token in new[] { "LiveHex", "PKHeX", "Pokémon", "Pokemon", "RNG", "PID", "ALM", "VC", "G5MGShiny", "ID", "AV", "GO" })
            text = text.Replace(token, "");
        return text;
    }

    private static bool HasLatin(string text)
        => text.Any(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z');

    /// <summary>自检用：递归收集某个控件树里会显示给用户的文本。</summary>
    private static void CollectDisplayTexts(IVisualTreeElement node, List<string> outp)    {
        switch (node)
        {
            case Label { Text: { Length: > 0 } t }:
                outp.Add("标签:" + t);
                break;
            case Entry en:
                if (!string.IsNullOrWhiteSpace(en.Text))
                    outp.Add("值:" + en.Text);
                if (!string.IsNullOrWhiteSpace(en.Placeholder))
                    outp.Add("占位:" + en.Placeholder);
                break;
            case comboBox cb:
                foreach (var item in cb.Items)
                    outp.Add("选项:" + item);
                break;
        }
        foreach (var child in node.GetVisualChildren())
            CollectDisplayTexts(child, outp);
    }
}
public class BoxManipulatorMAUI : BoxManipulator
{
    protected override SaveFile SAV => sav;
    protected override void FinishBoxManipulation(string message, bool all, int count) => Shell.Current.DisplayAlertAsync("完成", message + $" ({count})", L10n.T("pktab_ok"));

    protected override bool CanManipulateRegion(int start, int end, string prompt, string fail)
    {
        bool canModify = base.CanManipulateRegion(start, end, prompt, fail);
        if (!canModify && !string.IsNullOrEmpty(fail))
            Shell.Current.DisplayAlertAsync(L10n.T("shell_boxes"), fail, L10n.T("pktab_ok"));
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
        // CommandParameter 用 Route（稳定标识），不用 Title（会随界面语言变化）
        button.SetBinding(ImageButton.CommandParameterProperty, "Route");
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
        // 注意：这里必须绑 Title，ShellContent 没有 Text 属性
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
        // CommandParameter 用 AutomationId（稳定标识），不用 Text（会随界面语言变化）
        button.SetBinding(ImageButton.CommandParameterProperty, "AutomationId");
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
    // 标题已本地化，统一走 AppShell.MenuKey 归一化，避免改名后模板判定失效
    private static bool IsFlyoutItemTitle(string? t)
        => AppShell.MenuKey(t) is "Box/Party" or "pk editor" or "File";

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {        if (item is FlyoutItem e)
        {
            if (IsFlyoutItemTitle(e.Title))
                return FlyoutItemDataTemplate;
            else
                return MenuItemDataTemplate;
        }
        if(item is Tab t)
        {
            if (IsFlyoutItemTitle(t.Title))
                return FlyoutItemDataTemplate;
            else
                return MenuItemDataTemplate;
        }
        if (item is MenuItem mi)
        {
            // 一级菜单（文件 / 数据）带展开箭头，二级项缩进显示
            if (AppShell.MenuKey(mi.Text) is "File" or "Data")
                return MenuItemDropdownDataTemplate;
            return MenuItemDataTemplate2;
        }
        if (item.GetType().GetProperty("Title")?.GetValue(item) is string s)
        {
            if (AppShell.MenuKey(s) is "File" or "Data")
            return MenuItemDropdownDataTemplate;
        }
        return MenuItemDataTemplate2;
    }

    /// <summary>自检用：返回某个侧边栏项会被套用的模板名称。</summary>
    public string Probe(object item)
    {
        var t = OnSelectTemplate(item, null!);
        if (ReferenceEquals(t, FlyoutItemDataTemplate)) return "带箭头(主项)";
        if (ReferenceEquals(t, MenuItemDropdownDataTemplate)) return "带箭头(一级菜单)";
        if (ReferenceEquals(t, MenuItemDataTemplate)) return "普通项";
        if (ReferenceEquals(t, MenuItemDataTemplate2)) return "普通项(缩进)";
        return "未知";
    }
}
