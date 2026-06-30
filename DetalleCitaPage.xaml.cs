using AutoLavadoApp.ViewModels;

namespace AutoLavadoApp.Views.Cliente;

public partial class DetalleCitaPage : ContentPage
{
    private readonly DetalleCitaViewModel _viewModel;
    private readonly string _citaId;

    public DetalleCitaPage(string citaId)
    {
        InitializeComponent();

        _citaId = citaId;

        _viewModel = IPlatformApplication.Current?.Services.GetService<DetalleCitaViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver DetalleCitaViewModel desde DI.");

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarDetalleAsync(_citaId);
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
