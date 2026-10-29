using ApiModels;
using Moq;
using ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ViewModels.Tests;

[TestClass]
public class CurrencyViewModelTests
{
    private Mock<ICurrencyService> _mockService = null!;
    private CurrencyViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockService = new Mock<ICurrencyService>();
        _mockService.Setup(s => s.GetCurrenciesAsync()).ReturnsAsync(new List<Currency>
        {
            new() { Code = "RUB", Name = "Российский рубль" },
            new() { Code = "USD", Name = "Доллар США" },
            new() { Code = "EUR", Name = "Евро" }
        });

        _mockService.Setup(s => s.GetRatesAsync(It.IsAny<DateOnly>())).ReturnsAsync(new RatesResponse
        {
            RequestedDate = new DateOnly(2026, 10, 28),
            ActualDate = new DateOnly(2026, 10, 28),
            Rates = new List<CurrencyRate>
            {
                new() { Code = "RUB", Value = 1m },
                new() { Code = "USD", Value = 80m },
                new() { Code = "EUR", Value = 90m }
            }
        });

        _viewModel = new CurrencyViewModel(_mockService.Object);
    }

    [TestMethod]
    public async Task LoadData_ShouldFillCurrencies_AndSetDefaults()
    {
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
        Assert.AreEqual(3, _viewModel.AvailableCurrencies.Count, "Список валют должен заполниться.");
        Assert.AreEqual("USD", _viewModel.SelectedFromCurrency?.Code, "По умолчанию должна выбраться валюта USD (Из).");
        Assert.AreEqual("RUB", _viewModel.SelectedToCurrency?.Code, "По умолчанию должна выбраться валюта RUB (В).");
    }

    [TestMethod]
    public async Task RecalculateFrom_ShouldCalculateCorrectly()
    {
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
        _viewModel.FromAmount = 10;
        Assert.AreEqual(800m, _viewModel.ToAmount, "10 USD должно равняться 800 RUB.");
    }

    [TestMethod]
    public async Task RecalculateTo_ShouldCalculateCorrectly()
    {
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
        _viewModel.ToAmount = 900; 
        Assert.AreEqual(11.25m, _viewModel.FromAmount, "900 RUB должно равняться 11.25 USD.");
    }

    [TestMethod]
    public async Task ChangeCurrency_ShouldRecalculate()
    {
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
        _viewModel.FromAmount = 10;
        _viewModel.SelectedFromCurrency = _viewModel.AvailableCurrencies.First(c => c.Code == "EUR");
        Assert.AreEqual(900m, _viewModel.ToAmount, "После смены валюты на EUR сумма должна пересчитаться.");
    }

    [TestMethod]
    public async Task LoadRates_WhenActualDateDiffers_ShouldShowHint()
    {
        _mockService.Setup(s => s.GetRatesAsync(It.IsAny<DateOnly>())).ReturnsAsync(new RatesResponse
        {
            RequestedDate = new DateOnly(2026, 10, 28), 
            ActualDate = new DateOnly(2026, 10, 27), 
            Rates = new List<CurrencyRate> { new() { Code = "RUB", Value = 1m } }
        });
        await _viewModel.LoadRatesCommand.ExecuteAsync(null);
        StringAssert.Contains(_viewModel.DateHint, "ближайший к", "Подсказка должна содержать информацию о ближайшей дате.");
    }
}
