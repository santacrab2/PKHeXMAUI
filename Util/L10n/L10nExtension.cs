using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace PKHeXMAUI;

/// <summary>
/// XAML 用的本地化标记扩展：<c>{l10n:Tr Key=Some_Key}</c>。
///
/// <para><b>为什么返回 Binding 而不是字符串</b></para>
/// 返回静态字符串的话，切换语言后已渲染的界面不会刷新。
/// 返回 <see cref="Binding"/>（路径 <c>[Key]</c>、源为 <see cref="L10nManager"/> 单例），
/// 就能跟随 <c>PropertyChanged("")</c> 即时刷新（实测有效）。
///
/// <para><b>键名限制</b></para>
/// MAUI 的索引器路径解析不允许点号，键名只能用字母/数字/下划线/连字符。
///
/// <para><b>为什么加 <see cref="AcceptEmptyServiceProviderAttribute"/></b></para>
/// 本扩展不需要任何 DI 服务，但 XAML 编译器不知道这一点，
/// 每次使用都会报一条 XC0103 警告。声明该特性可让编译器跳过检查。
/// </summary>
[AcceptEmptyServiceProvider]
[ContentProperty(nameof(Key))]
public sealed class TrExtension : IMarkupExtension
{
    /// <summary>资源键。</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>可选：格式化的参数（用 <c>String.Format</c> 套到译文上）。</summary>
    public object?[]? Args { get; set; }

    /// <inheritdoc/>
    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var key = Key;
        var args = Args;
        if (args is null || args.Length == 0)
            return new Binding($"[{key}]", BindingMode.OneWay, source: L10nManager.Instance);

        // 带参数时用转换器，避免在 XAML 里写 StringFormat 的转义
        return new Binding($"[{key}]", BindingMode.OneWay, source: L10nManager.Instance)
        {
            Converter = new TrArgsConverter(args),
        };
    }

    /// <summary>把译文按参数格式化。</summary>
    private sealed class TrArgsConverter : IValueConverter
    {
        private readonly object?[] _args;

        public TrArgsConverter(object?[] args) => _args = args;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string s || s.Length == 0)
                return value ?? string.Empty;
            try
            {
                return string.Format(culture, s, _args);
            }
            catch (FormatException)
            {
                // 参数个数对不上时原样返回译文，不要抛异常把界面搞崩
                return s;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

/// <summary>
/// 代码里用的便捷入口：<c>L10n.T("Some_Key")</c>。
/// </summary>
public static class L10n
{
    /// <summary>取文案。</summary>
    public static string T(string key) => L10nManager.Get(key);

    /// <summary>取文案并格式化。</summary>
    public static string T(string key, params object?[] args)
    {
        var s = L10nManager.Get(key);
        if (args.Length == 0)
            return s;
        try
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, s, args);
        }
        catch (FormatException)
        {
            return s;
        }
    }
}
