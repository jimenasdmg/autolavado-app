using AutoLavadoApp.Models;
using AutoLavadoApp.Repositories.Interfaces;
using AutoLavadoApp.Services.Data;

namespace AutoLavadoApp.Repositories;

public class MensajeRepository : IMensajeRepository
{
    private readonly MensajeService _mensajeService;

    public MensajeRepository(MensajeService mensajeService)
    {
        _mensajeService = mensajeService;
    }

    public Task<bool> EnviarAsync(string adminUid, string remitenteId, string remitenteRol, string destinatarioId, string texto, string idToken)
        => _mensajeService.EnviarMensajeAsync(adminUid, remitenteId, remitenteRol, destinatarioId, texto, idToken);

    public Task<List<MensajeInterno>> ObtenerMensajesAsync(string conversacionId, string idToken)
        => _mensajeService.ObtenerMensajesAsync(conversacionId, idToken);

    public Task<List<string>> ObtenerConversacionesAsync(string uid, string idToken)
        => _mensajeService.ObtenerConversacionesAsync(uid, idToken);

    public Task<int> ContarNoLeidosAsync(string uid, string idToken)
        => _mensajeService.ContarNoLeidosAsync(uid, idToken);

    public Task MarcarComoLeidoAsync(string conversacionId, string uid, string idToken)
        => _mensajeService.MarcarComoLeidoAsync(conversacionId, uid, idToken);
}
