namespace PKHeXMAUI;

/// <summary>
/// 运行时判断某个图片资源是否真的打包进了 APK。
///
/// MAUI 的 Image.Source 用字符串指定图片名时，如果资源不存在，Android 端
/// 只会「什么都不画」（不会抛异常），表现出来就是图标位置一片空白。
/// 这里统一在拼图片名的地方做一次存在性检查，找不到就换成占位图，
/// 避免用户看到「缺图标」却不知道是什么东西。
/// </summary>
public static class ResUtil
{
    private static readonly Dictionary<string, bool> Cache = [];

    /// <summary>图片资源是否存在。</summary>
    public static bool ImageExists(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
        var key = fileName.ToLowerInvariant();
        if (Cache.TryGetValue(key, out var cached))
            return cached;

        bool exists;
#if ANDROID
        try
        {
            var ctx = Android.App.Application.Context;
            var res = ctx?.Resources;
            var pkg = ctx?.PackageName;
            if (res is null || string.IsNullOrEmpty(pkg))
                exists = true;
            else
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                exists = res.GetIdentifier(name, "drawable", pkg) != 0;
            }
        }
        catch
        {
            // 判断不了就当作存在，避免误把正常图标换成占位图
            exists = true;
        }
#else
        // 非 Android 平台（Windows 调试）不做检查，直接认为存在
        exists = true;
#endif
        Cache[key] = exists;
        return exists;
    }

    /// <summary>
    /// 返回可用的图片名：优先用 <paramref name="preferred"/>，
    /// 资源不存在时退回 <paramref name="fallback"/>，两个都没有就返回空串。
    /// </summary>
    public static string Pick(string? preferred, string? fallback = null)
    {
        if (!string.IsNullOrEmpty(preferred) && ImageExists(preferred))
            return preferred;
        if (!string.IsNullOrEmpty(fallback) && ImageExists(fallback))
            return fallback;
        return "";
    }

    /// <summary>
    /// 按顺序返回第一个真实存在的图片名；全都不存在时返回空串。
    /// </summary>
    public static string PickAny(params string?[] candidates)
    {
        foreach (var c in candidates)
        {
            if (!string.IsNullOrEmpty(c) && ImageExists(c))
                return c;
        }
        return "";
    }
}
