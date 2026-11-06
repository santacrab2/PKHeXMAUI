using System.Text;

namespace PKHeXMAUI;

/// <summary>
/// 轻量崩溃/退出诊断日志 + 全局异常兜底。
///
/// 背景：MAUI 里的 async void 事件处理函数一旦抛出未捕获异常，进程会直接终止，
/// 表现为“窗口自己关闭”。本项目有 60 处 async void 事件，逐个加 try/catch 不现实，
/// 这里统一在平台层拦截：记录到 debug.log、弹出中文提示、标记为已处理，程序继续运行。
/// </summary>
public static class CrashLog
{
    private static bool _installed;
    private static readonly object Gate = new();

    /// <summary>日志路径：%LOCALAPPDATA%\PKHeXMAUI\debug.log（Android 为应用私有目录）。</summary>
    public static string LogPath
    {
        get
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PKHeXMAUI");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "debug.log");
        }
    }

    public static void Install()
    {
        if (_installed)
            return;
        _installed = true;

        // 致命异常：只能记录，进程仍会结束
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Write("AppDomain", e.ExceptionObject as Exception, e.IsTerminating ? L10n.T("code_text") : null);

        // 被遗忘的 Task 异常：标记已观察，避免升级成崩溃
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Write("TaskScheduler", e.Exception);
            e.SetObserved();
        };

#if WINDOWS
        // WinUI 层的未处理异常（async void 绝大多数会走到这里）
        if (Microsoft.UI.Xaml.Application.Current is { } winApp)
        {
            winApp.UnhandledException += (_, e) =>
            {
                Write("WinUI", e.Exception, L10n.T("code_unhandled_exception"));
                ShowAlert(e.Exception);
                e.Handled = true;
            };
        }
#endif

#if ANDROID
        // Android 运行时层的未处理异常
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
        {
            Write("Android", e.Exception, L10n.T("code_unhandled_exception"));
            ShowAlert(e.Exception);
            e.Handled = true;
        };
#endif

        Write("Lifecycle", null, L10n.T("code_app_start"));
    }

    /// <summary>尽力弹一个中文提示；失败就只留日志。</summary>
    private static void ShowAlert(Exception? ex)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (Shell.Current is { } shell)
                        await shell.DisplayAlert(L10n.T("code_something_went_wrong"), Loc.ExceptionMessage(ex), L10n.T("pktab_ok"));
                }
                catch
                {
                    // 提示失败也不能再抛
                }
            });
        }
        catch
        {
            // 忽略
        }
    }

    public static void Write(string tag, Exception? ex, string? note = null)
    {
        try
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(" [").Append(tag).Append(']');
            if (!string.IsNullOrEmpty(note))
                sb.Append(' ').Append(note);
            if (ex is not null)
                sb.AppendLine().Append(ex);
            sb.AppendLine();
            lock (Gate)
                File.AppendAllText(LogPath, sb.ToString());
        }
        catch
        {
            // 诊断代码自身绝不能再抛异常
        }
    }
}
