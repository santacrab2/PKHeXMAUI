using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

namespace pk9reader;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        builder.UseMauiApp<App>().UseMauiCommunityToolkitCore();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
