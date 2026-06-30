using AutoLavadoApp.Models;

namespace AutoLavadoApp.Repositories.Interfaces;

public interface IUsuarioPerfilRepository
{
    Task<Usuario?> ObtenerPorIdAsync(string uid, string? idToken = null);
    Task<bool> ActualizarPerfilParcialAsync(
        string uid,
        string idToken,
        string nombre,
        string apellidoPaterno,
        string? apellidoMaterno,
        string correo);
}
