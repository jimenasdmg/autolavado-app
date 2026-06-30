using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoLavadoApp.Constants;
using AutoLavadoApp.Models;
using AutoLavadoApp.Services.Core;

namespace AutoLavadoApp.Services.Data
{
    public class NotificacionService : FirestoreBaseService
    {
        private readonly HttpClient _httpClient;

        public NotificacionService(HttpClient httpClient) : base(httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CrearAsync(Notificacion notificacion, string idToken)
        {
            if (notificacion is null || string.IsNullOrWhiteSpace(idToken)) return false;

            if (string.IsNullOrWhiteSpace(notificacion.Id))
            {
                notificacion.Id = Guid.NewGuid().ToString("N");
            }

            var body = new
            {
                fields = new Dictionary<string, object>
                {
                    ["id"] = new { stringValue = notificacion.Id },
                    ["usuarioId"] = new { stringValue = notificacion.UsuarioId ?? string.Empty },
                    ["titulo"] = new { stringValue = notificacion.Titulo ?? string.Empty },
                    ["mensaje"] = new { stringValue = notificacion.Mensaje ?? string.Empty },
                    ["tipo"] = new { stringValue = notificacion.Tipo ?? string.Empty },
                    ["fecha"] = new { timestampValue = notificacion.Fecha.ToUniversalTime().ToString("O") },
                    ["leida"] = new { booleanValue = notificacion.Leida },
                    ["citaId"] = new { stringValue = notificacion.CitaId ?? string.Empty }
                }
            };

            return await CrearDocumentoAsync(FirestoreCollections.Notificaciones, body, notificacion.Id, idToken);
        }

        public async Task<List<Notificacion>> ObtenerPorUsuarioAsync(string usuarioId, string idToken)
        {
            if (string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(idToken))
            {
                return new List<Notificacion>();
            }

            var query = new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = FirestoreCollections.Notificaciones } },
                    where = new
                    {
                        fieldFilter = new
                        {
                            field = new { fieldPath = "usuarioId" },
                            op = "EQUAL",
                            value = new { stringValue = usuarioId }
                        }
                    },
                    orderBy = new[]
                    {
                        new
                        {
                            field = new { fieldPath = "fecha" },
                            direction = "DESCENDING"
                        }
                    }
                }
            };

            var rows = await RunQueryAsync(query, idToken);
            if (rows.Count == 0)
            {
                return new List<Notificacion>();
            }

            var result = new List<Notificacion>();
            foreach (var row in rows)
            {
                if (!row.TryGetProperty("document", out var document))
                {
                    continue;
                }

                var item = ParseNotificacion(document);
                if (item is null) continue;
                result.Add(item);
            }

            return result.OrderByDescending(x => x.Fecha).ToList();
        }

        public async Task<int> ObtenerNoLeidasCountAsync(string usuarioId, string idToken)
        {
            var items = await ObtenerPorUsuarioAsync(usuarioId, idToken);
            return items.Count(x => !x.Leida);
        }

        public async Task<bool> MarcarComoLeidaAsync(string notificacionId, string idToken)
        {
            if (string.IsNullOrWhiteSpace(notificacionId) || string.IsNullOrWhiteSpace(idToken)) return false;

            var body = new
            {
                fields = new Dictionary<string, object>
                {
                    ["leida"] = new { booleanValue = true }
                }
            };

            return await ActualizarDocumentoAsync(FirestoreCollections.Notificaciones, notificacionId, body, idToken);
        }

        private async Task<List<JsonElement>> RunQueryAsync(object queryBody, string idToken)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{DefaultProjectId}/databases/(default)/documents:runQuery";
            var json = JsonSerializer.Serialize(queryBody);

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _httpClient.SendAsync(req);
            var payload = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[NOTIFICACIONES][RUNQUERY][ERROR] {(int)res.StatusCode} {payload}");
                return new List<JsonElement>();
            }

            using var doc = JsonDocument.Parse(payload);
            var result = new List<JsonElement>();

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                result.Add(item.Clone());
            }

            return result;
        }

        private static Notificacion? ParseNotificacion(JsonElement doc)
        {
            if (!doc.TryGetProperty("fields", out var f)) return null;

            var id = ReadString(f, "id") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id) && doc.TryGetProperty("name", out var n))
            {
                var path = n.GetString() ?? string.Empty;
                id = path.Split('/').LastOrDefault() ?? string.Empty;
            }

            return new Notificacion
            {
                Id = id,
                UsuarioId = ReadString(f, "usuarioId") ?? string.Empty,
                Titulo = ReadString(f, "titulo") ?? string.Empty,
                Mensaje = ReadString(f, "mensaje") ?? string.Empty,
                Tipo = ReadString(f, "tipo") ?? string.Empty,
                Fecha = ReadTimestamp(f, "fecha") ?? DateTime.UtcNow,
                Leida = ReadBool(f, "leida") ?? false,
                CitaId = ReadString(f, "citaId") ?? string.Empty
            };
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

        private static DateTime? ReadTimestamp(JsonElement fields, string key)
        {
            if (!fields.TryGetProperty(key, out var v)) return null;
            if (!v.TryGetProperty("timestampValue", out var ts)) return null;
            if (!DateTime.TryParse(ts.GetString(), out var parsed)) return null;
            return parsed;
        }
    }
}
