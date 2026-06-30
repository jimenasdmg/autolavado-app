using AutoLavadoApp.ViewModels.Auth;
using AutoLavadoApp.Views.Admin;
using AutoLavadoApp.Views.Empleado;
using AutoLavadoApp.Constants;
using AutoLavadoApp.Views.Cliente;

namespace AutoLavadoApp.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly AutenticacionViewModel _authViewModel;

    public LoginPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _authViewModel = IPlatformApplication.Current?.Services.GetService<AutenticacionViewModel>()
            ?? throw new InvalidOperationException("No se pudo resolver AutenticacionViewModel desde DI.");

        BindingContext = _authViewModel;
    }

    private async void OnIniciarSesionClicked(object? sender, EventArgs e)
    {
        var ok = await _authViewModel.LoginAsync(_authViewModel.Email, _authViewModel.Password);
        if (!ok) return;

        var rol = _authViewModel.RolActual?.Trim().ToLowerInvariant() ?? string.Empty;

        Page destino = rol switch
        {
            Roles.Empleado => new EmpleadoTabbedPage(),
            Roles.Admin => new AdminTabbedPage(),
            _ => new HomePage()
        };

        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new NavigationPage(destino);
    }

    private async void OnIrCrearCuentaClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void OnActionButtonPressed(object? sender, EventArgs e)
    {
        if (sender is not VisualElement view)
        {
            return;
        }

        await view.ScaleToAsync(0.98, 70, Easing.CubicOut);
    }

    private async void OnActionButtonReleased(object? sender, EventArgs e)
    {
        if (sender is not VisualElement view)
        {
            return;
        }

        await view.ScaleToAsync(1.0, 90, Easing.CubicIn);
    }
}
