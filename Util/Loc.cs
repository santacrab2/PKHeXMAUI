using System.Globalization;
using Microsoft.Maui.Controls;
using PKHeX.Core;

namespace PKHeXMAUI;

/// <summary>
/// 设置页 / 属性表格的本地化映射。
/// 这些页面的文字是靠反射读出来的属性名、分组名、类型名和枚举值，
/// 没法在 XAML 里直接改，所以统一在这里做「英文键 -> 本地化显示名」的翻译。
/// 注意：只影响显示，绝不影响绑定用的原始键（Preferences 的 key 仍是英文）。
///
/// <para><b>为什么是「惰性 + 随语言失效」</b></para>
/// 字典里的值来自 <see cref="L10n.T(string)"/>，而语言是可切换的 ——
/// 如果做成静态初始化，值会在类首次加载时就固定下来，之后切语言不会更新。
/// 所以这里记录构建时的语言，切换后自动重建。
/// </summary>
public static class Loc
{
    private static Dictionary<string, string>? _propsCache;
    private static L10nLang _propsLang;
    private static Dictionary<string, string> Props
    {
        get
        {
            if (_propsCache is null || _propsLang != L10nManager.Instance.Current)
            {
                _propsCache = new()
                {
                ["StartupPage"] = L10n.T("loc_props_startuppage"),
                ["IgnoreLegalPopup"] = L10n.T("loc_props_ignorelegalpopup"),
                ["RememberLastSave"] = L10n.T("loc_props_rememberlastsave"),
                ["DisplayLegalBallsOnly"] = L10n.T("loc_props_displaylegalballsonly"),
                ["AllowIncompatibleConversion"] = L10n.T("loc_props_allowincompatibleconversion"),
                ["SetUpdatePKM"] = L10n.T("loc_props_setupdatepkm"),
                ["FilterUnavailableSpecies"] = L10n.T("loc_props_filterunavailablespecies"),
                ["UsePkEditorAsCriteria"] = L10n.T("loc_props_usepkeditorascriteria"),
                ["ShowExtraEncounterInfo"] = L10n.T("loc_props_showextraencounterinfo"),
                ["DefaultOT"] = L10n.T("loc_props_defaultot"),
                ["DefaultTID"] = L10n.T("loc_props_defaulttid"),
                ["DefaultSID"] = L10n.T("loc_props_defaultsid"),
                ["UseTrainerData"] = L10n.T("loc_props_usetrainerdata"),
                ["TrainerFolderPath"] = L10n.T("loc_props_trainerfolderpath"),
                ["SetAllLegalRibbons"] = L10n.T("loc_props_setalllegalribbons"),
                ["SetBattleVersion"] = L10n.T("loc_props_setbattleversion"),
                ["SetBallByColor"] = L10n.T("loc_props_setballbycolor"),
                ["EnableMemesForIllegalSets"] = L10n.T("loc_props_enablememesforillegalsets"),
                ["ForceLevel100For50"] = L10n.T("loc_props_forcelevel100for50"),
                ["LivingDexAllForms"] = L10n.T("loc_props_livingdexallforms"),
                ["LivingDexNativeOnly"] = L10n.T("loc_props_livingdexnativeonly"),
                ["LivingDexSetAlpha"] = L10n.T("loc_props_livingdexsetalpha"),
                ["LivingDexSetShiny"] = L10n.T("loc_props_livingdexsetshiny"),
                ["CheckWordFilter"] = L10n.T("loc_props_checkwordfilter"),
                ["AllowGen1Tradeback"] = L10n.T("loc_props_allowgen1tradeback"),
                ["NicknamedTrade"] = L10n.T("loc_props_nicknamedtrade"),
                ["NicknamedMysteryGift"] = L10n.T("loc_props_nicknamedmysterygift"),
                ["RNGFrameNotFound"] = L10n.T("loc_props_rngframenotfound"),
                ["Gen7TransferStarPID"] = L10n.T("loc_props_gen7transferstarpid"),
                ["Gen8MemoryMissingHT"] = L10n.T("loc_props_gen8memorymissinght"),
                ["Gen8TransferTrackerNotPresent"] = L10n.T("loc_props_gen8transfertrackernotpresent"),
                ["NicknamedAnotherSpecies"] = L10n.T("loc_props_nicknamedanotherspecies"),
                ["ZeroHeightWeight"] = L10n.T("loc_props_zeroheightweight"),
                ["CurrentHandlerMismatch"] = L10n.T("loc_props_currenthandlermismatch"),
                ["CheckActiveHandler"] = L10n.T("loc_props_checkactivehandler"),
                };
                _propsLang = L10nManager.Instance.Current;
            }
            return _propsCache;
        }
    }

