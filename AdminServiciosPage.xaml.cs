using AutoLavadoApp.ViewModels;
using AutoLavadoApp.ViewModels.Admin;

namespace AutoLavadoApp.Views.Admin;

public partial class AdminServiciosPage : ContentPage
{
    private readonly AdminViewModel _viewModel;

    public AdminServiciosPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<AdminViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver AdminViewModel desde DI.");

        _viewModel.MostrarAlertaAsync = (titulo, mensaje, cancelar) => DisplayAlertAsync(titulo, mensaje, cancelar);
        _viewModel.SolicitarInputAsync = (titulo, placeholder, aceptar, cancelar, valorInicial) =>
            DisplayPromptAsync(titulo, placeholder, aceptar, cancelar, initialValue: valorInicial);

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarDashboardAsync();
    }

    private async void OnAgregarServicioClicked(object? sender, EventArgs e)
    {
        await _viewModel.AgregarServicioAsync();
    }

    private async void OnEditarServicioClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not ServicioAdminItem servicio)
        {
            return;
        }

        await _viewModel.EditarServicioAsync(servicio);
    }

    private async void OnCambiarEstadoServicioClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not ServicioAdminItem servicio)
        {
            return;
        }

        await _viewModel.CambiarEstadoServicioAsync(servicio);
    }
}
