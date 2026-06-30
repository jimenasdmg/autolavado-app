using AutoLavadoApp.ViewModels.Cliente;

namespace AutoLavadoApp.Views.Cliente;

public partial class ConfiguracionPage : ContentPage
{
    private readonly MiCuentaViewModel _viewModel;

    public ConfiguracionPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<MiCuentaViewModel>()
            ?? throw new InvalidOperationException("MiCuentaViewModel no está registrado en DI.");

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
