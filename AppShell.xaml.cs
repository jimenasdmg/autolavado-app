using Microsoft.Maui.Controls;
using AutoLavadoApp.Views.Admin;
using AutoLavadoApp.Views.Auth;
using AutoLavadoApp.Views.Cliente;
using AutoLavadoApp.Views.Empleado;
using AutoLavadoApp.Views.Shared;

namespace AutoLavadoApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(AgendarCitaPage), typeof(AgendarCitaPage));
            Routing.RegisterRoute(nameof(MisCitasPage), typeof(MisCitasPage));
            Routing.RegisterRoute(nameof(MisVehiculosPage), typeof(MisVehiculosPage));
            Routing.RegisterRoute(nameof(AgregarVehiculoPage), typeof(AgregarVehiculoPage));
            Routing.RegisterRoute(nameof(MiCuentaPage), typeof(MiCuentaPage));
            Routing.RegisterRoute(nameof(EmpleadoHomePage), typeof(EmpleadoHomePage));
            Routing.RegisterRoute(nameof(AdminHomePage), typeof(AdminHomePage));
            Routing.RegisterRoute(nameof(DetalleCitaPage), typeof(DetalleCitaPage));
        }
    }
}
