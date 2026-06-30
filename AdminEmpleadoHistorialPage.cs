using Microsoft.Maui.Controls;
using AutoLavadoApp.Repositories.Interfaces;
using AutoLavadoApp.Models;
using AutoLavadoApp.Services;

namespace AutoLavadoApp;

public class AdminEmpleadoHistorialPage : ContentPage
{
    private readonly string _empleadoId;
    private readonly ICitaRepository _citaRepository;
    private readonly SessionService _sessionService;

    private readonly CollectionView _lista;

    public AdminEmpleadoHistorialPage(
        string empleadoId,
        string nombreCompleto)
    {
        _empleadoId = empleadoId;

        _citaRepository =
            IPlatformApplication.Current.Services.GetService<ICitaRepository>()
            ?? throw new Exception("ICitaRepository no registrado");

        _sessionService =
            IPlatformApplication.Current.Services.GetService<SessionService>()
            ?? throw new Exception("SessionService no registrado");

        Title = $"Historial - {nombreCompleto}";

        _lista = new CollectionView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                var servicio = new Label
                {
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold
                };

                servicio.SetBinding(Label.TextProperty, "Servicio");

                var fecha = new Label();
                fecha.SetBinding(Label.TextProperty,
                    new Binding("Fecha", stringFormat: "Fecha: {0:dd/MM/yyyy}"));

                var hora = new Label();
                hora.SetBinding(Label.TextProperty,
                    new Binding("Hora", stringFormat: "Hora: {0}"));

                var estado = new Label
                {
                    TextColor = Colors.Green
                };

                estado.SetBinding(Label.TextProperty, "Estado");

                return new Frame
                {
                    Margin = 10,
                    Padding = 15,
                    CornerRadius = 15,
                    Content = new VerticalStackLayout
                    {
                        Children =
                        {
                            servicio,
                            fecha,
                            hora,
                            estado
                        }
                    }
                };
            })
        };

        Content =
        new Grid
        {
            Children =
            {
                new VerticalStackLayout
                {
                    Padding=20,
                    Children=
                    {
                        new Label
                        {
                            Text="Historial de citas",
                            FontSize=28,
                            FontAttributes=FontAttributes.Bold
                        },

                        _lista
                    }
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var token = _sessionService.ObtenerToken();

            var citas =
                await _citaRepository.ObtenerTodasAsync(token);

            var historial =
                citas
                .Where(c =>
                    c.EmpleadoId == _empleadoId &&
                    c.Estado == EstadosCita.Finalizada)
                .OrderByDescending(c => c.Fecha)
                .ToList();

            _lista.ItemsSource = historial;

            if (!historial.Any())
            {
                await DisplayAlert(
                    "Historial",
                    "Este empleado aún no tiene servicios realizados.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }
}