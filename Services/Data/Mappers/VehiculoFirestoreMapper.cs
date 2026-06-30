using System.Text.Json;
using AutoLavadoApp.Models;

namespace AutoLavadoApp.Services.Data.Mappers;

public static class VehiculoFirestoreMapper
{
    public const string Id = "id";
    public const string UsuarioId = "usuarioId";
    public const string ClienteId = "clienteId";
    public const string Placa = "placa";
    public const string PlacasLegacy = "placas";
    public const string Marca = "marca";
    public const string Modelo = "modelo";
    public const string Color = "color";
    public const string Anio = "anio";
    public const string Tipo = "tipo";
    public const string NombreVisual = "nombreVisual";

    public static Dictionary<string, object> BuildFields(Vehiculo vehiculo)
    {
        return new Dictionary<string, object>
        {
            [Id] = new { stringValue = vehiculo.Id },
            [UsuarioId] = new { stringValue = vehiculo.UsuarioId ?? string.Empty },
            [ClienteId] = new { stringValue = vehiculo.ClienteId ?? string.Empty },
            [Placa] = new { stringValue = vehiculo.Placa ?? string.Empty },
            [PlacasLegacy] = new { stringValue = vehiculo.Placas ?? string.Empty },
            [Marca] = new { stringValue = vehiculo.Marca ?? string.Empty },
            [Modelo] = new { stringValue = vehiculo.Modelo ?? string.Empty },
            [Color] = new { stringValue = vehiculo.Color ?? string.Empty },
            [Anio] = new { integerValue = vehiculo.Anio },
            [Tipo] = new { stringValue = vehiculo.Tipo ?? string.Empty },
            [NombreVisual] = new { stringValue = vehiculo.NombreVisual }
        };
    }

    public static Vehiculo? ParseFromDocument(JsonElement doc)
    {
        if (!doc.TryGetProperty("fields", out var f)) return null;

        var name = doc.TryGetProperty("name", out var n) ? n.GetString() : null;
        var id = ReadString(f, Id);
        if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
        {
            id = name.Split('/').Last();
        }

        return new Vehiculo
        {
            Id = id ?? string.Empty,
            UsuarioId = ReadString(f, UsuarioId) ?? string.Empty,
            ClienteId = ReadString(f, ClienteId) ?? string.Empty,
            Placa = ReadString(f, Placa) ?? ReadString(f, PlacasLegacy) ?? string.Empty,
            Marca = ReadString(f, Marca) ?? string.Empty,
            Modelo = ReadString(f, Modelo) ?? string.Empty,
            Color = ReadString(f, Color) ?? string.Empty,
            Anio = ReadInt(f, Anio),
            Tipo = ReadString(f, Tipo) ?? string.Empty
        };
    }

    public static Vehiculo? ParseFromQueryRow(JsonElement row)
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
}
