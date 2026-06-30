using System;

namespace AutoLavadoApp.Models;

public class MensajeInterno
{
    public string Id { get; set; } = string.Empty;
    public string ConversacionId { get; set; } = string.Empty;
    public string RemitenteId { get; set; } = string.Empty;
    public string RemitenteRol { get; set; } = string.Empty;
    public string DestinatarioId { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    public bool Leido { get; set; }

    // Compatibilidad con código anterior
    public string EmisorId
    {
        get => RemitenteId;
        set => RemitenteId = value;
    }

    public string ReceptorId
    {
        get => DestinatarioId;
        set => DestinatarioId = value;
    }

    public DateTime Fecha
    {
        get => FechaEnvio;
        set => FechaEnvio = value;
    }
}
