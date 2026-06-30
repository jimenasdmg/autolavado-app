using System.Text.Json;
using AutoLavadoApp.Models;
using AutoLavadoApp.Services.Core;

namespace AutoLavadoApp.Services.Data;

public class UsuarioService : FirestoreBaseService
{
    private const string Collection = "usuarios";

    public UsuarioService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<bool> GuardarUsuarioConRolAsync(Usuario usuario)
    {
        return await GuardarUsuarioAsync(usuario);
    }

    public async Task<bool> GuardarUsuarioAsync(Usuario usuario)
    {
        var uid = usuario.Uid;
        if (string.IsNullOrWhiteSpace(uid)) return false;
        if (string.IsNullOrWhiteSpace(usuario.IdToken)) return false;

        var rolNormalizado = string.IsNullOrWhiteSpace(usuario.Rol)
            ? "cliente"
            : usuario.Rol.Trim().ToLowerInvariant();

        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["uid"] = new { stringValue = uid },
                ["nombre"] = new { stringValue = (usuario.Nombre ?? string.Empty) },
                ["apellidoPaterno"] = new { stringValue = (usuario.ApellidoPaterno ?? string.Empty) },
                ["apellidoMaterno"] = new { stringValue = (usuario.ApellidoMaterno ?? string.Empty) },
                ["correo"] = new { stringValue = (usuario.Correo ?? usuario.Email ?? string.Empty) },
                ["rol"] = new { stringValue = rolNormalizado },
                ["activo"] = new { booleanValue = usuario.Activo },
                ["telefono"] = new { stringValue = (usuario.Telefono ?? string.Empty) }
            }
        };

        return await CrearDocumentoAsync(Collection, body, uid, usuario.IdToken);
    }

    public async Task<bool> ActualizarPerfilParcialAsync(
        string uid,
        string idToken,
        string nombre,
        string apellidoPaterno,
        string? apellidoMaterno,
        string correo)
    {
        if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(idToken))
        {
            return false;
        }

        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["nombre"] = new { stringValue = nombre?.Trim() ?? string.Empty },
                ["apellidoPaterno"] = new { stringValue = apellidoPaterno?.Trim() ?? string.Empty },
                ["apellidoMaterno"] = new { stringValue = apellidoMaterno?.Trim() ?? string.Empty },
                ["correo"] = new { stringValue = correo?.Trim() ?? string.Empty }
            }
        };

        return await ActualizarDocumentoAsync(Collection, uid, body, idToken);
    }

    public async Task<Usuario?> ObtenerUsuarioPorIdAsync(string uid, string? idToken = null)
    {
        var doc = await ObtenerDocumentoAsync(Collection, uid, idToken);
        if (doc is null) return null;

        var root = doc.Value;
        if (!root.TryGetProperty("fields", out var f)) return null;

        return new Usuario
        {
            Uid = ReadString(f, "uid") ?? uid,
            Nombre = ReadString(f, "nombre") ?? string.Empty,
            ApellidoPaterno = ReadString(f, "apellidoPaterno") ?? string.Empty,
            ApellidoMaterno = ReadString(f, "apellidoMaterno") ?? string.Empty,
            Correo = ReadString(f, "correo") ?? string.Empty,
            Email = ReadString(f, "correo") ?? string.Empty,
            Rol = (ReadString(f, "rol") ?? "cliente").ToLowerInvariant(),
            Activo = ReadBool(f, "activo") ?? true,
            Telefono = ReadString(f, "telefono") ?? string.Empty
        };
    }

    public async Task<string?> ObtenerRolPorUidAsync(string uid, string? idToken = null)
    {
        var usuario = await ObtenerUsuarioPorIdAsync(uid, idToken);
        return usuario?.Rol;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email, string? idToken = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var doc = await ObtenerColeccionAsync(Collection, idToken);
        if (doc is null || !doc.Value.TryGetProperty("documents", out var documents) || documents.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var emailBuscado = email.Trim().ToLowerInvariant();
        foreach (var d in documents.EnumerateArray())
        {
            if (!d.TryGetProperty("fields", out var f))
            {
                continue;
            }

            var correoActual = (ReadString(f, "correo") ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.Equals(correoActual, emailBuscado, StringComparison.Ordinal))
            {
                continue;
            }

            var uid = ReadString(f, "uid") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(uid) && d.TryGetProperty("name", out var nameElement))
            {
                var name = nameElement.GetString() ?? string.Empty;
                var partes = name.Split('/');
                uid = partes.Length > 0 ? partes[^1] : string.Empty;
            }

            return new Usuario
            {
                Uid = uid,
                Nombre = ReadString(f, "nombre") ?? string.Empty,
                ApellidoPaterno = ReadString(f, "apellidoPaterno") ?? string.Empty,
                ApellidoMaterno = ReadString(f, "apellidoMaterno") ?? string.Empty,
                Correo = ReadString(f, "correo") ?? string.Empty,
                Email = ReadString(f, "correo") ?? string.Empty,
                Rol = (ReadString(f, "rol") ?? "cliente").ToLowerInvariant(),
                Telefono = ReadString(f, "telefono") ?? string.Empty,
                Activo = ReadBool(f, "activo") ?? true
            };
        }

        return null;
    }

    public async Task<Usuario?> ObtenerPrimerUsuarioPorRolAsync(string rol, string? idToken = null)
    {
        if (string.IsNullOrWhiteSpace(rol))
        {
            return null;
        }

        var doc = await ObtenerColeccionAsync(Collection, idToken);
        if (doc is null || !doc.Value.TryGetProperty("documents", out var documents) || documents.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var rolBuscado = rol.Trim().ToLowerInvariant();
        foreach (var d in documents.EnumerateArray())
        {
            if (!d.TryGetProperty("fields", out var f))
            {
                continue;
            }

            var rolActual = (ReadString(f, "rol") ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.Equals(rolActual, rolBuscado, StringComparison.Ordinal))
            {
                continue;
            }

            var uid = ReadString(f, "uid") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(uid) && d.TryGetProperty("name", out var nameElement))
            {
                var name = nameElement.GetString() ?? string.Empty;
                var partes = name.Split('/');
                uid = partes.Length > 0 ? partes[^1] : string.Empty;
            }

            return new Usuario
            {
                Uid = uid,
                Nombre = ReadString(f, "nombre") ?? string.Empty,
                ApellidoPaterno = ReadString(f, "apellidoPaterno") ?? string.Empty,
                ApellidoMaterno = ReadString(f, "apellidoMaterno") ?? string.Empty,
                Correo = ReadString(f, "correo") ?? string.Empty,
                Rol = rolActual,
                Telefono = ReadString(f, "telefono") ?? string.Empty,
                Activo = ReadBool(f, "activo") ?? true
            };
        }

        return null;
    }

    private static string? ReadString(JsonElement fields, string key)
    {
        if (!fields.TryGetProperty(key, out var v)) return null;
        if (v.TryGetProperty("stringValue", out var s)) return s.GetString();
        return null;
    }

    private static bool? ReadBool(JsonElement fields, string key)
    {
        if (!fields.TryGetProperty(key, out var v)) return null;
        if (v.TryGetProperty("booleanValue", out var b)) return b.GetBoolean();
        return null;
    }
}
