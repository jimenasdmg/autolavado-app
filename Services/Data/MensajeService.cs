using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoLavadoApp.Models;
using AutoLavadoApp.Services.Core;

namespace AutoLavadoApp.Services.Data;

public class MensajeService : FirestoreBaseService
{
    private const string Coleccion = "mensajes";
    private const string FirebaseProjectId = "autolavadoapp-f9a62";
    private readonly HttpClient _http;

    public MensajeService(HttpClient httpClient) : base(httpClient)
    {
        _http = httpClient;
    }

    public static string ConstruirConversacionId(string adminUid, string empleadoUid)
    {
        return string.Compare(adminUid, empleadoUid, StringComparison.Ordinal) < 0
            ? $"{adminUid}_{empleadoUid}"
            : $"{empleadoUid}_{adminUid}";
    }

    public async Task<bool> EnviarMensajeAsync(
        string adminUid,
        string remitenteId,
        string remitenteRol,
        string destinatarioId,
        string texto,
        string idToken)
    {
        if (string.IsNullOrWhiteSpace(adminUid) ||
            string.IsNullOrWhiteSpace(remitenteId) ||
            string.IsNullOrWhiteSpace(destinatarioId) ||
            string.IsNullOrWhiteSpace(texto) ||
            string.IsNullOrWhiteSpace(idToken))
            return false;

        var adminEsRemitente = remitenteId == adminUid;
        var adminEsDestinatario = destinatarioId == adminUid;

        if (adminEsRemitente == adminEsDestinatario)
        {
            Debug.WriteLine($"[MENSAJES][SEND][DENY] Conversación inválida from={remitenteId} to={destinatarioId}");
            return false;
        }

        if (remitenteRol == "admin" && !adminEsRemitente) return false;
        if (remitenteRol != "admin" && adminEsRemitente) return false;

        var empleadoUid = adminEsRemitente ? destinatarioId : remitenteId;
        var conversacionId = ConstruirConversacionId(adminUid, empleadoUid);
        var mensajeId = Guid.NewGuid().ToString("N");

        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["id"] = new { stringValue = mensajeId },
                ["conversacionId"] = new { stringValue = conversacionId },
                ["remitenteId"] = new { stringValue = remitenteId },
                ["remitenteRol"] = new { stringValue = remitenteRol },
                ["destinatarioId"] = new { stringValue = destinatarioId },
                ["mensaje"] = new { stringValue = texto.Trim() },
                ["fechaEnvio"] = new { timestampValue = DateTime.UtcNow.ToString("O") },
                ["leido"] = new { booleanValue = false }
            }
        };

        Debug.WriteLine($"[MENSAJES][SEND] conv={conversacionId} from={remitenteId} to={destinatarioId} id={mensajeId}");
        await CrearDocumentoAsync(Coleccion, body, mensajeId, idToken);
        return true;
    }

    public async Task<List<MensajeInterno>> ObtenerMensajesAsync(string conversacionId, string idToken)
    {
        if (string.IsNullOrWhiteSpace(conversacionId) || string.IsNullOrWhiteSpace(idToken))
            return new List<MensajeInterno>();

        Debug.WriteLine($"[MENSAJES][READ] conv={conversacionId}");

        var query = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = Coleccion } },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new { fieldPath = "conversacionId" },
                        op = "EQUAL",
                        value = new { stringValue = conversacionId }
                    }
                },
                orderBy = new[]
                {
                    new
                    {
                        field = new { fieldPath = "fechaEnvio" },
                        direction = "ASCENDING"
                    }
                }
            }
        };

        var rows = await RunQueryAsync(query, idToken);
        return rows.Select(MapearDesdeQuery).Where(x => x != null).Select(x => x!).ToList();
    }

    public async Task<List<string>> ObtenerConversacionesAsync(string uid, string idToken)
    {
        var recibidos = await QueryPorCampoAsync("destinatarioId", uid, idToken);
        var enviados = await QueryPorCampoAsync("remitenteId", uid, idToken);

        var conversaciones = recibidos
            .Concat(enviados)
            .Select(MapearDesdeQuery)
            .Where(m => m != null)
            .Select(m => m!.ConversacionId)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .ToList();

        Debug.WriteLine($"[MENSAJES][CONVERSACIONES] uid={uid} total={conversaciones.Count}");
        return conversaciones;
    }

    public async Task<int> ContarNoLeidosAsync(string uid, string idToken)
    {
        if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(idToken))
            return 0;

        var query = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = Coleccion } },
                where = new
                {
                    compositeFilter = new
                    {
                        op = "AND",
                        filters = new object[]
                        {
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "destinatarioId" },
                                    op = "EQUAL",
                                    value = new { stringValue = uid }
                                }
                            },
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "leido" },
                                    op = "EQUAL",
                                    value = new { booleanValue = false }
                                }
                            }
                        }
                    }
                }
            }
        };

        var rows = await RunQueryAsync(query, idToken);
        var total = rows.Count(r => MapearDesdeQuery(r) != null);
        Debug.WriteLine($"[MENSAJES][BADGE] uid={uid} noLeidos={total}");
        return total;
    }

    public async Task MarcarComoLeidoAsync(string conversacionId, string uid, string idToken)
    {
        var query = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = Coleccion } },
                where = new
                {
                    compositeFilter = new
                    {
                        op = "AND",
                        filters = new object[]
                        {
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "conversacionId" },
                                    op = "EQUAL",
                                    value = new { stringValue = conversacionId }
                                }
                            },
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "destinatarioId" },
                                    op = "EQUAL",
                                    value = new { stringValue = uid }
                                }
                            },
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "leido" },
                                    op = "EQUAL",
                                    value = new { booleanValue = false }
                                }
                            }
                        }
                    }
                }
            }
        };

        var rows = await RunQueryAsync(query, idToken);
        foreach (var row in rows)
        {
            var mensaje = MapearDesdeQuery(row);
            if (mensaje == null) continue;

            var patch = new
            {
                fields = new Dictionary<string, object>
                {
                    ["leido"] = new { booleanValue = true }
                }
            };

            Debug.WriteLine($"[MENSAJES][READ_MARK] id={mensaje.Id} uid={uid}");
            await ActualizarDocumentoAsync(Coleccion, mensaje.Id, patch, idToken);
        }
    }

    private async Task<List<JsonElement>> QueryPorCampoAsync(string campo, string valor, string idToken)
    {
        var query = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = Coleccion } },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new { fieldPath = campo },
                        op = "EQUAL",
                        value = new { stringValue = valor }
                    }
                }
            }
        };

        return await RunQueryAsync(query, idToken);
    }

    private async Task<List<JsonElement>> RunQueryAsync(object queryBody, string idToken)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents:runQuery";
        var json = JsonSerializer.Serialize(queryBody);

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(req);
        var payload = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[MENSAJES][QUERY] code={(int)response.StatusCode} body={payload}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            System.Diagnostics.Debug.WriteLine($"[MENSAJES][QUERY] code=403 body={payload}");
            return new List<JsonElement>();
        }

        if (response.StatusCode == HttpStatusCode.BadRequest &&
            (payload.Contains("FAILED_PRECONDITION", StringComparison.OrdinalIgnoreCase) ||
             payload.Contains("requires an index", StringComparison.OrdinalIgnoreCase)))
        {
            System.Diagnostics.Debug.WriteLine("[MENSAJES][QUERY] Falta índice compuesto en Firestore. Se devuelve lista vacía.");
            return new List<JsonElement>();
        }

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(payload);
        var result = new List<JsonElement>();

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var item in doc.RootElement.EnumerateArray())
            result.Add(item.Clone());

        return result;
    }

    private static MensajeInterno? MapearDesdeQuery(JsonElement row)
    {
        if (!row.TryGetProperty("document", out var document)) return null;
        if (!document.TryGetProperty("fields", out var fields)) return null;

        string GetString(string key)
            => fields.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v)
                ? v.GetString() ?? string.Empty
                : string.Empty;

        bool GetBool(string key)
            => fields.TryGetProperty(key, out var p) && p.TryGetProperty("booleanValue", out var v) &&
               (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False)
                ? v.GetBoolean()
                : false;

        DateTime GetDate(string key)
        {
            if (!fields.TryGetProperty(key, out var p)) return DateTime.UtcNow;
            if (!p.TryGetProperty("timestampValue", out var v)) return DateTime.UtcNow;
            return DateTime.TryParse(v.GetString(), out var d) ? d.ToUniversalTime() : DateTime.UtcNow;
        }

        return new MensajeInterno
        {
            Id = GetString("id"),
            ConversacionId = GetString("conversacionId"),
            RemitenteId = GetString("remitenteId"),
            RemitenteRol = GetString("remitenteRol"),
            DestinatarioId = GetString("destinatarioId"),
            Mensaje = GetString("mensaje"),
            FechaEnvio = GetDate("fechaEnvio"),
            Leido = GetBool("leido")
        };
    }
}
