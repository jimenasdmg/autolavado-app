using AutoLavadoApp.Models;

namespace AutoLavadoApp.Repositories.Interfaces;

public interface IVehiculoRepository
{
    Task<List<Vehiculo>> ObtenerPorUsuarioAsync(string usuarioId, string? idToken = null);
    Task<bool> GuardarAsync(Vehiculo vehiculo, string? idToken = null);
    Task<bool> EliminarAsync(string vehiculoId, string? idToken = null);
}
