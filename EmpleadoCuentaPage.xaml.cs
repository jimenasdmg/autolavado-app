using AutoLavadoApp.Views.Empleado;
using AutoLavadoApp.ViewModels.Cliente;

namespace AutoLavadoApp.Views.Empleado;

public partial class EmpleadoCuentaPage : ContentPage
{
    private readonly MiCuentaViewModel _viewModel;

    public EmpleadoCuentaPage()
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
}
