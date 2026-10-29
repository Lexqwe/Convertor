using System.Text.Json;
using ApiModels;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Services;

public class CbrService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public CbrService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<RatesResponse> GetRatesAsync(DateOnly requestedDate)
    {
        string cacheKey = $"rates_{requestedDate:yyyy-MM-dd}";

        if (_cache.TryGetValue(cacheKey, out RatesResponse? cachedResult))
        {
            return cachedResult!;
        }

        DateOnly currentDate = requestedDate;
        CbrResponse? cbrData = null;

        for (int i = 0; i < 7; i++)
        {
            cbrData = await FetchFromCbrAsync(currentDate);
            if (cbrData != null && cbrData.Valute.Count > 0)
            {
                break;
            }
            currentDate = currentDate.AddDays(-1);
        }

        if (cbrData == null || cbrData.Valute.Count == 0)
        {
            throw new Exception($"Не удалось найти курсы валют за {requestedDate:yyyy-MM-dd} (и за 7 дней до неё).");
        }

        var rates = new List<CurrencyRate>();

        rates.Add(new CurrencyRate { Code = "RUB", Value = 1.0m });

        var currencies = new List<Currency> { new() { Code = "RUB", Name = "Российский рубль" } };

        foreach (var valute in cbrData.Valute.Values)
        {
            decimal valuePerUnit = valute.Value / valute.Nominal;
            rates.Add(new CurrencyRate
            {
                Code = valute.CharCode,
                Value = valuePerUnit
            });
            currencies.Add(new Currency
            {
                Code = valute.CharCode,
                Name = valute.Name
            });
        }

        var result = new RatesResponse
        {
            RequestedDate = requestedDate,
            ActualDate = currentDate,
            Rates = rates
        };

      
        _cache.Set(cacheKey, result, TimeSpan.FromHours(12));
        _cache.Set("currencies", currencies, TimeSpan.FromHours(12));

        return result;
    }

    public async Task<List<Currency>> GetCurrenciesAsync()
    {
        if (_cache.TryGetValue("currencies", out List<Currency>? cachedCurrencies))
        {
            return cachedCurrencies!;
        }

        await GetRatesAsync(DateOnly.FromDateTime(DateTime.Today));
        return _cache.Get<List<Currency>>("currencies")!;
    }

    private async Task<CbrResponse?> FetchFromCbrAsync(DateOnly date)
    {
        try
        {
            string url = date == DateOnly.FromDateTime(DateTime.Today)
                ? "https://www.cbr-xml-daily.ru/daily_json.js"
                : $"https://www.cbr-xml-daily.ru/archive/{date:yyyy}/{date:MM}/{date:dd}/daily_json.js";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CbrResponse>(json);
        }
        catch
        {
            return null;
        }
    }
}