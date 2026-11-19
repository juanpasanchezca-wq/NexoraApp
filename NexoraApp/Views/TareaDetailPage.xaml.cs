using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class TareaDetailPage : ContentPage, IQueryAttributable
    {
        private static readonly string[] Estados = { "Por hacer", "En progreso", "En revisión", "Hecha" };

        private readonly TareaDetailData datos;
        private readonly SelectorOpciones selectorEstado;

        public TareaDetailPage()
        {
            InitializeComponent();

            // La vista queda enlazada al modelo (Clase 8): todo lo que se ve sale de "datos"
            datos = new TareaDetailData();
            BindingContext = datos;

            selectorEstado = new SelectorOpciones(estadoGrid, Estados, 2, nuevo => datos.CambiarEstado(nuevo));
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            datos.TareaId = int.Parse(query["id"].ToString()!);
        }

        // Se recarga al volver del formulario de edición
        protected override void OnAppearing()
        {
            base.OnAppearing();
            datos.Cargar();
            selectorEstado.Seleccionar(datos.Estado);
        }

        // Del detalle de la tarea se pasa al detalle de su proyecto
        private async void OnProyectoTapped(object? sender, TappedEventArgs e)
        {
            if (datos.ProyectoId == 0) return;
            await Shell.Current.GoToAsync($"{nameof(ProyectoDetailPage)}?id={datos.ProyectoId}");
        }

        // ---------- Editar y eliminar ----------
        private async void OnEditarClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(TareaFormPage)}?id={datos.TareaId}");
        }

        private async void OnEliminarClicked(object? sender, EventArgs e)
        {
            var confirmar = await DisplayAlert("Eliminar tarea",
                "Se eliminarán también sus subtareas y comentarios. Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");
            if (!confirmar) return;

            datos.Eliminar();
            await Shell.Current.GoToAsync("..");
        }
    }

    // ------------------------------------------------------------------
    // Modelo de la vista (Clase 8): propiedades con notificación + colecciones observables
    // ------------------------------------------------------------------
    public class SubtareaItem : BindingUtilObject
    {
        private bool completada;

        public SubtareaItem(int id, string titulo, bool completada)
        {
            Id = id;
            Titulo = titulo;
            this.completada = completada;
        }

        public int Id { get; }
        public string Titulo { get; }

        // Comando compartido para quitar la subtarea (se enlaza con CommandParameter = Id)
        public ICommand? EliminarCommand { get; set; }

        // El CheckBox la modifica (TwoWay) y el modelo se entera por PropertyChanged
        public bool Completada
        {
            get => completada;
            set => SetProperty(ref completada, value);
        }
    }

    public class TareaDetailData : BindingUtilObject
    {
        private string titulo = "";
        private string descripcion = "";
        private string estado = "";
        private string prioridad = "";
        private string proyecto = "";
        private string responsable = "";
        private string iniciales = "";
        private Color avatarColor = Colors.Gray;
        private string fechaTexto = "";
        private string subtareasContador = "0/0";
        private double subtareasProgreso;
        private string nuevaSubtarea = "";
        private string nuevoComentario = "";
        private bool cargando;

        private readonly MiComando agregarSubtarea;
        private readonly MiComando enviarComentario;

        public TareaDetailData()
        {
            // Cada comando recibe: qué hacer (action) y cuándo se puede (canExecute)
            agregarSubtarea = new MiComando(
                _ => AgregarSubtarea(),
                _ => !string.IsNullOrWhiteSpace(NuevaSubtarea));
            enviarComentario = new MiComando(
                _ => EnviarComentario(),
                _ => !string.IsNullOrWhiteSpace(NuevoComentario));
            EliminarSubtareaCommand = new MiComando(parametro => EliminarSubtarea(parametro));
        }

        public ICommand AgregarSubtareaCommand => agregarSubtarea;
        public ICommand EnviarComentarioCommand => enviarComentario;
        public ICommand EliminarSubtareaCommand { get; }

        public int TareaId { get; set; }
        public int ProyectoId { get; private set; }

        public string Titulo { get => titulo; set => SetProperty(ref titulo, value); }
        public string Descripcion { get => descripcion; set => SetProperty(ref descripcion, value); }
        public string Estado { get => estado; set => SetProperty(ref estado, value); }

        public string Prioridad
        {
            get => prioridad;
            set
            {
                if (SetProperty(ref prioridad, value)) OnPropertyChanged(nameof(PrioridadTexto));
            }
        }
        public string PrioridadTexto => $"Prioridad {prioridad.ToLower()}";

        public string Proyecto { get => proyecto; set => SetProperty(ref proyecto, value); }
        public string Responsable { get => responsable; set => SetProperty(ref responsable, value); }
        public string Iniciales { get => iniciales; set => SetProperty(ref iniciales, value); }
        public Color AvatarColor { get => avatarColor; set => SetProperty(ref avatarColor, value); }
        public string FechaTexto { get => fechaTexto; set => SetProperty(ref fechaTexto, value); }
        public string SubtareasContador { get => subtareasContador; set => SetProperty(ref subtareasContador, value); }
        public double SubtareasProgreso { get => subtareasProgreso; set => SetProperty(ref subtareasProgreso, value); }

        // Se enlazan en modo TwoWay con los Entry
        public string NuevaSubtarea
        {
            get => nuevaSubtarea;
            set { if (SetProperty(ref nuevaSubtarea, value)) agregarSubtarea.RaiseCanExecuteChanged(); }
        }

        public string NuevoComentario
        {
            get => nuevoComentario;
            set { if (SetProperty(ref nuevoComentario, value)) enviarComentario.RaiseCanExecuteChanged(); }
        }

        // Las colecciones observables avisan a la vista al agregar o quitar elementos
        public ObservableCollection<SubtareaItem> Subtareas { get; } = new();
        public ObservableCollection<ComentarioItem> Comentarios { get; } = new();

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

        // ---------- Estado ----------
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
            {
                Subtareas.Add(CrearItem(s.Id, s.Titulo, s.Completada));
            }
            cargando = false;
            ActualizarProgreso();
        }

        private SubtareaItem CrearItem(int id, string titulo, bool completada)
        {
            var item = new SubtareaItem(id, titulo, completada) { EliminarCommand = EliminarSubtareaCommand };
            item.PropertyChanged += OnSubtareaCambio;
            return item;
        }

        // CommandParameter: llega el Id de la subtarea que se tocó
        private void EliminarSubtarea(object? parametro)
        {
            if (parametro is not int id) return;

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

        // Se dispara cuando el usuario marca o desmarca un CheckBox (TwoWay)
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

        private void AgregarSubtarea()
        {
            var texto = NuevaSubtarea.Trim();
            if (texto.Length == 0) return;

            var dbContext = new NexoraDbContext();
            var id = (dbContext.Subtareas.Any() ? dbContext.Subtareas.Max(s => s.Id) : 0) + 1;
            dbContext.Subtareas.Add(new Subtarea(id, TareaId, texto, false));
            dbContext.SaveChanges();

            Subtareas.Add(CrearItem(id, texto, false));   // la vista se actualiza sola
            ActualizarProgreso();
            NuevaSubtarea = "";           // y el Entry se limpia por el binding
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

        private void EnviarComentario()
        {
            var texto = NuevoComentario.Trim();
            if (texto.Length == 0) return;

            var dbContext = new NexoraDbContext();
            var id = (dbContext.Comentarios.Any() ? dbContext.Comentarios.Max(c => c.Id) : 0) + 1;
            var comentario = new Comentario(id, TareaId, Sesion.UsuarioActualId, texto, DateTime.Now);
            dbContext.Comentarios.Add(comentario);
            dbContext.SaveChanges();

            var autor = dbContext.Usuarios.First(u => u.Id == Sesion.UsuarioActualId).Nombre;
            Comentarios.Add(ComoItem(comentario, autor));
            NuevoComentario = "";
        }

        // ---------- Eliminar ----------
        public void Eliminar()
        {
            var dbContext = new NexoraDbContext();
            dbContext.Subtareas.RemoveRange(dbContext.Subtareas.Where(s => s.TareaId == TareaId));
            dbContext.Comentarios.RemoveRange(dbContext.Comentarios.Where(c => c.TareaId == TareaId));
            var tarea = dbContext.Tareas.FirstOrDefault(t => t.Id == TareaId);
            if (tarea != null) dbContext.Tareas.Remove(tarea);
            dbContext.SaveChanges();
        }
    }
}
