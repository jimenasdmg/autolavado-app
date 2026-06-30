using AutoLavadoApp.ViewModels;

namespace AutoLavadoApp.Views.Cliente;

public partial class MisCitasPage : ContentPage
{
    private readonly MisCitasViewModel _viewModel;

    public MisCitasPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<MisCitasViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver MisCitasViewModel desde DI.");

        _viewModel.MostrarAlertaAsync =
            (titulo, mensaje, cancelar) =>
                DisplayAlert(titulo, mensaje, cancelar);

        _viewModel.ConfirmarAsync =
            (titulo, mensaje, aceptar) =>
                DisplayAlert(titulo, mensaje, aceptar, "No");

        _viewModel.NavegarDetalleAsync =
            async citaId =>
                await Navigation.PushAsync(new DetalleCitaPage(citaId));

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}