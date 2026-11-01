using Api.Services;
using Scalar.AspNetCore;
using Serilog;


Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddMemoryCache();

    builder.Services.AddHttpClient<CbrService>();
    builder.Services.AddScoped<CbrService>();

    builder.Services.AddOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseSerilogRequestLogging();

    app.MapGet("/api/currencies", async (CbrService service) =>
    {
        var currencies = await service.GetCurrenciesAsync();
        return Results.Ok(currencies);
    });

    app.MapGet("/api/rates", async (DateOnly? date, CbrService service) =>
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        try
        {
            var rates = await service.GetRatesAsync(targetDate);
            return Results.Ok(rates);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ошибка при запуске");
}
finally
{
    Log.CloseAndFlush();
}

