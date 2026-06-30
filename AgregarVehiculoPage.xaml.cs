using AutoLavadoApp.ViewModels;

namespace AutoLavadoApp.Views.Cliente;

public partial class AgregarVehiculoPage : ContentPage
{
    private readonly AgregarVehiculoViewModel _viewModel;

    public AgregarVehiculoPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _viewModel = IPlatformApplication.Current?.Services.GetService<AgregarVehiculoViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver AgregarVehiculoViewModel desde DI.");

        _viewModel.MostrarAlertaAsync = (titulo, mensaje, cancelar) => DisplayAlertAsync(titulo, mensaje, cancelar);
        _viewModel.NavegarAMisVehiculosAsync = async () => await Navigation.PopAsync();

        BindingContext = _viewModel;
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
