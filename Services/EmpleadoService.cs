using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoLavadoApp.Models;
using AutoLavadoApp.Constants;
using AutoLavadoApp.Services.Core;

namespace AutoLavadoApp.Services.Data
{
    public class EmpleadoService : FirestoreBaseService
    {
        private const string ColeccionUsuarios = "usuarios";
        private const string ColeccionEmpleados = "empleados";

        public EmpleadoService(HttpClient httpClient)
            : base(httpClient)
        {
        }

        public async Task<string> CrearEmpleadoAsync(string id, string nombre, string apellidoPaterno, string apellidoMaterno, string email, string telefono, string rol, string? idToken = null)
        {
            if (string.IsNullOrWhiteSpace(idToken) || string.IsNullOrWhiteSpace(id))
            {
                return string.Empty;
            }

            if (!EsTelefonoValido(telefono))
            {
                return string.Empty;
            }

            var rolNormalizado = Roles.Empleado;

            var body = new
            {
                fields = new Dictionary<string, object>
                {
                    ["uid"] = new { stringValue = id.Trim() },
                    ["nombre"] = new { stringValue = nombre?.Trim() ?? string.Empty },
                    ["apellidoPaterno"] = new { stringValue = apellidoPaterno?.Trim() ?? string.Empty },
                    ["apellidoMaterno"] = new { stringValue = apellidoMaterno?.Trim() ?? string.Empty },
                    ["correo"] = new { stringValue = email?.Trim() ?? string.Empty },
                    ["email"] = new { stringValue = email?.Trim() ?? string.Empty },
                    ["telefono"] = new { stringValue = telefono.Trim() },
                    ["rol"] = new { stringValue = rolNormalizado },
                    ["activo"] = new { booleanValue = true }
                }
            };

            var resultadoUsuarios = await CrearDocumentoAsync(ColeccionUsuarios, body, id, idToken);
            var resultadoEmpleados = await CrearDocumentoAsync(ColeccionEmpleados, body, id, idToken);
            var resultado = resultadoUsuarios && resultadoEmpleados;
            return resultado ? id : string.Empty;
        }

