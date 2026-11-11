using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    // Sirve para crear (sin id) y para editar (con ?id=)
    public partial class TareaFormPage : ContentPage, IQueryAttributable
    {
        private int? tareaId;
        private readonly List<Proyecto> proyectos;
        private readonly List<Usuario> usuarios;
        private readonly SelectorOpciones selectorPrioridad;
        private readonly SelectorOpciones selectorEstado;

        public TareaFormPage()
        {
            InitializeComponent();

            var dbContext = new NexoraDbContext();
            proyectos = dbContext.Proyectos.OrderBy(p => p.Nombre).ToList();
            usuarios = dbContext.Usuarios.OrderBy(u => u.Nombre).ToList();
            proyectoPicker.ItemsSource = proyectos;
            responsablePicker.ItemsSource = usuarios;

            fechaPicker.Date = DateTime.Today.AddDays(7);

            selectorPrioridad = new SelectorOpciones(prioridadGrid, new[] { "Alta", "Media", "Baja" }, 3);
            selectorPrioridad.Seleccionar("Media");
            selectorEstado = new SelectorOpciones(estadoGrid, new[] { "Por hacer", "En progreso", "En revisión", "Hecha" }, 2);
            selectorEstado.Seleccionar("Por hacer");
        }

        // Modo edición: carga los datos de la tarea
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (!query.ContainsKey("id")) return;

            tareaId = int.Parse(query["id"].ToString()!);
            Title = "Editar tarea";

            var dbContext = new NexoraDbContext();
            var tarea = dbContext.Tareas.First(t => t.Id == tareaId);

            tituloEntry.Text = tarea.Titulo;
            descripcionEditor.Text = tarea.Descripcion;
            fechaPicker.Date = tarea.FechaLimite;
            proyectoPicker.SelectedItem = proyectos.First(p => p.Id == tarea.ProyectoId);
            responsablePicker.SelectedItem = usuarios.First(u => u.Id == tarea.UsuarioId);
            selectorPrioridad.Seleccionar(tarea.Prioridad);
            selectorEstado.Seleccionar(tarea.Estado);
        }

        private void MostrarError(string mensaje)
        {
            errorLabel.Text = mensaje;
            errorLabel.IsVisible = true;
        }

        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            errorLabel.IsVisible = false;

            var titulo = tituloEntry.Text?.Trim() ?? "";
            var descripcion = descripcionEditor.Text?.Trim() ?? "";
            var fecha = Convert.ToDateTime((object?)fechaPicker.Date);

            if (titulo.Length == 0) { MostrarError("Escribe el título de la tarea."); return; }
            if (proyectoPicker.SelectedItem is not Proyecto proyecto) { MostrarError("Selecciona el proyecto."); return; }
            if (responsablePicker.SelectedItem is not Usuario responsable) { MostrarError("Selecciona al responsable."); return; }

            var estado = selectorEstado.Valor;
            var dbContext = new NexoraDbContext();

            if (tareaId == null)
            {
                // Crear
                var id = (dbContext.Tareas.Any() ? dbContext.Tareas.Max(t => t.Id) : 0) + 1;
                var nueva = new Tarea(id, titulo, descripcion, selectorPrioridad.Valor, estado, fecha, proyecto.Id, responsable.Id);
                nueva.FechaCompletada = estado == "Hecha" ? DateTime.Now : null;
                dbContext.Tareas.Add(nueva);
            }
            else
            {
                // Editar: se copian los valores nuevos sobre el registro existente
                var original = dbContext.Tareas.First(t => t.Id == tareaId);
                var eraHecha = original.Estado == "Hecha";
                var fechaPrevia = original.FechaCompletada;
                dbContext.Entry(original).CurrentValues.SetValues(
                    new Tarea(original.Id, titulo, descripcion, selectorPrioridad.Valor, estado, fecha, proyecto.Id, responsable.Id));
                original.FechaCompletada = estado != "Hecha" ? null : (eraHecha ? fechaPrevia : DateTime.Now);
            }

            dbContext.SaveChanges();
            await Shell.Current.GoToAsync("..");
        }
    }
}
