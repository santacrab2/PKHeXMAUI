using System.Reflection;
using System.Text.Json;

namespace PKHeXMAUI;

/// <summary>
/// 资源表：key → 各语言文案。
///
/// <para><b>存储形式</b></para>
/// 每种语言一个 JSON 文件（<c>Resources/L10n/&lt;code&gt;.json</c>），
/// 以嵌入资源（<c>EmbeddedResource</c>）打进程序集，因此不依赖文件系统路径，
/// 在 Android / iOS / Windows 上行为一致，且无需额外的打包配置。
///
/// <para><b>回退链</b></para>
/// <see cref="Get"/> 的查找顺序：当前语言 → 英文 → key 本身（最后兜底，避免界面出现空白）。
/// </summary>
public static class L10nCatalog
{
    /// <summary>资源文件名前缀（同时用作嵌入资源的命名空间）。</summary>
    private const string ResourcePrefix = "L10n.";

    /// <summary>英文资源（回退基准）。</summary>
    public static readonly IReadOnlyDictionary<string, string> English = Load("en");

    private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> Cache = new();

    /// <summary>
    /// 取文案。找不到时按「当前语言 → 英文 → key」逐级回退。
    /// </summary>
    /// <param name="key">资源键。</param>
    /// <param name="lang">当前界面语言。</param>
    public static string Get(string key, L10nLang lang)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        var table = Table(lang);
        if (table.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v))
            return v;

        // 回退到英文
        if (!ReferenceEquals(table, English) && English.TryGetValue(key, out var en) && !string.IsNullOrEmpty(en))
            return en;

        // 英文也没有 → 返回 key，避免界面出现空白（至少能看出是哪个键缺了）
        return key;
    }

    /// <summary>某语言的完整资源表（带英文兜底）。</summary>
    public static IReadOnlyDictionary<string, string> Table(L10nLang lang)
    {
        var code = L10nLangs.Code(lang);
        if (Cache.TryGetValue(code, out var cached))
            return cached;

        var table = code == "en" ? English : Load(code);
        Cache[code] = table;
        return table;
    }

    /// <summary>读入某语言的资源文件；文件不存在时返回空表。</summary>
    private static IReadOnlyDictionary<string, string> Load(string code)
    {
        try
        {
            var asm = typeof(L10nCatalog).Assembly;
            var name = ResourcePrefix + code + ".json";
            using var stream = asm.GetManifestResourceStream(name);
            if (stream is null)
                return new Dictionary<string, string>();

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();

            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dict ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            CrashLog.Write("L10n 资源加载 " + code, ex);
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// 校验资源表完整性：列出在英文表里缺失、或在当前语言里缺失的键。
    /// 供开发期自检使用。
    /// </summary>
    public static (string[] MissingEnglish, string[] MissingInLang) Validate(L10nLang lang)
    {
        var table = Table(lang);
        var missingEn = new List<string>();
        var missingLang = new List<string>();
        foreach (var key in English.Keys)
        {
            if (!table.TryGetValue(key, out var v) || string.IsNullOrEmpty(v))
                missingLang.Add(key);
        }
        foreach (var key in table.Keys)
        {
            if (!English.ContainsKey(key))
                missingEn.Add(key);
        }
        missingEn.Sort(StringComparer.Ordinal);
        missingLang.Sort(StringComparer.Ordinal);
        return (missingEn.ToArray(), missingLang.ToArray());
    }

    /// <summary>全部嵌入资源的名称（自检用）。</summary>
    public static string[] EmbeddedNames()
        => typeof(L10nCatalog).Assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .ToArray();
}
