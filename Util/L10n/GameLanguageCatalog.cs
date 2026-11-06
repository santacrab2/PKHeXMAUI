using PKHeX.Core;

namespace PKHeXMAUI;

/// <summary>
/// 游戏内语言（宝可梦 / 存档 / 训练家的 Language 字段）的目录。
///
/// <para><b>两种「语言」要分清</b></para>
/// <list type="bullet">
/// <item><b>界面语言</b>（<see cref="L10nLang"/>）：本 App 自己的 UI 语言。
///   与手头存档是哪一代<b>无关</b>，永远是
///   <see cref="L10nLangs.All"/> 里那几种，每种都完整覆盖所有世代的界面文案。
///   详见 <see cref="L10nLangs"/>。</item>
/// <item><b>游戏内语言</b>（<see cref="LanguageID"/>）：写进存档的 Language 字段，
///   受该游戏版本<b>实际发行过的语言</b>限制。本类负责这一侧。</item>
/// </list>
///
/// <para><b>各世代游戏内可选语言（依据官方实际发行）</b></para>
/// <list type="bullet">
/// <item><b>第一世代</b>：日文（红/绿/蓝日版）；英/西/法/德/意（红蓝国际版）；
///   日+英/西/法/德/意（皮卡丘国际版）。</item>
/// <item><b>第二世代</b>：以上 6 种 + <b>韩文</b>。
///   《金/銀》2002 年在韩国首发，是系列第一个韩版作品。</item>
/// <item><b>第三世代</b>：以上 7 种（含韩文，绿宝石有韩文版）。
///   注：红蓝宝石在韩国是<b>日文</b>版发行，故韩文自 Gen2 起持续可用。</item>
/// <item><b>第四～六世代</b>：以上 7 种。</item>
/// <item><b>第七～八世代</b>：以上 7 种 + <b>繁體中文 + 简体中文</b>。
///   《太陽/月亮》（2016）起游戏内首次内置 9 种语言。
///   （第五世代黑2/白2 的港澳台发行是<b>日文版</b>，不含中文，故繁中不从 Gen5 起。）</item>
/// <item><b>第九世代</b>：以上 9 种 + <b>拉丁美洲西班牙文</b>。
///   拉美西语首发于《宝可梦传说 Z-A》（2025）。</item>
/// </list>
///
/// <para><b>数据来源</b></para>
/// <list type="bullet">
/// <item>Bulbapedia「Core series」：韩文 29 部（首个为金/銀）、
///   中英双语 14 部（首个为太陽/月亮）、拉美西语 1 部（传说 Z-A）。</item>
/// <item>任天堂台湾官方新闻稿（2016-05-10）：《太陽/月亮》内置 9 种语言。</item>
/// <item>52poke「寶可夢 金/銀」：金/銀 是首个韩版，之后到《鑽石/珍珠》前韩国无新韩版。</item>
/// </list>
///
/// <para><b>Gen1/Gen2 的特别处理</b></para>
/// 实测（<c>_verify3/Probe27</c>）：第一、二世代的 <c>pk.Language</c> 是
/// <b>只读推导值</b> —— 写入任何值读回都是 0。它的真实来源是
/// 「初训家名/图鉴名的字符编码」+ <c>Japanese</c>/<c>Korean</c> 标志
/// （Core 内部用 <c>LanguageGC</c> / <c>LanguageBR</c> 两套枚举）。
/// 因此这两代的语言<b>不可直接设置</b>，界面应提示而非给出无效下拉框。
/// </summary>
public static class GameLanguageCatalog
{
    /// <summary>一个可选语言项（供下拉框使用）。</summary>
    /// <param name="Value">写入存档的数值。</param>
    /// <param name="Display">显示名（按当前界面语言翻译）。</param>
    public readonly record struct Option(byte Value, string Display);

    /// <summary>
    /// 取某个存档版本实际支持的语言列表。
    /// </summary>
    /// <param name="sav">当前存档（决定世代与地区版本）。</param>
    public static List<Option> ForSave(SaveFile? sav)
    {
        if (sav is null)
            return ForGeneration(3);

        // 第一、二世代：语言不可直接设置（见类型注释），返回空列表让界面隐藏
        if (sav is SAV1 or SAV2)
            return [];

        return ForGeneration(GenerationOf(sav.Version));
    }

