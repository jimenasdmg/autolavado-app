using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoLavadoApp.Services
{
    public sealed class FirebaseServiceException : Exception
    {
        public FirebaseServiceException(string userMessage, string errorCode = "FIREBASE_ERROR", Exception? innerException = null)
            : base(userMessage, innerException)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }

    public static class FirebaseErrorMapper
    {
        public static string MapearErrorAuth(string responseBody)
        {
            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (document.RootElement.TryGetProperty("error", out var errorElement) &&
                    errorElement.TryGetProperty("message", out var messageElement))
                {
                    var message = messageElement.GetString() ?? string.Empty;
                    return message switch
                    {
                        "EMAIL_NOT_FOUND" => "No encontramos una cuenta con ese correo.",
                        "INVALID_PASSWORD" => "La contraseña no es correcta.",
                        "INVALID_LOGIN_CREDENTIALS" => "Correo o contraseña inválidos.",
                        "USER_DISABLED" => "Tu cuenta está deshabilitada. Contacta soporte.",
                        "EMAIL_EXISTS" => "Este correo ya está registrado.",
                        "INVALID_EMAIL" => "El correo electrónico no es válido.",
                        "WEAK_PASSWORD : Password should be at least 6 characters" => "La contraseña debe tener al menos 6 caracteres.",
                        _ => "No pudimos completar la autenticación. Intenta nuevamente."
                    };
                }
            }
            catch
            {
            }

            return "No fue posible autenticar con Firebase.";
        }

        public static FirebaseServiceException MapearErrorHttp(HttpStatusCode statusCode, string? responseBody, string defaultMessage)
        {
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                return new FirebaseServiceException($"HTTP {(int)statusCode} ({statusCode})\n{responseBody}", "FIREBASE_HTTP_ERROR");
            }

            if (statusCode == HttpStatusCode.Forbidden || statusCode == HttpStatusCode.Unauthorized)
            {
                return new FirebaseServiceException($"HTTP {(int)statusCode} ({statusCode})", "PERMISSION_DENIED");
            }

            if (statusCode == HttpStatusCode.RequestTimeout || statusCode == HttpStatusCode.GatewayTimeout)
            {
                return new FirebaseServiceException($"HTTP {(int)statusCode} ({statusCode})", "TIMEOUT");
            }

            if ((int)statusCode >= 500)
            {
                return new FirebaseServiceException($"HTTP {(int)statusCode} ({statusCode})", "SERVER_UNAVAILABLE");
            }

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try
                {
                    using var document = JsonDocument.Parse(responseBody);
                    if (document.RootElement.TryGetProperty("error", out var errorElement))
                    {
                        var code = errorElement.TryGetProperty("status", out var statusElement)
                            ? statusElement.GetString() ?? "FIREBASE_ERROR"
                            : "FIREBASE_ERROR";

                        var message = errorElement.TryGetProperty("message", out var messageElement)
                            ? messageElement.GetString() ?? string.Empty
                            : string.Empty;

                        if (code == "PERMISSION_DENIED")
                        {
                            return new FirebaseServiceException($"[{code}] {message}", code);
                        }

                        if (code == "DEADLINE_EXCEEDED")
                        {
                            return new FirebaseServiceException($"[{code}] {message}", code);
                        }

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            return new FirebaseServiceException($"[{code}] {message}", code);
                        }
                    }
                }
                catch
                {
                }
            }

            return new FirebaseServiceException($"HTTP {(int)statusCode} ({statusCode}) - {defaultMessage}", "FIREBASE_ERROR");
        }

        public static FirebaseServiceException MapearExcepcion(Exception ex, string defaultMessage)
        {
            if (ex is FirebaseServiceException firebaseEx)
            {
                return firebaseEx;
            }

            if (ex is TaskCanceledException)
            {
                return new FirebaseServiceException(ex.ToString(), "TIMEOUT", ex);
            }

            if (ex is HttpRequestException)
            {
                return new FirebaseServiceException(ex.ToString(), "NO_INTERNET", ex);
            }

            return new FirebaseServiceException(ex.ToString(), "UNEXPECTED_ERROR", ex);
        }
    }
}
