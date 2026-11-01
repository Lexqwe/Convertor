using ApiModels;
using Mono.Cecil.Cil;
using Moq;
using System.Net.Http.Headers;
using System.Xml.Linq;


namespace ViewModels.Tests;

[TestClass]
public class CurrencyViewModelTests
{
    private Mock<ICurrencyService> _mockService = null!;
    private CurrencyViewModel _viewModel = null!;

    private List<CurrencyRate> _rates = new List<CurrencyRate>
            {
                new () { Code = "RUB", Value = 1m },
                new() { Code = "USD", Value = 80m },
                new() { Code = "EUR", Value = 90m }
            };
    private List<Currency> _availableCurrencies = new List<Currency>
        {
            new() { Code = "RUB", Name = "Российский рубль" },
            new() { Code = "USD", Name = "Доллар США" },
            new() { Code = "EUR", Name = "Евро" }
        };

    [TestInitialize]
    public async Task Setup()
    {

        _mockService = new Mock<ICurrencyService>();
        _mockService.Setup(s => s.GetCurrenciesAsync()).ReturnsAsync(_availableCurrencies);

        _mockService.Setup(s => s.GetRatesAsync(It.IsAny<DateOnly>())).ReturnsAsync(new RatesResponse
        {
            ActualDate = new DateOnly(2025, 8, 15),
            RequestedDate = new DateOnly(2025, 8, 15),
            Rates = _rates
        });

        _viewModel = new CurrencyViewModel(_mockService.Object);
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }

    [TestMethod]
    public void LoadData_ShouldFillCurrencies_AndSetDefaults()
    {
        Assert.AreEqual(3, _viewModel.AvailableCurrencies.Count);
        Assert.AreEqual("USD", _viewModel.SelectedFromCurrency?.Code);
        Assert.AreEqual("RUB", _viewModel.SelectedToCurrency?.Code);
    }

    [TestMethod]
    public void RecalculateFrom_ShouldCalculateCorrectly()
    {
        _viewModel.FromAmount = 10;
        Assert.AreEqual(800m, _viewModel.ToAmount);
    }

    [TestMethod]
    public void RecalculateTo_ShouldCalculateCorrectly()
    {
        _viewModel.ToAmount = 800;
        Assert.AreEqual(10m, _viewModel.FromAmount);
    }

    [TestMethod]
    public void ChangeFromCurrency_ShouldRecalculate()
    {
        _viewModel.FromAmount = 10;
        _viewModel.SelectedFromCurrency = _viewModel.AvailableCurrencies.First(c => c.Code == "EUR");
        Assert.AreEqual(900m, _viewModel.ToAmount);
    }

    [TestMethod]
    public void ChangeToCurrency_ShouldRecalculate()
    {
        _viewModel.ToAmount = 10;
        _viewModel.SelectedToCurrency = _viewModel.AvailableCurrencies.First(c => c.Code == "EUR");
        Assert.AreEqual(11.25m, _viewModel.FromAmount);
    }


    [TestMethod]
    public void EnterZeroAmount_ShouldCalculateZero()
    {
        _viewModel.FromAmount = 0;
        Assert.AreEqual(0m, _viewModel.ToAmount);
    }

    [TestMethod]
    public void Convert_SameCurrencies_ShouldBeEqual()
    {
        _viewModel.SelectedFromCurrency = _viewModel.AvailableCurrencies.First(c => c.Code == "RUB");
        _viewModel.SelectedToCurrency = _viewModel.AvailableCurrencies.First(c => c.Code == "RUB");
        _viewModel.FromAmount = 100;
        Assert.AreEqual(100m, _viewModel.ToAmount);
    }

    [TestMethod]
    public void LoadRates_WhenDatesMatch_ShouldShowSimpleHint()
    {
        Assert.AreEqual("Курс на 15.08.2025", _viewModel.DateHint);
    }

    [TestMethod]
    public async Task LoadRates_WhenActualDateDiffers_ShouldShowHint()
    {
        _mockService.Setup(s => s.GetRatesAsync(It.IsAny<DateOnly>())).ReturnsAsync(new RatesResponse
        {
            ActualDate = new DateOnly(2025, 8, 14),
            RequestedDate = new DateOnly(2025, 8, 15),
            Rates = _rates
        });
        await _viewModel.LoadRatesCommand.ExecuteAsync(null);
        StringAssert.Contains(_viewModel.DateHint, "ближайший к");
        StringAssert.Contains(_viewModel.DateHint, "14.08.2025");
    }

    [TestMethod]
    public void ChangeDate_ShouldTriggerRatesLoad()
    {
        var oldHint = _viewModel.DateHint;
        _viewModel.SelectedDate = new DateTime(2026, 10, 28);
        _mockService.Verify(s => s.GetRatesAsync(It.IsAny<DateOnly>()), Times.AtLeast(2));
    }

    [TestMethod]
    public async Task LoadRates_WhenNotFoundForWeek_ShouldShowErrorHint()
    {
        _mockService.Setup(s => s.GetRatesAsync(It.IsAny<DateOnly>())).ThrowsAsync(new Exception("Не удалось найти курсы валют за 2026-10-28 (и за 7 дней до неё)."));
        await _viewModel.LoadRatesCommand.ExecuteAsync(null);
        StringAssert.Contains(_viewModel.DateHint, "Не удалось найти курсы");
    }

    [TestMethod]
    public async Task LoadData_WhenServiceThrows_ShouldShowErrorHint()
    {
        _mockService.Setup(s => s.GetCurrenciesAsync()).ThrowsAsync(new Exception("Ошибка загрузки валют"));
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
        StringAssert.Contains(_viewModel.DateHint, "Ошибка загрузки валют");
    }
}