    /// <summary>
    /// 取某一代实际发行过的游戏内语言列表。
    /// <para>
    /// 判定依据是「该世代<b>所有版本</b>官方实际发行过的语言的并集」，
    /// 而不是 Core 的枚举全集（那会给出该代并不存在的选项，如 Gen1 的韩文/中文）。
    /// </para>
    /// </summary>
    public static List<Option> ForGeneration(int generation)
    {
        var list = new List<Option>();
        void Add(LanguageID id)
            => list.Add(new Option((byte)id, L10n.T($"gamelang_{NameOf(id)}", DefaultDisplay(id))));

        // 日文：全世代都有
        Add(LanguageID.Japanese);

        // 英文：第一世代起（红蓝国际版）
        Add(LanguageID.English);

        // 法/德/意/西：第一世代起（红蓝国际版，1999）
        Add(LanguageID.French);
        Add(LanguageID.Italian);
        Add(LanguageID.German);
        Add(LanguageID.Spanish);

        // 韩文：第二世代起（《金/銀》2002 韩国首发；绿宝石有韩文版）
        if (generation >= 2)
            Add(LanguageID.Korean);

        // 繁中 + 简中：第七世代起（《太陽/月亮》2016 首次内置中文）
        if (generation >= 7)
        {
            Add(LanguageID.ChineseT);
            Add(LanguageID.ChineseS);
        }

        // 拉丁美洲西班牙文：第九世代起（《宝可梦传说 Z-A》2025）
        if (generation >= 9)
            Add(LanguageID.SpanishL);

        return list;
    }

    /// <summary>取存档版本对应的世代（1~9）。</summary>
    public static int GenerationOf(GameVersion version) => version switch
    {
        GameVersion.RD or GameVersion.C or GameVersion.GN or GameVersion.BU or GameVersion.YW => 1,
        GameVersion.GD or GameVersion.SI => 2,
        GameVersion.R or GameVersion.S or GameVersion.E or GameVersion.FR or GameVersion.LG => 3,
        GameVersion.D or GameVersion.P or GameVersion.Pt or GameVersion.HG or GameVersion.SS => 4,
        GameVersion.B or GameVersion.W or GameVersion.B2 or GameVersion.W2 => 5,
        GameVersion.X or GameVersion.Y or GameVersion.OR or GameVersion.AS => 6,
        GameVersion.SN or GameVersion.MN or GameVersion.US or GameVersion.UM or GameVersion.GP or GameVersion.GE => 7,
        GameVersion.BD or GameVersion.SP or GameVersion.SW or GameVersion.SH or GameVersion.PLA => 8,
        _ => 9,
    };

    /// <summary>该存档版本的语言是否可以直接设置（第一、二世代不行）。</summary>
    public static bool IsSettable(SaveFile? sav) => sav is not (SAV1 or SAV2);

    /// <summary>
    /// LanguageID 的枚举名 → 资源键后缀。
    /// Core 里的枚举名是 <c>ChineseS</c> / <c>ChineseT</c> / <c>SpanishL</c> 这种缩写，
    /// 直接拿来做资源键很容易和实际显示名对不上，这里统一成小写下划线。
    /// </summary>
    public static string NameOf(LanguageID id) => id switch
    {
        LanguageID.Japanese => "japanese",
        LanguageID.English => "english",
        LanguageID.French => "french",
        LanguageID.Italian => "italian",
        LanguageID.German => "german",
        LanguageID.Spanish => "spanish",
        LanguageID.Korean => "korean",
        LanguageID.ChineseS => "chineses",
        LanguageID.ChineseT => "chineset",
        LanguageID.SpanishL => "spanishl",
        _ => "unspecified",
    };

    /// <summary>游戏内语言名的默认英文（回退基准）。</summary>
    public static string DefaultDisplay(LanguageID id) => id switch
    {
        LanguageID.Japanese => "Japanese",
        LanguageID.English => "English",
        LanguageID.French => "French",
        LanguageID.Italian => "Italian",
        LanguageID.German => "German",
        LanguageID.Spanish => "Spanish",
        LanguageID.Korean => "Korean",
        LanguageID.ChineseS => "Simplified Chinese",
        LanguageID.ChineseT => "Traditional Chinese",
        LanguageID.SpanishL => "Latin American Spanish",
        _ => "Unspecified",
    };
}
