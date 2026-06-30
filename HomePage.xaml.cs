using AutoLavadoApp.ViewModels;
using AutoLavadoApp.Views.Empleado;

namespace AutoLavadoApp.Views.Cliente;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _homeViewModel;

    public HomePage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _homeViewModel = IPlatformApplication.Current?.Services.GetService<HomeViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver HomeViewModel desde DI.");
        BindingContext = _homeViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _homeViewModel.CargarUsuarioAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 360)
        {
            Padding = new Thickness(0, 24, 0, 0);
        }
    }

    private async void OnAgendarAhoraClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AgendarCitaPage());
    }

    private async void OnMisAutosClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MisVehiculosPage());
    }

    private async void OnIrMisCitasClicked(object? sender, EventArgs e)
    {
        // Si el usuario es un empleado, abrir la vista de citas para empleados (seguimiento).
        var session = IPlatformApplication.Current?.Services.GetService<AutoLavadoApp.Services.Core.SessionService>();
        var rol = session?.ObtenerRol()?.Trim().ToLowerInvariant() ?? string.Empty;

        if (rol == "empleado")
        {
            await Navigation.PushAsync(new EmpleadoCitasPage());
            return;
        }

        await Navigation.PushAsync(new MisCitasPage());
    }

    private async void OnMiCuentaClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MiCuentaPage());
    }

}
