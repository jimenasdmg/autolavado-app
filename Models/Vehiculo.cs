namespace AutoLavadoApp.Models
{
    public class Vehiculo
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        // Compatibilidad solicitada: propiedad requerida en flujos antiguos/nuevos
        public string Placas
        {
            get => Placa;
            set => Placa = value;
        }

        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string NombreVisual => $"{Marca} {Modelo} {Placa}".Trim();

        // Compatibilidad legacy
        public string Año
        {
            get => Anio.ToString();
            set => Anio = int.TryParse(value, out var n) ? n : 0;
        }
    }
}
