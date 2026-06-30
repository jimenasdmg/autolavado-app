using AutoLavadoApp.ViewModels;

namespace AutoLavadoApp.Views.Admin;

public partial class CrearEmpleadoPage : ContentPage
{
    private readonly CrearEmpleadoViewModel _viewModel;

    public CrearEmpleadoPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<CrearEmpleadoViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver CrearEmpleadoViewModel desde DI.");

        _viewModel.MostrarAlertaAsync = (titulo, mensaje, cancelar) => DisplayAlertAsync(titulo, mensaje, cancelar);
        _viewModel.NavegarAtrasAsync = async () => await Navigation.PopAsync();

        BindingContext = _viewModel;
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
