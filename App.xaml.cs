using AutoLavadoApp.Views.Cliente;
using AutoLavadoApp.Views.Auth;

namespace AutoLavadoApp;


public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Page root = new global::AutoLavadoApp.Views.Auth.LoginPage();

        var sessionService = IPlatformApplication.Current?.Services.GetService<AutoLavadoApp.Services.Core.SessionService>();
        if (sessionService is not null)
        {
            var token = sessionService.ObtenerToken();
            var rol = sessionService.ObtenerRol()?.Trim().ToLowerInvariant() ?? string.Empty;

            if (!AutoLavadoApp.Services.Core.SessionService.TokenExpirado(token))
            {
                root = rol switch
                {
                    AutoLavadoApp.Constants.Roles.Admin => new AutoLavadoApp.Views.Admin.AdminTabbedPage(),
                    AutoLavadoApp.Constants.Roles.Empleado => new AutoLavadoApp.Views.Empleado.EmpleadoTabbedPage(),
                    _ => new HomePage()
                };
            }
        }

        return new Window(new NavigationPage(root));
    }
}