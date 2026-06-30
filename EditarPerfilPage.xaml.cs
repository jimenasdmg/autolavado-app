using AutoLavadoApp.ViewModels.Cliente;

namespace AutoLavadoApp.Views.Cliente;

public partial class EditarPerfilPage : ContentPage
{
    private readonly MiCuentaViewModel _viewModel;

    public EditarPerfilPage()
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

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        var ok = await _viewModel.GuardarPerfilDesdeEdicionAsync();
        if (ok)
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