    private static Dictionary<string, string>? _categoriesCache;
    private static L10nLang _categoriesLang;
    private static Dictionary<string, string> Categories
    {
        get
        {
            if (_categoriesCache is null || _categoriesLang != L10nManager.Instance.Current)
            {
                _categoriesCache = new()
                {
                ["Legality"] = L10n.T("loc_categories_legality"),
                ["Event"] = L10n.T("loc_categories_event"),
                ["Misc"] = L10n.T("saveedit_miscellaneous"),
                ["Trainer"] = L10n.T("gen6_trainer"),
                ["Settings"] = L10n.T("shell_settings"),
                ["Encounter"] = L10n.T("shell_encounter"),
                ["Encounters"] = L10n.T("shell_encounter"),
                ["Box"] = L10n.T("shell_boxes"),
                ["Data"] = L10n.T("shell_data"),
                ["Other"] = L10n.T("common_other"),
                };
                _categoriesLang = L10nManager.Instance.Current;
            }
            return _categoriesCache;
        }
    }

    private static Dictionary<string, string>? _typenamesCache;
    private static L10nLang _typenamesLang;
    private static Dictionary<string, string> TypeNames
    {
        get
        {
            if (_typenamesCache is null || _typenamesLang != L10nManager.Instance.Current)
            {
                _typenamesCache = new()
                {
                ["PSettings"] = L10n.T("loc_typenames_psettings"),
                ["EncounterSettings"] = L10n.T("loc_typenames_encountersettings"),
                ["PluginSettings"] = L10n.T("shell_alm_settings"),
                ["LegalSettings"] = L10n.T("shell_legality_settings"),
                };
                _typenamesLang = L10nManager.Instance.Current;
            }
            return _typenamesCache;
        }
    }

    private static Dictionary<GameVersion, string>? _gamesCache;
    private static L10nLang _gamesLang;
    private static Dictionary<GameVersion, string> Games
    {
        get
        {
            if (_gamesCache is null || _gamesLang != L10nManager.Instance.Current)
            {
                _gamesCache = new()
                {
                [GameVersion.CP] = L10n.T("loc_games_cp"),
                [GameVersion.ZA] = L10n.T("loc_games_za"),
                [GameVersion.SL] = L10n.T("loc_games_sl"),
                [GameVersion.VL] = L10n.T("loc_games_vl"),
                [GameVersion.PLA] = L10n.T("loc_games_pla"),
                [GameVersion.BD] = L10n.T("loc_games_bd"),
                [GameVersion.SP] = L10n.T("loc_games_sp"),
                [GameVersion.SW] = L10n.T("loc_games_sw"),
                [GameVersion.SH] = L10n.T("loc_games_sh"),
                [GameVersion.GP] = L10n.T("loc_games_gp"),
                [GameVersion.GE] = L10n.T("loc_games_ge"),
                [GameVersion.SN] = L10n.T("loc_games_sn"),
                [GameVersion.MN] = L10n.T("loc_games_mn"),
                [GameVersion.US] = L10n.T("loc_games_us"),
                [GameVersion.UM] = L10n.T("loc_games_um"),
                [GameVersion.X] = L10n.T("loc_games_x"),
                [GameVersion.Y] = L10n.T("loc_games_y"),
                [GameVersion.OR] = L10n.T("loc_games_or"),
                [GameVersion.AS] = L10n.T("loc_games_as"),
                [GameVersion.B] = L10n.T("loc_games_b"),
                [GameVersion.W] = L10n.T("loc_games_w"),
                [GameVersion.B2] = L10n.T("loc_games_b2"),
                [GameVersion.W2] = L10n.T("loc_games_w2"),
                [GameVersion.D] = L10n.T("loc_games_d"),
                [GameVersion.P] = L10n.T("loc_games_p"),
                [GameVersion.Pt] = L10n.T("loc_games_pt"),
                [GameVersion.BATREV] = L10n.T("loc_games_batrev"),
                [GameVersion.HG] = L10n.T("loc_games_hg"),
                [GameVersion.SS] = L10n.T("loc_games_ss"),
                [GameVersion.R] = L10n.T("loc_games_r"),
                [GameVersion.S] = L10n.T("loc_games_s"),
                [GameVersion.E] = L10n.T("loc_games_e"),
                [GameVersion.FR] = L10n.T("loc_games_fr"),
                [GameVersion.LG] = L10n.T("loc_games_lg"),
                [GameVersion.CXD] = L10n.T("loc_games_cxd"),
                [GameVersion.GD] = L10n.T("loc_games_gd"),
                [GameVersion.SI] = L10n.T("loc_games_si"),
                [GameVersion.C] = L10n.T("loc_games_c"),
                [GameVersion.RD] = L10n.T("loc_games_rd"),
                [GameVersion.GN] = L10n.T("loc_games_gn"),
                [GameVersion.BU] = L10n.T("loc_games_bu"),
                [GameVersion.YW] = L10n.T("loc_games_yw"),
                [GameVersion.GO] = L10n.T("loc_games_go"),
                [GameVersion.Any] = L10n.T("code_text_2"),
                };
                _gamesLang = L10nManager.Instance.Current;
            }
            return _gamesCache;
        }
    }

