using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class TareaDetailPage : ContentPage, IQueryAttributable
    {
        private static readonly string[] Estados = { "Por hacer", "En progreso", "En revisión", "Hecha" };

        private int tareaId;
        private readonly SelectorOpciones selectorEstado;

        public TareaDetailPage()
        {
            InitializeComponent();
            selectorEstado = new SelectorOpciones(estadoGrid, Estados, 2, CambiarEstado);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            tareaId = int.Parse(query["id"].ToString()!);
        }

        // Se recarga al volver del formulario de edición
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Cargar();
        }

        private void Cargar()
        {
            var dbContext = new NexoraDbContext();
            var tarea = dbContext.Tareas
                .Include(t => t.Proyecto)
                .Include(t => t.Usuario)
                .FirstOrDefault(t => t.Id == tareaId);
            if (tarea == null) return;

            tituloLabel.Text = tarea.Titulo;
            descripcionLabel.Text = tarea.Descripcion;
            estadoLabel.Text = tarea.Estado;
            estadoChip.BackgroundColor = Estilos.ColorEstadoTarea(tarea.Estado);
            prioridadLabel.Text = $"Prioridad {tarea.Prioridad.ToLower()}";
            prioridadChip.BackgroundColor = Estilos.FondoPrioridad(tarea.Prioridad);
            proyectoLabel.Text = tarea.Proyecto.Nombre;
            responsableLabel.Text = tarea.Usuario.Nombre;
            inicialesLabel.Text = Estilos.Iniciales(tarea.Usuario.Nombre);
            avatar.BackgroundColor = Estilos.ColorAvatar(tarea.UsuarioId);
            fechaLabel.Text = $"{tarea.FechaLimite:dd/MM/yyyy}  ·  {Estilos.TextoVencimiento(tarea.FechaLimite, tarea.Estado)}";
            selectorEstado.Seleccionar(tarea.Estado);

            CargarSubtareas(dbContext);
            CargarComentarios(dbContext);
        }

        // ---------- Estado ----------
        private void CambiarEstado(string nuevo)
        {
            var dbContext = new NexoraDbContext();
            var tarea = dbContext.Tareas.First(t => t.Id == tareaId);
            dbContext.Entry(tarea).Property(t => t.Estado).CurrentValue = nuevo;
            tarea.FechaCompletada = nuevo == "Hecha" ? DateTime.Now : null;
            dbContext.SaveChanges();
            Cargar();
        }

        // ---------- Subtareas ----------
        private void CargarSubtareas(NexoraDbContext dbContext)
        {
            var subtareas = dbContext.Subtareas.Where(s => s.TareaId == tareaId).OrderBy(s => s.Id).ToList();
            var hechas = subtareas.Count(s => s.Completada);
            subtareasContador.Text = $"{hechas}/{subtareas.Count}";
            subtareasBarra.Progress = subtareas.Count == 0 ? 0 : (double)hechas / subtareas.Count;

            subtareasLayout.Children.Clear();
            foreach (var s in subtareas)
            {
                var check = new CheckBox { IsChecked = s.Completada, Color = Estilos.Primario, VerticalOptions = LayoutOptions.Center };
                var etiqueta = new Label
                {
                    Text = s.Titulo,
                    VerticalOptions = LayoutOptions.Center,
                    TextDecorations = s.Completada ? TextDecorations.Strikethrough : TextDecorations.None,
                    Opacity = s.Completada ? 0.6 : 1
                };
                var id = s.Id;
                check.CheckedChanged += (sender, e) => MarcarSubtarea(id, e.Value);

                var fila = new HorizontalStackLayout { Spacing = 6 };
                fila.Children.Add(check);
                fila.Children.Add(etiqueta);
                subtareasLayout.Children.Add(fila);
            }
        }

        private void MarcarSubtarea(int id, bool completada)
        {
            var dbContext = new NexoraDbContext();
            var subtarea = dbContext.Subtareas.First(s => s.Id == id);
            dbContext.Entry(subtarea).Property(s => s.Completada).CurrentValue = completada;
            dbContext.SaveChanges();
            CargarSubtareas(dbContext);
        }

        private void OnAgregarSubtarea(object? sender, EventArgs e)
        {
            var texto = subtareaEntry.Text?.Trim() ?? "";
            if (texto.Length == 0) return;

            var dbContext = new NexoraDbContext();
            var id = (dbContext.Subtareas.Any() ? dbContext.Subtareas.Max(s => s.Id) : 0) + 1;
            dbContext.Subtareas.Add(new Subtarea(id, tareaId, texto, false));
            dbContext.SaveChanges();

            subtareaEntry.Text = "";
            CargarSubtareas(dbContext);
        }

        // ---------- Comentarios ----------
        private void CargarComentarios(NexoraDbContext dbContext)
        {
            var cultura = new CultureInfo("es-ES");
            var comentarios = dbContext.Comentarios
                .Include(c => c.Usuario)
                .Where(c => c.TareaId == tareaId)
                .OrderBy(c => c.Fecha)
                .ToList();

            comentariosTitulo.Text = $"Comentarios ({comentarios.Count})";
            var items = comentarios
                .Select(c => new ComentarioItem(
                    Estilos.Iniciales(c.Usuario.Nombre), Estilos.ColorAvatar(c.UsuarioId),
                    c.Usuario.Nombre, c.Texto, c.Fecha.ToString("d MMM, HH:mm", cultura)))
                .ToList();
            BindableLayout.SetItemsSource(listaComentarios, items);
        }

        private void OnEnviarComentario(object? sender, EventArgs e)
        {
            var texto = comentarioEntry.Text?.Trim() ?? "";
            if (texto.Length == 0) return;

            var dbContext = new NexoraDbContext();
            var id = (dbContext.Comentarios.Any() ? dbContext.Comentarios.Max(c => c.Id) : 0) + 1;
            dbContext.Comentarios.Add(new Comentario(id, tareaId, Sesion.UsuarioActualId, texto, DateTime.Now));
            dbContext.SaveChanges();

            comentarioEntry.Text = "";
            CargarComentarios(dbContext);
        }

        // ---------- Editar y eliminar ----------
        private async void OnEditarClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(TareaFormPage)}?id={tareaId}");
        }

        private async void OnEliminarClicked(object? sender, EventArgs e)
        {
            var confirmar = await DisplayAlert("Eliminar tarea",
                "Se eliminarán también sus subtareas y comentarios. Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");
            if (!confirmar) return;

            var dbContext = new NexoraDbContext();
            dbContext.Subtareas.RemoveRange(dbContext.Subtareas.Where(s => s.TareaId == tareaId));
            dbContext.Comentarios.RemoveRange(dbContext.Comentarios.Where(c => c.TareaId == tareaId));
            var tarea = dbContext.Tareas.FirstOrDefault(t => t.Id == tareaId);
            if (tarea != null) dbContext.Tareas.Remove(tarea);
            dbContext.SaveChanges();

            await Shell.Current.GoToAsync("..");
        }
    }
}
