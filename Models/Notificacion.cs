using System;

namespace AutoLavadoApp.Models
{
    public class Notificacion
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public bool Leida { get; set; }
        public string CitaId { get; set; } = string.Empty;
    }
}
