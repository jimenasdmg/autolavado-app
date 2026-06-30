namespace AutoLavadoApp.Models
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? UserId { get; set; }
    public string? IdToken { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
