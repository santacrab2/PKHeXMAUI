using System.ComponentModel;
using System.Globalization;

namespace PKHeXMAUI;

/// <summary>
/// 本地化管理器：当前界面语言、切换、持久化、首次启动判定。
///
/// <para><b>为什么用「索引器绑定 + PropertyChanged(空名)」刷新界面</b></para>
/// 实测（<c>_verify3/Probe24</c>）：
/// <list type="bullet">
/// <item><c>Binding("[key]")</c> 索引器绑定 + 源对象 raise <c>PropertyChanged("")</c> → <b>会刷新</b>；</item>
/// <item>raise <c>PropertyChanged("Item[]")</c> 或具体键名 → <b>不会</b>刷新；</item>
/// <item><c>DynamicResource</c> 换字典 → <b>不会</b>刷新；</item>
/// <item>索引器键名<b>不能含点号</b>（MAUI 的 BindingExpression 解析索引路径时会报错）。</item>
/// </list>
/// 所以本类实现 <see cref="INotifyPropertyChanged"/> 的索引器，
/// 切换语言时统一 raise 空名，让所有绑定到它的控件立即刷新。
/// </summary>
public sealed class L10nManager : INotifyPropertyChanged
{
    /// <summary>持久化键。</summary>
    private const string PrefKey = "AppLanguage";

    private static L10nManager? _instance;

    /// <summary>全局单例。</summary>
    public static L10nManager Instance => _instance ??= new L10nManager();

    private L10nLang _current = L10nLang.ZhHans;

    /// <summary>当前界面语言。</summary>
    public L10nLang Current
    {
        get => _current;
        private set
        {
            if (_current == value)
                return;
            _current = value;
            Persist(value);
            LanguageChanged?.Invoke(value);
            // 空名 → 触发所有索引器绑定重新求值
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Current)));
        }
    }

    /// <summary>当前语言是否从右向左书写。</summary>
    public bool IsRightToLeft => L10nLangs.Info(Current).IsRtl;

    /// <summary>语言变更事件（供需要额外处理的场景，如 RTL 布局切换）。</summary>
    public static event Action<L10nLang>? LanguageChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>索引器：给 XAML 的 <c>Binding("[key]")</c> 用。</summary>
    public string this[string key] => L10nCatalog.Get(key, Current);

    /// <summary>取文案（代码里用）。</summary>
    public static string Get(string key) => L10nCatalog.Get(key, Instance.Current);

    /// <summary>
    /// 启动时调用：读取持久化的选择；没有则按系统语言判定；都取不到就回退英语。
    /// </summary>
    public void Initialize()
    {
        var saved = Preferences.Default.Get(PrefKey, string.Empty);
        if (!string.IsNullOrEmpty(saved) && L10nLangs.FromCode(saved) is { } lang)
        {
            _current = lang;
            return;
        }
        _current = DetectSystemLanguage();
    }

    /// <summary>切换界面语言并持久化。</summary>
    public void SetLanguage(L10nLang lang) => Current = lang;

    /// <summary>
    /// 按系统语言匹配界面语言；匹配不到回退英语。
    /// </summary>
    public static L10nLang DetectSystemLanguage()
    {
        foreach (var ci in CandidateCultures())
        {
            if (MatchCulture(ci) is { } lang)
                return lang;
        }
        return L10nLangs.Fallback;
    }

    /// <summary>取候选 CultureInfo：当前 UI 语言优先，再退到当前区域语言与不变文化。</summary>
    private static IEnumerable<CultureInfo> CandidateCultures()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ci in new[] { CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture })
        {
            if (ci is null || !seen.Add(ci.Name))
                continue;
            yield return ci;
            // 「zh-Hant-TW」这类要能退到「zh-Hant」，「zh-CN」退到「zh-Hans」
            if (ci.Parent is { } parent && !string.IsNullOrEmpty(parent.Name) && seen.Add(parent.Name))
                yield return parent;
        }
    }

    /// <summary>把一个 CultureInfo 映射到界面语言；不匹配返回 null。</summary>
    public static L10nLang? MatchCulture(CultureInfo? ci)
    {
        if (ci is null)
            return null;
        var name = ci.Name;
        if (string.IsNullOrEmpty(name))
            name = ci.TwoLetterISOLanguageName;

        // 中文需要细分简繁
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            if (name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
                || name.Contains("TW", StringComparison.OrdinalIgnoreCase)
                || name.Contains("HK", StringComparison.OrdinalIgnoreCase)
                || name.Contains("MO", StringComparison.OrdinalIgnoreCase))
                return L10nLang.ZhHant;
            return L10nLang.ZhHans;
        }

        var two = name.Length >= 2 ? name[..2] : name;

        // 先按完整标记匹配（区分 es 与 es-419 这类地区变体）
        foreach (var info in L10nLangs.All)
        {
            if (string.Equals(info.Code, name, StringComparison.OrdinalIgnoreCase))
                return info.Lang;
        }

        // 再按语言主码匹配
        foreach (var info in L10nLangs.All)
        {
            if (string.Equals(info.Code[..2], two, StringComparison.OrdinalIgnoreCase))
                return info.Lang;
        }
        return null;
    }

    /// <summary>持久化当前选择。</summary>
    private static void Persist(L10nLang lang)
    {
        try
        {
            Preferences.Default.Set(PrefKey, L10nLangs.Code(lang));
        }
        catch (Exception ex)
        {
            CrashLog.Write("L10n 持久化", ex);
        }
    }
}
