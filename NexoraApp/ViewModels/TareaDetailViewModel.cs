using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;
using NexoraApp.Services;
using NexoraApp.Views;

namespace NexoraApp.ViewModels
{
    // ViewModel de la pantalla Detalle de tarea (Clase 11).
    // Es "partial" porque el Source Generator del Toolkit completa la clase:
    //  - [ObservableProperty] genera la propiedad pública y su notificación
    //  - [RelayCommand] genera el comando ICommand de cada método
    public partial class TareaDetailViewModel : ViewModelGlobal, IQueryAttributable
    {
        private readonly INavegacionService navegacion;
        private bool cargando;

        [ObservableProperty] private string titulo = "";
        [ObservableProperty] private string descripcion = "";
        [ObservableProperty] private string estado = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PrioridadTexto))]
        private string prioridad = "";

        [ObservableProperty] private string proyecto = "";
        [ObservableProperty] private string responsable = "";
        [ObservableProperty] private string iniciales = "";
        [ObservableProperty] private Color avatarColor = Colors.Gray;
        [ObservableProperty] private string fechaTexto = "";
        [ObservableProperty] private string subtareasContador = "0/0";
        [ObservableProperty] private double subtareasProgreso;

        // Al cambiar el texto se vuelve a evaluar CanExecute del comando
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AgregarSubtareaCommand))]
        private string nuevaSubtarea = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EnviarComentarioCommand))]
        private string nuevoComentario = "";

        public int TareaId { get; private set; }
        public int ProyectoId { get; private set; }
        public string PrioridadTexto => $"Prioridad {Prioridad.ToLower()}";

        public ObservableCollection<SubtareaItem> Subtareas { get; } = new();
        public ObservableCollection<ComentarioItem> Comentarios { get; } = new();

        // El contenedor de dependencias entrega el servicio al crear el ViewModel
        public TareaDetailViewModel(INavegacionService navegacion)
        {
            this.navegacion = navegacion;
        }

        // El ViewModel (dueño de los datos) recibe el parámetro ?id= de la navegación
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            TareaId = int.Parse(query["id"].ToString()!);
        }

        // ---------- Cargar ----------
        public void Cargar()
        {
            var dbContext = new NexoraDbContext();
            var tarea = dbContext.Tareas
                .Include(t => t.Proyecto)
                .Include(t => t.Usuario)
                .FirstOrDefault(t => t.Id == TareaId);
            if (tarea == null) return;

            Titulo = tarea.Titulo;
            Descripcion = tarea.Descripcion;
            Estado = tarea.Estado;
            Prioridad = tarea.Prioridad;
            ProyectoId = tarea.ProyectoId;
            Proyecto = tarea.Proyecto.Nombre;
            Responsable = tarea.Usuario.Nombre;
            Iniciales = Estilos.Iniciales(tarea.Usuario.Nombre);
            AvatarColor = Estilos.ColorAvatar(tarea.UsuarioId);
            FechaTexto = $"{tarea.FechaLimite:dd/MM/yyyy}  ·  {Estilos.TextoVencimiento(tarea.FechaLimite, tarea.Estado)}";

            CargarSubtareas(dbContext);
            CargarComentarios(dbContext);
        }

        public void CambiarEstado(string nuevo)
        {
            var dbContext = new NexoraDbContext();
            var tarea = dbContext.Tareas.First(t => t.Id == TareaId);
            dbContext.Entry(tarea).Property(t => t.Estado).CurrentValue = nuevo;
            tarea.FechaCompletada = nuevo == "Hecha" ? DateTime.Now : null;
            dbContext.SaveChanges();
            Cargar();
        }

        // ---------- Subtareas ----------
        private void CargarSubtareas(NexoraDbContext dbContext)
        {
            cargando = true;
            foreach (var anterior in Subtareas) anterior.PropertyChanged -= OnSubtareaCambio;
            Subtareas.Clear();

            foreach (var s in dbContext.Subtareas.Where(s => s.TareaId == TareaId).OrderBy(s => s.Id).ToList())
                Subtareas.Add(CrearItem(s.Id, s.Titulo, s.Completada));

            cargando = false;
            ActualizarProgreso();
        }

        private SubtareaItem CrearItem(int id, string texto, bool completada)
        {
            var item = new SubtareaItem(id, texto, completada) { EliminarCommand = EliminarSubtareaCommand };
            item.PropertyChanged += OnSubtareaCambio;
            return item;
        }

        private void OnSubtareaCambio(object? sender, PropertyChangedEventArgs e)
        {
            if (cargando || sender is not SubtareaItem item || e.PropertyName != nameof(SubtareaItem.Completada)) return;

            var dbContext = new NexoraDbContext();
            var subtarea = dbContext.Subtareas.First(s => s.Id == item.Id);
            dbContext.Entry(subtarea).Property(s => s.Completada).CurrentValue = item.Completada;
            dbContext.SaveChanges();
            ActualizarProgreso();
        }

        private void ActualizarProgreso()
        {
            var hechas = Subtareas.Count(s => s.Completada);
            SubtareasContador = $"{hechas}/{Subtareas.Count}";
            SubtareasProgreso = Subtareas.Count == 0 ? 0 : (double)hechas / Subtareas.Count;
        }

        private bool PuedeAgregarSubtarea() => !string.IsNullOrWhiteSpace(NuevaSubtarea);

        [RelayCommand(CanExecute = nameof(PuedeAgregarSubtarea))]
        private void AgregarSubtarea()
        {
            var texto = NuevaSubtarea.Trim();

            var dbContext = new NexoraDbContext();
            var id = (dbContext.Subtareas.Any() ? dbContext.Subtareas.Max(s => s.Id) : 0) + 1;
            dbContext.Subtareas.Add(new Subtarea(id, TareaId, texto, false));
            dbContext.SaveChanges();

            Subtareas.Add(CrearItem(id, texto, false));
            ActualizarProgreso();
            NuevaSubtarea = "";
        }

        // El parámetro llega desde el XAML con CommandParameter="{Binding Id}"
        [RelayCommand]
        private void EliminarSubtarea(int id)
        {
            var dbContext = new NexoraDbContext();
            var subtarea = dbContext.Subtareas.FirstOrDefault(s => s.Id == id);
            if (subtarea != null)
            {
                dbContext.Subtareas.Remove(subtarea);
                dbContext.SaveChanges();
            }

            var item = Subtareas.FirstOrDefault(s => s.Id == id);
            if (item != null)
            {
                item.PropertyChanged -= OnSubtareaCambio;
                Subtareas.Remove(item);
            }
            ActualizarProgreso();
        }

        // ---------- Comentarios ----------
        private void CargarComentarios(NexoraDbContext dbContext)
        {
            Comentarios.Clear();
            foreach (var c in dbContext.Comentarios.Include(c => c.Usuario)
                         .Where(c => c.TareaId == TareaId).OrderBy(c => c.Fecha).ToList())
                Comentarios.Add(ComoItem(c, c.Usuario.Nombre));
        }

        private static ComentarioItem ComoItem(Comentario c, string autor) =>
            new(Estilos.Iniciales(autor), Estilos.ColorAvatar(c.UsuarioId), autor, c.Texto,
                c.Fecha.ToString("d MMM, HH:mm", new CultureInfo("es-ES")));

        private bool PuedeEnviarComentario() => !string.IsNullOrWhiteSpace(NuevoComentario);

        [RelayCommand(CanExecute = nameof(PuedeEnviarComentario))]
        private void EnviarComentario()
        {
            var dbContext = new NexoraDbContext();
            var id = (dbContext.Comentarios.Any() ? dbContext.Comentarios.Max(c => c.Id) : 0) + 1;
            var comentario = new Comentario(id, TareaId, Sesion.UsuarioActualId, NuevoComentario.Trim(), DateTime.Now);
            dbContext.Comentarios.Add(comentario);
            dbContext.SaveChanges();

            var autor = dbContext.Usuarios.First(u => u.Id == Sesion.UsuarioActualId).Nombre;
            Comentarios.Add(ComoItem(comentario, autor));
            NuevoComentario = "";
        }

        // ---------- Navegación (por el servicio, no por Shell) ----------
        [RelayCommand]
        private async Task IrAProyectoAsync()
        {
            if (ProyectoId == 0) return;
            await navegacion.IrAAsync($"{nameof(ProyectoDetailPage)}?id={ProyectoId}");
        }

        [RelayCommand]
        private async Task EditarTareaAsync()
        {
            await navegacion.IrAAsync($"{nameof(TareaFormPage)}?id={TareaId}");
        }

        [RelayCommand]
        private async Task EliminarTareaAsync()
        {
            var confirmar = await navegacion.ConfirmarAsync("Eliminar tarea",
                "Se eliminarán también sus subtareas y comentarios. Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");
            if (!confirmar) return;

            var dbContext = new NexoraDbContext();
            dbContext.Subtareas.RemoveRange(dbContext.Subtareas.Where(s => s.TareaId == TareaId));
            dbContext.Comentarios.RemoveRange(dbContext.Comentarios.Where(c => c.TareaId == TareaId));
            var tarea = dbContext.Tareas.FirstOrDefault(t => t.Id == TareaId);
            if (tarea != null) dbContext.Tareas.Remove(tarea);
            dbContext.SaveChanges();

            await navegacion.VolverAsync();
        }
    }
}
