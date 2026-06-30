using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutoLavadoApp.Models;
using AutoLavadoApp.Repositories.Interfaces;
using AutoLavadoApp.Services;

namespace AutoLavadoApp.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private readonly SessionService _sessionService;
        private readonly UsuarioService _usuarioService;
        private readonly IServicioRepository _servicioRepository;
        private readonly ICitaRepository _citaRepository;
        private readonly NotificacionService _notificacionService;
        private string _nombreUsuario = string.Empty;
        private int _notificacionesNoLeidas;

        public event PropertyChangedEventHandler? PropertyChanged;

        public HomeViewModel(
            SessionService sessionService,
            UsuarioService usuarioService,
            IServicioRepository servicioRepository,
            ICitaRepository citaRepository,
            NotificacionService notificacionService)
        {
            _sessionService = sessionService;
            _usuarioService = usuarioService;
            _servicioRepository = servicioRepository;
            _citaRepository = citaRepository;
            _notificacionService = notificacionService;

            Servicios = new ObservableCollection<ServicioItem>();

            ProximaCita = new ProximaCitaItem();

            NombreUsuario = "Cliente";
        }

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set => SetProperty(ref _nombreUsuario, value);
        }

        public string MensajeSaludo => $"Hola, {NombreUsuario}";

        public int NotificacionesNoLeidas
        {
            get => _notificacionesNoLeidas;
            private set
            {
                if (SetProperty(ref _notificacionesNoLeidas, value))
                {
                    OnPropertyChanged(nameof(TieneNotificacionesNoLeidas));
                    OnPropertyChanged(nameof(NotificacionesNoLeidasTexto));
                }
            }
        }

        public bool TieneNotificacionesNoLeidas => NotificacionesNoLeidas > 0;
        public string NotificacionesNoLeidasTexto => NotificacionesNoLeidas > 9 ? "9+" : NotificacionesNoLeidas.ToString();

        public ObservableCollection<ServicioItem> Servicios { get; }
        public bool TieneServicios => Servicios.Count > 0;
        public bool NoTieneServicios => !TieneServicios;

        public ProximaCitaItem ProximaCita { get; }

        public async Task CargarUsuarioAsync()
        {
            try
            {
                var uid = _sessionService.ObtenerUid();
                var idToken = await _sessionService.ObtenerTokenAsync();

                await CargarServiciosAsync(idToken);

                if (string.IsNullOrWhiteSpace(uid))
                {
                    NombreUsuario = "Cliente";
                    NotificacionesNoLeidas = 0;
                    return;
                }

                var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(uid, idToken);
                if (usuario == null)
                {
                    NombreUsuario = "Cliente";
                    return;
                }

                var nombreCompleto = $"{usuario.Nombre} {usuario.ApellidoPaterno}".Trim();
                NombreUsuario = string.IsNullOrWhiteSpace(nombreCompleto) ? "Cliente" : nombreCompleto;
                NotificacionesNoLeidas = await _notificacionService.ObtenerNoLeidasCountAsync(uid, idToken);

                var citas = await _citaRepository.ObtenerPorUsuarioAsync(uid, idToken);
                var proxima = citas
                    .Where(c => c.Fecha.Date >= DateTime.Today && !string.Equals(c.Estado, EstadosCita.Cancelada, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.Fecha)
                    .ThenBy(c => c.Hora)
                    .FirstOrDefault();

                if (proxima is not null)
                {
                    ProximaCita.Fecha = proxima.Fecha;
                    ProximaCita.Hora = proxima.Hora;
                    ProximaCita.Servicio = proxima.Servicio;
                    ProximaCita.Estado = proxima.EstadoTexto;
                }
                else
                {
                    ProximaCita.Fecha = DateTime.Today;
                    ProximaCita.Hora = "Sin hora";
                    ProximaCita.Servicio = "Sin próximas citas";
                    ProximaCita.Estado = "Pendiente";
                }

                OnPropertyChanged(nameof(ProximaCita));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HOME][CARGAR][ERROR] {ex}");
                NombreUsuario = "Cliente";
                NotificacionesNoLeidas = 0;
            }
        }

        private async Task CargarServiciosAsync(string idToken)
        {
            var servicios = await _servicioRepository.ObtenerActivosAsync(idToken);
            Servicios.Clear();

            // Always use a FontAwesome glyph for services and ignore any image stored in the DB
            foreach (var servicio in servicios.OrderBy(x => x.Precio))
            {
                Servicios.Add(new ServicioItem
                {
                    // Use the droplet glyph as default service icon
                    Icono = "\uf043",
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    Precio = $"${servicio.Precio:N2}",
                    Duracion = $"{servicio.DuracionMinutos} min",
                    EsImagenRemota = false
                });
            }

            OnPropertyChanged(nameof(TieneServicios));
            OnPropertyChanged(nameof(NoTieneServicios));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);

            if (propertyName == nameof(NombreUsuario))
            {
                OnPropertyChanged(nameof(MensajeSaludo));
            }

            return true;
        }
    }

    public class ServicioItem
    {
        public string Icono { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Precio { get; set; } = string.Empty;
        public string Duracion { get; set; } = string.Empty;
        public bool EsImagenRemota { get; set; }
        public bool MostrarIcono => !EsImagenRemota;
        public bool MostrarImagen => EsImagenRemota;
    }

    public class ProximaCitaItem
    {
        public DateTime Fecha { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoColor => Estado switch
        {
            "Finalizada" => "#16A34A",
            "Empleado asignado" => "#2563EB",
            "En camino" => "#7C3AED",
            "Lavando" => "#6D28D9",
            "Cancelada" => "#DC2626",
            _ => "#F59E0B"
        };
    }
}
