using ApiModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ViewModels;

public class CurrencyViewModel : ObservableObject
{
    private readonly ICurrencyService _service;
    private Dictionary<string, decimal> _rates = new();
    private bool _isUpdating;
    public AsyncRelayCommand LoadDataCommand { get; }
    public AsyncRelayCommand LoadRatesCommand { get; }
    public CurrencyViewModel(ICurrencyService service)
    {
        _service = service;
        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        LoadRatesCommand = new AsyncRelayCommand(LoadRatesAsync);
    }

    private List<Currency> _availableCurrencies = new(); 
    public List<Currency> AvailableCurrencies
    {
        get => _availableCurrencies;
        set => SetProperty(ref _availableCurrencies, value);
    }

    private string _dateHint = "Загрузка...";
    public string DateHint
    {
        get => _dateHint;
        set => SetProperty(ref _dateHint, value);
    }

    private DateTime _selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadRatesAsync();
            }
        }
    }

    private Currency? _selectedFromCurrency;
    public Currency? SelectedFromCurrency
    {
        get => _selectedFromCurrency;
        set
        {
            if(SetProperty(ref _selectedFromCurrency, value))
            {
                if (!_isUpdating)
                {
                    RecalculateFrom();
                }
            }
        }
    }

    private Currency? _selectedToCurrency;
    public Currency? SelectedToCurrency
    {
        get => _selectedToCurrency;
        set
        {
            if (SetProperty(ref _selectedToCurrency, value))
            {
                if (!_isUpdating)
                {
                    RecalculateTo();
                }
            }
        }
    }


    private decimal _fromAmount;
    public decimal FromAmount
    {
        get => _fromAmount;
        set
        {
            if (SetProperty(ref _fromAmount, value))
            {
                if (!_isUpdating)
                {
                    RecalculateFrom();
                }
            }
        }
    }

    private decimal _toAmount;
    public decimal ToAmount
    {
        get => _toAmount;
        set
        {
            if (SetProperty(ref _toAmount, value))
            {
                if (!_isUpdating)
                {
                    RecalculateTo();
                }
            }
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var currencies = await _service.GetCurrenciesAsync();
            AvailableCurrencies = currencies;

            SelectedFromCurrency = currencies.FirstOrDefault(c => c.Code == "USD");
            SelectedToCurrency = currencies.FirstOrDefault(c => c.Code == "RUB");

            await LoadRatesAsync();
        }
        catch (Exception ex)
        {
            DateHint = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadRatesAsync()
    {
        try
        {
            var dateOnly = DateOnly.FromDateTime(SelectedDate);
            var response = await _service.GetRatesAsync(dateOnly);

            _rates = response.Rates.ToDictionary(r => r.Code, r => r.Value);

            if (response.RequestedDate != response.ActualDate) 
            {
                DateHint = $"Курс на {response.ActualDate:dd.MM.yyyy} (ближайший к {response.RequestedDate:dd.MM.yyyy})";
            }
            else
            {
                DateHint = $"Курс на {response.ActualDate:dd.MM.yyyy}";
            }

            RecalculateFrom();
        }
        catch (Exception ex)
        {
            DateHint = $"Ошибка загрузки: {ex.Message}";
        }
    }


    private void RecalculateFrom()
    {
        if (SelectedFromCurrency == null || SelectedToCurrency == null)
        {
            return;
        }
        if (!_rates.ContainsKey(SelectedFromCurrency.Code) || !_rates.ContainsKey(SelectedToCurrency.Code))
        {
            return;
        }

        _isUpdating = true; 

        decimal rateFrom = _rates[SelectedFromCurrency.Code];
        decimal rateTo = _rates[SelectedToCurrency.Code];

        ToAmount = (FromAmount * rateFrom) / rateTo;

        _isUpdating = false;
    }

    private void RecalculateTo()
    {
        if (SelectedFromCurrency == null || SelectedToCurrency == null)
        {
            return;
        }
        if (!_rates.ContainsKey(SelectedFromCurrency.Code) || !_rates.ContainsKey(SelectedToCurrency.Code))
        {
            return;
        }

        _isUpdating = true;

        decimal rateFrom = _rates[SelectedFromCurrency.Code];
        decimal rateTo = _rates[SelectedToCurrency.Code];
        FromAmount = (ToAmount * rateTo) / rateFrom;

        _isUpdating = false;
    }

}

