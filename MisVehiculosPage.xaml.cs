using AutoLavadoApp.ViewModels;
using AutoLavadoApp.ViewModels.Cliente;

namespace AutoLavadoApp.Views.Cliente;

public partial class MisVehiculosPage : ContentPage
{
    private readonly MisVehiculosViewModel _viewModel;

    public MisVehiculosPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<MisVehiculosViewModel>()
            ?? throw new InvalidOperationException("MisVehiculosViewModel no está registrado en DI.");

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }
}
