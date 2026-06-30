using AutoLavadoApp.Constants;

namespace AutoLavadoApp.Helpers;

public static class RoleExtensions
{
    public static string ToDisplayName(this string? rol)
    {
        return (rol ?? Roles.Cliente).ToLowerInvariant() switch
        {
            Roles.Admin => "Administrador",
            Roles.Empleado => "Empleado",
            _ => "Cliente"
        };
    }
}
