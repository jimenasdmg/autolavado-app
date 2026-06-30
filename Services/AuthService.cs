using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AutoLavadoApp.Models;
using AutoLavadoApp.Services;

namespace AutoLavadoApp.Services.Auth;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseApiKey = "AIzaSyCa8N_kUd0RNEGxdK-ainHfk3DrfU_A4aA";
    private const string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";
    private const string SignUpUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";

    public string IdToken { get; private set; } = string.Empty;
    public string Uid { get; private set; } = string.Empty;

    public AuthService(HttpClient httpClient, UsuarioService usuarioService, SessionService sessionService)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        try
        {
            var payload = new
            {
                email = email.Trim(),
                password,
                returnSecureToken = true
            };

            var url = $"{SignInUrl}?key={FirebaseApiKey}";
            var body = JsonSerializer.Serialize(payload);
            Debug.WriteLine($"[AUTH][LOGIN] URL: {url}");
            Debug.WriteLine($"[AUTH][LOGIN] BODY: {body}");

            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[AUTH][LOGIN] RESPONSE: {(int)response.StatusCode} {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw new FirebaseServiceException(FirebaseErrorMapper.MapearErrorAuth(responseBody), "AUTH_LOGIN_FAILED");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            IdToken = root.TryGetProperty("idToken", out var tokenEl) ? tokenEl.GetString() ?? string.Empty : string.Empty;
            Uid = root.TryGetProperty("localId", out var uidEl) ? uidEl.GetString() ?? string.Empty : string.Empty;

            Debug.WriteLine($"[AUTH][LOGIN] UID: {Uid}");
            Debug.WriteLine($"[AUTH][LOGIN] TOKEN: {IdToken}");

            return !string.IsNullOrWhiteSpace(IdToken) && !string.IsNullOrWhiteSpace(Uid);
        }
        catch (FirebaseServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AUTH][LOGIN][ERROR] {ex}");
            throw FirebaseErrorMapper.MapearExcepcion(ex, "No fue posible iniciar sesión.");
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult
            {
                IsSuccess = false,
                ErrorMessage = "Email y contraseña son requeridos."
            };
        }

        try
        {
            var payload = new
            {
                email = email.Trim(),
                password,
                returnSecureToken = true
            };

            var url = $"{SignUpUrl}?key={FirebaseApiKey}";
            var body = JsonSerializer.Serialize(payload);
            Debug.WriteLine($"[AUTH][REGISTER] URL: {url}");
            Debug.WriteLine($"[AUTH][REGISTER] BODY: {body}");

            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[AUTH][REGISTER] RESPONSE: {(int)response.StatusCode} {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = FirebaseErrorMapper.MapearErrorAuth(responseBody)
                };
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            var userId = root.TryGetProperty("localId", out var uidEl) ? uidEl.GetString() ?? string.Empty : string.Empty;
            var idToken = root.TryGetProperty("idToken", out var tokenEl) ? tokenEl.GetString() ?? string.Empty : string.Empty;

            IdToken = idToken;
            Uid = userId;

            Debug.WriteLine($"[AUTH][REGISTER] UID: {Uid}");
            Debug.WriteLine($"[AUTH][REGISTER] TOKEN: {IdToken}");
            
            return new AuthResult
            {
                IsSuccess = true,
                UserId = userId,
                IdToken = idToken
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AUTH][REGISTER][ERROR] {ex}");
            return new AuthResult
            {
                IsSuccess = false,
                ErrorMessage = ex.ToString()
            };
        }
    }
}
