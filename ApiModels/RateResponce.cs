namespace ApiModels;

public class RatesResponse
{
    public DateOnly RequestedDate { get; set; }
    public DateOnly ActualDate { get; set; }    
    public List<CurrencyRate> Rates { get; set; } = new();
}