    private static Dictionary<string, string>? _languagesCache;
    private static L10nLang _languagesLang;
    private static Dictionary<string, string> Languages
    {
        get
        {
            if (_languagesCache is null || _languagesLang != L10nManager.Instance.Current)
            {
                _languagesCache = new()
                {
                ["Japanese"] = L10n.T("loc_languages_japanese"),
                ["English"] = L10n.T("loc_languages_english"),
                ["French"] = L10n.T("loc_languages_french"),
                ["German"] = L10n.T("loc_languages_german"),
                ["Italian"] = L10n.T("loc_languages_italian"),
                ["Spanish"] = L10n.T("loc_languages_spanish"),
                ["SpanishL"] = L10n.T("loc_languages_spanishl"),
                ["Korean"] = L10n.T("loc_languages_korean"),
                ["ChineseS"] = L10n.T("loc_languages_chineses"),
                ["ChineseT"] = L10n.T("loc_languages_chineset"),
                ["UNUSED_6"] = L10n.T("loc_languages_unused_6"),
                ["None"] = L10n.T("loc_languages_none"),
                };
                _languagesLang = L10nManager.Instance.Current;
            }
            return _languagesCache;
        }
    }

    private static Dictionary<string, string>? _eventflagsmapCache;
    private static L10nLang _eventflagsmapLang;
    private static Dictionary<string, string> EventFlagsMap
    {
        get
        {
            if (_eventflagsmapCache is null || _eventflagsmapLang != L10nManager.Instance.Current)
            {
                _eventflagsmapCache = new()
                {
                ["Lati@s Roaming"] = L10n.T("loc_eventflagsmap_lati_s_roaming"),
                ["Defeated Mew (Unreleased Internationally)"] = L10n.T("loc_eventflagsmap_defeated_mew_unreleased_internationally"),
                ["Captured Mew (Unreleased Internationally)"] = L10n.T("loc_eventflagsmap_captured_mew_unreleased_internationally"),
                ["Captured Lugia"] = L10n.T("loc_eventflagsmap_captured_lugia"),
                ["Captured Ho-Oh"] = L10n.T("loc_eventflagsmap_captured_ho_oh"),
                ["Defeated Ho-Oh"] = L10n.T("loc_eventflagsmap_defeated_ho_oh"),
                ["Defeated Mew"] = L10n.T("loc_eventflagsmap_defeated_mew"),
                ["Captured Mew"] = L10n.T("loc_eventflagsmap_captured_mew"),
                ["Obtained Mew"] = L10n.T("loc_eventflagsmap_obtained_mew"),
                ["Distributed Mew"] = L10n.T("loc_eventflagsmap_distributed_mew"),
                };
                _eventflagsmapLang = L10nManager.Instance.Current;
            }
            return _eventflagsmapCache;
        }
    }

