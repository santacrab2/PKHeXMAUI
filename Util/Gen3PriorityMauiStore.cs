namespace PKHeXMAUI;

/// <summary>
/// <see cref="Gen3Priority"/> 的 MAUI 设置存储实现。
///
/// 单独成文件，是为了让 <c>Gen3PidFixer.cs</c> 里的优先级核心逻辑
/// 不直接引用 MAUI 类型 —— 那样它就能脱离 MAUI 独立编译与验证。
/// </summary>
public sealed class Gen3PriorityMauiStore : Gen3Priority.IOrderStore
{
    /// <inheritdoc/>
    public string Get(string key, string defaultValue) => Preferences.Default.Get(key, defaultValue);

    /// <inheritdoc/>
    public void Set(string key, string value) => Preferences.Default.Set(key, value);
}
