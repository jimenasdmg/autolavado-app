namespace AutoLavadoApp.Models
{
    public class UserSession
    {
        public string Uid { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
    }
}