    private static Dictionary<string, string>? _inventorytypemapCache;
    private static L10nLang _inventorytypemapLang;
    private static Dictionary<string, string> InventoryTypeMap
    {
        get
        {
            if (_inventorytypemapCache is null || _inventorytypemapLang != L10nManager.Instance.Current)
            {
                _inventorytypemapCache = new()
                {
                ["KeyItems"] = L10n.T("loc_inventorytypemap_keyitems"),
                ["Balls"] = L10n.T("loc_inventorytypemap_balls"),
                ["BattleItems"] = L10n.T("loc_inventorytypemap_battleitems"),
                ["Berries"] = L10n.T("loc_inventorytypemap_berries"),
                ["Medicine"] = L10n.T("loc_inventorytypemap_medicine"),
                ["Machines"] = L10n.T("loc_inventorytypemap_machines"),
                ["TMHM"] = L10n.T("loc_inventorytypemap_tmhm"),
                ["CT"] = L10n.T("loc_inventorytypemap_ct"),
                ["FreeSpace"] = L10n.T("loc_inventorytypemap_freespace"),
                ["Candy"] = L10n.T("loc_inventorytypemap_candy"),
                ["ZCrystals"] = L10n.T("loc_inventorytypemap_zcrystals"),
                ["Mail"] = L10n.T("loc_inventorytypemap_mail"),
                ["MegaStone"] = L10n.T("loc_inventorytypemap_megastone"),
                ["RotomPower"] = L10n.T("loc_inventorytypemap_rotompower"),
                ["Nectar"] = L10n.T("loc_inventorytypemap_nectar"),
                ["Ingredient"] = L10n.T("loc_inventorytypemap_ingredient"),
                ["Wood"] = L10n.T("loc_inventorytypemap_wood"),
                ["Fishing"] = L10n.T("loc_inventorytypemap_fishing"),
                ["Recipe"] = L10n.T("loc_inventorytypemap_recipe"),
                ["Material"] = L10n.T("loc_inventorytypemap_material"),
                ["Picnic"] = L10n.T("loc_inventorytypemap_picnic"),
                ["Snack"] = L10n.T("loc_inventorytypemap_snack"),
                ["Valuables"] = L10n.T("loc_inventorytypemap_valuables"),
                ["Trash"] = L10n.T("loc_inventorytypemap_trash"),
                ["Items"] = L10n.T("saveedit_items"),
                ["Treasure"] = L10n.T("gen4_treasures"),
                ["Records"] = L10n.T("gen5_records"),
                };
                _inventorytypemapLang = L10nManager.Instance.Current;
            }
            return _inventorytypemapCache;
        }
    }

    private static Dictionary<string, string>? _exceptionsCache;
    private static L10nLang _exceptionsLang;
    private static Dictionary<string, string> Exceptions
    {
        get
        {
            if (_exceptionsCache is null || _exceptionsLang != L10nManager.Instance.Current)
            {
                _exceptionsCache = new()
                {
                ["IndexOutOfRangeException"] = L10n.T("loc_exceptions_indexoutofrangeexception"),
                ["ArgumentOutOfRangeException"] = L10n.T("loc_exceptions_argumentoutofrangeexception"),
                ["NullReferenceException"] = L10n.T("loc_exceptions_nullreferenceexception"),
                ["ArgumentNullException"] = L10n.T("loc_exceptions_argumentnullexception"),
                ["FormatException"] = L10n.T("loc_exceptions_formatexception"),
                ["InvalidOperationException"] = L10n.T("loc_exceptions_invalidoperationexception"),
                ["ArgumentException"] = L10n.T("loc_exceptions_argumentexception"),
                };
                _exceptionsLang = L10nManager.Instance.Current;
            }
            return _exceptionsCache;
        }
    }

    private static Dictionary<string, string>? _encountergroupsCache;
    private static L10nLang _encountergroupsLang;
    private static Dictionary<string, string> EncounterGroups
    {
        get
        {
            if (_encountergroupsCache is null || _encountergroupsLang != L10nManager.Instance.Current)
            {
                _encountergroupsCache = new()
                {
                ["Egg"] = L10n.T("loc_encountergroups_egg"),
                ["Mystery"] = L10n.T("loc_encountergroups_mystery"),
                ["Static"] = L10n.T("loc_encountergroups_static"),
                ["Trade"] = L10n.T("loc_encountergroups_trade"),
                ["Slot"] = L10n.T("loc_encountergroups_slot"),
                ["Invalid"] = L10n.T("livehex_invalid"),
                };
                _encountergroupsLang = L10nManager.Instance.Current;
            }
            return _encountergroupsCache;
        }
    }

