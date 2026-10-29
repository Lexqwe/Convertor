using System.Net.Http.Json;
using ApiModels;

namespace Http;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RatesResponse> GetRatesAsync(DateOnly date)
    {
        var url = $"/api/rates?date={date:yyyy-MM-dd}";
        var response = await _httpClient.GetFromJsonAsync<RatesResponse>(url);
        return response ?? throw new Exception("Сервер вернул пустой ответ на запрос курсов.");
    }

    public async Task<List<Currency>> GetCurrenciesAsync()
    {
        var url = "/api/currencies";
        var response = await _httpClient.GetFromJsonAsync<List<Currency>>(url);
        return response ?? throw new Exception("Сервер вернул пустой ответ на запрос валют.");
    }
}