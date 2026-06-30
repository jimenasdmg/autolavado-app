using System.ComponentModel;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AutoLavadoApp.Constants;
using AutoLavadoApp.Services.Auth;
using AutoLavadoApp.Services.Core;
using AutoLavadoApp.Services.Data;
using AutoLavadoApp.Views.Auth;

namespace AutoLavadoApp.ViewModels.Auth;

public class AutenticacionViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private readonly SessionService _sessionService;
    private readonly UsuarioService _usuarioService;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private bool _isPasswordHidden = true;
    private string _errorMessage = string.Empty;
    private string _emailError = string.Empty;
    private string _passwordError = string.Empty;

    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
            }
        }
    }

    public object? UsuarioActual { get; private set; }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanInteract));
            }
        }
    }

    public bool CanInteract => !IsBusy;

    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set
        {
            if (_isPasswordHidden != value)
            {
                _isPasswordHidden = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PasswordEyeIcon));
            }
        }
    }

    public string PasswordEyeIcon => IsPasswordHidden ? "\uf06e" : "\uf070";

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string EmailError
    {
        get => _emailError;
        private set
        {
            if (_emailError != value)
            {
                _emailError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEmailError));
            }
        }
    }

    public bool HasEmailError => !string.IsNullOrWhiteSpace(EmailError);

    public string PasswordError
    {
        get => _passwordError;
        private set
        {
            if (_passwordError != value)
            {
                _passwordError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPasswordError));
            }
        }
    }

    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordError);

    // Compatibilidad con XAML/flujo existente
    public ICommand IniciarSesionCommand { get; }
    public ICommand IrARegistroCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }

    public string RolActual => _sessionService.ObtenerRol();

    public AutenticacionViewModel(AuthService authService, SessionService sessionService, UsuarioService usuarioService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _usuarioService = usuarioService;

        IniciarSesionCommand = new Command(async () => await LoginAsync(Email, Password));
        TogglePasswordVisibilityCommand = new Command(() => IsPasswordHidden = !IsPasswordHidden);
        IrARegistroCommand = new Command(async () =>
        {
            if (Application.Current?.Windows.Count > 0)
            {
                var page = Application.Current.Windows[0].Page;
                if (page is NavigationPage nav)
                    await nav.Navigation.PushAsync(new RegisterPage());
            }
        });
    }

    // Constructor de respaldo para evitar errores en instancias legacy sin DI
    public AutenticacionViewModel()
        : this(
            IPlatformApplication.Current?.Services.GetService<AuthService>()
                ?? throw new InvalidOperationException("AuthService no registrado en DI."),
            IPlatformApplication.Current?.Services.GetService<SessionService>()
                ?? throw new InvalidOperationException("SessionService no registrado en DI."),
            IPlatformApplication.Current?.Services.GetService<UsuarioService>()
                ?? throw new InvalidOperationException("UsuarioService no registrado en DI."))
    {
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        if (IsBusy) return false;

        ErrorMessage = string.Empty;
        EmailError = string.Empty;
        PasswordError = string.Empty;

        Email = email?.Trim() ?? string.Empty;
        Password = password ?? string.Empty;

        if (!ValidarCampos()) return false;

        try
        {
            IsBusy = true;

            var authOk = await InvokeAuthLoginAsync(Email, Password);
            if (!authOk)
            {
                ErrorMessage = "Correo o contraseña incorrectos";
                return false;
            }

            var uid = ReadStringFromAuth("Uid", "LocalId", "UserId", "uid", "localId") ?? string.Empty;
            var idToken = ReadStringFromAuth("IdToken", "idToken", "Token", "AccessToken") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(idToken))
            {
                ErrorMessage = "No se pudo completar la sesión con Firebase Authentication.";
                return false;
            }

            UsuarioActual = await _usuarioService.ObtenerUsuarioPorIdAsync(uid, idToken)
                ?? await _usuarioService.ObtenerPorEmailAsync(Email, idToken)
                ?? await TryResolveUsuarioAsync(Email);

            var uidPerfil = GetStringProperty(UsuarioActual, "Uid", "uid", "Id") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(uidPerfil) && !string.Equals(uidPerfil, uid, StringComparison.Ordinal))
            {
                ErrorMessage = "La cuenta autenticada no coincide con el perfil de Firestore. Contacta al administrador.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(idToken))
            {
                idToken = GetStringProperty(UsuarioActual, "IdToken", "idToken", "Token", "AccessToken") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(idToken))
            {
                idToken = await _sessionService.ObtenerTokenAsync();
            }

            var nombre = GetStringProperty(UsuarioActual, "Nombre", "Name", "DisplayName") ?? string.Empty;
            var correoSesion = GetStringProperty(UsuarioActual, "Correo", "Email", "correo", "email") ?? Email;

            var rolFirestore = !string.IsNullOrWhiteSpace(uid)
                ? await _usuarioService.ObtenerRolPorUidAsync(uid, idToken)
                : null;
            var rolUsuario = GetStringProperty(UsuarioActual, "Rol", "rol");
            var rol = !string.IsNullOrWhiteSpace(rolFirestore)
                ? rolFirestore
                : rolUsuario;

            if (string.IsNullOrWhiteSpace(rol))
            {
                ErrorMessage = "No se pudo determinar el rol del usuario desde Firestore.";
                return false;
            }

            rol = rol.Trim().ToLowerInvariant();

            // Bloquear inicio de sesión para empleados desactivados en Firestore
            try
            {
                // Intentar leer el perfil desde el servicio de usuarios; si no está disponible, usar UsuarioActual
                var perfil = await _usuarioService.ObtenerUsuarioPorIdAsync(uid, idToken) ?? UsuarioActual;
                if (perfil != null)
                {
                    var prop = perfil.GetType().GetProperty("Activo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        var activoObj = prop.GetValue(perfil);
                        var activo = activoObj is bool b ? b
                                     : (bool.TryParse(activoObj?.ToString(), out var parsed) ? parsed : true);
                        if (!activo && string.Equals(rol, Roles.Empleado, StringComparison.OrdinalIgnoreCase))
                        {
                            ErrorMessage = "Tu cuenta ha sido desactivada. Contacta al administrador.";
                            return false;
                        }
                    }
                }
            }
            catch
            {
                // Si falla la lectura del perfil, no bloquear el inicio por este motivo
            }

            await _sessionService.GuardarSesionAsync(uid, correoSesion, idToken, rol, nombre);
            OnPropertyChanged(nameof(RolActual));
            return true;
        }
        catch (AutoLavadoApp.Services.FirebaseServiceException ex)
        {
            ErrorMessage = MapearErrorFirebase(ex.Message);
            return false;
        }
        catch
        {
            ErrorMessage = "Ocurrió un error al iniciar sesión. Intenta nuevamente.";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidarCampos()
    {
        var valido = true;

        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Ingresa tu correo";
            valido = false;
        }
        else if (!EsEmailValido(Email))
        {
            EmailError = "Correo inválido";
            valido = false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Ingresa tu contraseña";
            valido = false;
        }
        else if (Password.Length < 6)
        {
            PasswordError = "La contraseña debe tener al menos 6 caracteres";
            valido = false;
        }

        return valido;
    }

    private static bool EsEmailValido(string correo)
    {
        try
        {
            var m = new MailAddress(correo);
            return string.Equals(m.Address, correo.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string MapearErrorFirebase(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "No fue posible iniciar sesión.";
        var m = raw.ToLowerInvariant();
        if (m.Contains("invalid-credential") || m.Contains("invalid_login_credentials")) return "Correo o contraseña incorrectos";
        if (m.Contains("email-already-in-use") || m.Contains("email_exists")) return "Ese correo ya está registrado";
        if (m.Contains("weak-password") || m.Contains("password should be at least")) return "La contraseña es demasiado débil";
        return raw;
    }

    private async Task<bool> InvokeAuthLoginAsync(string email, string password)
    {
        // Firebase Auth: nunca persistir contraseñas en Firestore/modelo Usuario
        // Soporte a distintos nombres de método existentes en el proyecto
        var method = _authService.GetType().GetMethod("LoginAsync")
                     ?? _authService.GetType().GetMethod("IniciarSesionAsync")
                     ?? _authService.GetType().GetMethod("SignInAsync");

        if (method is null)
            throw new InvalidOperationException("No se encontró método de login en AuthService.");

        var result = method.Invoke(_authService, new object[] { email, password });
        if (result is Task<bool> taskBool)
            return await taskBool;

        if (result is Task task)
        {
            await task;
            var resultProp = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            var value = resultProp?.GetValue(task);
            if (value is bool b) return b;
            return true;
        }

        return result as bool? ?? false;
    }

    private async Task<object?> TryResolveUsuarioAsync(string email)
    {
        var method = _usuarioService.GetType().GetMethod("ObtenerPorEmailAsync")
                     ?? _usuarioService.GetType().GetMethod("GetByEmailAsync")
                     ?? _usuarioService.GetType().GetMethod("GetUsuarioPorEmailAsync");

        if (method is null)
            return null;

        var result = method.Invoke(_usuarioService, new object[] { email });
        if (result is Task task)
        {
            await task;
            var resultProp = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            return resultProp?.GetValue(task);
        }

        return result;
    }

    private string? ReadStringFromAuth(params string[] names)
    {
        return GetStringProperty(_authService, names);
    }

    private static string? GetStringProperty(object? obj, params string[] names)
    {
        if (obj is null) return null;
        var type = obj.GetType();
        foreach (var n in names)
        {
            var p = type.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var v = p?.GetValue(obj)?.ToString();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }
        return null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}