        public async Task<List<EmpleadoAdminResumen>> ObtenerEmpleadosAsync(string? idToken = null)
        {
            var document = await ObtenerColeccionAsync(ColeccionUsuarios, idToken);
            if (document is null)
            {
                return new List<EmpleadoAdminResumen>();
            }

            var empleados = new List<EmpleadoAdminResumen>();
            if (!document.Value.TryGetProperty("documents", out var docs) || docs.ValueKind != JsonValueKind.Array)
            {
                return empleados;
            }

            foreach (var doc in docs.EnumerateArray())
            {
                if (!doc.TryGetProperty("fields", out var fields))
                {
                    continue;
                }

                var rol = LeerString(fields, "rol") ?? "cliente";
                if (!string.Equals(rol, "empleado", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                empleados.Add(new EmpleadoAdminResumen
                {
                    Id = ObtenerIdDocumento(doc),
                    Nombre = LeerString(fields, "nombre") ?? string.Empty,
                    ApellidoPaterno = LeerString(fields, "apellidoPaterno") ?? string.Empty,
                    ApellidoMaterno = LeerString(fields, "apellidoMaterno") ?? string.Empty,
                    Email = LeerString(fields, "email") ?? string.Empty,
                    Telefono = LeerString(fields, "telefono") ?? string.Empty,
                    Rol = rol,
                    Activo = LeerBool(fields, "activo") ?? true
                });
            }

            return empleados
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        public async Task<List<EmpleadoAdminResumen>> ObtenerEmpleadosDesdeColeccionEmpleadosAsync(string? idToken = null)
        {
            var document = await ObtenerColeccionAsync(ColeccionEmpleados, idToken);
            if (document is null)
            {
                return new List<EmpleadoAdminResumen>();
            }

            var empleados = new List<EmpleadoAdminResumen>();
            if (!document.Value.TryGetProperty("documents", out var docs) || docs.ValueKind != JsonValueKind.Array)
            {
                return empleados;
            }

            foreach (var doc in docs.EnumerateArray())
            {
                if (!doc.TryGetProperty("fields", out var fields))
                {
                    continue;
                }

                empleados.Add(new EmpleadoAdminResumen
                {
                    Id = ObtenerIdDocumento(doc),
                    Nombre = LeerString(fields, "nombre") ?? string.Empty,
                    ApellidoPaterno = LeerString(fields, "apellidoPaterno") ?? string.Empty,
                    ApellidoMaterno = LeerString(fields, "apellidoMaterno") ?? string.Empty,
                    Email = LeerString(fields, "email") ?? string.Empty,
                    Telefono = LeerString(fields, "telefono") ?? string.Empty,
                    Rol = LeerString(fields, "rol") ?? "empleado",
                    Activo = LeerBool(fields, "activo") ?? true
                });
            }

            return empleados
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        public async Task ActualizarRolAsync(string empleadoId, string rol, string? idToken = null)
        {
            var body = new
            {
                fields = new Dictionary<string, object>
                {
                    ["rol"] = new { stringValue = rol }
                }
            };

            await CrearDocumentoAsync(ColeccionUsuarios, body, empleadoId, idToken);
        }

        public async Task ActualizarEstadoAsync(string empleadoId, bool activo, string? idToken = null)
        {
            var body = new
            {
                fields = new Dictionary<string, object>
                {
                    ["activo"] = new { booleanValue = activo }
                }
            };

            await CrearDocumentoAsync(ColeccionUsuarios, body, empleadoId, idToken);
        }

        public async Task<bool> EliminarEmpleadoAsync(string empleadoId, string? idToken = null)
        {
            if (string.IsNullOrWhiteSpace(empleadoId)) return false;

            // Eliminar documento de la colección 'usuarios' y 'empleados' si existen
            var okUsuarios = await EliminarDocumentoSiExisteAsync(ColeccionUsuarios, empleadoId, idToken);
            var okEmpleados = await EliminarDocumentoSiExisteAsync(ColeccionEmpleados, empleadoId, idToken);
            return okUsuarios || okEmpleados;
        }

        private async Task<bool> EliminarDocumentoSiExisteAsync(string coleccion, string id, string? idToken)
        {
            try
            {
                var doc = await ObtenerDocumentoAsync(coleccion, id, idToken);
                if (doc is null)
                    return false;

                return await EliminarDocumentoAsync(coleccion, id, idToken);
            }
            catch
            {
                return false;
            }
        }

        private static string ObtenerIdDocumento(JsonElement documento)
        {
            if (!documento.TryGetProperty("name", out var nameElement))
            {
                return string.Empty;
            }

            var name = nameElement.GetString() ?? string.Empty;
            var partes = name.Split('/');
            return partes.Length == 0 ? string.Empty : partes[^1];
        }

        private static string? LeerString(JsonElement fields, string propiedad)
        {
            if (!fields.TryGetProperty(propiedad, out var propiedadElement))
            {
                return null;
            }

            if (!propiedadElement.TryGetProperty("stringValue", out var stringValueElement))
            {
                return null;
            }

            return stringValueElement.GetString();
        }

        private static bool? LeerBool(JsonElement fields, string propiedad)
        {
            if (!fields.TryGetProperty(propiedad, out var propiedadElement))
            {
                return null;
            }

            if (!propiedadElement.TryGetProperty("booleanValue", out var boolElement))
            {
                return null;
            }

            return boolElement.GetBoolean();
        }

        private static bool EsTelefonoValido(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return false;
            }

            var t = telefono.Trim();
            return t.Length == 10 && t.All(char.IsDigit);
        }
    }

    public class EmpleadoAdminResumen
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = "empleado";
        public bool Activo { get; set; }

        public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();
    }
}
