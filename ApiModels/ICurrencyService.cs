namespace ApiModels;

public interface ICurrencyService
{
    Task<RatesResponse> GetRatesAsync(DateOnly date);
    Task<List<Currency>> GetCurrenciesAsync();
}