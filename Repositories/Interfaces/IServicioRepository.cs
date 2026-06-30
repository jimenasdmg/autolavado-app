using AutoLavadoApp.Models;

namespace AutoLavadoApp.Repositories.Interfaces;

public interface IServicioRepository
{
    Task<List<Servicio>> ObtenerActivosAsync(string? idToken = null);
}
