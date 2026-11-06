namespace PKHeXMAUI;

/// <summary>
/// 界面支持的语言标识。
///
/// <para><b>选取依据</b></para>
/// 界面语言是<b>本 App 自己的属性</b>，与手头存档是哪一代无关 ——
/// 用户拿着第三世代存档也希望能用中文界面。
/// 所以这里的列表取<b>全部世代、所有官方发行版本合起来出现过的语言</b>的并集，
/// 而不是「当前这一代支持什么就只显示什么」。
/// <list type="bullet">
/// <item>第一～二世代：日文、英文（欧美版另有法/德/意/西）</item>
/// <item>第三世代：日文、英文、法文、德文、意大利文、西班牙文、韩文</item>
/// <item>第四～六世代：以上 7 种</item>
/// <item>第七～八世代：以上 + 繁体中文 + 简体中文（《太陽/月亮》2016 起）</item>
/// <item>第九世代：以上 9 种 + 拉丁美洲西班牙文（《宝可梦传说 Z-A》2025 起）</item>
/// </list>
///
/// <para><b>覆盖范围</b></para>
/// 每种界面语言都<b>完整覆盖所有 975 条界面文案</b>，其中包含第一～九世代
/// 各自的专属界面元素（gen1～gen9 前缀的键），不因世代而缺失任何一条。
///
/// <para><b>与「游戏内语言」的区别</b></para>
/// 写进存档的 Language 字段受该游戏版本实际发行语言限制，
/// 由 <see cref="GameLanguageCatalog"/> 单独处理，两套列表不要混淆。
///
/// <para><b>关于阿拉伯语 / 希伯来语</b></para>
/// 官方宝可梦游戏从未发行过这两种语言，因此它们<b>不出现在语言选择器里</b>；
/// 但 <see cref="L10nLang.Arabic"/> / <see cref="L10nLang.Hebrew"/> 已在枚举里注册，
/// RTL 排版能力（镜像布局、文本方向、字体回退）也已完整实现，
/// 一旦补齐译文即可直接在 <see cref="L10nLangs.All"/> 里启用。
/// </summary>
public enum L10nLang
{
    /// <summary>简体中文。</summary>
    ZhHans,

    /// <summary>繁體中文。</summary>
    ZhHant,

    /// <summary>English.</summary>
    English,

    /// <summary>日本語。</summary>
    Japanese,

    /// <summary>Français.</summary>
    French,

    /// <summary>Deutsch。</summary>
    German,

    /// <summary>Italiano。</summary>
    Italian,

    /// <summary>Español（西班牙 / 拉美）。</summary>
    Spanish,

    /// <summary>Español Latinoamericano（拉丁美洲西班牙文，第九世代起部分地区发行）。</summary>
    SpanishLatam,

    /// <summary>한국어.</summary>
    Korean,

    /// <summary>العربية（阿拉伯语，从右向左书写；官方游戏未发行，机制已就绪）。</summary>
    Arabic,

    /// <summary>עברית（希伯来语，从右向左书写；官方游戏未发行，机制已就绪）。</summary>
    Hebrew,
}

/// <summary>语言的元数据。</summary>
/// <param name="Lang">语言标识。</param>
/// <param name="Code">BCP-47 语言标记（用于匹配系统语言、持久化）。</param>
/// <param name="NativeName">该语言的自称（语言选择器里显示）。</param>
/// <param name="EnglishName">英文名（用于英文界面下显示）。</param>
/// <param name="IsRtl">是否从右向左书写。</param>
public readonly record struct L10nLangInfo(
    L10nLang Lang,
    string Code,
    string NativeName,
    string EnglishName,
    bool IsRtl = false);

/// <summary>语言注册表。</summary>
public static class L10nLangs
{
    /// <summary>
    /// 全部可选界面语言（按显示顺序）。
    /// <para>
    /// 注意：这里的 <c>NativeName</c> 是<b>该语言的自称</b>，
    /// 用它自己的文字写（"简体中文"、"日本語"…），
    /// 这样在任何界面语言下看到的都是母语者最熟悉的名字。
    /// <b>不能</b>在这里调用 <c>L10n.T()</c> ——
    /// 那会形成循环依赖（<c>L10n.T</c> → <c>L10nManager</c> → <c>L10nLangs</c> → <c>L10n.T</c>），
    /// 静态初始化时必然 NRE。
    /// </para>
    /// </summary>
    public static readonly L10nLangInfo[] All =
    [
        new(L10nLang.ZhHans, "zh-Hans", "简体中文", "Simplified Chinese"),
        new(L10nLang.ZhHant, "zh-Hant", "繁體中文", "Traditional Chinese"),
        new(L10nLang.English, "en", "English", "English"),
        new(L10nLang.Japanese, "ja", "日本語", "Japanese"),
        new(L10nLang.French, "fr", "Français", "French"),
        new(L10nLang.German, "de", "Deutsch", "German"),
        new(L10nLang.Italian, "it", "Italiano", "Italian"),
        new(L10nLang.Spanish, "es", "Español", "Spanish"),
        new(L10nLang.SpanishLatam, "es-419", "Español Latinoamericano", "Latin American Spanish"),
        new(L10nLang.Korean, "ko", "한국어", "Korean"),
    ];

    /// <summary>
    /// 已注册但暂不提供的语言（官方游戏未发行，译文待补）。
    /// 它们带 RTL 元数据，补齐译文后加进 <see cref="All"/> 即可启用。
    /// </summary>
    public static readonly L10nLangInfo[] Registered =
    [
        new(L10nLang.Arabic, "ar", "العربية", "Arabic", IsRtl: true),
        new(L10nLang.Hebrew, "he", "עברית", "Hebrew", IsRtl: true),
    ];

    /// <summary>回退语言（英文）。</summary>
    public const L10nLang Fallback = L10nLang.English;

    /// <summary>按枚举取元数据（先查可选列表，再查已注册列表）。</summary>
    public static L10nLangInfo Info(L10nLang lang)
    {
        foreach (var i in All)
        {
            if (i.Lang == lang)
                return i;
        }
        foreach (var i in Registered)
        {
            if (i.Lang == lang)
                return i;
        }
        return new L10nLangInfo(lang, lang.ToString(), lang.ToString(), lang.ToString());
    }

    /// <summary>按 BCP-47 标记取语言标识；取不到返回 null。</summary>
    public static L10nLang? FromCode(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return null;
        foreach (var i in All)
        {
            if (string.Equals(i.Code, code, StringComparison.OrdinalIgnoreCase))
                return i.Lang;
        }
        foreach (var i in Registered)
        {
            if (string.Equals(i.Code, code, StringComparison.OrdinalIgnoreCase))
                return i.Lang;
        }
        return null;
    }

    /// <summary>语言标识的 BCP-47 标记。</summary>
    public static string Code(L10nLang lang) => Info(lang).Code;
}
