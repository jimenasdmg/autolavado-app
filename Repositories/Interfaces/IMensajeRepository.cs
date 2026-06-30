using AutoLavadoApp.Models;

namespace AutoLavadoApp.Repositories.Interfaces;

public interface IMensajeRepository
{
    Task<bool> EnviarAsync(string adminUid, string remitenteId, string remitenteRol, string destinatarioId, string texto, string idToken);
    Task<List<MensajeInterno>> ObtenerMensajesAsync(string conversacionId, string idToken);
    Task<List<string>> ObtenerConversacionesAsync(string uid, string idToken);
    Task<int> ContarNoLeidosAsync(string uid, string idToken);
    Task MarcarComoLeidoAsync(string conversacionId, string uid, string idToken);
}
