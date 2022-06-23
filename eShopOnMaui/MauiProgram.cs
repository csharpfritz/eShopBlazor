using eShopLib;

namespace eShopOnMaui;

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
			});

		builder.Services.AddMauiBlazorWebView();
#if DEBUG
	builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		builder.Services.AddTransient<ICatalogService, Services.CatalogService>();

		return builder.Build();
	}
}