using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;
using NexoraApp.Services;

namespace NexoraApp.ViewModels
{
    // Sincronización con el backend (Clase 13) y con la base local SQLite (Clase 14)
    public partial class SincronizacionViewModel : ViewModelGlobal
    {
        private readonly ISincronizacionService servicio;

        [ObservableProperty] private string urlServidor = Ajustes.UrlServidor;
        [ObservableProperty] private string estadoConexion = "";
        [ObservableProperty] private Color colorConexion = Colors.Gray;
        [ObservableProperty] private string tareasLocales = "";
        [ObservableProperty] private string mensaje = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarCommand))]
        [NotifyCanExecuteChangedFor(nameof(TraerCommand))]
        private bool ocupado;

        // El servicio llega por inyección de dependencias
        public SincronizacionViewModel(ISincronizacionService servicio)
        {
            this.servicio = servicio;
        }

        // La página lo llama al aparecer y al desaparecer
        public void Iniciar()
        {
            ActualizarConexion();
            ContarLocales();
            Connectivity.Current.ConnectivityChanged += OnConectividadCambio;
        }

        public void Detener()
        {
            Connectivity.Current.ConnectivityChanged -= OnConectividadCambio;
        }

        private void OnConectividadCambio(object? sender, ConnectivityChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(ActualizarConexion);
        }

        // ---------- Conectividad (Connectivity.Current) ----------
        private static bool HayConexion() => Connectivity.Current.NetworkAccess != NetworkAccess.None;

        private void ActualizarConexion()
        {
            if (!HayConexion())
            {
                EstadoConexion = "Sin conexión";
                ColorConexion = Color.FromArgb("#D32F2F");
                return;
            }

            var perfiles = Connectivity.Current.ConnectionProfiles.ToList();
            var tipo = perfiles.Contains(ConnectionProfile.WiFi) ? "Wi-Fi"
                : perfiles.Contains(ConnectionProfile.Cellular) ? "Datos móviles"
                : perfiles.Contains(ConnectionProfile.Ethernet) ? "Ethernet"
                : "Conectado";
            EstadoConexion = tipo;
            ColorConexion = Color.FromArgb("#06923E");
        }

        private void ContarLocales()
        {
            var dbContext = new NexoraDbContext();
            TareasLocales = $"{dbContext.Tareas.Count()} tareas en este dispositivo";
        }

        private bool PuedeSincronizar() => !Ocupado;

        // ---------- Enviar: base local -> servidor ----------
        [RelayCommand(CanExecute = nameof(PuedeSincronizar))]
        private async Task EnviarAsync()
        {
            if (!HayConexion())
            {
                Mensaje = "No hay conexión a la red. Inténtalo cuando vuelva.";
                return;
            }

            Ocupado = true;
            Mensaje = "Enviando…";
            try
            {
                Ajustes.UrlServidor = UrlServidor.Trim();

                var dbContext = new NexoraDbContext();
                var tareas = dbContext.Tareas.ToList()
                    .Select(t => new TareaDto(t.Id, t.Titulo, t.Descripcion, t.Prioridad, t.Estado,
                        t.FechaLimite, t.ProyectoId, t.UsuarioId, t.FechaCompletada))
                    .ToList();

                var enviadas = await servicio.EnviarTareasAsync(Ajustes.UrlServidor, tareas);
                Mensaje = $"Listo: {enviadas} tareas enviadas al servidor.";
            }
            catch (Exception ex)
            {
                Mensaje = MensajeDeError(ex);
            }
            finally
            {
                Ocupado = false;
            }
        }

        // ---------- Traer: servidor -> base local (SQLite) ----------
        [RelayCommand(CanExecute = nameof(PuedeSincronizar))]
        private async Task TraerAsync()
        {
            if (!HayConexion())
            {
                Mensaje = "No hay conexión a la red. Inténtalo cuando vuelva.";
                return;
            }

            Ocupado = true;
            Mensaje = "Descargando…";
            try
            {
                Ajustes.UrlServidor = UrlServidor.Trim();
                var remotas = await servicio.ObtenerTareasAsync(Ajustes.UrlServidor);

                var dbContext = new NexoraDbContext();
                var proyectos = dbContext.Proyectos.Select(p => p.Id).ToHashSet();
                var usuarios = dbContext.Usuarios.Select(u => u.Id).ToHashSet();

                int nuevas = 0, actualizadas = 0, omitidas = 0;
                foreach (var dto in remotas)
                {
                    // Solo se guardan tareas cuyo proyecto y responsable existen en este dispositivo
                    if (!proyectos.Contains(dto.ProyectoId) || !usuarios.Contains(dto.UsuarioId))
                    {
                        omitidas++;
                        continue;
                    }

                    var datos = new Tarea(dto.Id, dto.Titulo, dto.Descripcion, dto.Prioridad, dto.Estado,
                        dto.FechaLimite, dto.ProyectoId, dto.UsuarioId)
                    { FechaCompletada = dto.FechaCompletada };

                    var existente = dbContext.Tareas.FirstOrDefault(t => t.Id == dto.Id);
                    if (existente == null)
                    {
                        dbContext.Tareas.Add(datos);
                        nuevas++;
                    }
                    else
                    {
                        dbContext.Entry(existente).CurrentValues.SetValues(datos);
                        actualizadas++;
                    }
                }
                dbContext.SaveChanges();

                Mensaje = $"Listo: {nuevas} nuevas, {actualizadas} actualizadas, {omitidas} omitidas.";
                ContarLocales();
            }
            catch (Exception ex)
            {
                Mensaje = MensajeDeError(ex);
            }
            finally
            {
                Ocupado = false;
            }
        }

        private static string MensajeDeError(Exception ex) => ex switch
        {
            TaskCanceledException => "El servidor no respondió a tiempo. Revisa la dirección y que el contenedor esté en marcha.",
            HttpRequestException => "No se pudo conectar con el servidor. Revisa la dirección y que el contenedor esté en marcha.",
            _ => $"Ocurrió un error: {ex.Message}"
        };
    }
}
