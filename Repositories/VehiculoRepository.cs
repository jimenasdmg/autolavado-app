using AutoLavadoApp.Models;
using AutoLavadoApp.Repositories.Interfaces;
using AutoLavadoApp.Services.Data;

namespace AutoLavadoApp.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly VehiculoService _vehiculoService;

    public VehiculoRepository(VehiculoService vehiculoService)
    {
        _vehiculoService = vehiculoService;
    }

    public Task<List<Vehiculo>> ObtenerPorUsuarioAsync(string usuarioId, string? idToken = null)
        => _vehiculoService.ObtenerVehiculosPorUsuarioAsync(usuarioId, idToken);

    public Task<bool> GuardarAsync(Vehiculo vehiculo, string? idToken = null)
        => _vehiculoService.GuardarVehiculoAsync(vehiculo, idToken);

    public Task<bool> EliminarAsync(string vehiculoId, string? idToken = null)
        => _vehiculoService.EliminarVehiculoAsync(vehiculoId, idToken);
}
