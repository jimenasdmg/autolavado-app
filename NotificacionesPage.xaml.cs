using AutoLavadoApp.Views.Shared;
using AutoLavadoApp.ViewModels.Shared;
namespace AutoLavadoApp.Views.Shared;

public partial class NotificacionesPage : ContentPage
{
    private readonly NotificacionesViewModel _viewModel;

    public NotificacionesPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<NotificacionesViewModel>()
            ?? throw new InvalidOperationException("NotificacionesViewModel no está registrado en DI.");

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
