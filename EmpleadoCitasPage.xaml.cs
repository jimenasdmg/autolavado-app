using AutoLavadoApp.ViewModels;

namespace AutoLavadoApp.Views.Empleado;

public partial class EmpleadoCitasPage : ContentPage
{
    private readonly EmpleadoViewModel _viewModel;

    public EmpleadoCitasPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<EmpleadoViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver EmpleadoViewModel desde DI.");

        _viewModel.MostrarAlertaAsync = (titulo, mensaje, cancelar) => DisplayAlertAsync(titulo, mensaje, cancelar);

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarDatosAsync();
    }
}
