using AutoLavadoApp.Models;

namespace AutoLavadoApp.Repositories;

public static class CitaTransitionPolicy
{
    public static bool EsTransicionValida(string estadoActual, string nuevoEstado, bool esDomicilio)
    {
        var from = (estadoActual ?? string.Empty).Trim().ToLowerInvariant();
        var to = (nuevoEstado ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(to))
        {
            return false;
        }

        if (from == to)
        {
            return true;
        }

        return (from, to) switch
        {
            (EstadosCita.Pendiente, EstadosCita.Cancelada) => true,
            (EstadosCita.Pendiente, EstadosCita.EmpleadoAsignado) => true,
            (EstadosCita.Aceptada, EstadosCita.EmpleadoAsignado) => true,
            (EstadosCita.Aceptada, EstadosCita.Cancelada) => true,
            (EstadosCita.EmpleadoAsignado, EstadosCita.Lavando) when !esDomicilio => true,
            (EstadosCita.EmpleadoAsignado, EstadosCita.EnCamino) when esDomicilio => true,
            (EstadosCita.EmpleadoAsignado, EstadosCita.Cancelada) => true,
            (EstadosCita.EnCamino, EstadosCita.VehiculoRecogido) when esDomicilio => true,
            (EstadosCita.VehiculoRecogido, EstadosCita.Lavando) when esDomicilio => true,
            (EstadosCita.Lavando, EstadosCita.Finalizada) => true,
            (EstadosCita.Finalizada, EstadosCita.Entregado) => true,
            _ => false
        };
    }
}
