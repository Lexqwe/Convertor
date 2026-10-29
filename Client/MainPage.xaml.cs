using ViewModels;

namespace Client;

public partial class MainPage : ContentPage
{
    private readonly CurrencyViewModel _viewModel;

    public MainPage(CurrencyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel; 
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}