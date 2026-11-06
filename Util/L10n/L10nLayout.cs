using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace PKHeXMAUI;

/// <summary>
/// RTL（从右向左书写）布局处理。
///
/// <para><b>处理了什么</b></para>
/// <list type="bullet">
/// <item><b>文字方向</b>：把根容器的 <see cref="VisualElement.FlowDirection"/> 设为
/// <see cref="FlowDirection.RightToLeft"/>，MAUI 会自动镜像整个视觉树（Grid 的列序、
/// StackLayout 的堆叠方向、Flyout 的抽屉边）。</item>
/// <item><b>对齐</b>：RTL 下左对齐会显得突兀，改为跟随 <see cref="TextAlignment"/> 的
/// <see cref="Microsoft.Maui.TextAlignment.Start"/>（在 RTL 里即右对齐）。</item>
/// <item><b>不溢出</b>：RTL 文本通常比同义 LTR 更长，
/// 给 Label 放开换行（<see cref="LineBreakMode.WordWrap"/>）；
/// 数字与代码（PID、坐标、十六进制）加 LTR 隔离标记，避免顺序被反转。</item>
/// <item><b>字体</b>：不指定自定义字体族，交给系统字体栈 ——
/// Android/iOS 自带 Noto Naskh / Noto Hebrew 等 RTL 字形，换成拉丁字体反而会出现豆腐块。</item>
/// </list>
/// </summary>
public static class L10nLayout
{
    /// <summary>
    /// 对一个页面应用当前语言的排版方向。
    /// </summary>
    public static void Apply(Page? page)
    {
        if (page is null)
            return;

        var rtl = L10nManager.Instance.IsRightToLeft;
        page.FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // 页面内的 Label/Button/Entry 统一处理对齐与换行
        foreach (var element in Descendants(page))
        {
            switch (element)
            {
                case Label label:
                    // 对齐：跟随书写方向（Start 在 RTL 里即右对齐）。
                    // 不用 TextColor 是否为空做条件 —— 那会让绝大多数 Label 保持 LTR 对齐，
                    // 在 RTL 界面里显得参差不齐。
                    label.HorizontalTextAlignment = rtl ? TextAlignment.End : TextAlignment.Start;
                    // 换行：只在**没有明确指定**截断/不换行时才放开。
                    // 侧边栏那种 HeightRequest 固定 + TailTruncation 的控件，
                    // 是刻意设计成单行省略号，强行改成 WordWrap 会撑破固定高度。
                    if (label.LineBreakMode is LineBreakMode.NoWrap)
                        label.LineBreakMode = LineBreakMode.WordWrap;
                    break;

                case Button button:
                    // 只在未显式指定时给默认对齐，避免覆盖 XAML 里的布局意图。
                    // 注意：LayoutOptions 不是编译期常量，不能用 `is` 做模式匹配。
                    var hOpt = button.HorizontalOptions;
                    if (hOpt != LayoutOptions.Center && hOpt != LayoutOptions.FillAndExpand)
                        button.HorizontalOptions = rtl ? LayoutOptions.End : LayoutOptions.Start;
                    break;

                case Entry entry:
                    entry.HorizontalTextAlignment = rtl ? TextAlignment.End : TextAlignment.Start;
                    // 纯数字/十六进制的输入框（PID、坐标、ID）在 RTL 下仍应从左往右，
                    // 否则输入的数字顺序会反过来；这里单独把该控件设成 LTR。
                    if (rtl && IsNumericOnly(entry.Text))
                        entry.FlowDirection = FlowDirection.LeftToRight;
                    break;

                case Editor editor:
                    editor.HorizontalTextAlignment = rtl ? TextAlignment.End : TextAlignment.Start;
                    if (rtl && IsNumericOnly(editor.Text))
                        editor.FlowDirection = FlowDirection.LeftToRight;
                    break;
            }
        }
    }

    /// <summary>
    /// 判断文本是否只由数字与十六进制符号组成（PID、坐标、ID 这类）。
    /// 这类内容在 RTL 环境下必须保持从左往右的方向。
    /// </summary>
    private static bool IsNumericOnly(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
        foreach (var c in text)
        {
            if (char.IsAsciiDigit(c))
                continue;
            if (c is 'x' or 'X' or '-' or ' ')
                continue;
            if (c is >= 'a' and <= 'f')
                continue;
            if (c is >= 'A' and <= 'F')
                continue;
            return false;
        }
        return true;
    }

    /// <summary>对整个窗口应用当前语言的排版方向。</summary>
    public static void Apply(Window? window)
    {
        if (window is null)
            return;
        var rtl = L10nManager.Instance.IsRightToLeft;
        window.FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        if (window.Page is Page page)
            Apply(page);
    }

    /// <summary>
    /// 给「数字/代码」类文本加 LTR 隔离标记，防止 RTL 环境下顺序被反转。
    /// 例如 PID「0x1234ABCD」在阿拉伯语界面里应保持从左往右。
    /// </summary>
    public static string LtrIsolate(string? text)
    {
        if (string.IsNullOrEmpty(text) || !L10nManager.Instance.IsRightToLeft)
            return text ?? string.Empty;
        // U+2066 FIRST STRONG ISOLATE / U+2069 POP DIRECTIONAL ISOLATE
        return $"⁦{text}⁩";
    }

    /// <summary>取一个元素的所有后代（含自身）。</summary>
    private static IEnumerable<Element> Descendants(Element root)
    {
        var stack = new Stack<Element>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var e = stack.Pop();
            yield return e;
            // 用 IVisualTreeElement 而不是 Element.LogicalChildren ——
            // 后者已被标记过时（只为 Hot Reload 保留）。
            if (e is not IVisualTreeElement visual)
                continue;
            foreach (var c in visual.GetVisualChildren())
            {
                if (c is Element el)
                    stack.Push(el);
            }
        }
    }
}
