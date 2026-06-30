using AutoLavadoApp.ViewModels.Admin;
using AutoLavadoApp.Services.Core;
using AutoLavadoApp.Services.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace AutoLavadoApp.Views.Admin;

public partial class AdminMensajesPage : ContentPage
{
    private readonly AdminMensajesViewModel _viewModel;

    public AdminMensajesPage()
    {
        InitializeComponent();

        _viewModel = IPlatformApplication.Current?.Services.GetService<AdminMensajesViewModel>()
            ?? throw new InvalidOperationException("AdminMensajesViewModel no está registrado en DI.");

        _viewModel.MostrarAlertaAsync = async (titulo, mensaje, cancelar) => 
        {
            await DisplayAlertAsync(titulo, mensaje, cancelar);
            return true;
        };

        BindingContext = _viewModel;
    }

    public AdminMensajesPage(string empleadoUid, string empleadoNombre) : this()
    {
        _viewModel.ConfigurarEmpleadoObjetivo(empleadoUid, empleadoNombre);
    }

    private async void OnEnviarClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not AdminMensajesViewModel vm) return;
        var ok = await vm.EnviarMensajeAsync();
        Debug.WriteLine($"[MENSAJES][ADMIN][UI][SEND] ok={ok}");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.RefrescarMensajesAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden || (ex.Message?.Contains("403") ?? false))
        {
            await DisplayAlertAsync("Sin permisos", "No tienes permisos para ver mensajes internos (403).", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ADMIN_MENSAJES][ON_APPEARING][ERROR] {ex}");
            await DisplayAlertAsync("Error", "No se pudieron cargar los mensajes.", "OK");
        }

        if (BindingContext is AdminMensajesViewModel vm)
        {
            var session = IPlatformApplication.Current?.Services.GetService<SessionService>();
            if (session is not null)
            {
                vm.IdToken = await session.ObtenerTokenAsync();
                vm.AdminUid = session.ObtenerUid();
            }

            if (string.IsNullOrWhiteSpace(vm.AdminUid) && !string.IsNullOrWhiteSpace(vm.IdToken))
            {
                var usuarioService = IPlatformApplication.Current?.Services.GetService<UsuarioService>();
                var admin = usuarioService is null ? null : await usuarioService.ObtenerPrimerUsuarioPorRolAsync("admin", vm.IdToken);
                if (admin is not null)
                {
                    vm.AdminUid = admin.Uid;
                }
            }

            try
            {
                await vm.RefrescarMensajesAsync();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden || (ex.Message?.Contains("403") ?? false))
            {
                await DisplayAlertAsync("Sin permisos", "No tienes permisos para ver mensajes internos (403).", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ADMIN_MENSAJES][ON_APPEARING][ERROR][2] {ex}");
                await DisplayAlertAsync("Error", "No se pudieron cargar los mensajes.", "OK");
            }

            vm.IniciarPolling();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is AdminMensajesViewModel vm)
        {
            vm.DetenerPolling();
        }
    }

    private async void OnVolverClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