    public static string Prop(string? name)
        => name is not null && Props.TryGetValue(name, out var v) ? v : name ?? string.Empty;

    public static string Category(string? category)
        => category is not null && Categories.TryGetValue(category, out var v) ? v : category ?? string.Empty;

    public static string TypeName(string? fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return string.Empty;
        var shortName = fullName[(fullName.LastIndexOf('.') + 1)..];
        return TypeNames.TryGetValue(shortName, out var v) ? v : shortName;
    }

    public static string Language(LanguageID id)
    {
        var name = id.ToString();
        return Languages.TryGetValue(name, out var v) ? v : name;
    }

    public static string EventFlag(string? name)
        => name is not null && EventFlagsMap.TryGetValue(name, out var v) ? v : name ?? string.Empty;

    /// <summary>事件标志名的显示转换器：把英文标志名翻成本地化文案。</summary>
    public class EventFlagConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is string s ? EventFlag(s) : value?.ToString() ?? string.Empty;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public static string InventoryType(InventoryType type)
        => InventoryTypeMap.TryGetValue(type.ToString(), out var v) ? v : type.ToString();

    public static string ExceptionMessage(Exception? ex)
        => ex is not null && Exceptions.TryGetValue(ex.GetType().Name, out var v) ? v : ex?.Message ?? string.Empty;

    /// <summary>下拉项显示文本：布尔、Severity、启动页枚举翻成本地化文案，其它原样。</summary>
    public static string Value(object? item) => item switch
    {
        null => string.Empty,
        bool b => b ? L10n.T("common_yes") : L10n.T("common_no"),
        Severity s => s switch
        {
            Severity.Invalid => L10n.T("common_invalid"),
            Severity.Fishy => L10n.T("common_fishy"),
            Severity.Valid => L10n.T("common_valid"),
            _ => s.ToString(),
        },
        // 启动页选项（PSettings.StartupPage）
        StartPage p => p switch
        {
            StartPage.PKEditor => L10n.T("shell_pkm_editor"),
            StartPage.Box => L10n.T("shell_boxes"),
            StartPage.Encounters => L10n.T("shell_encounter_database"),
            StartPage.LiveHex => "LiveHex",
            StartPage.SaveEditors => L10n.T("shell_save_editor"),
            _ => p.ToString(),
        },
        _ => item.ToString() ?? string.Empty,
    };

    public static string Game(GameVersion version, string? fallback)
        => Games.TryGetValue(version, out var v) ? v : fallback ?? version.ToString();

    public static string LegalityReport(LegalityAnalysis la)
    {
        string text;
        try
        {
            text = LegalityFormatting.Report(la, "zh-Hans", true);
        }
        catch
        {
            try { return la.Report(); }
            catch { return string.Empty; }
        }
        return FixReportSummary(text);
    }

    private static string FixReportSummary(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (TryTail(lines[i], L10n.T("common_version_label"), out var version) && Enum.TryParse<GameVersion>(version, true, out var gv))
            {
                lines[i] = L10n.T("common_version_label_sp") + Game(gv, version);
            }
            else if (TryTail(lines[i], L10n.T("common_encounter_label"), out var encounter))
            {
                var name = encounter.Split(' ')[0];
                if (EncounterGroups.TryGetValue(name, out var cn))
                    lines[i] = L10n.T("common_encounter_label_sp") + cn + encounter[name.Length..];
            }
        }
        return string.Join('\n', lines);
    }

    private static bool TryTail(string line, string prefix, out string tail)
    {
        if (line.StartsWith(prefix, StringComparison.Ordinal))
        {
            tail = line[prefix.Length..].Trim();
            return true;
        }
        tail = string.Empty;
        return false;
    }

    public static string AutoModError(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return L10n.T("loc_autolegalize_no_reason");
        var m = message.Trim();
        if (m.StartsWith("No valid matching encounter available", StringComparison.OrdinalIgnoreCase))
            return L10n.T("loc_no_encounter_match", m);
        if (m.StartsWith("Ball is not possible", StringComparison.OrdinalIgnoreCase))
            return L10n.T("loc_illegal_ball", m);
        return L10n.T("loc_autolegalize_failed", m);
    }
}
