using AutoLavadoApp.Models;
using AutoLavadoApp.Repositories.Interfaces;
using AutoLavadoApp.Services.Data;

namespace AutoLavadoApp.Repositories;

public class UsuarioPerfilRepository : IUsuarioPerfilRepository
{
    private readonly UsuarioService _usuarioService;

    public UsuarioPerfilRepository(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public Task<Usuario?> ObtenerPorIdAsync(string uid, string? idToken = null)
        => _usuarioService.ObtenerUsuarioPorIdAsync(uid, idToken);

    public Task<bool> ActualizarPerfilParcialAsync(
        string uid,
        string idToken,
        string nombre,
        string apellidoPaterno,
        string? apellidoMaterno,
        string correo)
        => _usuarioService.ActualizarPerfilParcialAsync(uid, idToken, nombre, apellidoPaterno, apellidoMaterno, correo);
}
