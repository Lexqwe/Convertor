using Microsoft.Extensions.Logging;
using ViewModels;
using Http;
using ApiModels;


namespace Client;

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

        builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5123");
        });

        builder.Services.AddTransient<CurrencyViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
