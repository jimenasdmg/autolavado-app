using AutoLavadoApp.ViewModels;
using AutoLavadoApp.Views.Shared;


namespace AutoLavadoApp.Views.Empleado;

public partial class EmpleadoHomePage : ContentPage
{
    private readonly EmpleadoViewModel _viewModel;

    public EmpleadoHomePage()
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
        try
        {
            await _viewModel.CargarDatosAsync();
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Datos", "No se pudieron cargar los datos del empleado. Verifica tu perfil en Firestore e intenta nuevamente.", "OK");
        }
    }

    private async void OnVerDetalleCitaClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string citaId || string.IsNullOrWhiteSpace(citaId))
        {
            return;
        }

        await Navigation.PushAsync(new AutoLavadoApp.Views.Cliente.DetalleCitaPage(citaId));
    }

    private async void OnAbrirMensajesClicked(object? sender, EventArgs e)
    {
        // Mensajes internos removidos.
    }

    private async void OnAbrirNotificacionesClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new NotificacionesPage());
    }

}
