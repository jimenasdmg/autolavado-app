using AutoLavadoApp.Models;
using AutoLavadoApp.Repositories.Interfaces;
using AutoLavadoApp.Services.Data;

namespace AutoLavadoApp.Repositories;

public class ServicioRepository : IServicioRepository
{
    private readonly ServicioService _servicioService;

    public ServicioRepository(ServicioService servicioService)
    {
        _servicioService = servicioService;
    }

    public Task<List<Servicio>> ObtenerActivosAsync(string? idToken = null)
        => _servicioService.ObtenerServiciosActivosAsync(idToken);
}
