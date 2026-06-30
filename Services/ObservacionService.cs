using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoLavadoApp.Services.Core;

namespace AutoLavadoApp.Services.Data
{
    public class ObservacionService : FirestoreBaseService
    {
        private const string ColeccionObservaciones = "observaciones";
        private readonly HttpClient _http;

        public ObservacionService(HttpClient httpClient)
            : base(httpClient)
        {
            _http = httpClient;
        }

        public Task<bool> GuardarObservacionAsync(string citaId, string empleadoId, string comentario, DateTime fecha)
        {
            var datos = new Dictionary<string, object>
            {
                { "citaId", citaId },
                { "empleadoId", empleadoId },
                { "comentario", comentario },
                { "fecha", fecha }
            };

            return CrearDocumentoAsync(ColeccionObservaciones, datos);
        }

        public async Task<List<ObservacionResumen>> ObtenerObservacionesPorCitaAsync(string citaId)
        {
            var response = await _http.GetAsync(BuildDocumentUrl(ColeccionObservaciones));
            if (!response.IsSuccessStatusCode)
            {
                return new List<ObservacionResumen>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            var observaciones = new List<ObservacionResumen>();
            if (!document.RootElement.TryGetProperty("documents", out var docs) || docs.ValueKind != JsonValueKind.Array)
            {
                return observaciones;
            }

            foreach (var doc in docs.EnumerateArray())
            {
                if (!doc.TryGetProperty("fields", out var fields))
                {
                    continue;
                }

                var citaIdDoc = LeerString(fields, "citaId") ?? string.Empty;
                if (!string.Equals(citaIdDoc, citaId, StringComparison.Ordinal))
                {
                    continue;
                }

                observaciones.Add(new ObservacionResumen
                {
                    Id = ObtenerIdDocumento(doc),
                    CitaId = citaIdDoc,
                    EmpleadoId = LeerString(fields, "empleadoId") ?? string.Empty,
                    Comentario = LeerString(fields, "comentario") ?? string.Empty,
                    Fecha = LeerFecha(fields, "fecha")
                });
            }

            return observaciones
                .OrderBy(o => o.Fecha)
                .ToList();
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

        private static DateTime LeerFecha(JsonElement fields, string propiedad)
        {
            if (!fields.TryGetProperty(propiedad, out var propiedadElement))
            {
                return DateTime.Now;
            }

            if (propiedadElement.TryGetProperty("timestampValue", out var timestampElement))
            {
                var raw = timestampElement.GetString();
                if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
                {
                    return parsed.ToLocalTime();
                }
            }

            return DateTime.Now;
        }
    }

    public class ObservacionResumen
    {
        public string Id { get; set; } = string.Empty;
        public string CitaId { get; set; } = string.Empty;
        public string EmpleadoId { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
