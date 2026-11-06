using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class App : Application
{
	public App()
	{
        //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NGaF1cXGFCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWXZedXVTRmFfWEZxXkE=");
        InitializeComponent();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // 界面语言：优先读用户上次的选择；没有则按系统语言匹配；都取不到回退英语。
        // 必须在建窗口之前完成 —— 首屏文案一上来就是对的。
        L10nManager.Instance.Initialize();

        // PKHeX 全局字符串表跟随界面语言：物品/招式/特性/物种/地点/属性/性格/游戏名等
        // 目前 Core 内置 zh-Hans / zh-Hant / en / ja / fr / de / it / es / ko 资源包。
        ApplyCoreLanguage(L10nManager.Instance.Current);

        CrashLog.Install();
        var Version = Preferences.Default.Get("SaveFile", 50);
        Window window = new(PSettings.RememberLastSave
            ? new AppShell(BlankSaveFile.Get((GameVersion)Version, "PKHeX"))
            : (Page)new AppShell(BlankSaveFile.Get(GameVersion.SL, "PKHeX")));
        window.Destroying += (_, _) => CrashLog.Write("Lifecycle", null, "窗口关闭");
        window.Stopped += (_, _) => CrashLog.Write("Lifecycle", null, "窗口挂起");

        // RTL（阿拉伯语/希伯来语）布局：把书写方向应用到整棵视觉树
        L10nLayout.Apply(window);

        // 切换界面语言后：刷新所有已渲染文案 + 重新应用排版方向
        L10nManager.LanguageChanged += _ =>
        {
            try
            {
                window.Dispatcher.Dispatch(() =>
                {
                    // Core 的字符串表（物种名/道具名/招式名/性格/游戏版本名等）也要跟着换，
                    // 否则切了语言这些文字还是旧的。
                    ApplyCoreLanguage(L10nManager.Instance.Current);
                    L10nLayout.Apply(window);
                    L10nLayout.Apply(window.Page);
                });
            }
            catch (Exception ex)
            {
                CrashLog.Write("L10n 语言切换", ex);
            }
        };

        // 自检模式：启动 4 秒后自动跑一遍侧边栏展开/模板选择/设置页构建，结果写入 debug.log
        if (Environment.GetEnvironmentVariable("PKHEXMAUI_SELFTEST") == "1" && window.Page is AppShell shell)
        {
            shell.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(4), async () =>
            {
                try { CrashLog.Write("SelfTest", null, "\n" + await shell.SelfTest()); }
                catch (Exception ex) { CrashLog.Write("SelfTest", ex, "自检本身抛出异常"); }
            });
        }

        // 侧边栏诊断：PKHEXMAUI_SELFTEST=flyout → 打开侧边栏并转储其中实际渲染出来的文字
        if (Environment.GetEnvironmentVariable("PKHEXMAUI_SELFTEST") == "flyout" && window.Page is AppShell flyoutShell)
        {
            flyoutShell.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(3), async () =>
            {
                try
                {
                    flyoutShell.FlyoutIsPresented = true;
                    await Task.Delay(1200);
                    var texts = new List<string>();
                    var types = new List<string>();
                    foreach (var e in flyoutShell.GetVisualTreeDescendants())
                    {
                        types.Add(e.GetType().Name);
                        switch (e)
                        {
                            case Label { Text.Length: > 0 } l:
                                texts.Add("Label:" + l.Text);
                                break;
                            case Button { Text.Length: > 0 } b:
                                texts.Add("Button:" + b.Text);
                                break;
                            case ImageButton { Source: not null } ib:
                                texts.Add("ImageButton:" + ib.Source);
                                break;
                        }
                    }
                    CrashLog.Write("FlyoutTree", null,
                        $"侧边栏已呈现={flyoutShell.FlyoutIsPresented}，渲染出的控件 {texts.Count} 个：\n  " +
                        string.Join("\n  ", texts.Distinct()));
                    CrashLog.Write("FlyoutTree", null,
                        "控件类型统计：\n  " + string.Join("\n  ",
                            types.GroupBy(t => t).Select(g => $"{g.Key} x{g.Count()}").OrderByDescending(x => x).Take(40)));
                }
                catch (Exception ex)
                {
                    CrashLog.Write("FlyoutTree", ex, "侧边栏诊断失败");
                }
            });
        }

#if WINDOWS
        // 平台层诊断：MAUI 在桌面端到底渲染了什么（有没有汉堡按钮 / 标签栏）
        if (Environment.GetEnvironmentVariable("PKHEXMAUI_SELFTEST") == "platform")
        {
            window.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(6), () =>
            {
                try
                {
                    var platformWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                    var root = platformWindow?.Content as Microsoft.UI.Xaml.FrameworkElement;
                    if (root is null)
                    {
                        CrashLog.Write("Platform", null, "拿不到窗口根元素");
                        return;
                    }
                    var lines = new List<string>();
                    void Walk(Microsoft.UI.Xaml.FrameworkElement el, int depth)
                    {
                        if (depth > 6 || lines.Count > 120)
                            return;
                        var extra = "";
                        if (el is Microsoft.UI.Xaml.Controls.Button b)
                            extra = $" 内容=[{b.Content}]";
                        else if (el is Microsoft.UI.Xaml.Controls.TextBlock tb)
                            extra = $" 文本=[{tb.Text}]";
                        lines.Add($"{new string(' ', depth * 2)}{el.GetType().Name}{extra}");
                        int n = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(el);
                        for (int i = 0; i < n; i++)
                        {
                            if (Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(el, i) is Microsoft.UI.Xaml.FrameworkElement child)
                                Walk(child, depth + 1);
                        }
                    }
                    Walk(root, 0);
                    CrashLog.Write("Platform", null, "平台视觉树：\n" + string.Join("\n", lines));
                }
                catch (Exception ex)
                {
                    CrashLog.Write("Platform", ex, "平台诊断失败");
                }
            });
        }
#endif
        window.Resumed += (s, e) =>
        {
            if (LiveHex.Reconnect)
            {
                if (!Remote.Connected)
                {
                    Remote.com.Connect();
                }
                else
                {
                    Remote.com.Disconnect();
                    Remote.com.Connect();
                }
            }
        };
        return window;
    }

    /// <summary>
    /// 把 PKHeX Core 的全局字符串表切到指定界面语言。
    /// <para>
    /// Core 的 <c>GameInfo.Strings</c> 提供物种名、道具名、招式名、特性名、
    /// 性格名、属性名、游戏版本名等 —— 这些是界面上最常见的文字，
    /// 必须与界面语言保持一致。
    /// </para>
    /// <para>
    /// 切换语言后要重新赋值：Core 内部各处是「每次读取时查当前 Strings」，
    /// 所以重新赋值即刻生效（已实测确认）。
    /// </para>
    /// </summary>
    private static void ApplyCoreLanguage(L10nLang lang)
    {
        var coreLang = lang switch
        {
            L10nLang.ZhHans => "zh-Hans",
            L10nLang.ZhHant => "zh-Hant",
            L10nLang.English => "en",
            L10nLang.Japanese => "ja",
            L10nLang.French => "fr",
            L10nLang.German => "de",
            L10nLang.Italian => "it",
            L10nLang.Spanish => "es",
            L10nLang.SpanishLatam => "es-419",
            L10nLang.Korean => "ko",
            _ => "en",
        };
        GameInfo.Strings = GameInfo.GetStrings(coreLang);
        GameInfo.CurrentLanguage = coreLang;
    }
}
