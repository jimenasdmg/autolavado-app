using System.Text.Json;
using AutoLavadoApp.Models;

namespace AutoLavadoApp.Services.Data.Mappers;

public static class ServicioFirestoreMapper
{
    public const string Id = "id";
    public const string Nombre = "nombre";
    public const string Descripcion = "descripcion";
    public const string Precio = "precio";
    public const string DuracionMinutos = "duracionMinutos";
    public const string Activo = "activo";
    public const string Visible = "visible";
    public const string VisibleLegacy = "visibleClientes";
    public const string Imagen = "imagen";
    public const string FechaCreacion = "fechaCreacion";

    public static Dictionary<string, object> BuildCreateFields(
        string id,
        string nombre,
        string descripcion,
        decimal precio,
        int duracionMinutos,
        bool activo,
        bool visible,
        string imagen)
    {
        return new Dictionary<string, object>
        {
            [Id] = new { stringValue = id },
            [Nombre] = new { stringValue = nombre ?? string.Empty },
            [Descripcion] = new { stringValue = descripcion ?? string.Empty },
            [Precio] = new { doubleValue = (double)precio },
            [DuracionMinutos] = new { integerValue = duracionMinutos },
            [Activo] = new { booleanValue = activo },
            [Visible] = new { booleanValue = visible },
            [VisibleLegacy] = new { booleanValue = visible },
            [Imagen] = new { stringValue = imagen ?? string.Empty },
            [FechaCreacion] = new { timestampValue = DateTime.UtcNow.ToString("O") }
        };
    }

    public static Servicio? ParseFromDocument(JsonElement doc)
    {
        if (!doc.TryGetProperty("fields", out var f)) return null;

        var name = doc.TryGetProperty("name", out var n) ? n.GetString() : null;
        var id = ReadString(f, Id);
        if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name)) id = name.Split('/').Last();

        return new Servicio
        {
            Id = id ?? string.Empty,
            Nombre = ReadString(f, Nombre) ?? string.Empty,
            Descripcion = ReadString(f, Descripcion) ?? string.Empty,
            Precio = ReadDecimal(f, Precio),
            DuracionMinutos = ReadInt(f, DuracionMinutos),
            Activo = ReadBool(f, Activo, true),
            Visible = ReadBool(f, Visible, ReadBool(f, VisibleLegacy, true)),
            Imagen = ReadString(f, Imagen) ?? string.Empty,
            FechaCreacion = ReadTimestamp(f, FechaCreacion) ?? DateTime.UtcNow
        };
    }

    public static Servicio? ParseFromQueryRow(JsonElement row)
    {
        if (!row.TryGetProperty("document", out var document)) return null;
        return ParseFromDocument(document);
    }

    private static string? ReadString(JsonElement fields, string key)
    {
        if (!fields.TryGetProperty(key, out var v)) return null;
        if (v.TryGetProperty("stringValue", out var s)) return s.GetString();
        return null;
    }

    private static int ReadInt(JsonElement fields, string key)
    {
        if (!fields.TryGetProperty(key, out var v)) return 0;
        if (v.TryGetProperty("integerValue", out var i) && int.TryParse(i.GetString(), out var n)) return n;
        return 0;
    }

    private static decimal ReadDecimal(JsonElement fields, string key)
    {
        if (!fields.TryGetProperty(key, out var v)) return 0;
        if (v.TryGetProperty("doubleValue", out var d) && d.TryGetDouble(out var dv)) return (decimal)dv;
        if (v.TryGetProperty("integerValue", out var i) && decimal.TryParse(i.GetString(), out var iv)) return iv;
        return 0;
    }

    private static bool ReadBool(JsonElement fields, string key, bool fallback = false)
    {
        if (!fields.TryGetProperty(key, out var v)) return fallback;
        if (v.TryGetProperty("booleanValue", out var b) && b.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            return b.GetBoolean();
        }

        return fallback;
    }

    private static DateTime? ReadTimestamp(JsonElement fields, string key)
    {
        if (!fields.TryGetProperty(key, out var v)) return null;
        if (!v.TryGetProperty("timestampValue", out var ts)) return null;
        if (!DateTime.TryParse(ts.GetString(), out var parsed)) return null;
        return parsed;
    }
}
